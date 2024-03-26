using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public struct MoveAndLookInput : IComponentData
    {
        public float2 moveVal;
        public float2 lookVal;
    }
    public struct SprintInput : IComponentData, IEnableableComponent { }
    public struct ShootInput : IComponentData, IEnableableComponent { }
    public struct PlayerTag : IComponentData { }
    public class CameraTargetReference : IComponentData
    {
        public Transform ghostPlayer;
        public Transform cameraTarget;
    }

    //split the Attribute into different Component later
    //public struct PlayerAttribute : IComponentData
    //{
    //    public float MaxHealthPoint;
    //    public float HealthRegain;
    //    public float CriticalHitChance;
    //    public float AttackSpeed;
    //    public float PhysicalResistance;
    //    public float LifeStealPercentage;
    //    public float DamagePercentage;
    //}
    public struct PlayerAtttributeDamageRelated : IComponentData
    {
        public float4 MeleeRangedElementAttSpd;
        //Melee damage
        //Ranged damage
        //Element damage
        //Attack speed

        public float CriticalHitChange;
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
    public class ControllerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<ControllerAuthoring>
        {
            public override void Bake(ControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent<MoveAndLookInput>(entity);
                AddComponent<SprintInput>(entity);
                AddComponent<ShootInput>(entity);
                AddComponent<PlayerAttributeMain>(entity);
                AddComponent<PlayerAtttributeDamageRelated>(entity);

                //
                AddComponent<MainWeaponState>(entity);
            }
        }
    }

}
