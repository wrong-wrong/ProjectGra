using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class PauseUIManager : MonoBehaviour
    {
        [SerializeField] Button continueButton;
        [SerializeField] Button restartButton;
        [SerializeField] Button settingButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] RectTransform continueButtonRect;
        [SerializeField] RectTransform resultRect;
        [SerializeField] TextMeshProUGUI resultText;
        [SerializeField] string survivedString;
        [SerializeField] string failedString;
        public ButtonClickedEnum buttonClickedEnum;
        public static PauseUIManager Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            settingButton.onClick.AddListener(OnSettingButtonClicked);
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            restartButton.onClick.AddListener(OnRestartButtonClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
        public void ShowGameResult(bool isSurvived)
        {
            if (isSurvived)
            {
                resultText.text = survivedString;
            }
            else
            {
                resultText.text = failedString;
            }
        }
        public void SetContinueButtonRect(bool isShowing)
        {
            continueButtonRect.localScale = isShowing ? Vector3.one : Vector3.zero;
            resultRect.localScale = isShowing ? Vector3.zero : Vector3.one;
        }
        private void OnRestartButtonClicked()
        {
            buttonClickedEnum = ButtonClickedEnum.Restart;
            CanvasMonoSingleton.Instance.RestartGame();
            CanvasMonoSingleton.Instance.HidePauseCanvasGroup();
            CanvasMonoSingleton.Instance.HideInGameUI();
            CanvasMonoSingleton.Instance.ShowPresetChoosingCanvasGroup();
        }
        private void OnContinueButtonClicked()
        {
            buttonClickedEnum = ButtonClickedEnum.Continue;
        }
        private void OnMainMenuButtonClicked()
        {
            buttonClickedEnum = ButtonClickedEnum.MainMenu;
            CanvasMonoSingleton.Instance.HidePauseCanvasGroup();
            CanvasMonoSingleton.Instance.HideInGameUI();
            CanvasMonoSingleton.Instance.ShowMainMenuCanvasGroup();
        }
        private void OnSettingButtonClicked()
        {
            CanvasMonoSingleton.Instance.ShowSettingCanvasGroup();
        }
        private void OnDestroy()
        {
            settingButton.onClick.RemoveAllListeners();
        }
    }

    public enum ButtonClickedEnum
    {
        None,
        Restart,
        Continue,
        MainMenu,
    }
}