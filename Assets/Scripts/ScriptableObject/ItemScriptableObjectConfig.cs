using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyItemSO")]
    public class ItemScriptableObjectConfig : ScriptableObject
    {
        public int ItemIdx;
        public int ItemLevel;
        public int ItemBasePrice;
        public string ItemName;
        public Sprite ItemSprite;
        public List<int> AffectedAttributeIdx;
        //0MaxHp 
        //1HpRegain
        //2Armor
        //3Speed %
        //4Range %
        //5CriticalHitChance %
        //6Damage %
        //7Melee %
        //8Ranged %
        //9Element %
        //10AttackSpeed %
        public List<int> BonusedValueList;
    }
}