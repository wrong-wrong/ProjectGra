using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyConfigAuthoringToSS : MonoBehaviour
    {
        [Header("NormalMeleeConfig")]
        public float MeleeFollowSpeed;
        public float MeleeAttackDistance;
        public float MeleeAttackCooldown;
        public float MeleeDeathCountdown;

        [Header("NormalSprintConfig")]
        public float NormalSprintFollowSpeed;
        public float NormalSprintAttackDistance;
        public float NormalSprintDeathCountdown;
        public float NormalSprintAttackCooldown;
        public float NormalSprintSprintSpeed;
        public float NormalSprintHitDistanceDuringSprint;
        public float NormalSprintLootChance;


        [Header("NormalRangedConfig")]
        public float NormalRangedFollowSpeed;
        public float NormalRangedAttackDistance;
        public float NormalRangedAttackCooldown;
        public float NormalRangedDeathCountdown;

        public float NormalRangedFleeDistance;
        public float NormalRangedFleeSpeed;

        public class Baker : Baker<EnemyConfigAuthoringToSS>
        {
            public override void Bake(EnemyConfigAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new NormalMeleeConfigCom
                {
                    FollowSpeed = authoring.MeleeFollowSpeed,
                    AttackDistance = authoring.MeleeAttackDistance,
                    AttackCooldown = authoring.MeleeAttackCooldown,
                    DeathCountdown = authoring.MeleeDeathCountdown
                });
                AddComponent(entity, new NormalSprintConfigCom
                {
                    FollowSpeed = authoring.NormalSprintFollowSpeed,
                    AttackDistance = authoring.NormalSprintAttackDistance,
                    AttackCooldown = authoring.NormalSprintAttackCooldown,
                    DeathCountdown = authoring.NormalSprintDeathCountdown,
                    SprintSpeed = authoring.NormalSprintSprintSpeed,
                    HitDistance = authoring.NormalSprintHitDistanceDuringSprint,
                    LootChance = authoring.NormalSprintLootChance,
                });
                AddComponent(entity, new NormalRangedConfigCom
                {
                    FollowSpeed = authoring.NormalRangedFollowSpeed,
                    AttackDistance = authoring.NormalRangedAttackDistance,
                    AttackCooldown = authoring.NormalRangedAttackCooldown,
                    DeathCountdown = authoring.NormalRangedDeathCountdown,

                    FleeDistance = authoring.NormalRangedFleeDistance,
                    FleeSpeed = authoring.NormalRangedFleeSpeed,
                });
            }
        }
    }
    public struct NormalRangedConfigCom : IComponentData
    {
        public float FollowSpeed;
        public float AttackDistance;
        public float AttackCooldown;
        public float DeathCountdown;

        public float FleeDistance;
        public float FleeSpeed;
    }
    public struct NormalSprintConfigCom : IComponentData
    {
        public float FollowSpeed;
        public float AttackDistance;
        public float AttackCooldown;
        public float SprintSpeed;
        public float DeathCountdown;
        public float HitDistance;
        public float LootChance;
    }

    public struct NormalMeleeConfigCom : IComponentData
    {
        public float FollowSpeed;
        public float AttackDistance;
        public float AttackCooldown;
        public float DeathCountdown;
        public float SprintSpeed;
    }
}