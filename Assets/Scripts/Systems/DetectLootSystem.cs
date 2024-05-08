using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct DetectLootSystem : ISystem
    {
        private CollisionFilter materialFilter;
        private ComponentLookup<ItemTag> itemTagLookup;
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
            itemTagLookup = SystemAPI.GetComponentLookup<ItemTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            //if (!Input.GetKeyUp(KeyCode.Space)) return;
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>());
            var playerMaterial = SystemAPI.GetSingletonRW<PlayerMaterialCount>();
            var playerItem = SystemAPI.GetSingletonRW<PlayerItemCount>();
            var playerExperience = SystemAPI.GetSingletonRW<PlayerExperience>();
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var hits = new NativeList<DistanceHit>(state.WorldUpdateAllocator);
            if (collisionWorld.OverlapSphere(playerTransform.Position, 2f, ref hits, materialFilter))
            {
                itemTagLookup.Update(ref state);
                foreach (var hit in hits)
                {
                    if (!itemTagLookup.HasComponent(hit.Entity))
                    {
                        playerExperience.ValueRW.Exp += 1;
                        playerMaterial.ValueRW.Count += 1;
                    }
                    else
                    {
                        if (itemTagLookup.IsComponentEnabled(hit.Entity))
                        {
                            playerItem.ValueRW.Legendary += 1;
                        }
                        else
                        {
                            playerItem.ValueRW.Normal += 1;
                        }
                    }

                    ecb.DestroyEntity(hit.Entity);
                }
            }
        }
    }
}