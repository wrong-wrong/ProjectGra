using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct DetectMaterialsSystem : ISystem
    {
        private CollisionFilter materialFilter;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            materialFilter = new CollisionFilter
            {
                CollidesWith = 1 << 7, // materialsAndItem
                BelongsTo = 1 << 1,  // raycast
            };
        }

        public void OnUpdate(ref SystemState state)
        {
            //if (!Input.GetKeyUp(KeyCode.Space)) return;
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>());
            var playerMaterial = SystemAPI.GetSingletonRW<PlayerMaterialCount>();
            var playerItem = SystemAPI.GetSingletonRW<PlayerItemCount>();
            var playerExperience = SystemAPI.GetSingletonRW<PlayerExperienceAndLevel>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var itemTagLookup = SystemAPI.GetComponentLookup<ItemTag>();
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var hits = new NativeList<DistanceHit>(state.WorldUpdateAllocator);
            if(collisionWorld.OverlapSphere(playerTransform.Position, 2f, ref hits, materialFilter))
            {
                foreach(var hit in hits)
                {
                    if (!itemTagLookup.HasComponent(hit.Entity))
                    {
                        playerExperience.ValueRW.Exp += 1f;
                        playerMaterial.ValueRW.Count += 1;
                    }
                    else
                    {
                        playerItem.ValueRW.Count += 1;
                    }
                    
                    ecb.DestroyEntity(hit.Entity);
                }
            }
        }
    }
}