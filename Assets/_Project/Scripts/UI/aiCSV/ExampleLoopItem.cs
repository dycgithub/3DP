using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Loops
{
    /// <summary>
    /// Example implementation of ILoopItem for string data
    /// This component should be attached to your item prefab
    /// </summary>
    public class ExampleLoopItem : MonoBehaviour, ILoopItem<string>
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button itemButton;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
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
            // Cache components
            _rectTransform = GetComponent<RectTransform>();

            // Setup button click
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
                descriptionText.text = $"This is item number {data} with index {_dataIndex}";
            }

            // Alternate colors for visual distinction
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
            Debug.Log($"Item clicked: {_currentData} (Index: {_dataIndex})");

            // You can trigger events or callbacks here
            // Example: OnItemClick?.Invoke(_dataIndex, _currentData);
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