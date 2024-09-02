using RGN.Impl.Firebase;
using UnityEngine;
using UnityEngine.UI;

namespace RGN.Samples
{
    public sealed class SettingsScreen : IUIScreen
    {
        [SerializeField] private Button _openSignInScreenButton;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _openSignInScreenButton.onClick.AddListener(OnOpenSignInScreenButtonClick);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _openSignInScreenButton.onClick.RemoveListener(OnOpenSignInScreenButtonClick);
        }

        private void OnOpenSignInScreenButtonClick()
        {
            _rgnFrame.OpenScreen<SignInUpExample>();
        }
    }
}
