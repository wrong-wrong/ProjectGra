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
                AddComponent(entity, new PlayerItemCount { Count = 0 });
                AddComponent(entity, new PlayerMaterialCount { Count = 0 });
                AddComponent(entity, new PlayerExperienceAndLevel { Exp = 0 });
                AddComponent<EntityStateMachine>(entity);
                
                //
                var buffer = AddBuffer<AutoWeaponState>(entity);
                AddComponent<MainWeaponState>(entity, new MainWeaponState { WeaponIndex = -1});
                for(int i = 0; i < 3; i++)
                {
                    buffer.Add(new AutoWeaponState {  WeaponIndex = -1 });
                }
                AddComponent(entity, new PlayerOverlapRadius { Value = 0 });
            }
        }
    }

    public struct PlayerDamagedRecordCom : IComponentData
    {
        public int damagedThisFrame;
    }
    public struct PlayerAtttributeDamageRelated : IComponentData
    {
        public float CriticalHitChance;
        public float DamagePercentage;
        public float4 MeleeRangedElementAttSpd;
        //Melee damage
        //Ranged damage
        //Element damage
        //Attack speed
    }
    public struct PlayerAttributeMain : IComponentData
    {
        public int MaxHealthPoint;
        public int HealthRegain;
        public int Armor;
        public float SpeedPercentage;
        public float Range;
    }
    public struct PlayerOverlapRadius : IComponentData
    {
        public float Value;
    }
    public struct PlayerExperienceAndLevel : IComponentData
    {
        public float Exp;
        public int Level;
        public int LevelUpThisWave;
    }
    public struct PlayerMaterialCount : IComponentData
    {
        public int Count;
    }
    public struct PlayerItemCount : IComponentData
    {
        public int Count;
    }
}