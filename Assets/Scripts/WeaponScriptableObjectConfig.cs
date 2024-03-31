using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyWeaponSO")]
    public class WeaponScriptableObjectConfig : ScriptableObject
    {
        public GameObject WeaponModel;
        public GameObject SpawneePrefabs;

        //TODO : ImageIcon for Shop Display
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