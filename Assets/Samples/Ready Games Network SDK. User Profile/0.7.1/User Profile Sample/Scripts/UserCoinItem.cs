using RGN.Modules.Currency;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    internal sealed class UserCoinItem : MonoBehaviour, System.IDisposable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _quantity;

        private bool _disposed = false;

        internal void Init(int index, Currency currency)
        {
            _name.text = currency.name;
            _quantity.text = currency.quantity.ToString();
            _rectTransform.localPosition = new Vector3(0, -index * GetHeight(), 0);
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
            _disposed = true;
        }
        internal void SetIsLoading()
        {
            _name.text = "Loading...";
            _quantity.text = string.Empty;
        }
        internal float GetHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}
