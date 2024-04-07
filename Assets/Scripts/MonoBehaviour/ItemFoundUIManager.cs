using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ItemFoundUIManager : MonoBehaviour
    {
        public int ItemFoundThisWave;
        public static Dictionary<int, string> idxToAttributeName;
        [SerializeField] Image iconBg;
        [SerializeField] Image background;
        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] TextMeshProUGUI InfoText;
        [SerializeField] TextMeshProUGUI RecycleText;
        [SerializeField] Button TakeButton;
        [SerializeField] Button RecycleButton;
        [SerializeField] int ItemIdx;
        [SerializeField] int CurrentPrice;
        int ItemLevel;
        private ItemScriptableObjectConfig currrentItem;

        public void Awake()
        {
            TakeButton.onClick.AddListener(Take);
            RecycleButton.onClick.AddListener(Recycle);
            idxToAttributeName = new Dictionary<int, string>();
            idxToAttributeName[0] = "MaxHealthPoint";
            idxToAttributeName[1] = "HealthRegain";
            idxToAttributeName[2] = "Armor";
            idxToAttributeName[3] = "Speed";
            idxToAttributeName[4] = "Range";
            idxToAttributeName[5] = "CritHitChance";
            idxToAttributeName[6] = "Damage";
            idxToAttributeName[7] = "MeleeDamage";
            idxToAttributeName[8] = "RangeDamage";
            idxToAttributeName[9] = "ElementDamage";
            idxToAttributeName[10] = "AttackSpeed";

        }
        public void OnDestroy()
        {
            TakeButton.onClick.RemoveAllListeners();
            RecycleButton.onClick.RemoveAllListeners();
        }
        public void Reroll()
        {
            currrentItem = SOConfigSingleton.Instance.GetRandomItemConfig();
            InitAfterSetConfig();
        }
        public void Take()
        {
            for(int i = 0, n = currrentItem.AffectedAttributeIdx.Count;  i < n; i++)
            {
                PlayerDataModel.Instance.SetAttributeValWith(currrentItem.AffectedAttributeIdx[i], currrentItem.BonusedValueList[i]);
            }
            //CanvasMonoSingleton.Instance.AddingItem(ItemIdx);
            CanvasMonoSingleton.Instance.ItemFoundUINext();
        }

        public void Recycle()
        {
            PlayerDataModel.Instance.AddMaterialValWith(CurrentPrice);
            CanvasMonoSingleton.Instance.ItemFoundUINext();

        }
        private void InitAfterSetConfig()
        {
            ItemLevel = currrentItem.ItemLevel;
            ItemIdx = currrentItem.ItemIdx;
            CurrentPrice = currrentItem.ItemBasePrice;
            iconBg.color = SOConfigSingleton.Instance.levelBgColorLight[currrentItem.ItemLevel];
            background.color = SOConfigSingleton.Instance.levelBgColor[currrentItem.ItemLevel];
            icon.sprite = currrentItem.ItemSprite;
            NameText.text = currrentItem.ItemName;
            RecycleText.text = "Recycle (+" + CurrentPrice + ")";
            SetItemInfoText();
        }

        private void SetItemInfoText()
        {
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for(int i = 0,n = currrentItem.AffectedAttributeIdx.Count; i < n; ++i)
            {
                if (currrentItem.BonusedValueList[i] > 0)strBuilder.Append("+");
                strBuilder.Append(currrentItem.BonusedValueList[i]);
                strBuilder.Append(idxToAttributeName[currrentItem.AffectedAttributeIdx[i]]);
                strBuilder.AppendLine();
            }
            InfoText.text = strBuilder.ToString();
            strBuilder.Clear();
        }
    }
}