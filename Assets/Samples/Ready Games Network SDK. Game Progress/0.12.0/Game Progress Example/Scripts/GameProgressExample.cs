using System.Collections.Generic;
using System.Threading.Tasks;
using RGN.Impl.Firebase;
using RGN.Modules.GameProgress;
using RGN.UI;
using TMPro;
using UnityEngine;

namespace RGN.Samples
{
    [System.Serializable]
    internal sealed class CustomUserData
    {
        public int hitPoints;
        public float health;
        public string info;
        public int walkSpeed;
        public int regenerateHealth;
        public string[] equippedItemIds;
    }

    public class GameProgressExample : IUIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingIndicator _fullScreenLoadingIndicator;
        [SerializeField] private PullToRefresh _pullToRefresh;
        [SerializeField] private TextMeshProUGUI _userLevelText;
        [SerializeField] private TMP_InputField _integerHitPointsInputField;
        [SerializeField] private TMP_InputField _floatHealthInputField;
        [SerializeField] private TMP_InputField _stringInfoInputField;
        [SerializeField] private RGNButton _levelUpButton;
        [SerializeField] private RGNButton _storeInUserDataButton;
        [SerializeField] private TextMeshProUGUI _jsonRepresentationText;

        private bool _triedToLoad;

        public override void PreInit(IRGNFrame rgnFrame)
        {
            base.PreInit(rgnFrame);
            _pullToRefresh.RefreshRequested += LoadDataAsync;
            _levelUpButton.Button.onClick.AddListener(OnLevelUpButtonClickAsync);
            _storeInUserDataButton.Button.onClick.AddListener(OnStoreInUserDataButtonClickAsync);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _pullToRefresh.RefreshRequested -= LoadDataAsync;
            _levelUpButton.Button.onClick.RemoveListener(OnLevelUpButtonClickAsync);
            _storeInUserDataButton.Button.onClick.RemoveListener(OnStoreInUserDataButtonClickAsync);
        }
        protected override async void OnShow()
        {
            if (_triedToLoad)
            {
                return;
            }
            _triedToLoad = true;
            await LoadDataAsync();
        }

        public async Task<int> GetUserLevelAsync()
        {
            var result = await GameProgressModule.I.GetGameProgressAsync();
            return result.level;
        }

        private void SetUIInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
            _fullScreenLoadingIndicator.SetEnabled(!interactable);
        }
        private async void OnLevelUpButtonClickAsync()
        {
            SetUIInteractable(false);
            var result = await GameProgressModule.I.OnGameCompleteAsync(null);
            ToastMessage.I.ShowSuccess("User Level Up Successful: " + result.gameProgress.level.ToString());
            _userLevelText.text = result.gameProgress.level.ToString();
            SetUIInteractable(true);
        }
        private async void OnStoreInUserDataButtonClickAsync()
        {
            SetUIInteractable(false);
            string hitPointsStr = _integerHitPointsInputField.text;
            string healthStr = _floatHealthInputField.text;
            string infoStr = _stringInfoInputField.text;
            if (!int.TryParse(hitPointsStr, out int hitPoints))
            {
                hitPoints = Random.Range(1, 1000);
            }
            if (!float.TryParse(healthStr, out float health))
            {
                health = Random.Range(1f, 1000f);
            }
            int idsCount = Random.Range(3, 17);
            List<string> randomPseudoIds = new List<string>(idsCount);
            for (int i = 0; i < idsCount; i++)
            {
                randomPseudoIds.Add(System.Guid.NewGuid().ToString().ToLower().Substring(0, 8));
            }
            var customUserData = new CustomUserData() {
                hitPoints = hitPoints,
                health = health,
                info = infoStr,
                walkSpeed = Random.Range(20, 80),
                regenerateHealth = Random.Range(30, 200),
                equippedItemIds = randomPseudoIds.ToArray()
            };
            var result = await GameProgressModule.I.UpdateUserProgressAsync(customUserData, null);
            ToastMessage.I.ShowSuccess("Successfully updated the data");
            _jsonRepresentationText.text = JsonUtility.ToJson(result, true);

            SetUIInteractable(true);
        }

        private async Task LoadDataAsync()
        {
            SetUIInteractable(false);
            var userLevelResult = await GameProgressModule.I.GetGameProgressAsync();
            _userLevelText.text = userLevelResult.level.ToString();
            var userCustomDataResult = await GameProgressModule.I.GetUserProgressAsync<CustomUserData>();
            _stringInfoInputField.text = userCustomDataResult.playerProgress.info;
            _jsonRepresentationText.text = JsonUtility.ToJson(userCustomDataResult, true);
            SetUIInteractable(true);
        }
    }
}
