using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class WeaponAndAuthoringAddToss : MonoBehaviour
    {
        public class Baker : Baker<WeaponAndAuthoringAddToss> 
        {
            public override void Bake(WeaponAndAuthoringAddToss authoring)
            {
                
            }
        }
    }
    public struct WeaponTypeList : IBufferElementData
    {
        public Entity WeaponModel;
        public Entity SpawneePrefab;
        public float4 DamageBonus;
        public float BasicDamage;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;
        public float Cooldown;
        public float Range;
    }
    
    public struct MainWeaponState : IComponentData
    {
        public Entity WeaponModel;
        public Entity SpawneePrefab;
        public float4 DamageBonus;
        public float BasicDamage;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;
        public float Cooldown;
        public float Range;
        public float RealCooldown;
    }
}