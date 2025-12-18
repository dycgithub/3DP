using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Loops
{
    /// <summary>
    /// High-performance infinite scrolling list controller for UGUI
    /// Only instantiates items needed for viewport plus 1-2 extra items
    /// </summary>
    /// <typeparam name="T">The type of data to display</typeparam>
    [RequireComponent(typeof(ScrollRect))]
    public class LoopingScrollController<T> : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private float itemHeight = 100f;

        [Header("Performance Settings")]
        [SerializeField] private int extraItems = 2; // Extra items beyond viewport

        [Header("Debug")]
        [SerializeField] private bool debugLog = false;

        private ScrollRect scrollRect;
        private RectTransform contentRectTransform;
        private RectTransform viewportRectTransform;

        // Pool of recyclable items
        private readonly Queue<ILoopItem<T>> itemPool = new Queue<ILoopItem<T>>();
        private readonly List<ILoopItem<T>> activeItems = new List<ILoopItem<T>>();

        // Data management
        private IList<T> dataList;
        private int totalItemCount;
        private float contentHeight;

        // Performance optimization
        private readonly Vector2[] corners = new Vector2[4];
        private float viewportMinY;
        private float viewportMaxY;

        // Events
        public System.Action<int, T> OnItemVisible;

        /// <summary>
        /// Initialize the scrolling list with data
        /// </summary>
        /// <param name="data">List of data to display</param>
        public void Initialize(IList<T> data)
        {
            if (itemPrefab == null)
            {
                Debug.LogError("Item prefab is not assigned!", this);
                return;
            }

            // Cache references
            scrollRect = GetComponent<ScrollRect>();
            contentRectTransform = scrollRect.content;
            viewportRectTransform = scrollRect.viewport;

            // Setup scroll rect
            scrollRect.onValueChanged.RemoveListener(OnScrollPositionChanged);
            scrollRect.onValueChanged.AddListener(OnScrollPositionChanged);

            // Set data
            dataList = data;
            totalItemCount = data?.Count ?? 0;

            if (totalItemCount == 0)
            {
                ClearItems();
                return;
            }

            // Calculate content height
            contentHeight = totalItemCount * itemHeight;
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentHeight);

            // Initialize viewport bounds
            viewportRectTransform.GetWorldCorners(corners);
            viewportMinY = corners[0].y;
            viewportMaxY = corners[1].y;

            // Clear existing items
            ClearItems();

            // Initialize visible items
            InitializeVisibleItems();

            if (debugLog)
                Debug.Log($"Initialized with {totalItemCount} items, content height: {contentHeight}");
        }

        /// <summary>
        /// Clear all items and return them to pool
        /// </summary>
        private void ClearItems()
        {
            // Return all active items to pool
            for (int i = activeItems.Count - 1; i >= 0; i--)
            {
                var item = activeItems[i];
                ReturnItemToPool(item);
            }
            activeItems.Clear();
        }

        /// <summary>
        /// Initialize items that should be visible in the viewport
        /// </summary>
        private void InitializeVisibleItems()
        {
            if (totalItemCount == 0) return;

            float visibleHeight = viewportRectTransform.rect.height;
            int visibleItemCount = Mathf.CeilToInt(visibleHeight / itemHeight);
            int totalVisibleItems = visibleItemCount + extraItems * 2;

            // Clamp to total item count
            totalVisibleItems = Mathf.Min(totalVisibleItems, totalItemCount);

            // Calculate starting index based on scroll position
            float scrollNormalized = scrollRect.normalizedPosition.y;
            int startIndex = Mathf.FloorToInt((1f - scrollNormalized) * (totalItemCount - visibleItemCount));
            startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItemCount - visibleItemCount));

            if (debugLog)
                Debug.Log($"Creating {totalVisibleItems} visible items starting from index {startIndex}");

            // Create initial visible items
            for (int i = 0; i < totalVisibleItems; i++)
            {
                int dataIndex = startIndex + i;
                if (dataIndex >= totalItemCount) break;

                var item = GetItemFromPool();
                SetupItem(item, dataIndex);
                activeItems.Add(item);
            }
        }

        /// <summary>
        /// Get an item from the pool or create a new one
        /// </summary>
        private ILoopItem<T> GetItemFromPool()
        {
            if (itemPool.Count > 0)
            {
                var item = itemPool.Dequeue();
                item.RectTransform.gameObject.SetActive(true);
                return item;
            }

            // Create new item
            var newItem = Instantiate(itemPrefab, contentRectTransform);
            var loopItem = newItem.GetComponent<ILoopItem<T>>();

            if (loopItem == null)
            {
                Debug.LogError($"Item prefab must implement ILoopItem<{typeof(T).Name}> interface!", this);
                Destroy(newItem);
                return null;
            }

            // Set item height
            loopItem.RectTransform.sizeDelta = new Vector2(loopItem.RectTransform.sizeDelta.x, itemHeight);

            return loopItem;
        }

        /// <summary>
        /// Return an item to the pool for reuse
        /// </summary>
        private void ReturnItemToPool(ILoopItem<T> item)
        {
            if (item == null) return;

            item.RectTransform.gameObject.SetActive(false);
            item.ResetItem();
            itemPool.Enqueue(item);
        }

        /// <summary>
        /// Setup an item with data and position
        /// </summary>
        private void SetupItem(ILoopItem<T> item, int dataIndex)
        {
            if (item == null || dataIndex < 0 || dataIndex >= totalItemCount) return;

            // Set position
            item.RectTransform.anchoredPosition = new Vector2(0, -dataIndex * itemHeight);

            // Set data
            item.DataIndex = dataIndex;
            item.UpdateContent(dataList[dataIndex]);

            // Trigger event
            OnItemVisible?.Invoke(dataIndex, dataList[dataIndex]);
        }

        /// <summary>
        /// Handle scroll position changes
        /// </summary>
        private void OnScrollPositionChanged(Vector2 position)
        {
            if (totalItemCount == 0 || activeItems.Count == 0) return;

            // Update viewport bounds
            viewportRectTransform.GetWorldCorners(corners);
            viewportMinY = corners[0].y;
            viewportMaxY = corners[1].y;

            // Check which items need to be recycled
            RecycleItems();
        }

        /// <summary>
        /// Recycle items that are out of viewport and create new visible items
        /// </summary>
        private void RecycleItems()
        {
            float visibleHeight = viewportRectTransform.rect.height;
            float minY = -contentHeight;
            float maxY = 0;

            // Convert viewport bounds to content space
            viewportMinY = contentRectTransform.InverseTransformPoint(Vector3.up * viewportMinY).y;
            viewportMaxY = contentRectTransform.InverseTransformPoint(Vector3.up * viewportMaxY).y;

            // Extend bounds by extra items
            float extendedMinY = viewportMinY - itemHeight * extraItems;
            float extendedMaxY = viewportMaxY + itemHeight * extraItems;

            for (int i = activeItems.Count - 1; i >= 0; i--)
            {
                var item = activeItems[i];
                float itemY = item.RectTransform.anchoredPosition.y;

                // Check if item is outside the extended viewport
                if (itemY < extendedMinY || itemY > extendedMaxY + itemHeight)
                {
                    // Find new data index for this item
                    int newIndex = FindNewDataIndex(itemY);

                    if (newIndex >= 0 && newIndex < totalItemCount)
                    {
                        // Repurpose item for new data
                        SetupItem(item, newIndex);
                    }
                    else
                    {
                        // Return to pool if no valid data
                        activeItems.RemoveAt(i);
                        ReturnItemToPool(item);
                    }
                }
            }
        }

        /// <summary>
        /// Find the appropriate data index for an item at the given position
        /// </summary>
        private int FindNewDataIndex(float itemY)
        {
            // Find the closest visible item
            float closestY = float.MaxValue;
            int closestIndex = -1;

            for (int i = 0; i < activeItems.Count; i++)
            {
                var item = activeItems[i];
                float activeY = item.RectTransform.anchoredPosition.y;
                float distance = Mathf.Abs(activeY - itemY);

                if (distance < closestY)
                {
                    closestY = distance;
                    closestIndex = item.DataIndex;
                }
            }

            if (closestIndex == -1) return -1;

            // Calculate new index based on position relative to closest item
            float positionDiff = itemY - activeItems[0].RectTransform.anchoredPosition.y;
            int indexDiff = Mathf.RoundToInt(positionDiff / itemHeight);

            return closestIndex + indexDiff;
        }

        /// <summary>
        /// Refresh the list with new data
        /// </summary>
        public void Refresh(IList<T> newData)
        {
            Initialize(newData);
        }

        /// <summary>
        /// Update item at specific index
        /// </summary>
        public void UpdateItemAt(int index, T data)
        {
            if (index < 0 || index >= totalItemCount || dataList == null) return;

            dataList[index] = data;

            // Find active item with this index
            for (int i = 0; i < activeItems.Count; i++)
            {
                if (activeItems[i].DataIndex == index)
                {
                    activeItems[i].UpdateContent(data);
                    break;
                }
            }
        }

        /// <summary>
        /// Scroll to specific item index
        /// </summary>
        public void ScrollToIndex(int index, bool animated = true)
        {
            if (index < 0 || index >= totalItemCount) return;

            float targetY = (float)index / Mathf.Max(1, totalItemCount - 1);
            Vector2 targetPosition = new Vector2(0, 1 - targetY);

            if (animated)
            {
                // Simple animation - could be enhanced with coroutine for smooth scrolling
                scrollRect.normalizedPosition = Vector2.Lerp(scrollRect.normalizedPosition, targetPosition, Time.deltaTime * 10f);
            }
            else
            {
                scrollRect.normalizedPosition = targetPosition;
            }
        }

        /// <summary>
        /// Get the currently visible data indices
        /// </summary>
        public List<int> GetVisibleIndices()
        {
            var visibleIndices = new List<int>();
            for (int i = 0; i < activeItems.Count; i++)
            {
                visibleIndices.Add(activeItems[i].DataIndex);
            }
            return visibleIndices;
        }

        /// <summary>
        /// Cleanup when destroyed
        /// </summary>
        private void OnDestroy()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrollPositionChanged);
            }

            ClearItems();
        }
    }
}