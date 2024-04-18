using Unity.Entities;
using Unity.Transforms;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyEggSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyEggTimerCom>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
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
                            var hatched = ecb.Instantiate(allEnemyBuffer[hatch.ValueRO.PrefabIdx].Prefab);
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