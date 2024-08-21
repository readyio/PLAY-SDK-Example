using System.Collections.Generic;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.VirtualItems;
using RGN.UI;
using UnityEngine;

namespace RGN.Samples
{
    public interface IVirtualItemsExampleClient
    {
        Task<bool> DoesTheUserHasPrimaryWalletAddressAsync();
        void OpenWalletsScreen();
        int GetCurrentUserRGNCoinBalance();
        void OpenCurrenciesScreen();
        Task UpdateUserProfileAsync();
    }

    public sealed class VirtualItemsExample : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private RectTransform _scrollContentRectTrasform;
        [SerializeField] private PullToRefresh _pullToRefresh;
        [SerializeField] private RGNButton _loadMoreItemsButton;

        [SerializeField] private VirtualItemUI _virtualItemPrefab;

        private IVirtualItemsExampleClient _virtualItemsExampleClient;
        private List<VirtualItemUI> _virtualItems;
        private bool _triedToLoad;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _virtualItems = new List<VirtualItemUI>();
            _pullToRefresh.RefreshRequested += ReloadVirtualItemsAsync;
            _loadMoreItemsButton.Button.onClick.AddListener(OnLoadMoreItemsButtonAsync);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _pullToRefresh.RefreshRequested -= ReloadVirtualItemsAsync;
            _loadMoreItemsButton.Button.onClick.RemoveListener(OnLoadMoreItemsButtonAsync);
        }
        public void SetVirtualItemsExampleClient(IVirtualItemsExampleClient virtualItemsExampleClient)
        {
            _virtualItemsExampleClient = virtualItemsExampleClient;
        }

        protected override async void OnShow()
        {
            if (_triedToLoad)
            {
                return;
            }
            await ReloadVirtualItemsAsync();
        }

        private void SetUIInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _fullScreenLoadingIndicator.SetEnabled(!interactable);
        }

        private Task ReloadVirtualItemsAsync()
        {
            DisposeVirtualItems();
            return LoadItemsAsync(string.Empty);
        }
        private void DisposeVirtualItems()
        {
            if (_virtualItems == null)
            {
                return;
            }
            for (int i = 0; i < _virtualItems.Count; ++i)
            {
                _virtualItems[i].Dispose();
            }
            _virtualItems.Clear();
        }
        private async void OnLoadMoreItemsButtonAsync()
        {
            string lastLoadedVirtualItemId = string.Empty;
            if (_virtualItems.Count > 0)
            {
                lastLoadedVirtualItemId = _virtualItems[_virtualItems.Count - 1].Id;
            }
            await LoadItemsAsync(lastLoadedVirtualItemId);
        }
        private async Task LoadItemsAsync(string startAfter)
        {
            SetUIInteractable(false);
            _triedToLoad = true;
            var virtualItems = await VirtualItemsModule.I.GetVirtualItemsAsync(20, startAfter);
            for (int i = 0; i < virtualItems.Count; ++i)
            {
                VirtualItemUI ui = Instantiate(_virtualItemPrefab, _scrollContentRectTrasform);
                ui.Init(_rgnFrame, _virtualItems.Count, virtualItems[i], _virtualItemsExampleClient);
                _virtualItems.Add(ui);
            }
            float loadMoreItemsButtonPos = _virtualItems.Count * _virtualItemPrefab.GetHeight();
            _loadMoreItemsButton.RectTransform.anchoredPosition = new Vector2(0, -loadMoreItemsButtonPos);
            float loadedItemsHeight = _loadMoreItemsButton.GetHeight();
            Vector2 sizeDelta = _scrollContentRectTrasform.sizeDelta;
            _scrollContentRectTrasform.sizeDelta = new Vector2(sizeDelta.x, loadMoreItemsButtonPos + loadedItemsHeight);
            SetUIInteractable(true);
        }
    }
}
