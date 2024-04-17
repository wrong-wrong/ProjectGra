using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyWeaponSO")]
    public class WeaponScriptableObjectConfig : ScriptableObject
    {
        public bool IsMeleeWeapon;
        public bool IsMeleeSweep;
        public float SweepHalfWidth;
        public float MeleeForwardSpeed;

        [Header("UI only field")]
        public string WeaponName;
        public int BasePrice;
        //TODO : ImageIcon for Shop Display
        public Color color; // to temporarily replace Icon

        [Header("Common field")]
        public int WeaponIndex;

        public GameObject WeaponModel;
        public GameObject SpawneePrefabs;

        public float MeleeBonus;
        public float RangedBonus;
        public float ElementBonus;
        public float AttackSpeedBonus;

        public int BasicDamage;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;
        public float Cooldown;
        public float Range;

        public float3 WeaponPositionOffsetRelativeToCameraTarget;

        [Header("Weapon Category Config")]
        public List<int> WeaponCategoryList;

        
    }

}