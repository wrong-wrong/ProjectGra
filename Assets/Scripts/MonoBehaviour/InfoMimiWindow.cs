using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class InfoMimiWindow : MonoBehaviour
    {
        [SerializeField] Image Icon;
        [SerializeField] Image background;
        [SerializeField] List<Image> lightBackgroundList;
        public Button CombineButton;
        public Button RecycleButton;
        public Button CancelButton;
        public TextMeshProUGUI RecycleText;
        public TextMeshProUGUI NameText;
        public RectTransform ItemTextRect;
        public RectTransform WeaponFixedTextRect;
        public RectTransform WeaponInfoTextRect;
        public TextMeshProUGUI WeaponInfoText;
        public TextMeshProUGUI ItemInfoText;
        public RectTransform CombineButtonRect;
        public RectTransform InfoMiniWindowRect;

        [Header("Weapon Category Info Com")]
        public List<RectTransform> CategoryRectList;
        public List<TextMeshProUGUI> CateNameTextList;
        public List<TextMeshProUGUI> CateValueList;

        private int currentPrice;
        private bool lastCalledByWeaponSlot;

        public void Awake()
        {
            CancelButton.onClick.AddListener(() => { gameObject.SetActive(false); });
            CombineButton.onClick.AddListener(OnCombineButtonClicked);
            RecycleButton.onClick.AddListener(OnRecycleButtonClicked);
        }
        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;

                if (!RectTransformUtility.RectangleContainsScreenPoint(InfoMiniWindowRect, mousePosition))
                {

                    gameObject.SetActive(false);
                }
            }
        }

        private void OnRecycleButtonClicked()
        {
            if (lastCalledByWeaponSlot)
            {
                CanvasMonoSingleton.Instance.RecycleWeaponFromSlot(calledSlotIdx);
            }
            else
            {
                CanvasMonoSingleton.Instance.RecycleGameItemWithGO(itemIdx, itemPrice, calledItemSlot);
            }
            gameObject.SetActive(false);
            //WindowRect.localScale = Vector3.zero;
            //why I decided not to use localScale to 'hide' the window , because the update function would be called every frame
        }
        private void OnCombineButtonClicked()
        {
            CanvasMonoSingleton.Instance.CombineWeaponFromTo(combineSlotIdx, calledSlotIdx);
            gameObject.SetActive(false);
            //WindowRect.localScale = Vector3.zero;
        }
        public void OnDestroy()
        {
            CancelButton.onClick.RemoveAllListeners();
            CombineButton.onClick.RemoveAllListeners();
            RecycleButton.onClick.RemoveAllListeners();
        }
        private int combineSlotIdx;
        private int calledSlotIdx;
        public void InitInfoMimiWindowAndShowAtPositionWithWeapon(int weaponIdx, int weaponLevel, bool isCanCombine, int combineSlotIdx, int calledSlotIdx, Vector3 showPos)
        {
            WeaponFixedTextRect.localScale = Vector3.one;
            WeaponInfoTextRect.localScale = Vector3.one;
            ItemTextRect.localScale = Vector3.zero;
            lastCalledByWeaponSlot = true;
            this.calledSlotIdx = calledSlotIdx;
            this.combineSlotIdx = combineSlotIdx;

            showPos.y += 50;
            InfoMiniWindowRect.position = showPos;
            if (isCanCombine)
            {
                CombineButtonRect.localScale = Vector3.one;
                InfoMiniWindowRect.rect.Set(0, 0, 350, 450);
            }
            else
            {
                CombineButtonRect.localScale = Vector3.zero;
                InfoMiniWindowRect.rect.Set(0, 0, 350, 400);
            }
            var config = SOConfigSingleton.Instance.WeaponMapCom.wpNativeHashMap[weaponIdx];
            var colorLight = SOConfigSingleton.Instance.levelBgColorLight[weaponLevel];
            background.color = SOConfigSingleton.Instance.levelBgColor[weaponLevel];
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for (int i = 0, n = lightBackgroundList.Count; i < n; i++)
            {
                lightBackgroundList[i].color = colorLight;
            }

            var monoOnlyConfig = SOConfigSingleton.Instance.WeaponManagedConfigCom;
            currentPrice = monoOnlyConfig.weaponBasePriceMap[weaponIdx];
            Icon.sprite = null;
            NameText.text = monoOnlyConfig.weaponNameMap[weaponIdx];
            strBuilder.Append("Recycle (");
            strBuilder.Append(currentPrice);
            strBuilder.Append(")");
            RecycleText.text = strBuilder.ToString();
            strBuilder.Clear();

            var calculatedDamageAfterBonus = (int)((1 + PlayerDataModel.Instance.GetDamage())
            * (config.BasicDamage + math.csum(config.DamageBonus * PlayerDataModel.Instance.GetDamageBonus())));
            var calculatedCritHitChance = PlayerDataModel.Instance.GetCritHitChance() + config.WeaponCriticalHitChance;
            var calculatedCooldown = config.Cooldown * math.clamp(1 - PlayerDataModel.Instance.GetAttackSpeed(), 0.2f, 2f);
            var calculatedRange = PlayerDataModel.Instance.GetRange() + config.Range;
            strBuilder.Append(calculatedDamageAfterBonus);
            strBuilder.Append('|');
            strBuilder.Append(config.BasicDamage);
            strBuilder.AppendLine();
            strBuilder.Append('x');
            strBuilder.Append(config.WeaponCriticalHitRatio);
            strBuilder.Append('(');
            strBuilder.Append(calculatedCritHitChance);
            strBuilder.Append("chance)");
            strBuilder.AppendLine();
            strBuilder.Append(calculatedCooldown);
            strBuilder.AppendLine();
            strBuilder.Append(calculatedRange);
            strBuilder.Append('|');
            strBuilder.Append(config.Range);
            strBuilder.AppendLine();
            WeaponInfoText.text = strBuilder.ToString();
            strBuilder.Clear();

            var categoryIdxList = monoOnlyConfig.weaponCategoryIdxListMap[weaponIdx];
            //Check Category count and set Rect of category info window
            for(int i = 0; i < CategoryRectList.Count; i++) // using count of RectList to iterating all cateInfoWindow, not cateIdxList.Count
            {
                if (i >= categoryIdxList.Count)
                {
                    CategoryRectList[i].localScale = Vector3.zero;
                    continue;
                }
                CategoryRectList[i].localScale = Vector3.one;

                var cateConfig = SOConfigSingleton.Instance.WeaponCategorySOList[categoryIdxList[i]];
                //set name text
                CateNameTextList[i].text = cateConfig.CategoryName;

                //set info text, need to get bonus value, and bonused attribute name (can get from canvasmonomanager)
                //use <color="white"> </color> to current activated category count. Use <color=#00D400> </color> to color the value Green // green 00D400
                var activatedCount = CanvasMonoSingleton.Instance.GetCategoryActivatedCount(categoryIdxList[i]);
                for (int j = 0; j < 4; ++j)
                {
                    if (j + 1 == activatedCount) strBuilder.Append("<color=\"white\">");
                    //need to get current count;
                    strBuilder.Append('(');
                    strBuilder.Append(j + 1);
                    strBuilder.Append(')');
                    for(int k = 0; k < cateConfig.AffectedAttributeIdxList.Count; ++k)
                    {
                        if(k >0) { strBuilder.Append(',');}
                        var value = cateConfig.BonusValueInDifferentCountList[k][j];
                        //Set value
                        if (j + 1 == activatedCount)
                        {
                            if (value > 0) strBuilder.Append("<color=#00D400>");
                            else strBuilder.Append("<color=#D40000>");
                        }
                        if (value > 0) strBuilder.Append('+');
                        strBuilder.Append(value);
                        if (j + 1 == activatedCount) strBuilder.Append("</color>");
                        //Set attribute name
                        strBuilder.Append(' ');
                        strBuilder.Append(CanvasMonoSingleton.Instance.IdxToAttributeName[cateConfig.AffectedAttributeIdxList[k]]);
                    }
                    strBuilder.AppendLine();
                    if (j + 1 == activatedCount) strBuilder.Append("</color>");
                }
                CateValueList[i].text = strBuilder.ToString();
                strBuilder.Clear();

            }

            //Set name text
        }
        private GameObject calledItemSlot;
        private int itemIdx;
        private int itemPrice;
        public void InitInfoMimiWindowAndShowAtPositionWithItem(int itemIdx, int itemLevel, int currentPrice, GameObject itemSlot, Vector3 showPos)
        {
            //Setting info window
            this.itemIdx = itemIdx;
            itemPrice = currentPrice;
            calledItemSlot = itemSlot;
            ItemTextRect.localScale = Vector3.one;
            WeaponFixedTextRect.localScale = Vector3.zero;
            WeaponInfoTextRect.localScale = Vector3.zero;
            lastCalledByWeaponSlot = false;
            showPos.y += 50;
            InfoMiniWindowRect.position = showPos;
            CombineButtonRect.localScale = Vector3.zero;
            InfoMiniWindowRect.rect.Set(0, 0, 350, 400);

            //Setting color
            var currentItem = SOConfigSingleton.Instance.ItemSOList[itemIdx];
            var lightColor = SOConfigSingleton.Instance.levelBgColorLight[itemLevel];
            var darkColor = SOConfigSingleton.Instance.levelBgColor[itemLevel];
            for (int i = 0, n = lightBackgroundList.Count; i < n; ++i)
            {
                lightBackgroundList[i].color = lightColor;
            }
            background.color = darkColor;
            //Setting InfoText
            var strBuilder = CanvasMonoSingleton.Instance.stringBuilder;
            for (int i = 0, n = currentItem.AffectedAttributeIdx.Count; i < n; ++i)
            {
                if (currentItem.BonusedValueList[i] > 0) strBuilder.Append("+");
                strBuilder.Append(currentItem.BonusedValueList[i]);
                strBuilder.Append(CanvasMonoSingleton.Instance.IdxToAttributeName[currentItem.AffectedAttributeIdx[i]]);
                strBuilder.AppendLine();
            }
            ItemInfoText.text = strBuilder.ToString();
            strBuilder.Clear();
            //
            NameText.text = currentItem.ItemName;
            //
            strBuilder.Append("Recycle (");
            strBuilder.Append(currentPrice);
            strBuilder.Append(")");
            RecycleText.text = strBuilder.ToString();
            strBuilder.Clear();
            //
            Icon.color = Color.white;
            Icon.sprite = currentItem.ItemSprite;
        }

    }
}