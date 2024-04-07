using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class UpgradeUIManager : MonoBehaviour
    {
        public int LevelupThisWave;
        [SerializeField] Button rerollButton;
        [SerializeField] List<UpgradeChoice> upgradeChoiceList;

        public void Awake()
        {
            rerollButton.onClick.AddListener(Reroll);
            for(int i = 0, n = upgradeChoiceList.Count; i < n; i++)
            {
                upgradeChoiceList[i].Init(this);
            }
        }
        public void OnDestroy() 
        { 
            rerollButton.onClick.RemoveAllListeners();
        }
        public void Reroll()
        {
            for(int i = 0, n = upgradeChoiceList.Count; i < n; i++)
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