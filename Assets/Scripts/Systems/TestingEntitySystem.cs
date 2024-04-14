using Unity.Entities;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectGra
{
    public partial struct TestingEntitySystem : ISystem, ISystemStartStop
    {
        private CollisionFilter enemyCollidesWithRayCastAndPlayerSpawnee;
        private BatchMeshID RealMeshId;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<TestingEntityTag>();
            enemyCollidesWithRayCastAndPlayerSpawnee = new CollisionFilter
            {
                BelongsTo = 1 << 3, // enemy layer
                CollidesWith = 1 << 1 | 1 << 5, // ray cast & player spawnee
            };
        }

        public void OnStartRunning(ref SystemState state)
        {
            var batchMeshIdContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
            RealMeshId = batchMeshIdContainer.EnemyNormalMeleeMeshID;
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state) 
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            //foreach(var(timer, timerBit, materialAndMesh, collider) in SystemAPI.Query<RefRW<SpawningTimer>, EnabledRefRW<SpawningTimer>, RefRW<MaterialMeshInfo>, RefRW<PhysicsCollider>>())
            //{
            //    if((timer.ValueRW.time += deltatime) > 3f)
            //    {
            //        timerBit.ValueRW = false;
            //        Debug.Log("Setting timerBit to false : " + timerBit.ValueRO);
            //        materialAndMesh.ValueRW.MeshID = RealMeshId;
            //        collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
            //        Debug.Log("Setting collision filter");
            //    }
            //}
            foreach (var (flash, materialAndMesh, collider) in SystemAPI.Query<EnabledRefRO<FlashingCom>, RefRW<MaterialMeshInfo>, RefRW<PhysicsCollider>>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                if (!flash.ValueRO)
                {
                    materialAndMesh.ValueRW.MeshID = RealMeshId;
                    collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                    Debug.Log("Setting collision filter");
                }
            }
        }
    }
}