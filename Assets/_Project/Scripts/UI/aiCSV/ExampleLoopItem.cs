using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Loops
{
    /// <summary>
    /// 字符串数据的ILoopItem实现示例
    /// 此组件应附加到您的项预制体上
    /// </summary>
    public class ExampleLoopItem : MonoBehaviour, ILoopItem<string>
    {
        [Header("UI 组件")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button itemButton;
        [SerializeField] private Image backgroundImage;

        [Header("颜色")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        private RectTransform _rectTransform;
        private int _dataIndex = -1;
        private string _currentData;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public int DataIndex
        {
            get => _dataIndex;
            set => _dataIndex = value;
        }

        private void Awake()
        {
            // 缓存组件
            _rectTransform = GetComponent<RectTransform>();

            // 设置按钮点击
            if (itemButton != null)
            {
                itemButton.onClick.AddListener(OnItemClicked);
            }
            
            
        }

        public void UpdateContent(string data)
        {
            _currentData = data;

            if (titleText != null)
            {
                titleText.text = $"Item {data}";
            }

            if (descriptionText != null)
            {
                descriptionText.text = $"这是第 {data} 个项，索引为 {_dataIndex}";
            }

            // 交替颜色以实现视觉区分
            if (backgroundImage != null)
            {
                backgroundImage.color = _dataIndex % 2 == 0 ? normalColor : selectedColor * 0.3f;
            }
        }

        public void ResetItem()
        {
            _currentData = null;
            _dataIndex = -1;

            if (titleText != null)
                titleText.text = string.Empty;

            if (descriptionText != null)
                descriptionText.text = string.Empty;

            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }

        private void OnItemClicked()
        {
            Debug.Log($"项被点击: {_currentData} (索引: {_dataIndex})");

            // 您可以在此处触发事件或回调
            // 示例: OnItemClick?.Invoke(_dataIndex, _currentData);
        }

        private void OnDestroy()
        {
            if (itemButton != null)
            {
                itemButton.onClick.RemoveListener(OnItemClicked);
            }
        }
    }
}