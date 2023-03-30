using System.Collections.Generic;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.Currency;
using RGN.UI;
using UnityEngine;

namespace RGN.Samples
{
    public sealed class CurrencyExample : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private RectTransform _rgnCoinItemsContent;
        [SerializeField] private RectTransform _customCoinItemsContent;
        [SerializeField] private PullToRefresh _pullToReloadTheProducts;
        [SerializeField] private TMPro.TMP_InputField _currencyNameInputField;
        [SerializeField] private TMPro.TMP_InputField _currencyQuantityInputField;
        [SerializeField] private RGNButton _addCurrencyButton;

        [Header("Prefabs")]
        [SerializeField] private RGNCoinItem _rgnCoinItemPrefab;
        [SerializeField] private CustomCoinItem _customCoinItemPrefab;

        private bool _triedToLoad;
        private List<RGNCoinItem> _rgnCoinItems;
        private List<CustomCoinItem> _customCoinItems;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _rgnCoinItems = new List<RGNCoinItem>();
            _customCoinItems = new List<CustomCoinItem>();
            _pullToReloadTheProducts.RefreshRequested += ReloadAllProductsAsync;
            _addCurrencyButton.Button.onClick.AddListener(OnAddCurrencyButtonClickAsync);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _pullToReloadTheProducts.RefreshRequested -= ReloadAllProductsAsync;
            _addCurrencyButton.Button.onClick.RemoveListener(OnAddCurrencyButtonClickAsync);
        }
        protected override async void OnShow()
        {
            if (_triedToLoad)
            {
                return;
            }
            _triedToLoad = true;
            await ReloadAllProductsAsync();
        }

        internal void SetUIInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _fullScreenLoadingIndicator.SetEnabled(!interactable);
        }

        private async void OnAddCurrencyButtonClickAsync()
        {
            SetUIInteractable(false);
            bool failed = false;
            string currencyName = _currencyNameInputField.text;
            if (string.IsNullOrWhiteSpace(currencyName))
            {
                ToastMessage.I.Show("Please specify currency name");
                SetUIInteractable(true);
                return;
            }
            string currencyQuantityStr = _currencyQuantityInputField.text;
            if (string.IsNullOrWhiteSpace(currencyQuantityStr))
            {
                ToastMessage.I.Show("Please specify currency quantity");
                SetUIInteractable(true);
                return;
            }
            if (!int.TryParse(currencyQuantityStr, out int quantity))
            {
                ToastMessage.I.ShowError("Can not parse currency quantity to integer value");
                SetUIInteractable(true);
                return;
            }
            try
            {
                var currencyToAdd = new List<Currency>() {
                    new Currency(currencyName, quantity)
                };
                await CurrencyModule.I.AddCurrencyAsync(currencyToAdd);
            }
            catch (System.Exception ex)
            {
                failed = true;
                Debug.LogException(ex);
                ToastMessage.I.ShowError(ex.Message);
            }
            if (!failed)
            {
                ToastMessage.I.ShowSuccess("Successfully added " + currencyQuantityStr + " " + currencyName);
            }
            SetUIInteractable(true);
        }
        private async Task ReloadAllProductsAsync()
        {
            await ReloadRGNCoinOffersAsync();
            await ReloadCustomCoinOffersAsync();
        }
        private Task ReloadRGNCoinOffersAsync()
        {
            DisposeRGNCoinOffers();
            return LoadRGNCoinOffersAsync();
        }
        private async Task LoadRGNCoinOffersAsync()
        {
            SetUIInteractable(false);
            var offers = await CurrencyModule.I.GetRGNCoinEconomyAsync();
            for (int i = 0; i < offers.products.Count; i++)
            {
                var product = offers.products[i];
                RGNCoinItem item = Instantiate(_rgnCoinItemPrefab, _rgnCoinItemsContent);
                item.Init(this, i, product);
                _rgnCoinItems.Add(item);
            }
            _rgnCoinItemsContent.sizeDelta =
                new Vector2(
                    _rgnCoinItems.Count * (_rgnCoinItemPrefab.GetWidth() + RGNCoinItem.GAB_BETWEEN_ITEMS),
                    0);
            SetUIInteractable(true);
        }

        private void DisposeRGNCoinOffers()
        {
            for (int i = 0; i < _rgnCoinItems.Count; i++)
            {
                _rgnCoinItems[i].Dispose();
            }
            _rgnCoinItems.Clear();
        }
        private Task ReloadCustomCoinOffersAsync()
        {
            DisposeCustomCoinOffers();
            return LoadCustomCoinOffersAsync();
        }
        private async Task LoadCustomCoinOffersAsync()
        {
            SetUIInteractable(false);
            var offers = await CurrencyModule.I.GetInAppPurchaseCurrencyDataAsync();
            for (int i = 0; i < offers.products.Count; i++)
            {
                var product = offers.products[i];
                CustomCoinItem item = Instantiate(_customCoinItemPrefab, _customCoinItemsContent);
                item.Init(this, i, product);
                _customCoinItems.Add(item);
            }
            _customCoinItemsContent.sizeDelta =
                new Vector2(
                    _customCoinItems.Count * (_rgnCoinItemPrefab.GetWidth() + CustomCoinItem.GAB_BETWEEN_ITEMS),
                    0);
            SetUIInteractable(true);
        }

        private void DisposeCustomCoinOffers()
        {
            for (int i = 0; i < _customCoinItems.Count; i++)
            {
                _customCoinItems[i].Dispose();
            }
            _customCoinItems.Clear();
        }
    }
}
