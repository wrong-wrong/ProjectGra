using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyEggSystem : ISystem, ISystemStartStop
    {
        private CollisionFilter enemyCollidesWithRayCastAndPlayerSpawnee;
        private BatchMeshID RealMeshId;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemyEggTimerCom>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            enemyCollidesWithRayCastAndPlayerSpawnee = new CollisionFilter
            {
                BelongsTo = 1 << 3, // enemy layer
                CollidesWith = 1 << 1 | 1 << 5, // ray cast & player spawnee
            };
        }

        public void OnStartRunning(ref SystemState state)
        {
            var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
            RealMeshId = batchMeshIDContainer.EnemyEggMeshID;
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var deltatime = SystemAPI.Time.DeltaTime;
            foreach (var (eggtimer, hatch, transform, stateMachine, flashbit, materialAndMesh, collider, entity) in SystemAPI.Query<RefRW<EnemyEggTimerCom>, RefRO<EnemyEggToBeHatched>, RefRO<LocalTransform>
                , RefRW<EntityStateMachine>
                , EnabledRefRO<FlashingCom>
                , RefRW<MaterialMeshInfo>
                , RefRW<PhysicsCollider>>().WithEntityAccess()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.SpawnEffect:
                        if (flashbit.ValueRO) continue;
                        materialAndMesh.ValueRW.MeshID = RealMeshId;
                        collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                        stateMachine.ValueRW.CurrentState = EntityState.EggHatching;
                        break;
                    case EntityState.Dead:
                        ecb.DestroyEntity(entity);
                        break;
                    case EntityState.EggHatching:
                        if ((eggtimer.ValueRW.Timer -= deltatime) < 0)
                        {
                            var hatched = ecb.Instantiate(hatch.ValueRO.Prefab);
                            ecb.SetComponent(hatched, new LocalTransform { Position = transform.ValueRO.Position, Scale = 2f });
                            ecb.SetComponent(hatched, new EntityHealthPoint { HealthPoint = 100 });
                            ecb.DestroyEntity(entity);
                        }
                        break;
                }
            }
        }
    }
}