using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct EnemySpawnSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.EntityManager.AddComponent<EnemySpawnSystemDataSingleton>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new EnemySpawnSystemDataSingleton
            {
                random = Random.CreateFromIndex(0)
            });
        }

        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<EnemySpawningConfig>();
            var sysData = SystemAPI.GetComponentRW<EnemySpawnSystemDataSingleton>(state.SystemHandle);
            sysData.ValueRW.realCooldown = config.SpawningCooldown;
            sysData.ValueRW.spawningCooldown = config.SpawningCooldown;
            sysData.ValueRW.minRadius = config.minRadius;
            sysData.ValueRW.maxRadius = config.maxRadius;
            //Debug.Log("EnemySpawnSystem start running"); 
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            //if (Input.GetKeyUp(KeyCode.Backslash))
            //{
            //    //state.EntityManager.Instantiate()
            //}
            var sysData = SystemAPI.GetComponentRW<EnemySpawnSystemDataSingleton>(state.SystemHandle);
            //Debug.Log("SpawningCooldown" + sysData.ValueRO.spawningCooldown);
            //Debug.Log("realCooldown" + sysData.ValueRO.realCooldown);
            //Debug.Log("After substract deltatime" + sysData.ValueRO.realCooldown);
            //Debug.Log("deltatime " + deltaTime);
            if ((sysData.ValueRW.realCooldown -= SystemAPI.Time.DeltaTime) < 0f)
            {
                //Debug.Log("spawning enemy!!!!!!!!!!");
                var allEnemyPrefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                sysData.ValueRW.realCooldown = sysData.ValueRO.spawningCooldown;
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
                var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                var enemy = ecb.Instantiate(allEnemyPrefabBuffer[sysData.ValueRW.random.NextInt(allEnemyPrefabBuffer.Length)].Prefab);
                float2 f2pos;
                f2pos.x = sysData.ValueRW.random.NextFloat(playerPosition.x - sysData.ValueRO.minRadius, playerPosition.x + sysData.ValueRO.minRadius);
                f2pos.y = playerPosition.z + math.sqrt((sysData.ValueRO.minRadius + f2pos.x - playerPosition.x) * (sysData.ValueRO.minRadius - f2pos.x + playerPosition.x)) * (sysData.ValueRW.random.NextBool() ? -1 : 1);
                f2pos.x *= sysData.ValueRO.random.NextFloat(1, sysData.ValueRO.maxRadius / sysData.ValueRO.minRadius);
                f2pos.y *= sysData.ValueRO.random.NextFloat(1, sysData.ValueRO.maxRadius / sysData.ValueRO.minRadius);
                sysData.ValueRW.flip = !sysData.ValueRW.flip;
                if (sysData.ValueRW.flip)
                {
                    f2pos = f2pos.yx;
                }
                playerPosition.xz = f2pos;//GetRandomPointWithRangeRadius(playerPosition, sysData.ValueRO.minRadius, sysData.ValueRO.maxRadius); // now it is not player position,
                ecb.SetComponent<LocalTransform>(enemy, new LocalTransform { Position = playerPosition, Rotation = quaternion.identity, Scale = 1f });
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
        public bool flip;
        public float realCooldown;
        public float spawningCooldown;
        public float minRadius;
        public float maxRadius;
    }
}
