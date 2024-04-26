using System;
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

        public ButtonClickedEnum buttonClickedEnum;
        public static PauseUIManager Instance;

        private void Awake()
        {
            if(Instance!=null)
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
        public void SetContinueButtonRect(bool isShowing)
        {
            continueButtonRect.localScale = isShowing ? Vector3.one : Vector3.zero;
        }
        private void OnRestartButtonClicked()
        {
            buttonClickedEnum = ButtonClickedEnum.Restart;
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