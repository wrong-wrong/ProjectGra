using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp),OrderLast = true)]
    public partial struct GameWaveCleanupSystem : ISystem, ISystemStartStop
    {
        private float cleanupTimer;
        private float realTimer;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllWaveCleanup>();
            state.RequireForUpdate<GameControllNotPaused>();
            cleanupTimer = 3;
            realTimer = cleanupTimer;
        }

        public void OnStartRunning(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (summon, entity) in SystemAPI.Query<RefRO<SummonedExplosionCom>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            if((realTimer -= deltatime) < 0f)
            {
                realTimer = cleanupTimer;
                var waveControllSys = state.WorldUnmanaged.GetExistingUnmanagedSystem<GameWaveControllSystem>();
                state.EntityManager.RemoveComponent<GameControllWaveCleanup>(waveControllSys);
                var enemyList = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build().ToEntityArray(state.WorldUpdateAllocator);
                state.EntityManager.DestroyEntity(enemyList);
                var spawneeList = SystemAPI.QueryBuilder().WithAll<SpawneeTimer>().Build().ToEntityArray(state.WorldUpdateAllocator);
                state.EntityManager.DestroyEntity(spawneeList);

                //TODO destroy unpicked item
                //TODO maybe need to destory material to
            }
            else
            {
                foreach(var localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<EnemyTag>())
                {
                    localTransform.ValueRW.Scale = realTimer / cleanupTimer;
                }
                foreach (var localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<SpawneeTimer>())
                {
                    localTransform.ValueRW.Scale = realTimer / cleanupTimer;
                }
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
                foreach(var localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<MaterialTag>())
                {
                    localTransform.ValueRW.Position = math.lerp(localTransform.ValueRO.Position, playerTransform.Position, 1 - realTimer / cleanupTimer);
                }
            }
        }
    }
}