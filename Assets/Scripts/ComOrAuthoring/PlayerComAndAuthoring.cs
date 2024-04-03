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
                //AddComponent(entity, new PlayerDamagedRecordCom{damagedThisFrame = 0});
                AddComponent(entity, new EntityHealthPoint { HealthPoint = 100 });
                AddComponent(entity, new PlayerMaterialCount { Count = 0 });
                AddComponent(entity, new PlayerExperience { Value = 0 });
                AddComponent<EntityStateMachine>(entity);
                //
                var buffer = AddBuffer<AutoWeaponState>(entity);
                AddComponent<MainWeaponState>(entity, new MainWeaponState { WeaponIndex = -1});
                for(int i = 0; i < 3; i++)
                {
                    buffer.Add(new AutoWeaponState {  WeaponIndex = -1 });
                }
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
    public struct PlayerExperience : IComponentData
    {
        public float Value;
    }
    public struct PlayerMaterialCount : IComponentData
    {
        public int Count;
    }
}