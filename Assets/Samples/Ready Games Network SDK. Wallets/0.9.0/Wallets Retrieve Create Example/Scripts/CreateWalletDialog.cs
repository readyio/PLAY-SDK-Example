using System.Threading.Tasks;
using RGN.Modules.Wallets;
using RGN.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    internal sealed class CreateWalletDialog : MonoBehaviour, System.IDisposable
    {
        [SerializeField] private Button _createWalletButton;
        [SerializeField] private Button _closeDialogAreaButton;
        [SerializeField] private TMP_InputField _passwordInputField;
        [SerializeField] private TMP_InputField _confirmPasswordInputField;

        private WalletsExample _walletsExample;

        internal void Init(WalletsExample walletsExample)
        {
            _walletsExample = walletsExample;
            _createWalletButton.onClick.AddListener(OnCreateWalletButtonClickAsync);
            _closeDialogAreaButton.onClick.AddListener(OnCloseButtonClick);
            SetVisible(false);
        }
        public void Dispose()
        {
            _createWalletButton.onClick.RemoveListener(OnCreateWalletButtonClickAsync);
            _closeDialogAreaButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        internal void SetVisible(bool visible)
        {
            _passwordInputField.text = string.Empty;
            _confirmPasswordInputField.text = string.Empty;
            gameObject.SetActive(visible);
        }

        private async void OnCreateWalletButtonClickAsync()
        {
            if (string.IsNullOrWhiteSpace(_passwordInputField.text))
            {
                ToastMessage.I.ShowError("The password can not be empty");
                return;
            }
            if (string.IsNullOrWhiteSpace(_confirmPasswordInputField.text))
            {
                ToastMessage.I.ShowError("Please confirm the password");
                return;
            }
            if (_passwordInputField.text != _confirmPasswordInputField.text)
            {
                ToastMessage.I.ShowError("The passwords do not match");
                return;
            }
            _walletsExample.SetUIInteractable(false);
            try
            {
                //var result = await WalletsModule.I.CreateWalletAsync(_passwordInputField.text);
                //if (!result.wallet_created || !result.success)
                //{
                //    ToastMessage.I.ShowError(result.error);
                //    Debug.LogError(result.error);
                //    return;
                //}
                //SetVisible(false);
                //await _walletsExample.ReloadWalletItemsAsync();
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                ToastMessage.I.ShowError(ex.Message);
            }
            finally
            {
                _walletsExample.SetUIInteractable(true);
            }
        }
        private void OnCloseButtonClick()
        {
            SetVisible(false);
        }
    }
}
