using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class CanvasMonoSingleton : MonoBehaviour
    {
        public StringBuilder stringBuilder;
        public static CanvasMonoSingleton Instance;
        public static List<string> IdxToAttributeName;

        public Action OnShopContinueButtonClicked;
        public Action OnPauseContinueButtonClicked;

        [SerializeField] CanvasGroup InGameUICanvasGroup;
        [SerializeField] CanvasGroup PauseCanvasGroup;
        [SerializeField] CanvasGroup SingleAttributeCanvasGroup;
        [SerializeField] CanvasGroup ItemFoundAndUpgradeCanvasGroup;
        [SerializeField] CanvasGroup ShopCanvasGroup;
        [Header("InfoMiniWindow")]
        [SerializeField] InfoMimiWindow infoMiniWindow;
        [Header("AttributeTextInItsCanvas")]
        [SerializeField] TextMeshProUGUI attributeInfoText;

        public RectTransform DragLayer;

        [Header("InGameUI")]
        [SerializeField] Image healthBar;
        [SerializeField] Image experienceBar;
        [SerializeField] Image weaponCooldownFillingImg;
        [SerializeField] TextMeshProUGUI inGameMaterialCountText;
        [SerializeField] RectTransform ingameUIBackground;
        [Header("Pause Canvas")]
        [SerializeField] Button pauseContinueButton;

        [Header("Shop UI Manger")]
        [SerializeField] ShopUIManager shopUIManager;
        [Header("Item Found UI Manger")]
        [SerializeField] ItemFoundUIManager itemFoundUIManager;
        [SerializeField] RectTransform itemFoundUIRect;
        [Header("Upgrade UI Manger")]
        [SerializeField] UpgradeUIManager upgradeUIManager;
        [SerializeField] RectTransform upgradeUIRect;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            IdxToAttributeName = new List<string>(11);
            for(int i = 0; i < 11; i++)
            {
                IdxToAttributeName.Add("");
            }
            IdxToAttributeName[0] = "MaxHealthPoint";
            IdxToAttributeName[1] = "HealthRegain";
            IdxToAttributeName[2] = "Armor";
            IdxToAttributeName[3] = "Speed";
            IdxToAttributeName[4] = "Range";
            IdxToAttributeName[5] = "CritHitChance";
            IdxToAttributeName[6] = "Damage";
            IdxToAttributeName[7] = "MeleeDamage";
            IdxToAttributeName[8] = "RangeDamage";
            IdxToAttributeName[9] = "ElementDamage";
            IdxToAttributeName[10] = "AttackSpeed";
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
            ingameUIMaxHp = PlayerDataModel.Instance.GetMaxHealthPoint();
            ingameUIMaxExp = PlayerDataModel.Instance.GetMaxExp();
            HideShop();
            ShowInGameUI();
        }

        #region Shop UI
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
        public void SetSlotWeaponIdxInShop(int idx, int idx1, int idx2, int idx3)
        {
            shopUIManager.SetSlotWeaponIdx(idx,idx1, idx2, idx3);
        }
        public void GetSlowWeaponIdxInShop(out int idx, out int idx1, out int idx2, out int idx3)
        {
            shopUIManager.GetSlowWeaponIdx(out idx, out idx1, out idx2, out idx3);
        }
        public void ShowShopAndOtherUI(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute, int playerItemCountThisWave, int playerLevelupThisWave, int MaterialCount)
        {
            PlayerDataModel.Instance.SetAttributeWithStruct(attributeStruct, damageRelatedAttribute);
            PlayerDataModel.Instance.SetMaterialValWith(MaterialCount);
            this.itemCountThisWave = playerItemCountThisWave;
            this.upgradeThisWave = playerLevelupThisWave;
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
        public void IngameUIUpdataPlayerStats(int hp, int exp, int materialsCount)
        {
            healthBar.fillAmount = (float)hp / ingameUIMaxHp;
            experienceBar.fillAmount = exp / ingameUIMaxExp;
            stringBuilder.Append(materialsCount);
            inGameMaterialCountText.text = stringBuilder.ToString();
            stringBuilder.Clear();
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
        public void ShowAndInitInfoWindowWithWeapon(int WeaponIdx, int WeaponLevel, bool showCombine, int combineSlotIdx, int calledSlotIdx, Vector3 position)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPositionWithWeapon(WeaponIdx, WeaponLevel, showCombine, combineSlotIdx, calledSlotIdx, position);
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
                upgradeUIManager.Reroll();
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