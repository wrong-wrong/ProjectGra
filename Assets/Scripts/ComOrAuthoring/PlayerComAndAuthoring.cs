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
                AddComponent(entity, new EntityHealthPoint { HealthPoint = 100 });
                AddComponent(entity, new PlayerItemCount { Count = 0 });
                AddComponent(entity, new PlayerMaterialCount { Count = 0 });
                AddComponent(entity, new PlayerExperience { Exp = 0 });
                AddComponent<EntityStateMachine>(entity);

                //
                AddComponent<MainWeapon>(entity, new MainWeapon { WeaponIndex = -1 });
                var wpBuffer = AddBuffer<AutoWeaponBuffer>(entity);
                for(int i = 0; i < 3; i++)
                {
                    wpBuffer.Add(new AutoWeaponBuffer {  WeaponIndex = -1 });
                }
                //var wpStateMachineBuffer = AddBuffer<AutoWeaponStateMachineBuffer>(entity);
                //for(int i = 0; i < 3; i++)
                //{
                //    wpStateMachineBuffer.Add(new AutoWeaponStateMachineBuffer { CurrentState = AutoWeaponState.None });
                //}
                AddComponent(entity, new PlayerOverlapRadius { Value = 0 });
                AddComponent(entity, new FlashingCom { AccumulateTimer = 1f, Duration = 1, CycleTime = 1 });
                SetComponentEnabled<FlashingCom>(entity, false);

                AddComponent<EntityKnockBackCom>(entity);

                AddBuffer<PlayerDamagedRecordBuffer>(entity);
                AddComponent<PlayerSuccessfulAttackCount>(entity);
            }
        }
    }

    [InternalBufferCapacity(0)]
    public struct PlayerDamagedRecordBuffer : IBufferElementData
    {
        public int Value;
    }
    public struct PlayerSuccessfulAttackCount : IComponentData
    {
        public int Value;
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
        public float Armor;
        public float SpeedPercentage;
        public float Range;
        public float LifeSteal;
        public float Dodge;
    }
    public struct PlayerOverlapRadius : IComponentData
    {
        public float Value;
    }
    public struct PlayerExperience : IComponentData
    {
        public int Exp;
        //public int Level;
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