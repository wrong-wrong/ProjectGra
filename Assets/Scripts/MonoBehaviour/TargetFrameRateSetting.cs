using TMPro;
using UnityEngine;

namespace ProjectGra
{
    public class TargetFrameRateSetting : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown frameRateDropDown;
        private void Awake()
        {
            frameRateDropDown.onValueChanged.AddListener(SetttingTargetFrameRate);
            Application.targetFrameRate = 60;
            //SetttingTargetFrameRate(60);
        }
        private void OnDestroy()
        {
            frameRateDropDown.onValueChanged.RemoveAllListeners();
        }
        private void SetttingTargetFrameRate(int targetOption)
        {
            int targetFrameRate = int.Parse(frameRateDropDown.options[targetOption].text);
            //Debug.Log("SettingTargetFrameRate - " + targetFrameRate);
            Application.targetFrameRate = targetFrameRate;
        }
    }

}