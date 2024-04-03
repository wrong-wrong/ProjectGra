using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyWeaponSO")]
    public class WeaponScriptableObjectConfig : ScriptableObject
    {
        //TODO : ImageIcon for Shop Display
        public Color color; // to temporarily replace Icon

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
        
    }

}