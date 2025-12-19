using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Loops
{
    /// <summary>
    /// 循环滚动控制器的使用示例
    /// 将此脚本附加到游戏对象上来演示滚动列表功能
    /// </summary>
    public class LoopingScrollExample : MonoBehaviour
    {
        [Header("滚动列表设置")]
        [SerializeField] private LoopingScrollController<string> scrollController;
        [SerializeField] private int itemCount = 100;

        [Header("控制按钮")]
        [SerializeField] private Button addButton;
        [SerializeField] private Button removeButton;
        [SerializeField] private Button scrollToTopButton;
        [SerializeField] private Button scrollToBottomButton;

        private List<string> dataItems;

        private void Start()
        {
            InitializeData();
            SetupScrollController();
            SetupButtons();
        }

        private void InitializeData()
        {
            dataItems = new List<string>();
            for (int i = 0; i < itemCount; i++)
            {
                dataItems.Add($"Item_{i:D4}");
            }
        }

        private void SetupScrollController()
        {
            if (scrollController != null)
            {
                scrollController.Initialize(dataItems);
                scrollController.OnItemVisible += OnItemVisible;
            }
        }

        private void SetupButtons()
        {
            if (addButton != null)
            {
                addButton.onClick.AddListener(AddNewItem);
            }

            if (removeButton != null)
            {
                removeButton.onClick.AddListener(RemoveLastItem);
            }

            if (scrollToTopButton != null)
            {
                scrollToTopButton.onClick.AddListener(() => scrollController?.ScrollToIndex(0));
            }

            if (scrollToBottomButton != null)
            {
                scrollToBottomButton.onClick.AddListener(() => scrollController?.ScrollToIndex(dataItems.Count - 1));
            }
        }

        private void AddNewItem()
        {
            string newItem = $"Item_{dataItems.Count:D4}";
            dataItems.Add(newItem);
            scrollController?.Refresh(dataItems);
        }

        private void RemoveLastItem()
        {
            if (dataItems.Count > 0)
            {
                dataItems.RemoveAt(dataItems.Count - 1);
                scrollController?.Refresh(dataItems);
            }
        }

        private void OnItemVisible(int index, string data)
        {
            // Debug.Log($"可见项: {data} 索引 {index}");
        }

        private void Update()
        {
            // 示例：基于输入或游戏状态更新特定项
            if (Input.GetKeyDown(KeyCode.U))
            {
                int randomIndex = Random.Range(0, dataItems.Count);
                dataItems[randomIndex] = $"Updated_{randomIndex:D4}";
                scrollController?.UpdateItemAt(randomIndex, dataItems[randomIndex]);
            }
        }

        private void OnDestroy()
        {
            if (scrollController != null)
            {
                scrollController.OnItemVisible -= OnItemVisible;
            }

            // 清理按钮监听器
            if (addButton != null)
            {
                addButton.onClick.RemoveAllListeners();
            }

            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
            }

            if (scrollToTopButton != null)
            {
                scrollToTopButton.onClick.RemoveAllListeners();
            }

            if (scrollToBottomButton != null)
            {
                scrollToBottomButton.onClick.RemoveAllListeners();
            }
        }
    }
}