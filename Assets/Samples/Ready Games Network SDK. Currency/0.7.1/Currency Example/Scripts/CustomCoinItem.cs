using System.Threading.Tasks;
using RGN.Modules.Currency;
using RGN.UI;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    public sealed class CustomCoinItem : MonoBehaviour, System.IDisposable
    {
        internal const float GAB_BETWEEN_ITEMS = 8;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private RGNButton _buyButton;

        private CurrencyExample _currencyExample;
        private CurrencyProduct _product;

        internal void Init(CurrencyExample currencyExample, int index, CurrencyProduct product)
        {
            _currencyExample = currencyExample;
            _product = product;
            _quantityText.text = product.quantity.ToString();
            _priceText.text = product.price.ToString() + "$";
            _nameText.text = product.currencyName;
            _rectTransform.localPosition = new Vector3(index * (GetWidth() + GAB_BETWEEN_ITEMS), 0, 0);
            _buyButton.Button.onClick.AddListener(OnBuyButtonClickAsync);
        }
        public void Dispose()
        {
            _buyButton.Button.onClick.RemoveListener(OnBuyButtonClickAsync);
            Destroy(gameObject);
        }

        internal float GetWidth()
        {
            return _rectTransform.sizeDelta.x;
        }

        private async void OnBuyButtonClickAsync()
        {
            _currencyExample.SetUIInteractable(false);
            ToastMessage.I.Show("Simulating the in-app purchase process now...");
            await Task.Delay(5000);
            ToastMessage.I.ShowSuccess("In app purchase successfull");
            await Task.Delay(3000);
            ToastMessage.I.Show($"Adding {_product.currencyName} to users data...");
            bool failed = false;
            try
            {
                var result = await CurrencyModule.I.PurchaseCurrencyProductAsync(_product.id);
            }
            catch (System.Exception ex)
            {
                failed = true;
                Debug.LogException(ex);
                ToastMessage.I.ShowError(ex.Message);
            }
            if (!failed)
            {
                ToastMessage.I.ShowSuccess($"{_product.currencyName} successfully added to users data");
            }
            _currencyExample.SetUIInteractable(true);
        }
    }
}
