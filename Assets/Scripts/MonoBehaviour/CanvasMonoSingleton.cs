using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

namespace ProjectGra
{
    public class CanvasMonoSingleton : MonoBehaviour
    {
        [SerializeField] List<int> rerollBasePrice;
        [SerializeField] List<int> rerollPriceIncreasePerReroll;
        public StringBuilder stringBuilder;
        public static CanvasMonoSingleton Instance;
        [Header("Configs")]
        public float beforeWaveTime;
        public List<string> IdxToAttributeName;

        public Action OnShopContinueButtonClicked;
        //public Action OnPauseContinueButtonClicked;
        public Action<bool> OnWeaponCanvasGroupShowIsCurrentShopUI;
        public Action<int> OnNewWaveBegin;
        public int CodingWave;
        [SerializeField] TextMeshProUGUI shopWaveText;

        [SerializeField] CanvasGroup InGameUICanvasGroup;
        [SerializeField] CanvasGroup PauseCanvasGroup;
        [SerializeField] CanvasGroup SingleAttributeCanvasGroup;
        [SerializeField] CanvasGroup ItemFoundAndUpgradeCanvasGroup;
        [SerializeField] CanvasGroup ShopCanvasGroup;
        [SerializeField] CanvasGroup WeaponAndItemCanvasGroup;
        [SerializeField] CanvasGroup MainMenuCanvasGroup;
        [SerializeField] CanvasGroup PresetChoosingCanvasGroup;
        [SerializeField] CanvasGroup SettingCanvasGroup;

        [Header("Setting Manager")]
        [SerializeField] SettingManager settingManager;
        [Header("ChoiceManager")]
        [SerializeField] ChoiceManager choiceManager;

        [Header("InfoMiniWindow")]
        [SerializeField] InfoMiniWindow infoMiniWindow;
        //[Header("AttributeTextInItsCanvas")]
        //[SerializeField] TextMeshProUGUI attributeInfoText;

        public RectTransform DragLayer;

        [Header("InGameUI")]
        [SerializeField] InGameUIManager ingameUIManager;


        [Header("Pause Canvas")]
        //[SerializeField] Button pauseContinueButton;
        [SerializeField] PauseUIManager pauseUIManager;

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
            stringBuilder = new StringBuilder(100);
            Instance = this;
            InGameUICanvasGroup.interactable = false;
            InGameUICanvasGroup.blocksRaycasts = false;
            //pauseContinueButton.onClick.AddListener(() => { OnPauseContinueButtonClicked?.Invoke(); }); //tring to use lambda
            OnShopContinueButtonClicked += BeforeExitShopCallBack;
            PlayerDataModel.Instance.OnMaterialChanged += IngameUISetMaterial;
            itemFoundUIRect.localScale = Vector3.zero;
            upgradeUIRect.localScale = Vector3.zero;
        }
        public void Start()
        {
            UpdateWaveNumberText();
        }

        public void OnDestroy()
        {
            //pauseContinueButton.onClick.RemoveAllListeners();
        }
        public void ShopContinueButtonActionWrapper()
        {
            OnShopContinueButtonClicked?.Invoke();
        }
        //private void PauseContinueButtonActionWrapper()
        //{
        //    OnPauseContinueButtonClicked?.Invoke();
        //}
        public float GetMouseSensitivityModifier()
        {
            return settingManager.GetMouseSensitivityModifier();
        }
        private void BeforeExitShopCallBack()
        {
            ++CodingWave;
            SOConfigSingleton.Instance.Wave++;
            ingameUIManager.IngameUIInit(PlayerDataModel.Instance.GetMaxHealthPoint(), PlayerDataModel.Instance.GetMaxExp());
            OnNewWaveBegin?.Invoke(CodingWave);
            StartCountdownTimer(beforeWaveTime);
            HideShop();
            UpdateWaveNumberText();
            ShowInGameUI();
        }
        //private void SetWeaponAndItemIsCurrentShopUI(bool isCanvasShop)
        //{
        //    shopUIManager.SetWeaponAndItemInCanvas(isCanvasShop);
        //}

