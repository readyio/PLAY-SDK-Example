using RGN.Impl.Firebase;
using RGN.Modules.SignIn;
using RGN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    public sealed class SignInUpExample : IUIScreen
    {
        [SerializeField] private Button _tryToSignInButton;
        [SerializeField] private Button _signOutButton;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _loadingIndicator;

        [SerializeField] private TextMeshProUGUI _userInfoText;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _canvasGroup.interactable = false;
            _loadingIndicator.SetEnabled(true);
            _tryToSignInButton.onClick.AddListener(OnTryToSignInButtonClick);
            _signOutButton.onClick.AddListener(OnSignOutButtonClick);
            UpdateUserInfoText();
            RGNCore.I.AuthenticationChanged += OnAuthStateChanged;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _tryToSignInButton.onClick.RemoveListener(OnTryToSignInButtonClick);
            _signOutButton.onClick.RemoveListener(OnSignOutButtonClick);
            RGNCore.I.AuthenticationChanged -= OnAuthStateChanged;
        }

        private void OnTryToSignInButtonClick()
        {
            _canvasGroup.interactable = false;
            _loadingIndicator.SetEnabled(true);
            EmailSignInModule.I.TryToSignIn();
        }
        private void OnSignOutButtonClick()
        {
            _canvasGroup.interactable = false;
            _loadingIndicator.SetEnabled(true);
            EmailSignInModule.I.SignOut();
        }
        private void OnAuthStateChanged(AuthState authState)
        {
            switch (authState.LoginState)
            {
                case EnumLoginState.NotLoggedIn:
                    ToastMessage.I.Show("Not Logged In");
                    break;
                case EnumLoginState.Success:
                    string messageSuffix = string.Empty;
                    if (RGNCore.I.AuthorizedProviders == EnumAuthProvider.Guest)
                    {
                        messageSuffix = " As Guest";
                    }
                    else if (RGNCore.I.AuthorizedProviders == EnumAuthProvider.Email)
                    {
                        messageSuffix = " with " + RGNCore.I.MasterAppUser.Email;
                    }
                    ToastMessage.I.ShowSuccess("Successfully Logged In" + messageSuffix);
                    break;
                case EnumLoginState.Error:
                    ToastMessage.I.ShowError("Login Error: " + authState.LoginResult);
                    break;
            };
            bool isProcessing = authState.LoginState == EnumLoginState.Processing;
            _canvasGroup.interactable = !isProcessing;
            _loadingIndicator.SetEnabled(isProcessing);
            UpdateUserInfoText();
        }
        private void UpdateUserInfoText()
        {
            if (RGNCore.I.MasterAppUser == null)
            {
                _userInfoText.text = "User is not logged in";
                return;
            }
            var user = RGNCore.I.MasterAppUser;
            var sb = new System.Text.StringBuilder();
            sb.Append("Email: ").AppendLine(user.Email);
            sb.Append("Id: ").AppendLine(user.UserId);
            sb.Append("AuthorizedProviders: ").AppendLine(RGNCore.I.AuthorizedProviders.ToString());
            _userInfoText.text = sb.ToString();
        }
    }
}
