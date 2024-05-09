using RGN.Modules.VirtualItems;
using RGN.UI;
using RGN.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    internal sealed class VirtualItemUI : MonoBehaviour, System.IDisposable
    {
        public string Id { get => _virtualItem.id; }

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _createdAtText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private Image _isNFTItemImage;
        [SerializeField] private Image _isStackableItemImage;

        [SerializeField] private Button _openVirtualItemScreenButton;

        private Impl.Firebase.IRGNFrame _rgnFrame;
        private VirtualItem _virtualItem;
        private IVirtualItemsExampleClient _virtualItemsExampleClient;
        private bool _disposed = false;

        internal void Init(
            Impl.Firebase.IRGNFrame rgnFrame,
            int index,
            VirtualItem virtualItem,
            IVirtualItemsExampleClient virtualItemsExampleClient)
        {
            _rgnFrame = rgnFrame;
            _virtualItem = virtualItem;
            _virtualItemsExampleClient = virtualItemsExampleClient;
            _rectTransform.localPosition = new Vector3(0, -index * GetHeight(), 0);
            _idText.text = virtualItem.id;
            _nameText.text = virtualItem.name;
            _createdAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(virtualItem.createdAt);
            _updatedAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(virtualItem.updatedAt);
            _descriptionText.text = virtualItem.description;
            _isNFTItemImage.color = virtualItem.IsNFT() ? RGNUISettings.I.ActiveColor : Color.gray;
            _isStackableItemImage.color = virtualItem.isStackable ? RGNUISettings.I.ActiveColor : Color.gray;
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
            _rgnFrame.OpenScreen<VirtualItemScreen>(
                new VirtualItemScreenParameters(
                    _virtualItem,
                    _virtualItemsExampleClient));
        }
    }
}
