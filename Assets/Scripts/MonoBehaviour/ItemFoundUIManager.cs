using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ItemFoundUIManager : MonoBehaviour
    {
        public int ItemFoundThisWave;
        //public static Dictionary<int, string> idxToAttributeName;
        [SerializeField] Image iconBg;
        [SerializeField] Image background;
        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] TextMeshProUGUI InfoText;
        [SerializeField] TextMeshProUGUI RecycleText;
        [SerializeField] Button TakeButton;
        [SerializeField] Button RecycleButton;
        [SerializeField] int itemIdx;
        int CurrentPrice;
        int itemLevel;
        //private ItemScriptableObjectConfig currrentItem;

        public void Awake()
        {
            TakeButton.onClick.AddListener(Take);
            RecycleButton.onClick.AddListener(Recycle);
            //idxToAttributeName = new Dictionary<int, string>();
            //idxToAttributeName[0] = "MaxHealthPoint";
            //idxToAttributeName[1] = "HealthRegain";
            //idxToAttributeName[2] = "Armor";
            //idxToAttributeName[3] = "Speed";
            //idxToAttributeName[4] = "Range";
            //idxToAttributeName[5] = "CritHitChance";
            //idxToAttributeName[6] = "Damage";
            //idxToAttributeName[7] = "MeleeDamage";
            //idxToAttributeName[8] = "RangeDamage";
            //idxToAttributeName[9] = "ElementDamage";
            //idxToAttributeName[10] = "AttackSpeed";

        }
        public void OnDestroy()
        {
            TakeButton.onClick.RemoveAllListeners();
            RecycleButton.onClick.RemoveAllListeners();
        }
        public void Reroll()
        {
            //currrentItem = SOConfigSingleton.Instance.GetRandomItemConfig();
            itemIdx = SOConfigSingleton.Instance.GetRandomItemConfigIdx();
            CurrentPrice = SOConfigSingleton.Instance.GetItemCurrentPrice(itemIdx);
            InitAfterSetConfig();
        }
        public void Take()
        {
            CanvasMonoSingleton.Instance.AddGameItem(itemIdx, itemLevel,CurrentPrice);
            CanvasMonoSingleton.Instance.ItemFoundUINext();
        }

        public void Recycle()
        {
            PlayerDataModel.Instance.AddMaterialValWith(CurrentPrice);
            CanvasMonoSingleton.Instance.ItemFoundUINext();

        }
        private void InitAfterSetConfig()
        {
            var currentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            itemLevel = currentItem.ItemLevel;
            itemIdx = currentItem.ItemIdx;
            //CurrentPrice = currentItem.ItemBasePrice;
            iconBg.color = SOConfigSingleton.Instance.levelBgColorLight[currentItem.ItemLevel];
            background.color = SOConfigSingleton.Instance.levelBgColor[currentItem.ItemLevel];
            icon.sprite = currentItem.ItemSprite;
            NameText.text = currentItem.ItemName;
            RecycleText.text = "Recycle (+" + CurrentPrice + ")";
            SetItemInfoText();
        }

        private void SetItemInfoText()
        {
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            var currentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            for(int i = 0,n = currentItem.AffectedAttributeIdx.Count; i < n; ++i)
            {
                if (currentItem.BonusedValueList[i] > 0)strBuilder.Append("+");
                strBuilder.Append(currentItem.BonusedValueList[i]);
                strBuilder.Append(CanvasMonoSingleton.IdxToAttributeName[currentItem.AffectedAttributeIdx[i]]);
                strBuilder.AppendLine();
            }
            InfoText.text = strBuilder.ToString();
            strBuilder.Clear();
        }
    }
}