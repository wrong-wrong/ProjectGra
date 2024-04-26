using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class SettingManager : MonoBehaviour
    {
        [SerializeField] Button backButton;
        [SerializeField] Slider audioVolumeSlider;
        [SerializeField] Slider sensitivitySlider;
        float lastUpdateTime;
        public void Awake()
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
            audioVolumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);
            
        }
        public float GetMouseSensitivityModifier()
        {
            return sensitivitySlider.value * 0.5f + 0.5f;
        }
        public void OnVolumeSliderChanged(float value)
        {
            var currentTime = Time.realtimeSinceStartup;
            if(currentTime > lastUpdateTime+0.5f) 
            {
                AudioManager.Instance.SetAudioMixerVolume(value);
                lastUpdateTime = currentTime;
            }
        }
        public void OnDestroy()
        {
            backButton.onClick.RemoveAllListeners();
            audioVolumeSlider.onValueChanged.RemoveAllListeners();
        }
        public void OnBackButtonClicked()
        {
            Debug.Log(audioVolumeSlider.value);
            Debug.Log(sensitivitySlider.value);
            CanvasMonoSingleton.Instance.HideSettingCanvasGroup();
        }
    }
}