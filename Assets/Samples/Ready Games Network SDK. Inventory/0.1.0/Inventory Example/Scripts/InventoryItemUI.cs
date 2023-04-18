using RGN.Modules.Inventory;
using RGN.Modules.VirtualItems;
using RGN.UI;
using RGN.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    internal sealed class InventoryItemUI : MonoBehaviour, System.IDisposable
    {
        public string Id { get => _inventoryItemData.id; }

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _inventoryItemIdText;
        [SerializeField] private TextMeshProUGUI _virtualItemIdText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _createdAtText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Image _isNFTItemImage;
        [SerializeField] private Image _isStackableItemImage;

        [SerializeField] private Button _openVirtualItemScreenButton;

        private Impl.Firebase.IRGNFrame _rgnFrame;
        private InventoryItemData _inventoryItemData;
        private VirtualItem _virtualItem;
        private bool _disposed = false;

        internal void Init(
            Impl.Firebase.IRGNFrame rgnFrame,
            int index,
            InventoryItemData inventoryItemData)
        {
            _rgnFrame = rgnFrame;
            _inventoryItemData = inventoryItemData;
            _virtualItem = inventoryItemData.GetItem();
            _rectTransform.localPosition = new Vector3(0, -index * GetHeight(), 0);
            _inventoryItemIdText.text = inventoryItemData.id;
            _virtualItemIdText.text = _virtualItem == null ? "Virtual Item Is null" : _virtualItem.id;
            _quantityText.text = inventoryItemData.quantity.ToString();
            _nameText.text = _virtualItem == null ? "Virtual Item Is null" : _virtualItem.name;
            _descriptionText.text = _virtualItem == null ? "Virtual Item Is null" : _virtualItem.description;
            if (_virtualItem == null)
            {
                return;
            }
            _createdAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(_virtualItem.createdAt);
            _updatedAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(_virtualItem.updatedAt);
            _isNFTItemImage.color = _virtualItem.IsNFT() ? RGNUISettings.I.ActiveColor : Color.gray;
            _isStackableItemImage.color = _virtualItem.isStackable ? RGNUISettings.I.ActiveColor : Color.gray;
            _openVirtualItemScreenButton.onClick.AddListener(OnOpenVirtualItemScreenButtonClick);
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            _openVirtualItemScreenButton.onClick.RemoveListener(OnOpenVirtualItemScreenButtonClick);
            _disposed = true;
        }

        internal float GetHeight()
        {
            return _rectTransform.sizeDelta.y;
        }

        private void OnOpenVirtualItemScreenButtonClick()
        {
            _rgnFrame.OpenScreen<InventoryItemScreen>(
                new InventoryItemScreenParameters(_inventoryItemData));
        }
    }
}
