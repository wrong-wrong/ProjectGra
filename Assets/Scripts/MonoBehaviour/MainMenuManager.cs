using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] Button PlayButton;
        [SerializeField] Button SettingButton;
        [SerializeField] Button QuitButton;
        private void Awake()
        {
            PlayButton.onClick.AddListener(OnPlayerButtonClicked);
            SettingButton.onClick.AddListener(OnSettingButtonClicked);
            QuitButton.onClick.AddListener(() => { Application.Quit(); });
        }
        private void OnDestroy()
        {
            PlayButton.onClick.RemoveAllListeners();
            SettingButton.onClick.RemoveAllListeners();
            QuitButton.onClick.RemoveAllListeners();
        }
        private void OnSettingButtonClicked()
        {
            CanvasMonoSingleton.Instance.ShowSettingCanvasGroup();
        }
        private void OnPlayerButtonClicked()
        {
            CanvasMonoSingleton.Instance.HideMainMenuCanvasGroup();
            CanvasMonoSingleton.Instance.ShowPresetChoosingCanvasGroup();
        }
    }
}