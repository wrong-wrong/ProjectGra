using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyConfigAuthoringToSS : MonoBehaviour
    {
        [SerializeField] List<EnemyBasicAttributeScriptableObjectConfig> EnemySOList;
        #region elite config
        [Header("ElitePrefab")]
        public GameObject EliteSprintAndShootPrefab;
        public GameObject EliteEggAndShootPrefab;
        public GameObject EliteShooterPrefab;

        [Header("EliteShooterConfig")]
        public float EliteShooterStageOneSpeed;
        public float EliteShooterStageTwoSpeed;
        public float EliteShooterStageOneInSkillShootingInterval;
        public float EliteShooterStageTwoInSkillShootingInterval;
        public int EliteShooterStageOneSkillShootCount;
        public int EliteShooterStageTwoSkillShootCount;
        //public GameObject ElietShooterScalingSpawnee;

        [Header("EliteEggAndShootConfig")]
        public float EliteEggAndShootSpeed;
        public float EliteEggAndShootStageOneInSkillShootingInterval;
        public float EliteEggAndShootSpawnEggSkillSpawningInterval;
        public int EliteEggAndShootStageOneSkillShootCount;
        public int EliteEggAndShootSpawnEggSkillspawnCount;

        [Header("EliteSprintAndShootConfig")]
        public float3 EliteSprintAndShootRightSpawnPosOffset;
        public float3 EliteSprintAndShootLeftSpawnPosOffset;
        public float EliteSprintAndShootSpawnYAxisRotation;
        public float EliteSprintAndShootSkillCooldown;
        public int EliteSprintAndShootSpawnCount;
        public float EliteSprintAndShootSpawnInterval;
        public float EliteSprintAndShootSpeed;
        public float EliteSprintAndShootSprintSpeed;
        public float EliteSprintAndShootSprintDistance;
        public float EliteSprintAndShootCollideDistance;
        public int EliteSprintAndShootSprintDamage;
        #endregion

        [Header("NormalMeleeConfig")]
        //public GameObject NormalMeleePrefab;
        //public EnemyBasicAttributeScriptableObjectConfig NormalMeleeAttributeSO;
        public float NormalMeleeAttackDistance;
        public float NormalMeleeAttackCooldown;
        public float NormalMeleeDeathCountdown;

        [Header("NormalRangedConfig")]
        //public GameObject NormalRangedPrefab;
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

        [Header("NormalSprintConfig")]
        //public GameObject NormalSprintPrefab;
        public int NormalSprintAttackVal;
        public float NormalSprintFollowSpeed;
        public float NormalSprintAttackDistance;
        public float NormalSprintDeathCountdown;
        public float NormalSprintAttackCooldown;
        public float NormalSprintSprintSpeed;
        public float NormalSprintHitDistanceDuringSprint;
        public float NormalSprintLootChance;
        public Color NormalSprintFlashColor;
        public float NormalSprintSprintWaitTimerSetting;

        [Header("EnemyEggConfig")]
        //public GameObject EggPrefab;

        [Header("EnemySummonerConfig")]
        //public float EnemySummonerDeathCountdown;
        //public GameObject EnemySummonerPrefab;
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


        public void Reset()
        {
            Debug.Log("Unity function Reset called");
        }
        public class Baker : Baker<EnemyConfigAuthoringToSS>
        {
            public override void Bake(EnemyConfigAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<AllEnemyPrefabBuffer>(entity);
                var SOList = authoring.EnemySOList;
                Debug.Log("Baking SOList.Count : " + SOList.Count);
                var waveNewEnemyBuffer = AddBuffer<WaveNewEnemyBuffer>(entity);
                for(int i = 0; i < 19; ++i)
                {
                    waveNewEnemyBuffer.Add(new WaveNewEnemyBuffer { Value = 0 });
                }
                int[] tmpArr = new int[SOList.Count];
                string[] tmpStrArr = new string[SOList.Count];
                // create a tmp list;
                // process EnemySO
                //      Add to AllEnemyBuffer;  ++WaveNewEnemyList[AppearCodingWave];   tmpList.Add(AppCodingWave)
                for (int i = 0, n = SOList.Count; i < n; i++)
                {
                    buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(SOList[i].EnemyPrefab, TransformUsageFlags.Dynamic) });
                    tmpArr[i] = SOList[i].AppearCodingWave;
                    tmpStrArr[i] = SOList[i].EnemyTypeName;
                    ++waveNewEnemyBuffer.ElementAt(SOList[i].AppearCodingWave).Value;
                }
                // sort AllEnemyBuffer According to AppearingCodingWave according to tmpList

                for(int i = 1; i < SOList.Count; ++i)
                {
                    int baseValue = tmpArr[i];
                    Entity baseEntity = buffer[i].Prefab;
                    string baseName = tmpStrArr[i];
                    int j = i - 1;
                    while(j >= 0 && baseValue < tmpArr[j] )
                    {
                        buffer.ElementAt(j + 1).Prefab = buffer[j].Prefab;
                        tmpArr[j + 1] = tmpArr[j];
                        tmpStrArr[j + 1] = tmpStrArr[j];
                        --j;
                    }
                    buffer.ElementAt(j + 1).Prefab = baseEntity;
                    tmpArr[j + 1] = baseValue;
                    tmpStrArr[j + 1] = baseName;
                }

                for(int i = 0; i < SOList.Count; i++)
                {
                    Debug.Log(tmpStrArr[i] + " - Idx :" + i);
                }


                //buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.NormalMeleePrefab, TransformUsageFlags.Dynamic) });
                //buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.NormalRangedPrefab, TransformUsageFlags.Dynamic) });
                //buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.NormalSprintPrefab, TransformUsageFlags.Dynamic) });
                //buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.EggPrefab, TransformUsageFlags.Dynamic) });
                //buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.EnemySummonerPrefab, TransformUsageFlags.Dynamic) });

                buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.EliteEggAndShootPrefab, TransformUsageFlags.Dynamic) });
                buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.EliteSprintAndShootPrefab, TransformUsageFlags.Dynamic) });
                buffer.Add(new AllEnemyPrefabBuffer { Prefab = GetEntity(authoring.EliteShooterPrefab, TransformUsageFlags.Dynamic) });
                Debug.Log("EnemyConfigAuthoring - PrefabBuffer.Length:" + buffer.Length);

                EnemyBasicAttributeScriptableObjectConfig SO;
                SO = SOList[0];
                AddComponent(entity, new NormalMeleeConfigCom
                {
                    AttackDistance = authoring.NormalMeleeAttackDistance,
                    AttackCooldown = authoring.NormalMeleeAttackCooldown,
                    DeathCountdown = authoring.NormalMeleeDeathCountdown,
                    BasicAttribute = new EnemyBasicAttribute
                    {
                        HealthPoint = SO.HealthPoint,
                        HpIncreasePerWave = SO.HPIncreasePerWave,
                        Damage = SO.Damage,
                        DmgIncreasePerWave = SO.DmgIncreasePerWave,
                        Speed = SO.Speed,
                        MaterialsDropped = SO.MaterialsDropped,
                        LootCrateDropRate = SO.LootCrateDropRate,
                        ConsumableDropate = SO.ConsumableDropRate
                    }
                });
                SO = SOList[1];

                AddComponent(entity, new NormalRangedConfigCom
                {
                    //EnemyPrefab = GetEntity(authoring.NormalRangedPrefab, TransformUsageFlags.Dynamic),
                    AttackDistance = authoring.NormalRangedAttackDistance,
                    AttackCooldown = authoring.NormalRangedAttackCooldown,
                    DeathCountdown = authoring.NormalRangedDeathCountdown,
                    FleeDistance = authoring.NormalRangedFleeDistance,
                    FleeSpeed = authoring.NormalRangedFleeSpeed,
                    //SpawneePrefab = GetEntity(authoring.NormalSpawneePrefab, TransformUsageFlags.Dynamic),
                    SpawneeSpeed = authoring.SpawneeSpeed,
                    SpawneeTimer = authoring.SpawneeTimer,
                    BasicAttribute = new EnemyBasicAttribute
                    {
                        HealthPoint = SO.HealthPoint,
                        HpIncreasePerWave = SO.HPIncreasePerWave,
                        Damage = SO.Damage,
                        DmgIncreasePerWave = SO.DmgIncreasePerWave,
                        Speed = SO.Speed,
                        MaterialsDropped = SO.MaterialsDropped,
                        LootCrateDropRate = SO.LootCrateDropRate,
                        ConsumableDropate = SO.ConsumableDropRate
                    }
                });
                SO = SOList[2];
                AddComponent(entity, new NormalSprintConfigCom
                {
                    AttackDistance = authoring.NormalSprintAttackDistance,
                    AttackCooldown = authoring.NormalSprintAttackCooldown,
                    DeathCountdown = authoring.NormalSprintDeathCountdown,
                    SprintSpeed = authoring.NormalSprintSprintSpeed,
                    HitDistance = authoring.NormalSprintHitDistanceDuringSprint,
                    FlashColorDifference = new float3(1 - authoring.NormalSprintFlashColor.r, 1 - authoring.NormalSprintFlashColor.g, 1 - authoring.NormalSprintFlashColor.b),
                    SprintWaitTimerSetting = authoring.NormalSprintSprintWaitTimerSetting,
                    BasicAttribute = new EnemyBasicAttribute
                    {
                        HealthPoint = SO.HealthPoint,
                        HpIncreasePerWave = SO.HPIncreasePerWave,
                        Damage = SO.Damage,
                        DmgIncreasePerWave = SO.DmgIncreasePerWave,
                        Speed = SO.Speed,
                        MaterialsDropped = SO.MaterialsDropped,
                        LootCrateDropRate = SO.LootCrateDropRate,
                        ConsumableDropate = SO.ConsumableDropRate
                    }
                });
                // For egg
                SO = SOList[3];
                AddComponent(entity, new EnemyEggConfigCom
                {
                    BasicAttribute = new EnemyBasicAttribute
                    {
                        HealthPoint = SO.HealthPoint,
                        HpIncreasePerWave = SO.HPIncreasePerWave,
                        Damage = SO.Damage,
                        DmgIncreasePerWave = SO.DmgIncreasePerWave,
                        Speed = SO.Speed,
                        MaterialsDropped = SO.MaterialsDropped,
                        LootCrateDropRate = SO.LootCrateDropRate,
                        ConsumableDropate = SO.ConsumableDropRate
                    }
                });

                SO = SOList[4];
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
                    EnemySummonerExplodeDistance = authoring.EnemySummonerExplodeDistance,
                    BasicAttribute = new EnemyBasicAttribute
                    {
                        HealthPoint = SO.HealthPoint,
                        HpIncreasePerWave = SO.HPIncreasePerWave,
                        Damage = SO.Damage,
                        DmgIncreasePerWave = SO.DmgIncreasePerWave,
                        Speed = SO.Speed,
                        MaterialsDropped = SO.MaterialsDropped,
                        LootCrateDropRate = SO.LootCrateDropRate,
                        ConsumableDropate = SO.ConsumableDropRate
                    }
                });

                // For summoned explosion
                AddComponent(entity, new SummonedExplosionSystemConfigCom
                {
                    ScaleToSetInLaterThreeState = authoring.ScaleToSetInLaterThreeState,
                    TimerToSetInLaterThreeState = authoring.TimerToSetInLaterThreeState,
                });

                //because no data need to set to the EggSystem, there is no need to creat a component

                // For EliteSprintAndShoot
                AddComponent(entity, new EliteSprintAndShootConfigCom
                {
                    EliteSprintAndShootLeftSpawnPosOffset = authoring.EliteSprintAndShootLeftSpawnPosOffset,
                    EliteSprintAndShootRightSpawnPosOffset = authoring.EliteSprintAndShootRightSpawnPosOffset,
                    EliteSprintAndShootSpawnCount = authoring.EliteSprintAndShootSpawnCount,
                    EliteSprintAndShootSpawnInterval = authoring.EliteSprintAndShootSpawnInterval,
                    EliteSprintAndShootSpawnYAxisRotation = authoring.EliteSprintAndShootSpawnYAxisRotation,
                    EliteSprintAndShootSpeed = authoring.EliteSprintAndShootSpeed,
                    EliteSprintAndShootSprintDistance = authoring.EliteSprintAndShootSprintDistance,
                    EliteSprintAndShootSprintSpeed = authoring.EliteSprintAndShootSprintSpeed,
                    EliteSprintAndShootCollideDistance = authoring.EliteSprintAndShootCollideDistance,
                    EliteSprintAndShootSkillCooldown = authoring.EliteSprintAndShootSkillCooldown,
                    EliteSprintAndShootSprintDamage = authoring.EliteSprintAndShootSprintDamage,

                });

                // For EliteEggAndShoot
                AddComponent(entity, new EliteEggAndShootConfig
                {
                    SpawnEggSkillspawnCount = authoring.EliteEggAndShootSpawnEggSkillspawnCount,
                    SpawnEggSkillSpawningInterval = authoring.EliteEggAndShootSpawnEggSkillSpawningInterval,
                    Speed = authoring.EliteEggAndShootSpeed,
                    StageOneInSkillShootingInterval = authoring.EliteEggAndShootStageOneInSkillShootingInterval,
                    StageOneSkillShootCount = authoring.EliteEggAndShootStageOneSkillShootCount
                });

                // For EliteShooter
                AddComponent(entity, new EliteShooterConfigCom
                {
                    //EliteSpawnee = GetEntity(authoring.get)
                    StageOneSpeed = authoring.EliteShooterStageOneSpeed,
                    StageTwoSpeed = authoring.EliteShooterStageTwoSpeed,
                    StageOneInSkillShootingInterval = authoring.EliteShooterStageOneInSkillShootingInterval,
                    StageOneSkillShootCount = authoring.EliteEggAndShootStageOneSkillShootCount,
                    StageTwoInSkillShootingInterval = authoring.EliteShooterStageTwoInSkillShootingInterval,
                    StageTwoSkillShootCount = authoring.EliteShooterStageTwoSkillShootCount,
                });


            }
        }
    }
    [InternalBufferCapacity(0)]
    public struct WaveNewEnemyBuffer : IBufferElementData
    {
        public int Value;
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
        public float AttackCooldown;
        public float DeathCountdown;
        public float AttackDistance;
        public float FleeDistance;
        public float FleeSpeed;

        //public Entity SpawneePrefab;
        public float SpawneeSpeed;
        public float SpawneeTimer;

        public EnemyBasicAttribute BasicAttribute;
    }
    public struct NormalSprintConfigCom : IComponentData
    {
        //public Entity EnemyPrefab;
        public float AttackCooldown;
        public float DeathCountdown;
        public float SprintWaitTimerSetting;
        public float AttackDistance;
        public float SprintSpeed;
        public float HitDistance;
        public float3 FlashColorDifference;
        public EnemyBasicAttribute BasicAttribute;
    }
    public struct EnemyEggConfigCom : IComponentData
    {
        public EnemyBasicAttribute BasicAttribute;
    }
    public struct EnemySummonerConfigCom : IComponentData
    {
        public float EnemySummonerAttackCooldown;
        public float EnemySummonerFloatingRange;
        public float EnemySummonerFloatingCycleSpeed;
        public float EnemySummonerChasingSpeed;
        public float EnemySummonerFleeDistance;
        public float EnemySummonerChasingDistance;
        public float EnemySummonerExplodeDistance;
        public float EnemySummonerSummonDistance;
        public EnemyBasicAttribute BasicAttribute;
    }

    public struct NormalMeleeConfigCom : IComponentData
    {
        //public Entity EnemyPrefab;
        public float AttackCooldown;
        public float DeathCountdown;
        public float AttackDistance;
        public float SprintSpeed;
        public EnemyBasicAttribute BasicAttribute;
    }

    public struct EliteShooterConfigCom : IComponentData
    {
        //public Entity EliteSpawnee;
        public float StageOneSpeed;
        public float StageTwoSpeed;
        public float StageOneInSkillShootingInterval;
        public float StageTwoInSkillShootingInterval;
        public int StageOneSkillShootCount;
        public int StageTwoSkillShootCount;
        public EnemyBasicAttribute BasicAttribute;
    }

    public struct EliteEggAndShootConfig : IComponentData
    {
        public float Speed;
        public float StageOneInSkillShootingInterval;
        public float SpawnEggSkillSpawningInterval;
        public int StageOneSkillShootCount;
        public int SpawnEggSkillspawnCount;
        public EnemyBasicAttribute BasicAttribute;
    }

    public struct EliteSprintAndShootConfigCom : IComponentData
    {
        public float3 EliteSprintAndShootRightSpawnPosOffset;
        public float3 EliteSprintAndShootLeftSpawnPosOffset;
        public float EliteSprintAndShootSpawnYAxisRotation;
        public float EliteSprintAndShootSkillCooldown;
        public int EliteSprintAndShootSpawnCount;
        public float EliteSprintAndShootSpawnInterval;
        public float EliteSprintAndShootSprintDistance;
        public float EliteSprintAndShootSpeed;
        public float EliteSprintAndShootSprintSpeed;
        public float EliteSprintAndShootCollideDistance;
        public int EliteSprintAndShootSprintDamage;
        public EnemyBasicAttribute BasicAttribute;
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
    public struct EnemyBasicAttribute
    {
        public int HealthPoint;
        public int HpIncreasePerWave;
        public int Damage;
        public float DmgIncreasePerWave;
        public float Speed;
        public int MaterialsDropped;
        public float LootCrateDropRate;
        public float ConsumableDropate;
    }
}