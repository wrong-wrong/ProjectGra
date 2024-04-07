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
        [SerializeField] TextMeshProUGUI inGameMaterialCountText;

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
            stringBuilder = new StringBuilder(100);
            Instance = this;
            InGameUICanvasGroup.interactable = false;
            InGameUICanvasGroup.blocksRaycasts = false;
            pauseContinueButton.onClick.AddListener(() => { OnPauseContinueButtonClicked?.Invoke(); }); //tring to use lambda
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



        #region Shop UI

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
            ShowItemFoundUI();
        }
        public void ShowShopUI()
        {
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

        #region refactor
        public void ShowAndInitInfoWindow(int WeaponIdx, int WeaponLevel, bool showCombine, int combineSlotIdx, int calledSlotIdx, Vector3 position)
        {
            infoMiniWindow.gameObject.SetActive(true);
            infoMiniWindow.InitInfoMimiWindowAndShowAtPosition(WeaponIdx, WeaponLevel, showCombine, combineSlotIdx, calledSlotIdx, position);
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
        private void ShowItemFoundUI()
        {
            ShowItemFoundAndUpgradeCanvasGroup();
            ShowSingleAttributeUI();
            if (itemCountThisWave > 0)
            {
                itemFoundUIRect.localScale = Vector3.one;
                itemFoundUIManager.Reroll();
            }
            else
            {
                ShowUpgradeUI();
            }
        }

        public void ShowUpgradeUI()
        {
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
            ShowUpgradeUI();
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
        #endregion

    }

}