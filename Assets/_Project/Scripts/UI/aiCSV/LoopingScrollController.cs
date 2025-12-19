using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Loops
{
    /// <summary>
    /// 用于UGUI的高性能无限滚动列表控制器
    /// 只实例化视口所需项以及1-2个额外项
    /// </summary>
    /// <typeparam name="T">要显示的数据类型</typeparam>
    [RequireComponent(typeof(ScrollRect))]
    public class LoopingScrollController<T> : MonoBehaviour
    {
        [Header("设置")]
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private float itemHeight = 100f;

        [Header("性能设置")]
        [SerializeField] private int extraItems = 2; // 视口之外的额外项

        [Header("调试")]
        [SerializeField] private bool debugLog = false;

        private ScrollRect scrollRect;
        private RectTransform contentRectTransform;
        private RectTransform viewportRectTransform;

        // 可回收项的池
        private readonly Queue<ILoopItem<T>> itemPool = new Queue<ILoopItem<T>>();
        private readonly List<ILoopItem<T>> activeItems = new List<ILoopItem<T>>();

        // 数据管理
        private IList<T> dataList;
        private int totalItemCount;
        private float contentHeight;

        // 性能优化
        private readonly Vector3[] corners = new Vector3[4];
        private float viewportMinY;
        private float viewportMaxY;

        // 事件
        public System.Action<int, T> OnItemVisible;

        /// <summary>
        /// 使用数据初始化滚动列表
        /// </summary>
        /// <param name="data">要显示的数据列表</param>
        public void Initialize(IList<T> data)
        {
            if (itemPrefab == null)
            {
                Debug.LogError("未分配项预制体！", this);
                return;
            }

            // 缓存引用
            scrollRect = GetComponent<ScrollRect>();
            contentRectTransform = scrollRect.content;
            viewportRectTransform = scrollRect.viewport;

            // 设置滚动矩形
            scrollRect.onValueChanged.RemoveListener(OnScrollPositionChanged);
            scrollRect.onValueChanged.AddListener(OnScrollPositionChanged);

            // 设置数据
            dataList = data;
            totalItemCount = data?.Count ?? 0;

            if (totalItemCount == 0)
            {
                ClearItems();
                return;
            }

            // 计算内容高度
            contentHeight = totalItemCount * itemHeight;
            contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, contentHeight);

            // 初始化视口边界
            viewportRectTransform.GetWorldCorners(corners);
            viewportMinY = corners[0].y;
            viewportMaxY = corners[1].y;

            // 清除现有项
            ClearItems();

            // 初始化可见项
            InitializeVisibleItems();

            if (debugLog)
                Debug.Log($"初始化了 {totalItemCount} 个项目，内容高度: {contentHeight}");
        }

        /// <summary>
        /// 清除所有项并将其返回到池中
        /// </summary>
        private void ClearItems()
        {
            // 将所有活动项返回到池中
            for (int i = activeItems.Count - 1; i >= 0; i--)
            {
                var item = activeItems[i];
                ReturnItemToPool(item);
            }
            activeItems.Clear();
        }

        /// <summary>
        /// 初始化应在视口中可见的项
        /// </summary>
        private void InitializeVisibleItems()
        {
            if (totalItemCount == 0) return;

            float visibleHeight = viewportRectTransform.rect.height;
            int visibleItemCount = Mathf.CeilToInt(visibleHeight / itemHeight);
            int totalVisibleItems = visibleItemCount + extraItems * 2;

            // 限制总项数
            totalVisibleItems = Mathf.Min(totalVisibleItems, totalItemCount);

            // 根据滚动位置计算起始索引
            float scrollNormalized = scrollRect.normalizedPosition.y;
            int startIndex = Mathf.FloorToInt((1f - scrollNormalized) * (totalItemCount - visibleItemCount));
            startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, totalItemCount - visibleItemCount));

            if (debugLog)
                Debug.Log($"创建 {totalVisibleItems} 个可见项，起始索引为 {startIndex}");

            // 创建初始可见项
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
        /// 从池中获取项或创建新项
        /// </summary>
        private ILoopItem<T> GetItemFromPool()
        {
            if (itemPool.Count > 0)
            {
                var item = itemPool.Dequeue();
                item.RectTransform.gameObject.SetActive(true);
                return item;
            }

            // 创建新项
            var newItem = Instantiate(itemPrefab, contentRectTransform);
            var loopItem = newItem.GetComponent<ILoopItem<T>>();

            if (loopItem == null)
            {
                Debug.LogError($"项预制体必须实现 ILoopItem<{typeof(T).Name}> 接口！", this);
                Destroy(newItem);
                return null;
            }

            // 设置项高度
            loopItem.RectTransform.sizeDelta = new Vector2(loopItem.RectTransform.sizeDelta.x, itemHeight);

            return loopItem;
        }

        /// <summary>
        /// 将项返回到池中以供重用
        /// </summary>
        private void ReturnItemToPool(ILoopItem<T> item)
        {
            if (item == null) return;

            item.RectTransform.gameObject.SetActive(false);
            item.ResetItem();
            itemPool.Enqueue(item);
        }

        /// <summary>
        /// 使用数据和位置设置项
        /// </summary>
        private void SetupItem(ILoopItem<T> item, int dataIndex)
        {
            if (item == null || dataIndex < 0 || dataIndex >= totalItemCount) return;

            // 设置位置
            item.RectTransform.anchoredPosition = new Vector2(0, -dataIndex * itemHeight);

            // 设置数据
            item.DataIndex = dataIndex;
            item.UpdateContent(dataList[dataIndex]);

            // 触发事件
            OnItemVisible?.Invoke(dataIndex, dataList[dataIndex]);
        }

        /// <summary>
        /// 处理滚动位置变化
        /// </summary>
        private void OnScrollPositionChanged(Vector2 position)
        {
            if (totalItemCount == 0 || activeItems.Count == 0) return;

            // 更新视口边界
            viewportRectTransform.GetWorldCorners(corners);
            viewportMinY = corners[0].y;
            viewportMaxY = corners[1].y;

            // 检查哪些项需要回收
            RecycleItems();
        }

        /// <summary>
        /// 回收视口外的项并创建新的可见项
        /// </summary>
        private void RecycleItems()
        {
            float visibleHeight = viewportRectTransform.rect.height;
            float minY = -contentHeight;
            float maxY = 0;

            // 将视口边界转换为内容空间
            viewportMinY = contentRectTransform.InverseTransformPoint(Vector3.up * viewportMinY).y;
            viewportMaxY = contentRectTransform.InverseTransformPoint(Vector3.up * viewportMaxY).y;

            // 通过额外项扩展边界
            float extendedMinY = viewportMinY - itemHeight * extraItems;
            float extendedMaxY = viewportMaxY + itemHeight * extraItems;

            for (int i = activeItems.Count - 1; i >= 0; i--)
            {
                var item = activeItems[i];
                float itemY = item.RectTransform.anchoredPosition.y;

                // 检查项是否在扩展视口之外
                if (itemY < extendedMinY || itemY > extendedMaxY + itemHeight)
                {
                    // 为此项查找新数据索引
                    int newIndex = FindNewDataIndex(itemY);

                    if (newIndex >= 0 && newIndex < totalItemCount)
                    {
                        // 重新利用项用于新数据
                        SetupItem(item, newIndex);
                    }
                    else
                    {
                        // 如果没有有效数据则返回到池中
                        activeItems.RemoveAt(i);
                        ReturnItemToPool(item);
                    }
                }
            }
        }

        /// <summary>
        /// 查找给定位置项的适当数据索引
        /// </summary>
        private int FindNewDataIndex(float itemY)
        {
            // 查找最近的可见项
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

            // 根据相对于最近项的位置计算新索引
            float positionDiff = itemY - activeItems[0].RectTransform.anchoredPosition.y;
            int indexDiff = Mathf.RoundToInt(positionDiff / itemHeight);

            return closestIndex + indexDiff;
        }

        /// <summary>
        /// 使用新数据刷新列表
        /// </summary>
        public void Refresh(IList<T> newData)
        {
            Initialize(newData);
        }

        /// <summary>
        /// 更新指定索引处的项
        /// </summary>
        public void UpdateItemAt(int index, T data)
        {
            if (index < 0 || index >= totalItemCount || dataList == null) return;

            dataList[index] = data;

            // 查找具有此索引的活动项
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
        /// 滚动到指定项索引
        /// </summary>
        public void ScrollToIndex(int index, bool animated = true)
        {
            if (index < 0 || index >= totalItemCount) return;

            float targetY = (float)index / Mathf.Max(1, totalItemCount - 1);
            Vector2 targetPosition = new Vector2(0, 1 - targetY);

            if (animated)
            {
                // 简单动画 - 可以通过协程增强以实现平滑滚动
                scrollRect.normalizedPosition = Vector2.Lerp(scrollRect.normalizedPosition, targetPosition, Time.deltaTime * 10f);
            }
            else
            {
                scrollRect.normalizedPosition = targetPosition;
            }
        }

        /// <summary>
        /// 获取当前可见的数据索引
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
        /// 销毁时清理
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