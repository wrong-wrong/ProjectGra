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
                AddComponent(entity, new EntityHealthPoint { HealthPoint = 100 });
                AddComponent(entity, new PlayerMaterialCount { Count = 0 });
                AddComponent<EntityStateMachine>(entity);
                //
                AddComponent<MainWeaponState>(entity);
            }
        }
    }

    public struct PlayerDamagedRecordCom : IComponentData
    {
        public int damagedThisFrame;
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
        public float HealthRegain;
        public float Armor;
        public float SpeedPercentage;
        public float Range;
    }

    public struct PlayerMaterialCount : IComponentData
    {
        public int Count;
    }
}