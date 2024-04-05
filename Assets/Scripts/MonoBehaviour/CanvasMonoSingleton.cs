using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class CanvasMonoSingleton : MonoBehaviour
    {
        private StringBuilder stringBuilder;
        public static CanvasMonoSingleton Instance;
        public Action OnShopContinueButtonClicked;
        public Action OnPauseContinueButtonClicked;
        public PlayerAtttributeDamageRelated damagedAttribute;
        public PlayerAttributeMain mainAttribute;
        [SerializeField] CanvasGroup ShopCanvasGroup;
        [SerializeField] CanvasGroup InGameUICanvasGroup;
        [SerializeField] CanvasGroup PauseCanvasGroup;
        [SerializeField] Button ShopContinueButton;
        [SerializeField] Button ShopRerollButton;
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
        //[SerializeField] TextMeshProUGUI text13;
        //[SerializeField] TextMeshProUGUI text14;
        //[SerializeField] TextMeshProUGUI attackSpeedBonus;
        //[SerializeField] TextMeshProUGUI critHitChance;
        //[SerializeField] TextMeshProUGUI critHitRatio;
        //[SerializeField] TextMeshProUGUI cooldown;
        //[SerializeField] TextMeshProUGUI range;
        //[SerializeField] TextMeshProUGUI damagePercentage;

        [Header("WeaponSlot")]
        [SerializeField] WeaponSlot mainWeaponSlot;
        [SerializeField] WeaponSlot leftAutoSlot;
        [SerializeField] WeaponSlot midAutoSlot;
        [SerializeField] WeaponSlot rightAutoSlot;



        [Header("InGameUI")]
        [SerializeField] Image healthBar;
        [SerializeField] Image experienceBar;
        [SerializeField] TextMeshProUGUI MaterialCount;

        [Header("Pause Canvas")]
        [SerializeField] Button pauseContinueButton;
        [SerializeField] TextMeshProUGUI pauseAttributeInfoText;

        [Header("Shop")]
        [SerializeField] ShopItem leftShopItem;
        [SerializeField] ShopItem midShopItem;
        [SerializeField] ShopItem rightShopItem;
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
            mainWeaponSlot.isMainSlot = true;
        }
        public void OnDestroy()
        {
            ShopContinueButton.onClick.RemoveListener(ShopContinueButtonActionWrapper);
            pauseContinueButton.onClick.RemoveAllListeners();
            ShopRerollButton.onClick.RemoveAllListeners();
        }
        private void Reroll()
        {
            if (!leftShopItem.isLock)
            {
                leftShopItem.Reroll();
            }
            if (!midShopItem.isLock)
            {
                midShopItem.Reroll();
            }
            if (!rightShopItem.isLock)
            {
                rightShopItem.Reroll();
            }
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

        public void UpdatePlayerAttribute(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
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
        private void UpdatePlayerAttribute(TextMeshProUGUI pauseAttributeInfo, PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
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

        public void UpdateMainWeaponInfo()
        {
            if(mainWeaponSlot.WeaponIdx == -1)
            {
                return;
            }
            var config = WeaponSOConfigSingleton.Instance.MapCom.wpNativeHashMap[mainWeaponSlot.WeaponIdx];
            var managedConfig = WeaponSOConfigSingleton.Instance.ManagedConfigCom.managedConfigMap[mainWeaponSlot.WeaponIdx];
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

        public void SetSlotWeaponIdx(int idx, int idx1, int idx2, int idx3)
        {
            mainWeaponSlot.InitSlot(idx,0);
            leftAutoSlot.InitSlot(idx1, 1);
            midAutoSlot.InitSlot(idx2, 2);
            rightAutoSlot.InitSlot(idx3, 3);
        }
        public void GetSlowWeaponIdx(out int idx, out int idx1, out int idx2, out int idx3)
        {
            idx = mainWeaponSlot.WeaponIdx;
            idx1 = leftAutoSlot.WeaponIdx;
            idx2 = midAutoSlot.WeaponIdx;
            idx3 = rightAutoSlot.WeaponIdx;
        }
        public void ShowPause(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
        {
            PauseCanvasGroup.alpha = 1;
            PauseCanvasGroup.interactable = true;
            PauseCanvasGroup.blocksRaycasts = true;
            UpdatePlayerAttribute(pauseAttributeInfoText, attributeStruct, damageRelatedAttribute);
        }
        public void HidePause()
        {
            PauseCanvasGroup.alpha = 0;
            PauseCanvasGroup.interactable = false;
            PauseCanvasGroup.blocksRaycasts = false;
        }

        public void ShowShop(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute, MainWeaponState weaponState)
        {
            ShopCanvasGroup.alpha = 1;
            ShopCanvasGroup.interactable = true;
            ShopCanvasGroup.blocksRaycasts = true;
            UpdatePlayerAttribute(attributeStruct, damageRelatedAttribute);
            UpdateMainWeaponInfo();
        }
        public void HideShop()
        {
            ShopCanvasGroup.alpha = 0;
            ShopCanvasGroup.interactable = false;
            ShopCanvasGroup.blocksRaycasts = false;
        }
        public void ShowInGameUI()
        {
            InGameUICanvasGroup.alpha = 1;
        }
        public void HideInGameUI()
        {
            InGameUICanvasGroup.alpha = 0;
        }

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
            MaterialCount.text = materialsCount.ToString();
        }
        #endregion
    }

    //public class MyCanvasGroupManagedCom : IComponentData
    //{
    //    public CanvasGroup canvasGroup;
    //    public Cursor cursor;
    //}
}