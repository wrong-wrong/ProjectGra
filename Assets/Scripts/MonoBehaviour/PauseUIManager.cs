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

        private void Awake()
        {
            settingButton.onClick.AddListener(OnSettingButtonClicked);
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
}