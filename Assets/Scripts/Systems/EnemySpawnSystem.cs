using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct EnemySpawnSystem : ISystem, ISystemStartStop
    {
        int _MaxEnemyBufferIdxExclusive;
        int _LastUpdateWave;
        //bool _IsInit;
        int groupSpawnRealCount;
        float groupSpawnRealTimer;
        float groupSpawnTimer;
        float2 groupSpawnFixedRangePointOne;
        float2 groupSpawnFixedRangePointTwo;
        int groupSpawnCount;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.EntityManager.AddComponent<EnemySpawnSystemDataSingleton>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new EnemySpawnSystemDataSingleton
            {
                random = Random.CreateFromIndex(0),
                IsInit = false
            });

           
        }

        public void OnStartRunning(ref SystemState state)
        {
            var sysData = SystemAPI.GetSingletonRW<EnemySpawnSystemDataSingleton>();
            var updateEnemyCom = SystemAPI.GetSingleton<GameControllShouldUpdateEnemy>();
            if (!sysData.ValueRO.IsInit)
            {
                sysData.ValueRW.IsInit = true;
                _MaxEnemyBufferIdxExclusive = 0;
                _LastUpdateWave = 0;
                var config = SystemAPI.GetSingleton<EnemySpawningConfig>();
                sysData.ValueRW.minRadius = config.minRadius;
                sysData.ValueRW.maxRadius = config.maxRadius;

                var spawningConfig = SystemAPI.GetSingletonBuffer<SpawningConfigBuffer>();
                var WaveNewEnemyBuffer = SystemAPI.GetSingletonBuffer<WaveNewEnemyBuffer>();

                var spawnConfigCd = spawningConfig[updateEnemyCom.CodingWave].SpawnCooldown;
                if (spawnConfigCd > 0)
                {
                    sysData.ValueRW.realCooldown = spawnConfigCd;
                    sysData.ValueRW.IsHordeOrElite = false;
                }
                else
                {
                    sysData.ValueRW.realCooldown = -spawnConfigCd;
                    sysData.ValueRW.IsHordeOrElite = true;
                }
                sysData.ValueRW.pointSpawnChance = spawningConfig[updateEnemyCom.CodingWave].PointSpawnChance;
                sysData.ValueRW.spawningCooldown = spawningConfig[updateEnemyCom.CodingWave].SpawnCooldown;

                for (int n = updateEnemyCom.CodingWave; _LastUpdateWave <= n; ++_LastUpdateWave)
                {
                    _MaxEnemyBufferIdxExclusive += WaveNewEnemyBuffer[_LastUpdateWave].Value;
                }
            }else if (updateEnemyCom.Value)
            {
                //var sysData = SystemAPI.GetSingletonRW<EnemySpawnSystemDataSingleton>();
                var spawningConfig = SystemAPI.GetSingletonBuffer<SpawningConfigBuffer>();
                var WaveNewEnemyBuffer = SystemAPI.GetSingletonBuffer<WaveNewEnemyBuffer>();

                var spawnConfigCd = spawningConfig[updateEnemyCom.CodingWave].SpawnCooldown;
                if (spawnConfigCd > 0)
                {
                    sysData.ValueRW.realCooldown = spawnConfigCd;
                    sysData.ValueRW.IsHordeOrElite = false;
                }
                else
                {
                    sysData.ValueRW.realCooldown = -spawnConfigCd;
                    sysData.ValueRW.IsHordeOrElite = true;
                }
                sysData.ValueRW.pointSpawnChance = spawningConfig[updateEnemyCom.CodingWave].PointSpawnChance;
                sysData.ValueRW.spawningCooldown = spawningConfig[updateEnemyCom.CodingWave].SpawnCooldown;

                for (int n = updateEnemyCom.CodingWave; _LastUpdateWave <= n; ++_LastUpdateWave)
                {
                    _MaxEnemyBufferIdxExclusive += WaveNewEnemyBuffer[_LastUpdateWave].Value;
                }
                Debug.Log("EnemySpawnSystem - SpawnCooldown : " + sysData.ValueRO.spawningCooldown +" - PointSpawnChance : " + sysData.ValueRO.pointSpawnChance);
                groupSpawnCount = math.clamp(updateEnemyCom.CodingWave + 1, 5, 13);
            }
            Debug.Log("EnemySpawnSystem  - MaxEnemyBufferIdx: " + _MaxEnemyBufferIdxExclusive);
            Debug.Log("EnemySpawnSystem  - _LashUpdateWave: " + _LastUpdateWave);
            //Debug.Log("EnemySpawnSystem start running"); 
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            //if (Input.GetKeyUp(KeyCode.Backslash))
            //{
            //    //state.EntityManager.Instantiate()
            //}
            var sysData = SystemAPI.GetSingletonRW<EnemySpawnSystemDataSingleton>();
            //Debug.Log("SpawningCooldown" + sysData.ValueRO.spawningCooldown);
            //Debug.Log("realCooldown" + sysData.ValueRO.realCooldown);
            //Debug.Log("After substract deltatime" + sysData.ValueRO.realCooldown);
            //Debug.Log("deltatime " + deltaTime);
            var deltatime = SystemAPI.Time.DeltaTime;

            if ((sysData.ValueRW.realCooldown -= deltatime) < 0f)
            {
                //Debug.Log("spawning enemy!!!!!!!!!!");
                var allEnemyPrefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                sysData.ValueRW.realCooldown = sysData.ValueRO.spawningCooldown;
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
                var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                float2 f2pos;
                f2pos.x = sysData.ValueRW.random.NextFloat(playerPosition.x - sysData.ValueRO.minRadius, playerPosition.x + sysData.ValueRO.minRadius);
                f2pos.y = playerPosition.z + math.sqrt((sysData.ValueRO.minRadius + f2pos.x - playerPosition.x) * (sysData.ValueRO.minRadius - f2pos.x + playerPosition.x)) * (sysData.ValueRW.random.NextBool() ? -1 : 1);
                f2pos.x *= sysData.ValueRW.random.NextFloat(1, sysData.ValueRO.maxRadius / sysData.ValueRO.minRadius);
                f2pos.y *= sysData.ValueRW.random.NextFloat(1, sysData.ValueRO.maxRadius / sysData.ValueRO.minRadius);
                sysData.ValueRW.flip = !sysData.ValueRW.flip;
                if (sysData.ValueRW.flip)
                {
                    f2pos = f2pos.yx;
                }
                if(sysData.ValueRW.random.NextFloat() < sysData.ValueRO.pointSpawnChance ) // spawn a group 
                {
                    var enemy = ecb.Instantiate(allEnemyPrefabBuffer[sysData.ValueRW.random.NextInt(_MaxEnemyBufferIdxExclusive)].Prefab);
                    playerPosition.xz = f2pos;//GetRandomPointWithRangeRadius(playerPosition, sysData.ValueRO.minRadius, sysData.ValueRO.maxRadius); // now it is not player position,
                    ecb.SetComponent<LocalTransform>(enemy, new LocalTransform { Position = playerPosition, Rotation = quaternion.identity, Scale = 1f });
                }
                else
                {
                    Debug.Log("Group Spawn");
                    groupSpawnRealCount = groupSpawnCount;
                    var dir = f2pos.yx - playerPosition.zx;
                    dir.x = -dir.x;
                    groupSpawnFixedRangePointOne = f2pos + dir;// + groupSpawnRangeOffset;
                    groupSpawnFixedRangePointTwo = f2pos - dir;// - groupSpawnRangeOffset;
                }

            }

            if (groupSpawnRealCount > 0)
            {
                if ((groupSpawnRealTimer -= deltatime) < 0f)
                {
                    --groupSpawnRealCount;
                    groupSpawnRealTimer = groupSpawnTimer;
                    var allEnemyPrefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                    var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                    var enemy = ecb.Instantiate(allEnemyPrefabBuffer[sysData.ValueRW.random.NextInt(_MaxEnemyBufferIdxExclusive)].Prefab);
                    var posf2 = sysData.ValueRW.random.NextFloat2(groupSpawnFixedRangePointOne, groupSpawnFixedRangePointTwo);
                    ecb.SetComponent(enemy, new LocalTransform { Position = new float3(posf2.x, 0, posf2.y), Scale = 1f });
                }
            }

        }

        //private float2 GetRandomPointWithRangeRadius(float3 playerPos, float minRadius, float maxRadius)
        //{

        //    float2 f2pos;
        //    f2pos.x = random.NextFloat(playerPos.x - minRadius, playerPos.x + minRadius);
        //    f2pos.y = playerPos.z + math.sqrt((minRadius + f2pos.x - playerPos.x) * (minRadius - f2pos.x + playerPos.x)) * (random.NextBool() ? -1 : 1);
        //    f2pos.x *= random.NextFloat(1, maxRadius / minRadius);
        //    f2pos.y *= random.NextFloat(1, maxRadius / minRadius);
        //    flip = !flip;
        //    if (flip)
        //    {
        //        f2pos = f2pos.yx;
        //    }
        //    return f2pos;
        //}


    }
    public struct EnemySpawnSystemDataSingleton : IComponentData
    {
        public Random random;
        public bool IsInit;
        public float realCooldown;
        public float spawningCooldown;
        public float minRadius;
        public float maxRadius;
        public float pointSpawnChance;
        public bool flip;
        public bool IsHordeOrElite;
    }
}
