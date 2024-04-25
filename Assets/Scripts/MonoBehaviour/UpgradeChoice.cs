using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class UpgradeChoice : MonoBehaviour
    {
        [SerializeField] Image iconBg;
        [SerializeField] Image icon;
        [SerializeField] Image background;
        [SerializeField] Image chooseButtonBg;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI infoText;
        [SerializeField] Button chooseButton;
        int attributeIdx;
        int upgradeVal;
        int upgradeLevel;
        UpgradeScriptableObjectConfig config;
        UpgradeUIManager upgradeUIManager;
        public void Awake()
        {
            chooseButton.onClick.AddListener(Choose);
        }
        public void OnDestroy()
        {
            chooseButton.onClick.RemoveAllListeners();
        }
        public void Init(UpgradeUIManager mgr)
        {
            upgradeUIManager = mgr;
        }
        private void Choose()
        {
            upgradeUIManager.Upgrade(attributeIdx, upgradeVal);
        }
        public void Reroll()
        {
            attributeIdx = SOConfigSingleton.Instance.GetRandomAttributeIdx();
            upgradeLevel = SOConfigSingleton.Instance.GetRandomLevel();
            config = SOConfigSingleton.Instance.UpgradeSOList[attributeIdx];
            upgradeVal = config.BonusValueInFourLevel[upgradeLevel];
            InitVisual();
        }

        private void InitVisual()
        {
            //var level = SOConfigSingleton.Instance.GetRandomLevel();
            var colorLight = SOConfigSingleton.Instance.levelBgColorLight[upgradeLevel];
            var color = SOConfigSingleton.Instance.levelBgColor[upgradeLevel];
            nameText.text = config.UpgradeName;
            background.color = color;
            nameText.color = colorLight;
            iconBg.color = colorLight;
            chooseButtonBg.color = colorLight;
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            strBuilder.Append('+');
            strBuilder.Append(upgradeVal);
            strBuilder.Append(config.UpgradeName);
            infoText.text = strBuilder.ToString();
            strBuilder.Clear();
            icon.sprite = config.Icon;
        }
    }
}