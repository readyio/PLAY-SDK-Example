using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.Inventory;
using RGN.Modules.VirtualItems;
using RGN.UI;
using RGN.Utility;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    internal sealed class InventoryItemScreenParameters
    {
        internal InventoryItemData InventoryItemData { get; }

        internal InventoryItemScreenParameters(InventoryItemData inventoryItemData)
        {
            InventoryItemData = inventoryItemData;
        }
    }

    public sealed class InventoryItemScreen : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _idText;
        [SerializeField] private TextMeshProUGUI _createdAtText;
        [SerializeField] private TextMeshProUGUI _updatedAtText;
        [SerializeField] private TextMeshProUGUI _createdByText;
        [SerializeField] private TextMeshProUGUI _updatedByText;
        [SerializeField] private TextMeshProUGUI _tagsText;
        [SerializeField] private TextMeshProUGUI _appIdsText;
        [SerializeField] private TextMeshProUGUI _childIdsText;
        [SerializeField] private TextMeshProUGUI _propertiesText;
        [SerializeField] private TextMeshProUGUI _isStackableText;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private IconImage _virtualItemIconImage;
        [SerializeField] private RectTransform _scrollRectContent;
        [SerializeField] private UnityEngine.UI.Image _isNFTImage;
        [SerializeField] private UnityEngine.UI.Image _isStackableImage;
        [SerializeField] private TextMeshProUGUI _inventoryItemIdText;
        [SerializeField] private TextMeshProUGUI _inventoryItemTagsText;
        [SerializeField] private TextMeshProUGUI _inventoryItemAppIdsText;
        [SerializeField] private TextMeshProUGUI _inventoryItemQuantityText;
        [SerializeField] private TextMeshProUGUI _inventoryItemUpgradesText;
        [SerializeField] private TextMeshProUGUI _inventoryItemPropertiesText;

        private InventoryItemData _inventoryItemData;
        private VirtualItem _virtualItem;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
        public override async void OnWillAppearNow(object parameters)
        {
            var castedParams = parameters as InventoryItemScreenParameters;
            _inventoryItemData = castedParams.InventoryItemData;
            _virtualItem = _inventoryItemData.GetItem();
            if (_inventoryItemData == null)
            {
                Debug.LogError("The provided inventory item data is null or invalid");
                return;
            }
            if (_virtualItem == null)
            {
                Debug.LogError("The provided virtual item is null or invalid");
                return;
            }
            _titleText.text = _virtualItem.name;
            _descriptionText.text = _virtualItem.description;
            _idText.text = _virtualItem.id;
            _createdAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(_virtualItem.createdAt);
            _updatedAtText.text = DateTimeUtility.UnixTimeStampToISOLikeStringNoMilliseconds(_virtualItem.updatedAt);
            _createdByText.text = _virtualItem.createdBy;
            _updatedByText.text = _virtualItem.updatedBy;
            _isStackableText.text = _virtualItem.isStackable ? "Item is stackable" : "Item is not stackable";
            _tagsText.text = StringsUtility.BuildStringFromStringsList(_virtualItem.tags, "tags");
            _appIdsText.text = StringsUtility.BuildStringFromStringsList(_virtualItem.appIds, "app ids");
            _childIdsText.text = StringsUtility.BuildStringFromStringsList(_virtualItem.childs, "virtual item childs");
            _propertiesText.text = Properties.BuildStringFromPropertiesList(_virtualItem.properties);
            _isNFTImage.color = _virtualItem.IsNFT() ? RGNUISettings.I.ActiveColor : Color.gray;
            _isStackableImage.color = _virtualItem.isStackable ? RGNUISettings.I.ActiveColor : Color.gray;
            _inventoryItemIdText.text = _inventoryItemData.id;
            _inventoryItemTagsText.text = StringsUtility.BuildStringFromStringsList(_inventoryItemData.tags, "tags");
            _inventoryItemAppIdsText.text = StringsUtility.BuildStringFromStringsList(_inventoryItemData.appIds, "appIds");
            _inventoryItemQuantityText.text = _inventoryItemData.quantity.ToString();
            _inventoryItemUpgradesText.text = VirtualItemUpgrade.BuildStringFromUpgradesList(_inventoryItemData.itemUpgrades);
            _inventoryItemPropertiesText.text = Properties.BuildStringFromPropertiesList(_inventoryItemData.properties);

            _fullScreenLoadingIndicator.SetEnabled(false);
            await LoadIconImageAsync(_virtualItem.id, false);
        }

        private async Task LoadIconImageAsync(string virtualItemId, bool tryToloadFromCache)
        {
            _canvasGroup.interactable = false;
            _virtualItemIconImage.SetLoading(true);
            string localPath = Path.Combine(Application.persistentDataPath, "virtual_items", virtualItemId + ".png");
            Texture2D image = null;
            if (tryToloadFromCache)
            {
                if (File.Exists(localPath))
                {
                    byte[] bytes = File.ReadAllBytes(localPath);
                    image = new Texture2D(1, 1);
                    image.LoadImage(bytes);
                    image.Apply();
                }
            }
            if (image == null)
            {
                byte[] bytes = await VirtualItemsModule.I.DownloadImageAsync(virtualItemId);

                if (bytes != null)
                {
                    image = new Texture2D(1, 1);
                    image.LoadImage(bytes);
                    image.Apply();
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                    File.WriteAllBytes(localPath, bytes);
                }
            }
            _virtualItemIconImage.SetProfileTexture(image);
            _canvasGroup.interactable = true;
            _virtualItemIconImage.SetLoading(false);
        }
    }
}
