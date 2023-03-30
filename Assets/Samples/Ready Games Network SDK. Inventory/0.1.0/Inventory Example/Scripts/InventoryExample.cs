using System.Collections.Generic;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.Inventory;
using RGN.UI;
using UnityEngine;

namespace RGN.Samples
{
    public class InventoryExample : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private RectTransform _scrollContentRectTrasform;
        [SerializeField] private PullToRefresh _pullToRefresh;
        [SerializeField] private RGNButton _loadMoreItemsButton;

        [SerializeField] private InventoryItemUI _inventoryItemPrefab;

        private List<InventoryItemUI> _inventoryItems;
        private bool _triedToLoad;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _inventoryItems = new List<InventoryItemUI>();
            _pullToRefresh.RefreshRequested += ReloadVirtualItemsAsync;
            _loadMoreItemsButton.Button.onClick.AddListener(OnLoadMoreItemsButtonAsync);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _pullToRefresh.RefreshRequested -= ReloadVirtualItemsAsync;
            _loadMoreItemsButton.Button.onClick.RemoveListener(OnLoadMoreItemsButtonAsync);
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
            if (_inventoryItems == null)
            {
                return;
            }
            for (int i = 0; i < _inventoryItems.Count; ++i)
            {
                _inventoryItems[i].Dispose();
            }
            _inventoryItems.Clear();
        }
        private async void OnLoadMoreItemsButtonAsync()
        {
            string lastLoadedInventoryItemId = string.Empty;
            if (_inventoryItems.Count > 0)
            {
                lastLoadedInventoryItemId = _inventoryItems[_inventoryItems.Count - 1].Id;
            }
            await LoadItemsAsync(lastLoadedInventoryItemId);
        }
        private async Task LoadItemsAsync(string startAfter)
        {
            SetUIInteractable(false);
            _triedToLoad = true;
            var inventoryItems = await InventoryModule.I.GetWithVirtualItemsDataForCurrentAppAsync(startAfter);
            for (int i = 0; i < inventoryItems.Count; ++i)
            {
                InventoryItemUI ui = Instantiate(_inventoryItemPrefab, _scrollContentRectTrasform);
                ui.Init(_rgnFrame, _inventoryItems.Count, inventoryItems[i]);
                _inventoryItems.Add(ui);
            }
            float loadMoreItemsButtonPos = _inventoryItems.Count * _inventoryItemPrefab.GetHeight();
            _loadMoreItemsButton.RectTransform.anchoredPosition = new Vector2(0, -loadMoreItemsButtonPos);
            float loadMoreItemsButtonHeight = _loadMoreItemsButton.GetHeight();
            Vector2 sizeDelta = _scrollContentRectTrasform.sizeDelta;
            _scrollContentRectTrasform.sizeDelta = new Vector2(sizeDelta.x, loadMoreItemsButtonPos + loadMoreItemsButtonHeight);
            SetUIInteractable(true);
        }
    }
}
