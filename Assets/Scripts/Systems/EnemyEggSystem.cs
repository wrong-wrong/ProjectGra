using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(EnemySummonerSystem))]
    public partial struct EnemyEggSystem : ISystem, ISystemStartStop
    {
        int _HealthPoint;
        int _HpIncreasePerWave;
        bool isInit;
        Random random;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyEggTimerCom>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            random = Random.CreateFromIndex(0);
        }

        public void OnStartRunning(ref SystemState state)
        {
            if(!isInit)
            {
                isInit = true;
                var config = SystemAPI.GetSingleton<EnemyEggConfigCom>();
                var basicAttribute = config.BasicAttribute;
                _HealthPoint = basicAttribute.HealthPoint;
                _HpIncreasePerWave = basicAttribute.HpIncreasePerWave;
            }
            var shouldUpdate = SystemAPI.GetSingleton<GameControllShouldUpdateEnemy>();
            // Per Wave Update
            //      Needs to set EnemyPrefab's HealthPoint, update System's damage, and attribute of enemy's projectile 
            if (shouldUpdate.Value)
            {
                _HealthPoint += _HpIncreasePerWave;
                var attModifier = SystemAPI.GetSingleton<EnemyHpAndDmgModifierWithDifferentDifficulty>();
                _HealthPoint = (int)(_HealthPoint * attModifier.HealthPointModifier);
                var prefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                SystemAPI.SetComponent(prefabBuffer[3].Prefab, new EntityHealthPoint { HealthPoint = _HealthPoint });
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var deltatime = SystemAPI.Time.DeltaTime;
            var allEnemyBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
            foreach (var (eggtimer, hatch, transform, stateMachine, entity) in SystemAPI.Query<RefRW<EnemyEggTimerCom>, RefRO<EnemyEggToBeHatched>, RefRO<LocalTransform>
                , RefRW<EntityStateMachine>>().WithEntityAccess()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.Dead:
                        ecb.DestroyEntity(entity);
                        // request particle
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList.Add(transform.ValueRO.Position);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList.Add(ParticleEnum.Default);
                        break;
                    case EntityState.EggHatching:
                        if ((eggtimer.ValueRW.Timer -= deltatime) < 0)
                        {
                            //var hatched = ecb.Instantiate(hatch.ValueRO.Prefab);
                            var hatched = ecb.Instantiate(allEnemyBuffer[random.NextInt(4)].Prefab);
                            ecb.SetComponent(hatched, new LocalTransform { Position = transform.ValueRO.Position, Scale = 2f });
                            ecb.SetComponent(hatched, new EntityHealthPoint { HealthPoint = 100 });
                            ecb.SetComponentEnabled<FlashingCom>(hatched, false);
                            ecb.DestroyEntity(entity);
                        }
                        break;
                }
            }
        }
    }
}