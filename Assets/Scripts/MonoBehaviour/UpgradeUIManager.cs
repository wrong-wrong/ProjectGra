using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class UpgradeUIManager : MonoBehaviour
    {
        public int LevelupThisWave;
        [SerializeField]int rerollPrice;
        [SerializeField] TextMeshProUGUI PriceText;
        [SerializeField] Button rerollButton;
        [SerializeField] List<UpgradeChoice> upgradeChoiceList;
        private string RerollString;
        private int rerollTimes;

        public void Awake()
        {
            Debug.LogWarning("Current reroll is adding player's material");
            rerollButton.onClick.AddListener(RerollClicked);
            for(int i = 0, n = upgradeChoiceList.Count; i < n; i++)
            {
                upgradeChoiceList[i].Init(this);
            }
            RerollString = "Reroll -";
        }
        public void OnDestroy() 
        { 
            rerollButton.onClick.RemoveAllListeners();
        }
        public void RerollClicked()
        {
            ++rerollTimes;
            PlayerDataModel.Instance.AddMaterialValWith(rerollPrice);
            UpdateRerollButton();
            Reroll();
        }
        public void ResetReroll()
        {
            rerollTimes = 0;
            UpdateRerollButton();
            Reroll();
        }
        private void UpdateRerollButton()
        {
            rerollPrice = CanvasMonoSingleton.Instance.GetRerollPrice(rerollTimes);
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            strBuilder.Append(rerollPrice);
            PriceText.text = RerollString + strBuilder.ToString();
            strBuilder.Clear();
        }
        public void Reroll()
        {
            for (int i = 0, n = upgradeChoiceList.Count; i < n; i++)
            {
                upgradeChoiceList[i].Reroll();
            }
        }
        public void Upgrade(int attributeIdx, int value)
        {
            PlayerDataModel.Instance.AddAttributeValWith(attributeIdx, value);
            CanvasMonoSingleton.Instance.UpgradeUINext();
        }
    }
}