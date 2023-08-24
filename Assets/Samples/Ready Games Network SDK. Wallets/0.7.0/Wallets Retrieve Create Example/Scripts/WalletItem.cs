using RGN.Modules.Wallets;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    internal sealed class WalletItem : MonoBehaviour, System.IDisposable
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _addressText;

        private bool _disposed = false;

        internal void Init(int index, RGNWallet wallet)
        {
            _rectTransform.localPosition = new Vector3(0, -index * GetHeight(), 0);
            _addressText.text = wallet.address;
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

        internal float GetHeight()
        {
            return _rectTransform.sizeDelta.y;
        }
    }
}
