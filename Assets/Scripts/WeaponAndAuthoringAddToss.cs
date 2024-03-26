using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class WeaponAndAuthoringAddToss : MonoBehaviour
    {

        [SerializeField] List<WeaponScriptableObjectConfig> WeaponSOList;
        public class Baker : Baker<WeaponAndAuthoringAddToss> 
        {
            public override void Bake(WeaponAndAuthoringAddToss authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<WeaponTypeList>(entity);
                for(int i = 0, count = authoring.WeaponSOList.Count; i < count; ++i)
                {
                    var so = authoring.WeaponSOList[i];
                    buffer.Add(new WeaponTypeList
                    {
                        WeaponModel = GetEntity(so.WeaponModel, TransformUsageFlags.Dynamic),
                        SpawneePrefab = GetEntity(so.SpawneePrefabs, TransformUsageFlags.Dynamic),
                        DamageBonus = new float4
                        {
                            x = so.MeleeBonus,
                            y = so.RangedBonus,
                            z = so.ElementBonus,
                            w = so.AttackSpeedBonus
                        },
                        BasicDamage = so.BasicDamage,
                        WeaponCriticalHitChance = so.WeaponCriticalHitChance,
                        WeaponCriticalHitRatio = so.WeaponCriticalHitRatio,
                        Cooldown = so.Cooldown,
                        Range = so.Range,
                        WeaponPositionOffset = so.WeaponPositionOffsetRelativeToCameraTarget,
                    });
                }
            }
        }
    }
    [InternalBufferCapacity(0)]
    public struct WeaponTypeList : IBufferElementData
    {
        public Entity WeaponModel;
        public Entity SpawneePrefab;

        public float4 DamageBonus;
        public int BasicDamage;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;

        public float Cooldown;
        public float Range;
        public float3 WeaponPositionOffset;
    }
    
    public struct MainWeaponState : IComponentData
    {
        public Entity WeaponModel;
        public Entity SpawneePrefab;

        public float4 DamageBonus;
        public int BasicDamage;
        public float WeaponCriticalHitChance;
        public float WeaponCriticalHitRatio;

        public float Cooldown;
        public float Range;
        public float3 WeaponPositionOffset;

        public float RealCooldown;

    }
}