        public void ShowSettingCanvasGroup()
        {
            SettingCanvasGroup.alpha = 1f;
            SettingCanvasGroup.interactable = true;
            SettingCanvasGroup.blocksRaycasts = true;
        }
        public void HideSettingCanvasGroup()
        {
            SettingCanvasGroup.alpha = 0f;
            SettingCanvasGroup.interactable = false;
            SettingCanvasGroup.blocksRaycasts = false;
        }
        public void ShowMainMenuCanvasGroup()
        {
            Cursor.lockState = CursorLockMode.None;
            MainMenuCanvasGroup.alpha = 1f;
            MainMenuCanvasGroup.interactable = true;
            MainMenuCanvasGroup.blocksRaycasts = true;
        }
        public void HideMainMenuCanvasGroup()
        {
            MainMenuCanvasGroup.alpha = 0f;
            MainMenuCanvasGroup.interactable = false;
            MainMenuCanvasGroup.blocksRaycasts = false;
        }
        public void HidePresetChoosingCanvasGroup()
        {
            PresetChoosingCanvasGroup.alpha = 0f;
            PresetChoosingCanvasGroup.blocksRaycasts = false;
            PresetChoosingCanvasGroup.interactable = false;
        }
        public void ShowPresetChoosingCanvasGroup()
        {
            Cursor.lockState = CursorLockMode.None;
            choiceManager.ResetState();
            shopUIManager.ResetShopLockState();
            // bad smell..
            CodingWave = 0;
            PresetChoosingCanvasGroup.alpha = 1f;
            PresetChoosingCanvasGroup.blocksRaycasts = true;
            PresetChoosingCanvasGroup.interactable = true;
        }

        private void ShowWeaponAndItemCanvas()
        {
            WeaponAndItemCanvasGroup.alpha = 1f;
            WeaponAndItemCanvasGroup.interactable = true;
            WeaponAndItemCanvasGroup.blocksRaycasts = true;
        }
        private void HideWeaponAndItemCanvas()
        {
            WeaponAndItemCanvasGroup.alpha = 0f;
            WeaponAndItemCanvasGroup.interactable = false;
            WeaponAndItemCanvasGroup.blocksRaycasts = false;
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
            if (weaponIdx != -1) OnWeaponAddOrDeleteAction?.Invoke(weaponIdx, isAddingWeapon);
        }
        internal void CombineWeaponFromTo(int combineSlotIdx, int calledSlotIdx)
        {
            shopUIManager.CombineWeaponFromTo(combineSlotIdx, calledSlotIdx);
        }

