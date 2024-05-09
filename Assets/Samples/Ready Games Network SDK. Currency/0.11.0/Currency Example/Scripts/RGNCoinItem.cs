using System.Threading.Tasks;
using RGN.Modules.Currency;
using RGN.UI;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    public sealed class RGNCoinItem : MonoBehaviour, System.IDisposable
    {
        internal const float GAB_BETWEEN_ITEMS = 8;

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private RGNButton _buyButton;

        private CurrencyExample _currencyExample;
        private RGNCoinEconomyProduct _product;

        internal void Init(CurrencyExample currencyExample, int index, RGNCoinEconomyProduct product)
        {
            _currencyExample = currencyExample;
            _product = product;
            _quantityText.text = product.quantity.ToString();
            _priceText.text = product.priceInUSD.ToString() + "$";
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
            ToastMessage.I.Show("Adding RGN Coin to users data...");
            string iapTransactionId = System.Guid.NewGuid().ToString();// TODO: get it from the iap plugin callback
            string iapReceipt = System.Guid.NewGuid().ToString();// TODO: get it from the iap plugin callback

            bool failed = false;
            try
            {
                var result = await CurrencyModule.I.PurchaseRGNCoinAsync(_product.uid, iapTransactionId, iapReceipt);
            }
            catch (System.Exception ex)
            {
                failed = true;
                Debug.LogException(ex);
                ToastMessage.I.ShowError(ex.Message);
            }
            if (!failed)
            {
                ToastMessage.I.ShowSuccess("RGNCoin successfully added to users data");
            }
            _currencyExample.SetUIInteractable(true);
        }
    }
}
