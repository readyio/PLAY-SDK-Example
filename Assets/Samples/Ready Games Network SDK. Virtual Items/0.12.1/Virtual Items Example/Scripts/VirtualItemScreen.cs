using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Model;
using RGN.Modules.Store;
using RGN.Modules.VirtualItems;
using RGN.UI;
using RGN.Utility;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    internal sealed class VirtualItemScreenParameters
    {
        internal VirtualItem VirtualItem { get; }
        internal IVirtualItemsExampleClient VirtualItemsExampleClient { get; }

        internal VirtualItemScreenParameters(VirtualItem virtualItem, IVirtualItemsExampleClient virtualItemsExampleClient)
        {
            VirtualItem = virtualItem;
            VirtualItemsExampleClient = virtualItemsExampleClient;
        }
    }

    public sealed class VirtualItemScreen : IUIScreen
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
        [SerializeField] private TextMeshProUGUI _propertiesText;
        [SerializeField] private TextMeshProUGUI _isStackableText;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private IconImage _virtualItemIconImage;
        [SerializeField] private RectTransform _scrollRectContent;
        [SerializeField] private RectTransform _buyButtonsAnchor;
        [SerializeField] private UnityEngine.UI.Image _isNFTImage;
        [SerializeField] private UnityEngine.UI.Image _isStackableImage;

        [SerializeField] private RGNButton _actionButtonForBuyPrefab;

        private VirtualItem _virtualItem;
        private IVirtualItemsExampleClient _virtualItemsExampleClient;
        private List<RGNButton> _buyButtons;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _buyButtons = new List<RGNButton>();
            _virtualItemIconImage.OnClick.AddListener(OnUploadNewProfilePictureButtonClickAsync);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _virtualItemIconImage.OnClick.RemoveListener(OnUploadNewProfilePictureButtonClickAsync);
        }
        public override async void OnWillAppearNow(object parameters)
        {
            var castedParams = parameters as VirtualItemScreenParameters;
            _virtualItem = castedParams.VirtualItem;
            _virtualItemsExampleClient = castedParams.VirtualItemsExampleClient;
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
            _propertiesText.text = Properties.BuildStringFromPropertiesList(_virtualItem.properties);
            _isNFTImage.color = _virtualItem.IsNFT() ? RGNUISettings.I.ActiveColor : Color.gray;
            _isStackableImage.color = _virtualItem.isStackable ? RGNUISettings.I.ActiveColor : Color.gray;
            InstantiateBuyButtonsForEachPrice(_virtualItem.id, _virtualItem.prices);
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
                byte[] bytes = await VirtualItemsModule.I.DownloadImageAsync(virtualItemId, ImageSize.Small);

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
        private async void OnUploadNewProfilePictureButtonClickAsync()
        {
            _canvasGroup.interactable = false;
            _fullScreenLoadingIndicator.SetEnabled(true);
            var tcs = new TaskCompletionSource<bool>();
            NativeGallery.GetImageFromGallery(async path => {
                try
                {
                    if (path == null)
                    {
                        Debug.Log("User cancelled the image upload, or no permission granted");
                        tcs.TrySetResult(false);
                        return;
                    }
                    if (!File.Exists(path))
                    {
                        Debug.LogError("File does not exist at path: " + path);
                        tcs.TrySetResult(false);
                        return;
                    }
                    byte[] textureBytes = File.ReadAllBytes(path);
                    await VirtualItemsModule.I.UploadImageAsync(_virtualItem.id, textureBytes);
                    await LoadIconImageAsync(_virtualItem.id, false);
                    tcs.TrySetResult(true);
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                    tcs.TrySetException(ex);
                }
            },
            "Select Virtual Item Image");
            await tcs.Task;
            _fullScreenLoadingIndicator.SetEnabled(false);
            _canvasGroup.interactable = true;
        }

        private void InstantiateBuyButtonsForEachPrice(string virtualItemId, List<PriceInfo> priceInfos)
        {
            DisposeAllPriceButtons();
            if (priceInfos == null || priceInfos.Count == 0)
            {
                return;
            }
            float pricesTitleTextPosition = _buyButtonsAnchor.localPosition.y;
            float buttonsCurrentYPos = pricesTitleTextPosition;
            Dictionary<string, List<PriceInfo>> pricesByGroup = new Dictionary<string, List<PriceInfo>>();
            const float GAB = 8f;
            for (int i = 0; i < priceInfos.Count; ++i)
            {
                var priceInfo = priceInfos[i];
                if (!string.IsNullOrEmpty(priceInfo.group))
                {
                    if (pricesByGroup.TryGetValue(priceInfo.group, out var group))
                    {
                        group.Add(priceInfo);
                    }
                    else
                    {
                        pricesByGroup.Add(priceInfo.group, new List<PriceInfo>() { priceInfo });
                    }
                    continue;
                }
                RGNButton button = Instantiate(_actionButtonForBuyPrefab, _scrollRectContent);
                button.RectTransform.localPosition = new Vector2(16, buttonsCurrentYPos);
                buttonsCurrentYPos -= button.GetHeight() + GAB;
                button.ButtonText.text = priceInfo.ToDiscountPriceCurrencyString();
                button.Button.onClick.AddListener(async () => {
                    await OnBuyButtonClickAsync(virtualItemId, new List<string> { priceInfo.name });
                });
                _buyButtons.Add(button);
            }
            foreach (var priceGroup in pricesByGroup)
            {
                RGNButton button = Instantiate(_actionButtonForBuyPrefab, _scrollRectContent);
                float buttonInitialHeight = _actionButtonForBuyPrefab.GetHeight();
                button.SetHeight(buttonInitialHeight * priceGroup.Value.Count);
                button.RectTransform.localPosition = new Vector2(16, buttonsCurrentYPos);
                buttonsCurrentYPos -= button.GetHeight() + GAB;
                var sb = new System.Text.StringBuilder();
                sb.Append("PriceGroup ").Append(priceGroup.Key).AppendLine(": ");
                var currencies = new List<string>();
                for (int i = 0; i < priceGroup.Value.Count; i++)
                {
                    var priceInfo = priceGroup.Value[i];
                    sb.Append(priceInfo.ToDiscountPriceCurrencyString());
                    if (i < priceGroup.Value.Count - 1)
                    {
                        sb.AppendLine(", ");
                    }
                    currencies.Add(priceInfo.name);
                }
                button.ButtonText.text = sb.ToString();
                button.Button.onClick.AddListener(async () => {
                    await OnBuyButtonClickAsync(virtualItemId, currencies);
                });
                _buyButtons.Add(button);
            }
            var sizeDelta = _scrollRectContent.sizeDelta;
            _scrollRectContent.sizeDelta = new Vector2(sizeDelta.x, -buttonsCurrentYPos);
        }

        private async Task OnBuyButtonClickAsync(string virtualItemId, List<string> currencies)
        {
            bool failed = false;
            _canvasGroup.interactable = false;
            _fullScreenLoadingIndicator.SetEnabled(true);
            try
            {
                if (_virtualItem.IsNFT())
                {
                    Debug.Log("The virtual item is NFT: " + _virtualItem.id);
                    if (_virtualItemsExampleClient == null)
                    {
                        string message = "The virtual item is an NFT, you need to use WalletsModule. " +
                            "Please open the UIRoot Sample from Firebase Impl package to use this functionality";
                        Debug.LogError(message);
                        ToastMessage.I.Show(message);
                        _fullScreenLoadingIndicator.SetEnabled(false);
                        _canvasGroup.interactable = true;
                        return;
                    }
                    int userRGNCoinBalance = _virtualItemsExampleClient.GetCurrentUserRGNCoinBalance();
                    if (userRGNCoinBalance < _virtualItem.GetRGNCoinPrice())
                    {
                        ToastMessage.I.Show("Not enough RGN Coins, please purchase more.");
                        _virtualItemsExampleClient.OpenCurrenciesScreen();
                        _fullScreenLoadingIndicator.SetEnabled(false);
                        _canvasGroup.interactable = true;
                        return;
                    }
                    var primaryWalletExists = await _virtualItemsExampleClient.DoesTheUserHasPrimaryWalletAddressAsync();
                    if (!primaryWalletExists)
                    {
                        ToastMessage.I.Show("Please create a primary wallet to purchase the NFT virtual item");
                        _virtualItemsExampleClient.OpenWalletsScreen();
                        _fullScreenLoadingIndicator.SetEnabled(false);
                        _canvasGroup.interactable = true;
                        return;
                    }
                }
                await StoreModule.I.BuyVirtualItemsAsync(
                    new List<string>() { virtualItemId },
                    currencies);
                if (_virtualItemsExampleClient != null)
                {
                    await _virtualItemsExampleClient.UpdateUserProfileAsync();
                }
            }
            catch (System.Exception ex)
            {
                failed = true;
                Debug.LogException(ex);
                ToastMessage.I.ShowError(ex.Message);
            }
            _fullScreenLoadingIndicator.SetEnabled(false);
            _canvasGroup.interactable = true;
            if (!failed)
            {
                ToastMessage.I.ShowSuccess("Successfully purchased virtual item with id: " + virtualItemId);
            }
        }

        private void DisposeAllPriceButtons()
        {
            for (int i = 0; i < _buyButtons.Count; ++i)
            {
                _buyButtons[i].Dispose();
            }
            _buyButtons.Clear();
        }
    }
}