        internal void RecycleWeaponFromSlot(int calledSlotIdx)
        {
            shopUIManager.RecycleWeaponFromSlot(calledSlotIdx);
        }
        public void AddGameItem(int itemIdx, int itemLevel, int basePrice, int costMaterialCount)
        {
            shopUIManager.AddGameItem(itemIdx, itemLevel, basePrice, costMaterialCount);
        }
        public void SetSlotWeaponIdxInShop(int4 wpIdxInt4, int4 wpLevel)
        {
            shopUIManager.SetSlotWeaponIdx(wpIdxInt4, wpLevel);
        }
        public int4 GetSlotWeaponIdxInShop()
        {
            return shopUIManager.GetSlotWeaponIdx();
        }
        public int4 GetSlotWeaponLevelInShop()
        {
            return shopUIManager.GetSlotWeaponLevelInShop();
        }
        //internal bool4 GetSlowWeaponIsMeleeInShop()
        //{
        //    return shopUIManager.GetSlotWeaponIsMelee();
        //}
        private void UpdateWaveNumberText()
        {
            stringBuilder.Append("Wave ");
            stringBuilder.Append(CodingWave + 1);
            shopWaveText.text = stringBuilder.ToString();
            ingameUIManager.UpdateWaveText(CodingWave);
            stringBuilder.Clear();
        }
        public void ShowShopAndOtherUI()
        {
            //show Cursor
            Cursor.lockState = CursorLockMode.None;
            ShowItemFoundUIandIngameBackground();
        }
        public void ShowShopAndOtherUI(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute, int playerItemCountThisWave, int MaterialCount)
        {
            //show Cursor
            Cursor.lockState = CursorLockMode.None;
            PlayerDataModel.Instance.SetAttributeWithStruct(attributeStruct, damageRelatedAttribute);
            PlayerDataModel.Instance.SetMaterialValWith(MaterialCount);
            this.normalCrateThisWave = playerItemCountThisWave;
            ShowItemFoundUIandIngameBackground();
        }
        private void ShowShopUI()
        {

            HideInGameUI();
            HideItemFoundAndUpgradeCanvasGroup();
            HideSingleAttributeUI();
            ShopCanvasGroup.alpha = 1;
            ShopCanvasGroup.interactable = true;
            ShopCanvasGroup.blocksRaycasts = true;
            shopUIManager.ShowShopReset();
            //SetWeaponAndItemIsCurrentShopUI(true);
            OnWeaponCanvasGroupShowIsCurrentShopUI?.Invoke(true);
            ShowWeaponAndItemCanvas();
        }
        public void HideShop()
        {
            Cursor.lockState = CursorLockMode.Locked;
            ShopCanvasGroup.alpha = 0;
            ShopCanvasGroup.interactable = false;
            ShopCanvasGroup.blocksRaycasts = false;
            HideWeaponAndItemCanvas();
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

        public void ShowPauseCanvasGroup(bool isShowingContinueButton, bool isSurvived = default)
        {
            Cursor.lockState = CursorLockMode.None;
            PauseCanvasGroup.alpha = 1;
            PauseCanvasGroup.interactable = true;
            PauseCanvasGroup.blocksRaycasts = true;
            ingameUIManager.StopCountdown();
            if (isShowingContinueButton)
            {
                pauseUIManager.SetContinueButtonRect(isShowingContinueButton);
            }
            else
            {
                pauseUIManager.SetContinueButtonRect(isShowingContinueButton);
                pauseUIManager.ShowGameResult(isSurvived);
            }

            ShowSingleAttributeUI();
            //SetWeaponAndItemIsCurrentShopUI(false);
            OnWeaponCanvasGroupShowIsCurrentShopUI?.Invoke(false);
            ShowWeaponAndItemCanvas();
        }
        public void HidePauseCanvasGroup()
        {
            Cursor.lockState = CursorLockMode.Locked;
            PauseCanvasGroup.alpha = 0;
            PauseCanvasGroup.interactable = false;
            PauseCanvasGroup.blocksRaycasts = false;
            ingameUIManager.StartCountdown();
            HideSingleAttributeUI();
            HideWeaponAndItemCanvas();
        }
        #endregion



        #region In-game UI

        public void StartCountdownTimer(float totalTimer)
        {
            ingameUIManager.StartCountdownTimer(totalTimer);
        }
        public void StopCountdown()
        {
            ingameUIManager.StopCountdown();
        }
        private void InitIngameUIWeaponCooldown()
        {
            ingameUIManager.InitIngameUIWeaponCooldown();
        }
        private void ShowIngameUIBackground()
        {
            ingameUIManager.ShowIngameUIBackground();
        }
        private void HideIngameUIBackground()
        {
            ingameUIManager.HideIngameUIBackground();
        }
        //public void InGameUIUpdateCountdown(int countdown)
        //{

        //}
        public void IngameUIWeaponCooldownFilling(float fillAmount)
        {
            ingameUIManager.IngameUIWeaponCooldownFilling(fillAmount);
        }
        private void IngameUISetMaterial(int materialCount)
        {
            ingameUIManager.IngameUISetMaterial(materialCount);
        }
        public void IngameUIInit(int maxHp, int maxExp)
        {
            ingameUIManager.IngameUIInit(maxHp, maxExp);
        }
        //public void IngameUIUpdataPlayerStats(int hp, int currentTotalExp, int materialsCount)
        //{
        //    //healthBar.fillAmount = (float)hp / ingameUIMaxHp;
        //    //stringBuilder.Append(materialsCount);
        //    //inGameMaterialCountText.text = stringBuilder.ToString();
        //    //stringBuilder.Clear();
        //    //stringBuilder.Append(hp);
        //    //stringBuilder.Append(" / ");
        //    //stringBuilder.Append(ingameUIMaxHp);
        //    //healthBarText.text = stringBuilder.ToString();
        //    //stringBuilder.Clear();
        //    //currentExp += currentTotalExp - lastTotalExp;
        //    //lastTotalExp = currentTotalExp;
        //    //if (currentExp > ingameUIMaxExp)
        //    //{
        //    //    currentExp -= ingameUIMaxExp;
        //    //    ++currentPlayerLevel;
        //    //    ++upgradeThisWave;
        //    //    ingameUIMaxExp = (currentPlayerLevel + 3) * (currentPlayerLevel + 3);
        //    //    expBarText.text = "LV." + currentPlayerLevel;
        //    //}
        //    //experienceBar.fillAmount = (float)currentExp / ingameUIMaxExp;
        //}
        public void IngameUIUpdatePlayerHp(int hp)
        {
            ingameUIManager.IngameUIUpdatePlayerHp(hp);
        }
        public void IngameUIUpdatePlayerExp(int currentTotalExp)
        {
            upgradeThisWave += ingameUIManager.IngameUIUpdatePlayerExp(currentTotalExp);
        }
        public void IngameUIUpdatePlayerMaterial(int materialsCount)
        {
            ingameUIManager.IngameUIUpdatePlayerMaterial(materialsCount);
        }
        public void IngameUIAddNormalCrateIcon(int normal)
        {
            ++normalCrateThisWave;
            ingameUIManager.IngameUIAddNormalCrateIcon(normal);
        }
        public void IngameUIAddLegendaryCrateIcon(int legendary)
        {
            ++legendaryCrateThisWave;
            ingameUIManager.IngameUIAddLegendaryCrateIcon(legendary);
        }
        public void ShowInGameUI()
        {
            Cursor.lockState = CursorLockMode.Locked;

            InitIngameUIWeaponCooldown();
            HideIngameUIBackground();
            InGameUICanvasGroup.alpha = 1;
        }
        public void HideInGameUI()
        {
            InGameUICanvasGroup.alpha = 0;
            ingameUIManager.DestroyAllIcon();
        }
        #endregion

        #region InfoMiniWindow
        public void ShowAndInitInfoWindowWithWeapon(int WeaponIdx, int WeaponLevel, Vector3 position)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithWeapon(WeaponIdx, WeaponLevel, position);
        }
        public void ShowAndInitInfoWindowWithWeapon(int WeaponIdx, int WeaponLevel, int currentPrice, bool showCombine, int combineSlotIdx, int calledSlotIdx, Vector3 position)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithWeapon(WeaponIdx, WeaponLevel, currentPrice, showCombine, combineSlotIdx, calledSlotIdx, position);
        }
        public void ShowAndInitInfoWindowWithItem(int itemIdx, int itemLevel, int currentPrice, GameObject itemSlotGO, Vector3 showPos)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithItem(itemIdx, itemLevel, currentPrice, itemSlotGO, showPos);
        }
        public void ShowAndInitInfoWindowWithItem(int itemIdx, int itemLevel, GameObject itemSlotGO, Vector3 showPos)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithItem(itemIdx, itemLevel, itemSlotGO, showPos);
        }
        #endregion

        #region ItemFound And Level Up UI
        //for item found and level up
        int legendaryCrateThisWave;
        int normalCrateThisWave;
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
            ////++upgradeThisWave;
            //Debug.LogWarning("adding upgrade times for test");
            if (legendaryCrateThisWave > 0)
            {
                --legendaryCrateThisWave;
                itemFoundUIRect.localScale = Vector3.one;
                itemFoundUIManager.RerollLegendary();
            }
            else if (normalCrateThisWave > 0)
            {
                --normalCrateThisWave;
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
            if(legendaryCrateThisWave-- > 0)
            {
                itemFoundUIManager.RerollLegendary();
            }
            else if (normalCrateThisWave-- > 0)
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

        public void RecycleGameItemWithGO(int itemIdx, int currentPrice, GameObject calledItemSlot)
        {
            shopUIManager.RecycleGameItemWithGO(itemIdx, currentPrice, calledItemSlot);
        }




        #endregion

    }

}