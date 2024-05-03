using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class ItemFoundUIManager : MonoBehaviour
    {
        public int ItemFoundThisWave;
        //public static Dictionary<int, string> idxToAttributeName;
        [SerializeField] float RectWidth;
        [SerializeField] float RectBasicHeight;
        [SerializeField] float heightModifyValuePerRow;
        [SerializeField] RectTransform backgroundRect;
        [SerializeField] Image iconBg;
        [SerializeField] Image background;
        [SerializeField] Image icon;
        [SerializeField] TextMeshProUGUI NameText;
        [SerializeField] TextMeshProUGUI InfoText;
        [SerializeField] TextMeshProUGUI RecycleText;
        [SerializeField] Button TakeButton;
        [SerializeField] Button RecycleButton;
        [SerializeField] int itemIdx;
        int basePrice;
        int currentPrice;
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
        public void RerollLegendary()
        {
            itemIdx = SOConfigSingleton.Instance.GetRandomItemConfigIdxFromRarities(3);
            InitAfterSetConfig();
        }
        public void Reroll()
        {
            //currrentItem = SOConfigSingleton.Instance.GetRandomItemConfig();
            var itemRarities = SOConfigSingleton.Instance.GetRandomLevel();
            itemIdx = SOConfigSingleton.Instance.GetRandomItemConfigIdxFromRarities(itemRarities);
            InitAfterSetConfig();
        }
        public void Take()
        {
            CanvasMonoSingleton.Instance.AddGameItem(itemIdx, itemLevel, basePrice, 0);
            CanvasMonoSingleton.Instance.ItemFoundUINext();
        }

        public void Recycle()
        {
            PlayerDataModel.Instance.AddMaterialValWith(currentPrice);
            CanvasMonoSingleton.Instance.ItemFoundUINext();

        }
        private void InitAfterSetConfig()
        {
            var currentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            basePrice = currentItem.ItemBasePrice;
            currentPrice = CanvasMonoSingleton.Instance.CalculateFinalPrice(currentItem.ItemBasePrice);
            itemLevel = currentItem.ItemLevel;
            itemIdx = currentItem.ItemIdx;
            //CurrentPrice = currentItem.ItemBasePrice;
            iconBg.color = SOConfigSingleton.Instance.levelBgColorLight[currentItem.ItemLevel];
            background.color = SOConfigSingleton.Instance.levelBgColor[currentItem.ItemLevel];
            icon.sprite = currentItem.ItemSprite;
            NameText.text = currentItem.ItemName;
            RecycleText.text = "Recycle (+" + currentPrice + ")";
            //SetItemInfoText();
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            int cnt = 0;
            for (int i = 0, n = currentItem.AffectedAttributeIdx.Count; i < n; ++i)
            {
                ++cnt;
                if (currentItem.BonusedValueList[i] > 0) strBuilder.Append("+");
                strBuilder.Append(currentItem.BonusedValueList[i]);
                strBuilder.Append(CanvasMonoSingleton.Instance.IdxToAttributeName[currentItem.AffectedAttributeIdx[i]]);
                strBuilder.AppendLine();
            }
            InfoText.text = strBuilder.ToString();
            strBuilder.Clear();
            SetRectHeight(cnt * heightModifyValuePerRow);

        }
        private void SetRectHeight(float height)
        {
            backgroundRect.sizeDelta = new Vector2(RectWidth, RectBasicHeight + height);
        }
        private void SetItemInfoText()
        {
            int cnt = 0;
            var currentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for (int i = 0, n = currentItem.AffectedAttributeIdx.Count; i < n; ++i)
            {
                ++cnt;
                if (currentItem.BonusedValueList[i] > 0) strBuilder.Append("+");
                strBuilder.Append(currentItem.BonusedValueList[i]);
                strBuilder.Append(CanvasMonoSingleton.Instance.IdxToAttributeName[currentItem.AffectedAttributeIdx[i]]);
                strBuilder.AppendLine();
            }
            InfoText.text = strBuilder.ToString();
            strBuilder.Clear();
            SetRectHeight(cnt * heightModifyValuePerRow);
        }
    }
}