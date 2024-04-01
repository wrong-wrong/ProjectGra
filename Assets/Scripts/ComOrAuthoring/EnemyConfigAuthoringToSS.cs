using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyConfigAuthoringToSS : MonoBehaviour
    {
        [Header("NormalMeleeConfig")]
        public int MeleeAttackVal;
        public float MeleeFollowSpeed;
        public float MeleeAttackDistance;
        public float MeleeAttackCooldown;
        public float MeleeDeathCountdown;
        public float MeleeLootChance;

        [Header("NormalSprintConfig")]
        public int NormalSprintAttackVal;
        public float NormalSprintFollowSpeed;
        public float NormalSprintAttackDistance;
        public float NormalSprintDeathCountdown;
        public float NormalSprintAttackCooldown;
        public float NormalSprintSprintSpeed;
        public float NormalSprintHitDistanceDuringSprint;
        public float NormalSprintLootChance;


        [Header("NormalRangedConfig")]
        public int NormalRangedAttackVal;
        public float NormalRangedFollowSpeed;
        public float NormalRangedAttackCooldown;
        public float NormalRangedDeathCountdown;
        public float NormalRangedLootChance;

        public float NormalRangedAttackDistance;
        public float NormalRangedFleeDistance;
        public float NormalRangedFleeSpeed;

        public GameObject NormalRangedSpawneePrefab;
        public float SpawneeSpeed;
        public float SpawneeTimer;

        public class Baker : Baker<EnemyConfigAuthoringToSS>
        {
            public override void Bake(EnemyConfigAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new NormalMeleeConfigCom
                {
                    AttackVal = authoring.MeleeAttackVal,
                    FollowSpeed = authoring.MeleeFollowSpeed,
                    AttackDistance = authoring.MeleeAttackDistance,
                    AttackCooldown = authoring.MeleeAttackCooldown,
                    DeathCountdown = authoring.MeleeDeathCountdown,
                    LootChance = authoring.MeleeLootChance,
                });
                AddComponent(entity, new NormalSprintConfigCom
                {
                    AttackVal = authoring.NormalRangedAttackVal,
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
                    AttackVal = authoring.NormalRangedAttackVal,
                    FollowSpeed = authoring.NormalRangedFollowSpeed,
                    AttackDistance = authoring.NormalRangedAttackDistance,
                    AttackCooldown = authoring.NormalRangedAttackCooldown,
                    DeathCountdown = authoring.NormalRangedDeathCountdown,
                    LootChance = authoring.NormalRangedLootChance,

                    FleeDistance = authoring.NormalRangedFleeDistance,
                    FleeSpeed = authoring.NormalRangedFleeSpeed,

                    SpawneePrefab = GetEntity(authoring.NormalRangedSpawneePrefab, TransformUsageFlags.Dynamic),
                    SpawneeSpeed = authoring.SpawneeSpeed,
                    SpawneeTimer = authoring.SpawneeTimer,
                });
            }
        }
    }
    public struct NormalRangedConfigCom : IComponentData
    {
        //NormalCom
        public int AttackVal;
        public float FollowSpeed;
        public float AttackCooldown;
        public float DeathCountdown;
        public float LootChance;


        public float AttackDistance;
        public float FleeDistance;
        public float FleeSpeed;

        public Entity SpawneePrefab;
        public float SpawneeSpeed;
        public float SpawneeTimer;


    }
    public struct NormalSprintConfigCom : IComponentData
    {
        public int AttackVal;
        public float FollowSpeed;
        public float AttackCooldown;
        public float DeathCountdown;
        public float LootChance;

        public float AttackDistance;
        public float SprintSpeed;
        public float HitDistance;
    }

    public struct NormalMeleeConfigCom : IComponentData
    {
        public int AttackVal;
        public float FollowSpeed;
        public float AttackCooldown;
        public float DeathCountdown;
        public float LootChance;

        public float AttackDistance;
        public float SprintSpeed;


    }
}