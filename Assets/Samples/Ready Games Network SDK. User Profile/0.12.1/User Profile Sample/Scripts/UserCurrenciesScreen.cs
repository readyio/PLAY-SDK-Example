using System.Collections.Generic;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.Currency;
using RGN.Modules.UserProfile;
using RGN.Modules.VirtualItems;
using RGN.UI;
using UnityEngine;

namespace RGN.Samples
{
    public sealed class UserCurrenciesScreen : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private PullToRefresh _pullToRefresh;
        [SerializeField] private RectTransform _content;
        [Header("Prefabs")]
        [SerializeField] private UserCoinItem _userCoinItemPrefab;


        private List<UserCoinItem> _userCoinItems;
        private bool _triedToLoad;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _userCoinItems = new List<UserCoinItem>();
            _pullToRefresh.RefreshRequested += OnPullToRefreshButtonClickAsync;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _pullToRefresh.RefreshRequested -= OnPullToRefreshButtonClickAsync;
        }
        public override void OnWillAppearNow(object parameters)
        {
            if (_triedToLoad)
            {
                return;
            }
            var userCoins = parameters as List<Currency>;
            ReloadUserCoinItems(userCoins);
        }

        private Task OnPullToRefreshButtonClickAsync()
        {
            return ReloadUserCoinItems(null);
        }
        private void SetUIInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _fullScreenLoadingIndicator.SetEnabled(!interactable);
        }
        private Task ReloadUserCoinItems(List<Currency> userCoins)
        {
            DisposeUserCoinItems();
            return LoadItemsAsync(userCoins);
        }
        private void DisposeUserCoinItems()
        {
            if (_userCoinItems == null)
            {
                return;
            }
            for (int i = 0; i < _userCoinItems.Count; ++i)
            {
                _userCoinItems[i].Dispose();
            }
            _userCoinItems.Clear();
        }
        private async Task LoadItemsAsync(List<Currency> userCoins)
        {
            SetUIInteractable(false);
            _triedToLoad = true;
            var virtualItems = userCoins ?? await UserProfileModule.I.GetUserCurrenciesAsync();
            for (int i = 0; i < virtualItems.Count; ++i)
            {
                UserCoinItem ui = Instantiate(_userCoinItemPrefab, _content);
                ui.Init(_userCoinItems.Count, virtualItems[i]);
                _userCoinItems.Add(ui);
            }
            float loadedItemsHeight = _userCoinItems.Count * _userCoinItemPrefab.GetHeight();
            Vector2 sizeDelta = _content.sizeDelta;
            _content.sizeDelta = new Vector2(sizeDelta.x, loadedItemsHeight);
            SetUIInteractable(true);
        }
    }
}
