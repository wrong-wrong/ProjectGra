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

        [SerializeField] CanvasGroup ShopCanvasGroup;
        [SerializeField] CanvasGroup InGameUICanvasGroup;
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
            stringBuilder = new StringBuilder();
            Instance = this;
            InGameUICanvasGroup.interactable = false;
            InGameUICanvasGroup.blocksRaycasts = false;
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



        public void ShowShop(PlayerAttributeMain attributeStruct, PlayerAtttributeDamageRelated damageRelatedAttribute)
        {
            ShopCanvasGroup.alpha = 1;
            ShopCanvasGroup.interactable = true;
            ShopCanvasGroup.blocksRaycasts = true;
            UpdatePlayerAttribute(attributeStruct, damageRelatedAttribute);
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
    }

    //public class MyCanvasGroupManagedCom : IComponentData
    //{
    //    public CanvasGroup canvasGroup;
    //    public Cursor cursor;
    //}
}