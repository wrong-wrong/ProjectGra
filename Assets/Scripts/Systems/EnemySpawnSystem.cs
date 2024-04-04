using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct EnemySpawnSystem : ISystem,ISystemStartStop
    {
        private Random random;
        private bool flip;
        private float realCooldown;
        private float spawningCooldown;
        private float minRadius;
        private float maxRadius;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            random = Random.CreateFromIndex(0);
        }

        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<EnemySpawningConfig>();
            realCooldown = config.SpawningCooldown;
            spawningCooldown = config.SpawningCooldown;
            minRadius = config.minRadius;
            maxRadius = config.maxRadius;
        }

        public void OnStopRunning(ref SystemState state){}

        public void OnUpdate(ref SystemState state) 
        {
            if((realCooldown -= SystemAPI.Time.DeltaTime) < 0f)
            {
                var allEnemyPrefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                realCooldown = spawningCooldown;
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
                var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                var enemy = ecb.Instantiate(allEnemyPrefabBuffer[random.NextInt(allEnemyPrefabBuffer.Length)].Prefab);
                playerPosition.xz = GetRandomPointWithRangeRadius(playerPosition, minRadius, maxRadius); // now it is not player position,
                ecb.SetComponent<LocalTransform>(enemy, new LocalTransform { Position = playerPosition, Rotation = quaternion.identity, Scale = 1f });
            }

        }
        private float2 GetRandomPointWithRangeRadius(float3 playerPos, float minRadius, float maxRadius)
        {

            float2 f2pos;
            f2pos.x = random.NextFloat(playerPos.x - minRadius, playerPos.x + minRadius);
            f2pos.y = playerPos.z + math.sqrt((minRadius + f2pos.x - playerPos.x) * (minRadius - f2pos.x + playerPos.x)) * (random.NextBool() ? -1 : 1);
            f2pos.x *= random.NextFloat(1, maxRadius / minRadius);
            f2pos.y *= random.NextFloat(1, maxRadius / minRadius);
            flip = !flip;
            if (flip)
            {
                f2pos = f2pos.yx;
            }
            return f2pos;
        }

    }
}