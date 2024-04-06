using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class CanvasMonoSingleton : MonoBehaviour
    {
        public StringBuilder stringBuilder;
        public static CanvasMonoSingleton Instance;
        public Action OnShopContinueButtonClicked;
        public Action OnPauseContinueButtonClicked;
        public PlayerAtttributeDamageRelated damagedAttribute;
        public PlayerAttributeMain mainAttribute;
        public int playerMaterialCount;
        [SerializeField] CanvasGroup ShopCanvasGroup;
        [SerializeField] CanvasGroup InGameUICanvasGroup;
        [SerializeField] CanvasGroup PauseCanvasGroup;
        [SerializeField] Button ShopContinueButton;
        [SerializeField] Button ShopRerollButton;
        [SerializeField] InfoMimiWindow infoMiniWindow;
        public RectTransform DragLayer;

        [Header("Player Attribute")]
        [SerializeField] TextMeshProUGUI text1;
        [SerializeField] TextMeshProUGUI text2;
        [SerializeField] TextMeshProUGUI text3;
        [SerializeField] TextMeshProUGUI text4;
        [SerializeField] TextMeshProUGUI text5;
        [SerializeField] TextMeshProUGUI text6;
        [SerializeField] TextMeshProUGUI text7;
        [SerializeField] TextMeshProUGUI text8;
        [SerializeField] TextMeshProUGUI text9;
        [SerializeField] TextMeshProUGUI text10;
        [SerializeField] TextMeshProUGUI text11;


        [Header("MainWeaponInfo")]
        [SerializeField] TextMeshProUGUI MainWeaponInfoText;

        [Header("WeaponSlot")]
        [SerializeField] List<WeaponSlot> weaponSlotList;

        [Header("InGameUI")]
        [SerializeField] Image healthBar;
        [SerializeField] Image experienceBar;
        [SerializeField] TextMeshProUGUI inGameMaterialCountText;

        [Header("Pause Canvas")]
        [SerializeField] Button pauseContinueButton;
        [SerializeField] TextMeshProUGUI pauseAttributeInfoText;

        [Header("Shop")]
        [SerializeField] List<ShopItem> shopItemList;
        [SerializeField] TextMeshProUGUI shopMaterialCountText;
        private Vector3 tmpShopItemPosition; // for swap;
        private ShopItem tmpShopItem;
        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            stringBuilder = new StringBuilder(100);
            Instance = this;
            InGameUICanvasGroup.interactable = false;
            InGameUICanvasGroup.blocksRaycasts = false;
            ShopContinueButton.onClick.AddListener(ShopContinueButtonActionWrapper);
            pauseContinueButton.onClick.AddListener(() => { OnPauseContinueButtonClicked?.Invoke(); }); //tring to use lambda
            ShopRerollButton.onClick.AddListener(Reroll);
        }

        private void Start()
        {
            weaponSlotList[0].isMainSlot = true;
        }
        public void OnDestroy()
        {
            ShopContinueButton.onClick.RemoveListener(ShopContinueButtonActionWrapper);
            pauseContinueButton.onClick.RemoveAllListeners();
            ShopRerollButton.onClick.RemoveAllListeners();
        }
        private void ShopContinueButtonActionWrapper()
        {
            OnShopContinueButtonClicked?.Invoke();
            //Debug.Log("ButtonClicked");
        }
        private void PauseContinueButtonActionWrapper()
        {
            OnPauseContinueButtonClicked?.Invoke();
        }

        #region Shop UI
        public void RecycleWeaponFromSlot(int slotIdx)
        {
            playerMaterialCount += weaponSlotList[slotIdx].CurrentPrice;
            weaponSlotList[slotIdx].InitSlot(-1, 0);
            UpdateMaterialCount();
        }
        public void CombineWeaponFromTo(int combineSlotIdx, int callSlotIdx)
        {
            weaponSlotList[callSlotIdx].WeaponLevel++;
            weaponSlotList[callSlotIdx].InitSlot();
            weaponSlotList[combineSlotIdx].InitSlot(-1, 0);
        }
        public void HideInfoMiniWindow()
        {
            infoMiniWindow.WindowRect.localScale = Vector3.zero;
        }
        public void ShowInfoMiniWindow(WeaponSlot slot)
        {
            var showCombine = false;
            var calledSlotIdx = -1;
            var combineSlotIdx = -1;
            for(int i = 0, n = weaponSlotList.Count; i < n; ++i)
            {
                if (slot == weaponSlotList[i])
                {
                    calledSlotIdx = i;
                }
                else if(slot.WeaponIdx == weaponSlotList[i].WeaponIdx && slot.WeaponLevel == weaponSlotList[i].WeaponLevel && slot.WeaponLevel != 3)
                {
                    combineSlotIdx = i;
                    showCombine = true;
                    //break;
                }
            }
            infoMiniWindow.InitInfoMimiWindowAndShotAtPosition(damagedAttribute, mainAttribute.Range, slot.WeaponIdx, slot.WeaponLevel, showCombine, combineSlotIdx, calledSlotIdx,slot.gameObject.transform.position);
        }
        private void UpdateAllShopItemBuyState(int price)
        {
            playerMaterialCount -= price;
            UpdateAllShopItemBuyState();
        }
        private void UpdateAllShopItemBuyState()
        {
            for (int i = 0, n = shopItemList.Count; i < n; ++i)
            {
                shopItemList[i].UpdateBuyButtonState(playerMaterialCount);
            }
        }
        public bool CheckWeaponSlotTryBuyShopItem(int weaponIdx, int weaponLevel, int price)
        {
            WeaponSlot tmp = null;
            for(int i = 0, n = weaponSlotList.Count; i < n; ++i)
            {
                if (weaponSlotList[i].WeaponIdx == -1)
                {
                    weaponSlotList[i].WeaponIdx = weaponIdx;
                    weaponSlotList[i].WeaponLevel = weaponLevel;
                    weaponSlotList[i].CurrentPrice = price;
                    weaponSlotList[i].InitSlot();
                    UpdateAllShopItemBuyState(price);
                    UpdateMaterialCount();
                    return true;
                }
                else if (weaponSlotList[i].WeaponIdx == weaponIdx && weaponSlotList[i].WeaponLevel == weaponLevel)
                {
                    tmp = weaponSlotList[i];
                }
            }
            if(tmp == null)
            {
                return false;
            }
            else
            {
                tmp.WeaponLevel++;
                tmp.InitSlot();
                UpdateAllShopItemBuyState(price);
                UpdateMaterialCount();
                return true;
            }
        }
        private void SwapShopItem(int idx1, int idx2)
        {
            tmpShopItem = shopItemList[idx1];
            shopItemList[idx1] = shopItemList[idx2];
            shopItemList[idx2] = tmpShopItem;
            tmpShopItemPosition = shopItemList[idx1].transform.localPosition;
            shopItemList[idx1].transform.localPosition = shopItemList[idx2].transform.localPosition;
            shopItemList[idx2].transform.localPosition = tmpShopItemPosition;
        }
        private void Reroll()
        {
            for(int i = 0,n = shopItemList.Count, notlocked = -1; i < n; ++i)
            {                
                if (!shopItemList[i].isLock) 
                {
                    if(notlocked == -1)notlocked = i;
                    shopItemList[i].Reroll(playerMaterialCount);
                }
                else
                {
                    if(notlocked != -1)
                    {
                        SwapShopItem(i, notlocked);
                        notlocked = i;
                    }
                }
            }
        }
        public void UpdateShopPlayerAttribute(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
        {
            this.mainAttribute = attributeStruct;
            this.damagedAttribute = damageRelatedAttribute;
            text1.text = attributeStruct.MaxHealthPoint.ToString();
            text2.text = attributeStruct.HealthRegain.ToString();
            text3.text = attributeStruct.Armor.ToString();
            text4.text = attributeStruct.SpeedPercentage.ToString();

            text5.text = damageRelatedAttribute.CriticalHitChance.ToString();
            text6.text = damageRelatedAttribute.MeleeRangedElementAttSpd.x.ToString();
            text7.text = damageRelatedAttribute.MeleeRangedElementAttSpd.y.ToString();
            text8.text = damageRelatedAttribute.MeleeRangedElementAttSpd.z.ToString();
            text9.text = damageRelatedAttribute.MeleeRangedElementAttSpd.w.ToString();
            text10.text = damageRelatedAttribute.DamagePercentage.ToString();
            text11.text = attributeStruct.Range.ToString();
        }

        public void UpdateMainWeaponInfo()
        {
            if (weaponSlotList[0].WeaponIdx == -1)
            {
                return;
            }
            var config = WeaponSOConfigSingleton.Instance.MapCom.wpNativeHashMap[weaponSlotList[0].WeaponIdx];
            var managedConfig = WeaponSOConfigSingleton.Instance.ManagedConfigCom.weaponNameMap[weaponSlotList[0].WeaponIdx];
            stringBuilder.Append(managedConfig);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.BasicDamage);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.DamageBonus.x);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.DamageBonus.y);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.DamageBonus.z);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.DamageBonus.w);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.WeaponCriticalHitChance);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.WeaponCriticalHitRatio);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.Range);
            stringBuilder.AppendLine();
            stringBuilder.Append(config.Cooldown);
            stringBuilder.AppendLine();
            MainWeaponInfoText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        private void UpdateMaterialCount()
        {
            stringBuilder.Append(playerMaterialCount);
            shopMaterialCountText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        private void UpdateMaterialCount(int count)
        {
            playerMaterialCount = count;
            UpdateMaterialCount();
        }

        public void SetSlotWeaponIdx(int idx, int idx1, int idx2, int idx3)
        {
            weaponSlotList[0].InitSlot(idx, 0);
            weaponSlotList[1].InitSlot(idx1, 1);
            weaponSlotList[2].InitSlot(idx2, 2);
            weaponSlotList[3].InitSlot(idx3, 3);
        }
        public void GetSlowWeaponIdx(out int idx, out int idx1, out int idx2, out int idx3)
        {
            idx = weaponSlotList[0].WeaponIdx;
            idx1 = weaponSlotList[1].WeaponIdx;
            idx2 = weaponSlotList[2].WeaponIdx;
            idx3 = weaponSlotList[3].WeaponIdx;
        }
        public void ShowShop(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute, MainWeaponState weaponState, int MaterialCount)
        {
            ShopCanvasGroup.alpha = 1;
            ShopCanvasGroup.interactable = true;
            ShopCanvasGroup.blocksRaycasts = true;
            UpdateShopPlayerAttribute(attributeStruct, damageRelatedAttribute);
            UpdateMainWeaponInfo();
            UpdateMaterialCount(MaterialCount);
            Reroll();
            UpdateAllShopItemBuyState();
        }
        public void HideShop()
        {
            ShopCanvasGroup.alpha = 0;
            ShopCanvasGroup.interactable = false;
            ShopCanvasGroup.blocksRaycasts = false;
        }
        #endregion


        #region PauseUI
        private void UpdatePausePlayerAttribute(TextMeshProUGUI pauseAttributeInfo, PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
        {
            stringBuilder.Append(attributeStruct.MaxHealthPoint);
            stringBuilder.AppendLine();
            stringBuilder.Append(attributeStruct.HealthRegain);
            stringBuilder.AppendLine();
            stringBuilder.Append(attributeStruct.Armor);
            stringBuilder.AppendLine();
            stringBuilder.Append(attributeStruct.SpeedPercentage);
            stringBuilder.AppendLine();
            stringBuilder.Append(attributeStruct.Range);
            stringBuilder.AppendLine();
            stringBuilder.Append(damageRelatedAttribute.MeleeRangedElementAttSpd.x);
            stringBuilder.AppendLine();
            stringBuilder.Append(damageRelatedAttribute.MeleeRangedElementAttSpd.y);
            stringBuilder.AppendLine();
            stringBuilder.Append(damageRelatedAttribute.MeleeRangedElementAttSpd.z);
            stringBuilder.AppendLine();
            stringBuilder.Append(damageRelatedAttribute.MeleeRangedElementAttSpd.w);
            stringBuilder.AppendLine();
            stringBuilder.Append(damageRelatedAttribute.DamagePercentage);
            stringBuilder.AppendLine();
            stringBuilder.Append(damageRelatedAttribute.CriticalHitChance);
            stringBuilder.AppendLine();
            pauseAttributeInfo.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }


        public void ShowPause(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
        {
            PauseCanvasGroup.alpha = 1;
            PauseCanvasGroup.interactable = true;
            PauseCanvasGroup.blocksRaycasts = true;
            UpdatePausePlayerAttribute(pauseAttributeInfoText, attributeStruct, damageRelatedAttribute);
        }
        public void HidePause()
        {
            PauseCanvasGroup.alpha = 0;
            PauseCanvasGroup.interactable = false;
            PauseCanvasGroup.blocksRaycasts = false;
        }
        #endregion


        #region In-game UI
        private int maxHp;
        private float maxExp;
        public void SetMaxHpExp(int maxHp, float maxExp)
        {
            this.maxHp = maxHp;
            this.maxExp = maxExp;
        }
        public void UpdateInGameUI(int hp, float exp, int materialsCount)
        {
            healthBar.fillAmount = (float)hp / maxHp;
            experienceBar.fillAmount = exp / maxExp;
            inGameMaterialCountText.text = materialsCount.ToString();
        }
        public void ShowInGameUI()
        {
            InGameUICanvasGroup.alpha = 1;
        }
        public void HideInGameUI()
        {
            InGameUICanvasGroup.alpha = 0;
        }
        #endregion
    }

}