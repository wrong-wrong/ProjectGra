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
        public Action OnContinueButtonClicked;

        [SerializeField] CanvasGroup ShopCanvasGroup;
        [SerializeField] CanvasGroup InGameUICanvasGroup;
        [SerializeField] Button ContinueButton;
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
            ContinueButton.onClick.AddListener(ActionWrapper);
        }

        public void OnDestroy()
        {
            ContinueButton.onClick.RemoveListener(ActionWrapper);   
        }
        private void ActionWrapper()
        {
            OnContinueButtonClicked?.Invoke();
            //Debug.Log("ButtonClicked");
        }
        
        public void UpdatePlayerAttribute(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
        {
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

        public void UpdateMainWeaponInfo(MainWeaponState mainWeaponState)
        {
            stringBuilder.AppendLine("Weapon Unknown");
            stringBuilder.Append(mainWeaponState.BasicDamage);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.DamageBonus.x);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.DamageBonus.y);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.DamageBonus.z);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.DamageBonus.w);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.WeaponCriticalHitChance);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.WeaponCriticalHitRatio);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.Range);
            stringBuilder.AppendLine();
            stringBuilder.Append(mainWeaponState.Cooldown);
            stringBuilder.AppendLine();
            MainWeaponInfoText.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }


        public void ShowShop(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute, MainWeaponState weaponState)
        {
            ShopCanvasGroup.alpha = 1;
            ShopCanvasGroup.interactable = true;
            ShopCanvasGroup.blocksRaycasts = true;
            UpdatePlayerAttribute(attributeStruct, damageRelatedAttribute);
            UpdateMainWeaponInfo(weaponState);
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