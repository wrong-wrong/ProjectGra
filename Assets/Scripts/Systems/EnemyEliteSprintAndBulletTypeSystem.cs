using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    [UpdateBefore(typeof(EnemySummonedExplosionSystem))]
    public partial struct EnemyEliteSprintAndBulletSystem : ISystem, ISystemStartStop
    {
        private float4 spawneePosOffsetX;
        private float4 spawneePosOffsetY;
        //public float 
        private int currentSpawnPosIdx;
        private int spawneeShootOutCount;
        private float spawnTimer;
        private bool isDoingSkill;
        private Entity eliteSpawneePrefab;
        private Entity summonExplosionPrefab;
        private Random random;
        private float2 negtiveOne;
        private float2 postiveOne;
        private float3 offsetMin;
        private float3 offsetMax;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            spawneePosOffsetX = new float4(-3,-1.5f,1.5f,3f);
            spawneePosOffsetY = new float4(2,3.5f,3.5f,2);
            negtiveOne = new float2(-1, -1);
            postiveOne = new float2(1, 1);
            offsetMin = new float3(-8, 0, -8);
            offsetMax = new float3(8, 2, 8);
            currentSpawnPosIdx = 0;
        }

        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<EliteSpringAndBulletConfigCom>();
            eliteSpawneePrefab = config.EliteSpawnee;
            var prefabContainer = SystemAPI.GetSingleton<PrefabContainerCom>();
            summonExplosionPrefab = prefabContainer.SummonExplosionPrefab;
            random = Random.CreateFromIndex(0);
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            //updates spawnTimer< 0->set to 1, spawn prefab at pos; if PosIdx >= 4,-> set not doing skill, posIdx set to 0, spawnTimer set to 0;
            //updates scaling timer, if timer > setting, -> look rotation player need to fire, set moveTag to true;

            // random move test code
            //var eliteDataCom = SystemAPI.GetSingletonRW<EnemyEliteSprintAndBulletCom>();
            //var deltatime = SystemAPI.Time.DeltaTime;
            //var eliteEntity = SystemAPI.GetSingletonEntity<EnemyEliteSprintAndBulletCom>();
            //if ((eliteDataCom.ValueRW.MovingTimer -= deltatime) > 0f)
            //{
            //    var eliteTransform = SystemAPI.GetComponentRW<LocalTransform>(eliteEntity);
            //    eliteTransform.ValueRW.Position += eliteDataCom.ValueRO.TargetDirNormalized * deltatime * eliteDataCom.ValueRO.Speed;
            //}

            if (isDoingSkill)
            {
                // random move test code
                //var playerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>());
                //playerPos.Position.xz += random.NextFloat2(negtiveOne,postiveOne)*5;
                //var eliteTransform = SystemAPI.GetComponentRW<LocalTransform>(eliteEntity);
                //var tarDir = playerPos.Position - eliteTransform.ValueRO.Position;
                //eliteDataCom.ValueRW.TargetDirNormalized = math.normalize(tarDir);
                //eliteDataCom.ValueRW.MovingTimer = math.length(tarDir) / eliteDataCom.ValueRO.Speed;
                //isDoingSkill = false;

                //random shoot test code
                //ShootingSkill(ref state, 0.1f, 16, offsetMin, offsetMax);

                //sommoned explosion test code
                //var summonedExplosion = SystemAPI.GetSingletonEntity<SommonedExplosionCom>();
                //var transformRW = SystemAPI.GetComponentRW<LocalTransform>(summonedExplosion);
                //transformRW.ValueRW.Scale += 1f;
                //isDoingSkill = false;

                //sommon explosion full test
                var eliteEntity = SystemAPI.GetSingletonEntity<EnemyEliteSprintAndBulletCom>();
                //var eliteTransform = SystemAPI.GetComponentRW<LocalTransform>(eliteEntity);
                var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                // instantiate at next frame. This can let the Sommon system handle the LocalTransform of the explosion;
                var explosion = ecb.Instantiate(summonExplosionPrefab);
                ecb.SetComponent<SummonedExplosionCom>(explosion, new SummonedExplosionCom { FollowingEntity = eliteEntity ,CurrentState = SummonExplosionState.Summoning});
                ecb.SetComponent(explosion, new AttackCurDamage { damage = 100 });
                isDoingSkill = false;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                isDoingSkill = true;

                //random shoot test code
                //state.EntityManager.SetComponentData(eliteSpawnee, new SpawneeScalingCom { MaxScale = 0.5f });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ShootingSkill(ref SystemState state, float spawnInterval, int count, float3 offsetmin, float3 offsetmax) // four spawnee spawned at 1second interval
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            if ((spawnTimer -= deltatime) < 0f)
            {
                spawnTimer = spawnInterval;
                var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                var spawnee = ecb.Instantiate(eliteSpawneePrefab);
                var elite = SystemAPI.GetSingletonEntity<EnemyEliteSprintAndBulletCom>();
                var eliteTransform = SystemAPI.GetComponent<LocalTransform>(elite);
                var spawnPos = eliteTransform.Position + eliteTransform.Up() * spawneePosOffsetY[currentSpawnPosIdx] + eliteTransform.Forward() * spawneePosOffsetX[currentSpawnPosIdx];
                var tarDir = SystemAPI.GetComponentRO<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).ValueRO.Position - spawnPos;
                tarDir.y += 1f;
                tarDir += random.NextFloat3(offsetmin, offsetmax);
                ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(tarDir, math.up()) });
                currentSpawnPosIdx = (currentSpawnPosIdx + 1) % 4;
                if (++spawneeShootOutCount > count)
                {
                    spawneeShootOutCount = 0;
                    currentSpawnPosIdx = 0;
                    isDoingSkill = false;
                    spawnTimer = 0f;
                }
            }
        }

    }

    //public struct EnemyEliteSprintAndBulletStateCom : IComponentData
    //{

    //}

    
}