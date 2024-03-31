using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class PlayerComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<PlayerComAndAuthoring>
        {
            public override void Bake(PlayerComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerAttributeMain>(entity);
                AddComponent<PlayerAtttributeDamageRelated>(entity);
                AddComponent(entity, new PlayerDamagedRecordCom
                {
                    damagedThisFrame = 0
                });
                //
                AddComponent<MainWeaponState>(entity);
            }
        }
    }

    public struct PlayerDamagedRecordCom : IComponentData
    {
        public float damagedThisFrame;
    }
    public struct PlayerAtttributeDamageRelated : IComponentData
    {
        public float4 MeleeRangedElementAttSpd;
        //Melee damage
        //Ranged damage
        //Element damage
        //Attack speed
        public float CriticalHitChance;
        public float DamagePercentage;
    }
    public struct PlayerAttributeMain : IComponentData
    {
        public float MaxHealthPoint;
        public float CurrentHealthPoint;
        public float HealthRegain;
        public float Armor;
        public float SpeedPercentage;
        public float Range;
    }
}