using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyConfigAuthoringToSS : MonoBehaviour
    {
        [Header("NormalMeleeConfig")]
        public GameObject NormalMeleePrefab;
        public int NormalMeleeAttackVal;
        public float NormalMeleeFollowSpeed;
        public float NormalMeleeAttackDistance;
        public float NormalMeleeAttackCooldown;
        public float NormalMeleeDeathCountdown;
        public float NormalMeleeLootChance;

        [Header("NormalSprintConfig")]
        public GameObject NormalSprintPrefab;
        public int NormalSprintAttackVal;
        public float NormalSprintFollowSpeed;
        public float NormalSprintAttackDistance;
        public float NormalSprintDeathCountdown;
        public float NormalSprintAttackCooldown;
        public float NormalSprintSprintSpeed;
        public float NormalSprintHitDistanceDuringSprint;
        public float NormalSprintLootChance;


        [Header("NormalRangedConfig")]
        public GameObject NormalRangedPrefab;
        public int NormalRangedAttackVal;
        public float NormalRangedFollowSpeed;
        public float NormalRangedAttackCooldown;
        public float NormalRangedDeathCountdown;
        public float NormalRangedLootChance;
        public float NormalRangedAttackDistance;
        public float NormalRangedFleeDistance;
        public float NormalRangedFleeSpeed;
        //public GameObject NormalSpawneePrefab;
        public float SpawneeSpeed;
        public float SpawneeTimer;
        [Header("EnemyEggConfig")]
        public GameObject EggPrefab;
        [Header("EliteShooterConfig")]
        public GameObject ElietSpawnee;

        [Header("EnemySummonerConfig")]
        //public float EnemySummonerDeathCountdown;
        public float EnemySummonerSpeed;
        public float EnemySummonerAttackCooldown;
        public float EnemySummonerFloatingRange;
        public float EnemySummonerFloatingCycleSpeed;
        public float EnemySummonerChasingSpeed;
        public float EnemySummonerFleeDistance;
        public float EnemySummonerChasingDistance;
        public float EnemySummonerExplodeDistance;
        public float EnemySummonerSummonDistance;
        [Header("Summoned Explosion System Config")]

        //scaling to set in every state,  basic scale should be previous scale,
        public float3 ScaleToSetInLaterThreeState;
        public float3 TimerToSetInLaterThreeState;

        public class Baker : Baker<EnemyConfigAuthoringToSS>
        {
            public override void Bake(EnemyConfigAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var NormalMeleePrefab = GetEntity(authoring.NormalMeleePrefab, TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<AllEnemyPrefabBuffer>(entity);
                buffer.Add(new AllEnemyPrefabBuffer { Prefab = NormalMeleePrefab });
                AddComponent(entity, new NormalMeleeConfigCom
                {
                    EnemyPrefab = NormalMeleePrefab,
                    AttackVal = authoring.NormalMeleeAttackVal,
                    FollowSpeed = authoring.NormalMeleeFollowSpeed,
                    AttackDistance = authoring.NormalMeleeAttackDistance,
                    AttackCooldown = authoring.NormalMeleeAttackCooldown,
                    DeathCountdown = authoring.NormalMeleeDeathCountdown,
                    LootChance = authoring.NormalMeleeLootChance,
                });
                buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.NormalSprintPrefab, TransformUsageFlags.Dynamic) });
                AddComponent(entity, new NormalSprintConfigCom
                {
                    EnemyPrefab = GetEntity(authoring.NormalSprintPrefab, TransformUsageFlags.Dynamic),
                    AttackVal = authoring.NormalRangedAttackVal,
                    FollowSpeed = authoring.NormalSprintFollowSpeed,
                    AttackDistance = authoring.NormalSprintAttackDistance,
                    AttackCooldown = authoring.NormalSprintAttackCooldown,
                    DeathCountdown = authoring.NormalSprintDeathCountdown,
                    SprintSpeed = authoring.NormalSprintSprintSpeed,
                    HitDistance = authoring.NormalSprintHitDistanceDuringSprint,
                    LootChance = authoring.NormalSprintLootChance,
                });
                buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.NormalRangedPrefab, TransformUsageFlags.Dynamic) });
                AddComponent(entity, new NormalRangedConfigCom
                {
                    //EnemyPrefab = GetEntity(authoring.NormalRangedPrefab, TransformUsageFlags.Dynamic),
                    AttackVal = authoring.NormalRangedAttackVal,
                    FollowSpeed = authoring.NormalRangedFollowSpeed,
                    AttackDistance = authoring.NormalRangedAttackDistance,
                    AttackCooldown = authoring.NormalRangedAttackCooldown,
                    DeathCountdown = authoring.NormalRangedDeathCountdown,
                    LootChance = authoring.NormalRangedLootChance,

                    FleeDistance = authoring.NormalRangedFleeDistance,
                    FleeSpeed = authoring.NormalRangedFleeSpeed,

                    //SpawneePrefab = GetEntity(authoring.NormalSpawneePrefab, TransformUsageFlags.Dynamic),
                    SpawneeSpeed = authoring.SpawneeSpeed,
                    SpawneeTimer = authoring.SpawneeTimer,
                });

                //because no data need to set to the EggSystem, there is no need to creat a component
                buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.EggPrefab, TransformUsageFlags.Dynamic) });

                // For Elite Sprint and Bullet
                AddComponent(entity, new EliteShooterConfigCom { EliteSpawnee = GetEntity(authoring.ElietSpawnee, TransformUsageFlags.Dynamic) });
                Debug.Log("EnemyConfigAuthoring - PrefabBuffer.Length:" + buffer.Length);

                // For summoned explosion
                AddComponent(entity, new SummonedExplosionSystemConfigCom
                {
                    ScaleToSetInLaterThreeState = authoring.ScaleToSetInLaterThreeState,
                    TimerToSetInLaterThreeState = authoring.TimerToSetInLaterThreeState,
                });

                // For summoner
                AddComponent(entity, new EnemySummonerConfigCom
                {
                    EnemySummonerAttackCooldown = authoring.EnemySummonerAttackCooldown,
                    EnemySummonerChasingDistance = authoring.EnemySummonerChasingDistance,
                    EnemySummonerFleeDistance = authoring.EnemySummonerFleeDistance,
                    EnemySummonerSummonDistance = authoring.EnemySummonerSummonDistance,
                    EnemySummonerChasingSpeed = authoring.EnemySummonerChasingSpeed,
                    EnemySummonerFloatingCycleSpeed = authoring.EnemySummonerFloatingCycleSpeed,
                    EnemySummonerFloatingRange = authoring.EnemySummonerFloatingRange,
                    EnemySummonerSpeed = authoring.EnemySummonerSpeed,
                    EnemySummonerExplodeDistance = authoring.EnemySummonerExplodeDistance,
                });
            }
        }
    }
    [InternalBufferCapacity(0)]
    public struct AllEnemyPrefabBuffer : IBufferElementData
    {
        public Entity Prefab;
    }
    public struct NormalRangedConfigCom : IComponentData
    {
        //public Entity EnemyPrefab;
        //NormalCom
        public int AttackVal;
        public float FollowSpeed;
        public float AttackCooldown;
        public float DeathCountdown;
        public float LootChance;


        public float AttackDistance;
        public float FleeDistance;
        public float FleeSpeed;

        //public Entity SpawneePrefab;
        public float SpawneeSpeed;
        public float SpawneeTimer;


    }
    public struct NormalSprintConfigCom : IComponentData
    {
        public Entity EnemyPrefab;
        public int AttackVal;
        public float FollowSpeed;
        public float AttackCooldown;
        public float DeathCountdown;
        public float LootChance;

        public float AttackDistance;
        public float SprintSpeed;
        public float HitDistance;
    }

    public struct EnemySummonerConfigCom : IComponentData
    {
        public float EnemySummonerSpeed;
        public float EnemySummonerAttackCooldown;
        public float EnemySummonerFloatingRange;
        public float EnemySummonerFloatingCycleSpeed;
        public float EnemySummonerChasingSpeed;
        public float EnemySummonerFleeDistance;
        public float EnemySummonerChasingDistance;
        public float EnemySummonerExplodeDistance;
        public float EnemySummonerSummonDistance;
    }

    public struct NormalMeleeConfigCom : IComponentData
    {
        public Entity EnemyPrefab;
        public int AttackVal;
        public float FollowSpeed;
        public float AttackCooldown;
        public float DeathCountdown;
        public float LootChance;

        public float AttackDistance;
        public float SprintSpeed;

    }

    public struct EliteShooterConfigCom : IComponentData
    {
        public Entity EliteSpawnee;
    }

    public struct SummonedExplosionSystemConfigCom : IComponentData
    {
        //for flying state
        //public float StartSpeed;
        //public float SpeedVariation;
        //public float StopSpeed;

        //scaling to set in every state,  basic scale should be previous scale,
        public float3 ScaleToSetInLaterThreeState;
        public float3 TimerToSetInLaterThreeState;
    }
}