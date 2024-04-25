using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class CanvasMonoSingleton : MonoBehaviour
    {
        [SerializeField] List<int> rerollBasePrice;
        [SerializeField] List<int> rerollPriceIncreasePerReroll;
        public StringBuilder stringBuilder;
        public static CanvasMonoSingleton Instance;
        public List<string> IdxToAttributeName;

        public Action OnShopContinueButtonClicked;
        public Action OnPauseContinueButtonClicked;
        public int CodingWave;
        [SerializeField] TextMeshProUGUI shopWaveText;
        [SerializeField] TextMeshProUGUI inGameWaveText;

        [SerializeField] CanvasGroup InGameUICanvasGroup;
        [SerializeField] CanvasGroup PauseCanvasGroup;
        [SerializeField] CanvasGroup SingleAttributeCanvasGroup;
        [SerializeField] CanvasGroup ItemFoundAndUpgradeCanvasGroup;
        [SerializeField] CanvasGroup ShopCanvasGroup;
        [Header("InfoMiniWindow")]
        [SerializeField] InfoMiniWindow infoMiniWindow;
        //[Header("AttributeTextInItsCanvas")]
        //[SerializeField] TextMeshProUGUI attributeInfoText;

        public RectTransform DragLayer;

        [Header("InGameUI")]
        [SerializeField] Image healthBar;
        [SerializeField] Image experienceBar;
        [SerializeField] Image weaponCooldownFillingImg;
        [SerializeField] TextMeshProUGUI inGameMaterialCountText;
        [SerializeField] RectTransform ingameUIBackground;
        [SerializeField] TextMeshProUGUI healthBarText;
        [SerializeField] TextMeshProUGUI countDownText;
        [Header("Pause Canvas")]
        [SerializeField] Button pauseContinueButton;

        [Header("Shop UI Manger")]
        [SerializeField] ShopUIManager shopUIManager;
        //For Weapon Category
        public Action<int, bool> OnWeaponAddOrDeleteAction;

        [Header("Item Found UI Manger")]
        [SerializeField] ItemFoundUIManager itemFoundUIManager;
        [SerializeField] RectTransform itemFoundUIRect;
        [Header("Upgrade UI Manger")]
        [SerializeField] UpgradeUIManager upgradeUIManager;
        [SerializeField] RectTransform upgradeUIRect;

        [Header("Category Manager")]
        [SerializeField] WeaponCategoryManager weaponCategoryManager;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            //IdxToAttributeName = new List<string>(11);
            //for(int i = 0; i < 11; i++)
            //{
            //    IdxToAttributeName.Add("");
            //}
            //IdxToAttributeName[0] = "MaxHealthPoint";
            //IdxToAttributeName[1] = "HealthRegain";
            //IdxToAttributeName[2] = "Armor";
            //IdxToAttributeName[3] = "Speed";
            //IdxToAttributeName[4] = "Range";
            //IdxToAttributeName[5] = "CritHitChance";
            //IdxToAttributeName[6] = "Damage";
            //IdxToAttributeName[7] = "MeleeDamage";
            //IdxToAttributeName[8] = "RangeDamage";
            //IdxToAttributeName[9] = "ElementDamage";
            //IdxToAttributeName[10] = "AttackSpeed";
            stringBuilder = new StringBuilder(100);
            Instance = this;
            InGameUICanvasGroup.interactable = false;
            InGameUICanvasGroup.blocksRaycasts = false;
            pauseContinueButton.onClick.AddListener(() => { OnPauseContinueButtonClicked?.Invoke(); }); //tring to use lambda
            OnShopContinueButtonClicked += BeforeExitShopCallBack;
            PlayerDataModel.Instance.OnMaterialChanged += IngameUISetMaterial;
            itemFoundUIRect.localScale = Vector3.zero;
            upgradeUIRect.localScale = Vector3.zero;
        }
        public void Start()
        {
            UpdateWaveNumberText();
        }
        private void Update()
        {
            if (updateCountdown)
            {
                var deltatime = Time.deltaTime;
                if((countdownTimer -= deltatime) < lastCountdown)
                {
                    lastCountdown = (int)countdownTimer;
                    stringBuilder.Append(lastCountdown);
                    countDownText.text = stringBuilder.ToString();
                    stringBuilder.Clear();
                }
            }
        }
        public void OnDestroy()
        {
            pauseContinueButton.onClick.RemoveAllListeners();
        }
        public void ShopContinueButtonActionWrapper()
        {
            OnShopContinueButtonClicked?.Invoke();
        }
        private void PauseContinueButtonActionWrapper()
        {
            OnPauseContinueButtonClicked?.Invoke();
        }

        private void BeforeExitShopCallBack()
        {
            ++CodingWave;
            SOConfigSingleton.Instance.Wave++;
            ingameUIMaxHp = PlayerDataModel.Instance.GetMaxHealthPoint();
            ingameUIMaxExp = PlayerDataModel.Instance.GetMaxExp();
            HideShop();
            UpdateWaveNumberText();
            ShowInGameUI();
        }

        public int GetCategoryActivatedCount(int categoryIdx)
        {
            return weaponCategoryManager.GetCategoryActivatedCount(categoryIdx);
        }

        public int CalculateFinalPrice(float basePrice)
        {
            return (int)(basePrice + CodingWave + (basePrice * CodingWave * 0.1));
        }

        public int GetRerollPrice(int rerollTimes)
        {
            return rerollBasePrice[CodingWave] + rerollPriceIncreasePerReroll[CodingWave] * rerollTimes;
        }

        #region Shop UI
        public void OnWeaponAddOrDelete(int weaponIdx, bool isAddingWeapon)
        {
                    //For Weapon
            if(weaponIdx != -1)OnWeaponAddOrDeleteAction?.Invoke(weaponIdx,isAddingWeapon);
        }
        internal void CombineWeaponFromTo(int combineSlotIdx, int calledSlotIdx)
        {
            shopUIManager.CombineWeaponFromTo(combineSlotIdx, calledSlotIdx);
        }

        internal void RecycleWeaponFromSlot(int calledSlotIdx)
        {
            shopUIManager.RecycleWeaponFromSlot(calledSlotIdx);
        }
        public void AddGameItem(int itemIdx, int itemLevel,int currentPrice, int costMaterialCount)
        {
            shopUIManager.AddGameItem(itemIdx, itemLevel,currentPrice, costMaterialCount);
        }
        public void SetSlotWeaponIdxInShop(int4 wpIdxInt4, bool4 isMeleeWp)
        {
            shopUIManager.SetSlotWeaponIdx(wpIdxInt4,isMeleeWp);
        }
        public int4 GetSlotWeaponIdxInShop()
        {
            return shopUIManager.GetSlotWeaponIdx();
        }
        public int4 GetSlotWeaponLevelInShop()
        {
            return shopUIManager.GetSlotWeaponLevelInShop();
        }
        internal bool4 GetSlowWeaponIsMeleeInShop()
        {
            return shopUIManager.GetSlotWeaponIsMelee();
        }
        private void UpdateWaveNumberText()
        {
            stringBuilder.Append("Wave ");
            stringBuilder.Append(CodingWave + 1);
            shopWaveText.text = stringBuilder.ToString();
            inGameWaveText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        public void ShowShopAndOtherUI(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute, int playerItemCountThisWave, int MaterialCount)
        {
            PlayerDataModel.Instance.SetAttributeWithStruct(attributeStruct, damageRelatedAttribute);
            PlayerDataModel.Instance.SetMaterialValWith(MaterialCount);
            this.itemCountThisWave = playerItemCountThisWave;
            ShowItemFoundUIandIngameBackground();
        }
        public void ShowShopUI()
        {
            
            HideInGameUI();
            HideItemFoundAndUpgradeCanvasGroup();
            HideSingleAttributeUI();
            ShopCanvasGroup.alpha = 1;
            ShopCanvasGroup.interactable = true;
            ShopCanvasGroup.blocksRaycasts = true;
            shopUIManager.ShowShop();
        }
        public void HideShop()
        {
            ShopCanvasGroup.alpha = 0;
            ShopCanvasGroup.interactable = false;
            ShopCanvasGroup.blocksRaycasts = false;
        }
        #endregion

        private void ShowSingleAttributeUI()
        {
            SingleAttributeCanvasGroup.alpha = 1;
        }

        private void HideSingleAttributeUI()
        {
            SingleAttributeCanvasGroup.alpha = 0;
        }
        #region PauseUI

        public void ShowPauseCanvasGroup()
        {
            PauseCanvasGroup.alpha = 1;
            PauseCanvasGroup.interactable = true;
            PauseCanvasGroup.blocksRaycasts = true;
            ShowSingleAttributeUI();
        }
        public void HidePauseCanvasGroup()
        {
            PauseCanvasGroup.alpha = 0;
            PauseCanvasGroup.interactable = false;
            PauseCanvasGroup.blocksRaycasts = false;
            HideSingleAttributeUI();
        }
        #endregion

        

        #region In-game UI
        private int ingameUIMaxHp;
        private int ingameUIMaxExp;
        private int currentPlayerLevel;
        private int lastTotalExp;
        private int currentExp;
        private float countdownTimer;
        private int lastCountdown;
        private bool updateCountdown;
        public void StartCountdownTimer(float totalTimer)
        {
            countdownTimer = totalTimer;
            lastCountdown = (int)totalTimer;
            stringBuilder.Append(lastCountdown);
            countDownText.text = stringBuilder.ToString();
            stringBuilder.Clear();
            updateCountdown = true;
        }
        public void StopCountdown()
        {
            updateCountdown = false;
        }
        private void InitIngameUIWeaponCooldown()
        {
            weaponCooldownFillingImg.fillAmount = 1f;
        }
        private void ShowIngameUIBackground()
        {
            ingameUIBackground.localScale = Vector3.one;
        }
        private void HideIngameUIBackground()
        {
            ingameUIBackground.localScale = Vector3.zero;
        }
        public void InGameUIUpdateCountdown(int countdown)
        {

        }
        public void IngameUIWeaponCooldownFilling(float fillAmount)
        {
            weaponCooldownFillingImg.fillAmount = fillAmount;
        }
        private void IngameUISetMaterial(int materialCount)
        {
            stringBuilder.Append(materialCount);
            inGameMaterialCountText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }
        public void IngameUISetMaxHpExp(int maxHp, int maxExp)
        {
            this.ingameUIMaxHp = maxHp;
            this.ingameUIMaxExp = maxExp;
        }
        public void IngameUIUpdataPlayerStats(int hp, int currentTotalExp, int materialsCount)
        {
            healthBar.fillAmount = (float)hp / ingameUIMaxHp;
            stringBuilder.Append(materialsCount);
            inGameMaterialCountText.text = stringBuilder.ToString();
            stringBuilder.Clear();
            stringBuilder.Append(hp);
            stringBuilder.Append(" / ");
            stringBuilder.Append(ingameUIMaxHp);
            healthBarText.text = stringBuilder.ToString();
            stringBuilder.Clear();
            currentExp += currentTotalExp - lastTotalExp;
            if(currentExp > ingameUIMaxExp)
            {
                currentExp -= ingameUIMaxExp;
                ++currentPlayerLevel;
                ++upgradeThisWave;
                ingameUIMaxExp = (currentPlayerLevel + 3) * (currentPlayerLevel + 3);
            }
            experienceBar.fillAmount = (float)currentExp / ingameUIMaxExp;
        }
        public void ShowInGameUI()
        {
            InitIngameUIWeaponCooldown();
            HideIngameUIBackground();
            InGameUICanvasGroup.alpha = 1;
        }
        public void HideInGameUI()
        {
            InGameUICanvasGroup.alpha = 0;
        }
        #endregion

        #region InfoMiniWindow
        public void ShowAndInitInfoWindowWithWeapon(int WeaponIdx, int WeaponLevel, int currentPrice, bool showCombine, int combineSlotIdx, int calledSlotIdx, Vector3 position)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithWeapon(WeaponIdx, WeaponLevel, currentPrice, showCombine, combineSlotIdx, calledSlotIdx, position);
        }
        public void ShowAndInitInfoWindowWithItem(int itemIdx, int itemLevel,int currentPrice, GameObject itemSlotGO, Vector3 showPos)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithItem(itemIdx,itemLevel, currentPrice,itemSlotGO, showPos);
        }
        #endregion

        #region ItemFound And Level Up UI
        //for item found and level up
        int itemCountThisWave;
        int upgradeThisWave;
        private void ShowItemFoundAndUpgradeCanvasGroup()
        {
            ItemFoundAndUpgradeCanvasGroup.alpha = 1;
            ItemFoundAndUpgradeCanvasGroup.interactable = true;
            ItemFoundAndUpgradeCanvasGroup.blocksRaycasts = true;
        }
        private void HideItemFoundAndUpgradeCanvasGroup()
        {
            ItemFoundAndUpgradeCanvasGroup.alpha = 0;
            ItemFoundAndUpgradeCanvasGroup.interactable = false;
            ItemFoundAndUpgradeCanvasGroup.blocksRaycasts = false;
        }
        private void ShowItemFoundUIandIngameBackground()
        {
            ShowIngameUIBackground();
            ShowItemFoundAndUpgradeCanvasGroup();
            ShowSingleAttributeUI();
            if (itemCountThisWave > 0)
            {
                itemFoundUIRect.localScale = Vector3.one;
                itemFoundUIManager.Reroll();
            }
            else
            {
                ShowUpgradeUIandIngameBackground();
            }
        }

        public void ShowUpgradeUIandIngameBackground()
        {
            ShowIngameUIBackground();
            ShowItemFoundAndUpgradeCanvasGroup();
            ShowSingleAttributeUI();
            if (upgradeThisWave > 0)
            {
                upgradeUIRect.localScale = Vector3.one;
                upgradeUIManager.ResetReroll();
            }
            else
            {
                ShowShopUI();
            }
        }
        private void HideItemFoundUI()
        {
            itemFoundUIRect.localScale = Vector3.zero;
            ShowUpgradeUIandIngameBackground();
        }
        private void HideUpgradeUI()
        {
            upgradeUIRect.localScale = Vector3.zero;
            ShowShopUI();
        }
        public void ItemFoundUINext()
        {
            if (--itemCountThisWave > 0)
            {
                itemFoundUIManager.Reroll();
            }
            else
            {
                HideItemFoundUI();
            }
        }

        public void UpgradeUINext()
        {
            if (--upgradeThisWave > 0)
            {
                upgradeUIManager.Reroll();
            }
            else
            {
                HideUpgradeUI();
            }
        }

        public void RecycleGameItemWithGO(int itemIdx, int currentPrice,GameObject calledItemSlot)
        {
            shopUIManager.RecycleGameItemWithGO(itemIdx, currentPrice, calledItemSlot);
        }

        


        #endregion

    }

}