using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.Marketplace;
using RGN.Modules.Messaging;
using RGN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    public class UIRoot : IUIScreen, IUserProfileClient, IVirtualItemsExampleClient, IInventoryExampleClient
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _exploreUserProfileButton;
        [SerializeField] private Button _exploreVirtualItemsButton;
        [SerializeField] private Button _exploreCurrenciesButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;


        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _titleText.text = Application.productName;
            _loginButton.onClick.AddListener(OnLoginButtonClick);
            _exploreUserProfileButton.onClick.AddListener(OnExploreUserProfileButtonClick);
            _exploreVirtualItemsButton.onClick.AddListener(OnExploreVirtualItemsButtonClick);
            _exploreCurrenciesButton.onClick.AddListener(OnExploreCurrenciesButtonClick);
            _settingsButton.onClick.AddListener(OnSettingsButtonClick);
            _canvasGroup.interactable = false;
            _fullScreenLoadingIndicator.SetEnabled(true);
            SetUserLoggedIn(false);
            RGNCore.I.AuthenticationChanged += OnAuthenticationChanged;
        }
        public override Task InitAsync()
        {
            base.InitAsync();
            _rgnFrame.GetScreen<UserProfileExample>().SetUserProfileClient(this);
            _rgnFrame.GetScreen<VirtualItemsExample>().SetVirtualItemsExampleClient(this);
            _rgnFrame.GetScreen<InventoryExample>().SetInventoryExampleClient(this);
            return Task.CompletedTask;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _loginButton.onClick.RemoveListener(OnLoginButtonClick);
            _exploreUserProfileButton.onClick.RemoveListener(OnExploreUserProfileButtonClick);
            _exploreVirtualItemsButton.onClick.RemoveListener(OnExploreVirtualItemsButtonClick);
            _exploreCurrenciesButton.onClick.RemoveListener(OnExploreCurrenciesButtonClick);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            RGNCore.I.AuthenticationChanged -= OnAuthenticationChanged;
        }

        private void OnAuthenticationChanged(AuthState authState)
        {
            SetUserLoggedIn(authState.LoginState == EnumLoginState.Success &&
                RGNCore.I.AuthorizedProviders == EnumAuthProvider.Email);
            bool isProcessing = authState.LoginState == EnumLoginState.Processing;
            _canvasGroup.interactable = !isProcessing;
            _fullScreenLoadingIndicator.SetEnabled(isProcessing);
        }
        private void OnLoginButtonClick()
        {
            _rgnFrame.OpenScreen<SignInUpExample>();
        }
        private void OnSettingsButtonClick()
        {
            _rgnFrame.OpenScreen<SettingsScreen>();
        }
        private void OnExploreUserProfileButtonClick()
        {
            _rgnFrame.OpenScreen<UserProfileExample>();
        }
        private void OnExploreVirtualItemsButtonClick()
        {
            _rgnFrame.OpenScreen<VirtualItemsExample>();
        }
        private void OnExploreCurrenciesButtonClick()
        {
            _rgnFrame.OpenScreen<CurrencyExample>();
        }

        Task<string> IUserProfileClient.GetPrimaryWalletAddressAsync()
        {
            return _rgnFrame.GetScreen<WalletsExample>().GetPrimaryWalletAddressAsync();
        }
        void IUserProfileClient.OpenWalletsScreen()
        {
            _rgnFrame.OpenScreen<WalletsExample>();
        }
        void IUserProfileClient.OpenInventoryScreen()
        {
            _rgnFrame.OpenScreen<InventoryExample>();
        }
        void IUserProfileClient.OpenGameProgressScreen()
        {
            _rgnFrame.OpenScreen<GameProgressExample>();
        }
        Task<int> IUserProfileClient.GetUserLevelAsync()
        {
            return _rgnFrame.GetScreen<GameProgressExample>().GetUserLevelAsync();
        }
        Task<bool> IVirtualItemsExampleClient.DoesTheUserHasPrimaryWalletAddressAsync()
        {
            return _rgnFrame.GetScreen<WalletsExample>().DoesTheUserHasPrimaryWalletAddressAsync();
        }
        void IVirtualItemsExampleClient.OpenWalletsScreen()
        {
            _rgnFrame.OpenScreen<WalletsExample>();
        }
        int IVirtualItemsExampleClient.GetCurrentUserRGNCoinBalance()
        {
            return _rgnFrame.GetScreen<UserProfileExample>().GetRGNCoinBalance();
        }
        void IVirtualItemsExampleClient.OpenCurrenciesScreen()
        {
            _rgnFrame.OpenScreen<CurrencyExample>();
        }
        Task IVirtualItemsExampleClient.UpdateUserProfileAsync()
        {
            return _rgnFrame.GetScreen<UserProfileExample>().ReloadUserProfileAsync();
        }
        void IInventoryExampleClient.OpenMarketpace()
        {
            MarketplaceModule.I.OpenMarketplace();
        }

        private void SetUserLoggedIn(bool loggedInWithEmail)
        {
            _loginButton.gameObject.SetActive(!loggedInWithEmail);
            _exploreUserProfileButton.gameObject.SetActive(loggedInWithEmail);
            _exploreVirtualItemsButton.gameObject.SetActive(loggedInWithEmail);
            _exploreCurrenciesButton.gameObject.SetActive(loggedInWithEmail);
        }
    }
}
