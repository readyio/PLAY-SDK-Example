using RGN.Modules.Currency;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    internal sealed class CoinInfoItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _quantity;
        [SerializeField] private string _coinName;

        internal void Init(Currency currency)
        {
            if (currency.name == _coinName)
            {
                _quantity.text = currency.quantity.ToString();
            }
        }
        internal void SetIsLoading()
        {
            _quantity.text = "Loading...";
        }
    }
}
