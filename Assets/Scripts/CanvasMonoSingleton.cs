using ProjectGra.PlayerController;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class CanvasMonoSingleton : MonoBehaviour
    {
        public static CanvasMonoSingleton instance;

        public CanvasGroup canvasGroup;
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

        public void Awake()
        {
            //HideCursor();
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            //CameraTargetTransform = transform;
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

    }

    public class MyCanvasGroupManagedCom : IComponentData
    {
        public CanvasGroup canvasGroup;
        public Cursor cursor;
    }
}