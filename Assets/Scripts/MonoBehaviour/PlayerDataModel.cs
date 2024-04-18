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

        //11 life steal
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
            attributeValueList.Capacity = 11;
            for(int i = 0; i < 11; ++i)
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
            attributeValueList[2] = mainAttribute.Armor;
            attributeValueList[3] = (int)mainAttribute.SpeedPercentage * 100;
            attributeValueList[4] = (int)mainAttribute.Range;

            attributeValueList[5] = (int)(damageAttribute.CriticalHitChance * 100);
            attributeValueList[6] = (int)(damageAttribute.DamagePercentage * 100);
            attributeValueList[7] = (int)(damageAttribute.MeleeRangedElementAttSpd.x);
            attributeValueList[8] = (int)(damageAttribute.MeleeRangedElementAttSpd.y);
            attributeValueList[9] = (int)(damageAttribute.MeleeRangedElementAttSpd.z);
            attributeValueList[10] = (int)(damageAttribute.MeleeRangedElementAttSpd.w);
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
                MeleeRangedElementAttSpd = new float4(attributeValueList[7],
                attributeValueList[8], attributeValueList[9],
                attributeValueList[10])
            };
        }

        public PlayerAttributeMain GetMainAttribute()
        {
            return new PlayerAttributeMain
            {
                MaxHealthPoint = (int)attributeValueList[0],
                HealthRegain = (int)attributeValueList[1],
                Armor = (int)attributeValueList[2],
                SpeedPercentage = attributeValueList[3]/100,
                Range = attributeValueList[4],
            };
        }

        internal int GetMaxExp()
        {
            return 15;
        }
    }
}