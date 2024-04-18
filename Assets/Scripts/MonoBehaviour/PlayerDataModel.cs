using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class PlayerDataModel : MonoBehaviour
    {
        //0MaxHp 
        //1HpRegain
        //2Armor
        //3Speed %
        //4Range 
        //5CriticalHitChance %
        //6Damage %
        //7Melee 
        //8Ranged 
        //9Element 
        //10AttackSpeed %

        //11 life steal %
        //12 dodge %
        //Should not store decimals here
        //public List<string> AttributeNameList;

        public List<int> attributeValueList;
        public int playerMaterialCount;

        public static PlayerDataModel Instance;
        public Action<int> OnMaterialChanged;
        public Action OnPlayerAttributeChanged;
        public void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
            }
            Instance = this;
            attributeValueList.Capacity = 13;
            for(int i = 0; i < 13; ++i)
            {
                attributeValueList.Add(0);
            }
            //Debug.Log("PlayerDataModel - Awake");
        }

        public float4 GetDamageBonus()
        {
            return new float4(attributeValueList[7], attributeValueList[8], attributeValueList[9], attributeValueList[10]);
        }
        public float GetRange()
        {
            return attributeValueList[4];
        }
        public float GetCritHitChance()
        {
            return attributeValueList[5];
        }
        public float GetDamage()
        {
            return attributeValueList[6];
        }
        public float GetAttackSpeed()
        {
            return attributeValueList[10];
        }
        public void SetAttributeWithStruct(PlayerAttributeMain mainAttribute, PlayerAtttributeDamageRelated damageAttribute)
        {
            attributeValueList[0] = mainAttribute.MaxHealthPoint;
            attributeValueList[1] = mainAttribute.HealthRegain;
            attributeValueList[2] = (int)(mainAttribute.Armor * 100 + 0.0000001);
            attributeValueList[3] = (int)(mainAttribute.SpeedPercentage * 100 + 0.0000001);
            attributeValueList[4] = (int)mainAttribute.Range;

            attributeValueList[5] = (int)(damageAttribute.CriticalHitChance * 100 + 0.0000001);
            attributeValueList[6] = (int)(damageAttribute.DamagePercentage * 100 + 0.0000001);
            attributeValueList[7] = (int)(damageAttribute.MeleeRangedElementAttSpd.x * 100 + 0.0000001);
            attributeValueList[8] = (int)(damageAttribute.MeleeRangedElementAttSpd.y * 100 + 0.0000001);
            attributeValueList[9] = (int)(damageAttribute.MeleeRangedElementAttSpd.z * 100 + 0.0000001);
            attributeValueList[10] = (int)(damageAttribute.MeleeRangedElementAttSpd.w * 100 + 0.0000001);

            //life steal
            attributeValueList[11] = (int)(mainAttribute.LifeSteal * 100 + 0.0000001);
            // dodge
            attributeValueList[12] = (int)(mainAttribute.Dodge * 100 + 0.0000001);
            OnPlayerAttributeChanged?.Invoke();
        }
        public void AddAttributeValWith(int attributeIdx, int val)
        {
            attributeValueList[attributeIdx] += val;
            OnPlayerAttributeChanged?.Invoke();
        }
        public void SetAttributeValWith(int attributeIdx, int val)
        {
            attributeValueList[attributeIdx] = val;
            OnPlayerAttributeChanged?.Invoke();
        }

        public void AddMaterialValWith(int addNumber)
        {
            playerMaterialCount += addNumber;
            OnMaterialChanged?.Invoke(playerMaterialCount);
        }
        public void SetMaterialValWith(int setNumber)
        {
            playerMaterialCount = setNumber;
            OnMaterialChanged?.Invoke(playerMaterialCount);
        }

        public int GetMaxHealthPoint()
        {
            return (int)attributeValueList[0];
        }

        public PlayerAtttributeDamageRelated GetDamageAttribute()
        {
            return new PlayerAtttributeDamageRelated
            {
                CriticalHitChance = attributeValueList[5]/100f,
                DamagePercentage = attributeValueList[6]/100f,
                MeleeRangedElementAttSpd = new float4(attributeValueList[7]/100f,
                attributeValueList[8] / 100f, attributeValueList[9]/100f,
                attributeValueList[10]/100f)
            };
        }

        public PlayerAttributeMain GetMainAttribute()
        {
            return new PlayerAttributeMain
            {
                MaxHealthPoint = (int)attributeValueList[0],
                HealthRegain = (int)attributeValueList[1],
                Armor = attributeValueList[2] / 100f,
                SpeedPercentage = attributeValueList[3]/100f,
                Range = attributeValueList[4],
                LifeSteal = attributeValueList[11] / 100f,
                Dodge = attributeValueList[12] / 100f,
            };
        }

        internal int GetMaxExp()
        {
            return 15;
        }
    }
}