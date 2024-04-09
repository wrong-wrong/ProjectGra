using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct DetectTargetForAutoWpSystem : ISystem
    {
        private CollisionFilter enemyCollisionFilter;
        private float detectFrequency;
        private float timer;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            enemyCollisionFilter = new CollisionFilter
            {
                BelongsTo = 1 << 1, // Raycast
                CollidesWith = 1 << 3 | 1 << 9, // EnemyLayer & RagdollLayer
            };
            detectFrequency = 1 / 30f;
            timer = detectFrequency;
        }
        public void OnUpdate(ref SystemState state)
        {
            //TODO playerRadius == 0 then return ?
            timer -= SystemAPI.Time.DeltaTime;
            if (timer > 0.01f) return;
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var hits = new NativeList<DistanceHit>(state.WorldUpdateAllocator);
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var radius = SystemAPI.GetComponent<PlayerOverlapRadius>(playerEntity);
            var autoWpBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponBuffer>();
            var playerTransform = SystemAPI.GetComponentRO<LocalTransform>(playerEntity);
            //magic number represent wp's distance to the player position
            //TODO radius almost not zero;
            if (radius.Value != 0)
            {
                if (collisionWorld.OverlapSphere(playerTransform.ValueRO.Position, radius.Value + 2f, ref hits, enemyCollisionFilter))
                {
                    for (int i = 0, n = autoWpBuffer.Length; i < n; ++i)
                    {
                        ref var wp = ref autoWpBuffer.ElementAt(i);
                        if (wp.WeaponIndex == -1) continue;
                        var rangeSq = wp.Range * wp.Range;
                        float minDistance = 1000f;
                        float preMinDistance = 1000f;
                        float3 tarDir = float3.zero;
                        var wpModel = SystemAPI.GetComponentRW<LocalTransform>(wp.WeaponModel);
                        for (int j = 0, m = hits.Length; j < m; ++j)
                        {
                            minDistance = math.min(math.lengthsq(hits[j].Position - wpModel.ValueRO.Position), minDistance);
                            tarDir = (preMinDistance - minDistance) < 0.1f ? tarDir : hits[j].Position - wpModel.ValueRO.Position;
                            preMinDistance = minDistance;
                        }
                        if (minDistance < rangeSq)
                        {
                            tarDir.y = 0;
                            wpModel.ValueRW.Rotation = quaternion.LookRotation(math.normalizesafe(tarDir), math.up());
                            wp.autoWeaponLocalTransform = wpModel.ValueRO;
                            Debug.DrawLine(wp.autoWeaponLocalTransform.Position, wp.autoWeaponLocalTransform.Position + tarDir);
                            wp.DamageAfterBonus = wp.DamageAfterBonus > 0 ? wp.DamageAfterBonus : -wp.DamageAfterBonus;
                        }
                        else
                        {

                            wp.DamageAfterBonus = wp.DamageAfterBonus > 0 ? -wp.DamageAfterBonus : wp.DamageAfterBonus;
                        }
                    }
                }
                else
                {
                    for (int i = 0, n = autoWpBuffer.Length; i < n; ++i)
                    {
                        ref var wp = ref autoWpBuffer.ElementAt(i);
                        if (wp.WeaponIndex == -1) continue;
                        wp.DamageAfterBonus = wp.DamageAfterBonus > 0 ? -wp.DamageAfterBonus : wp.DamageAfterBonus;
                    }
                    //Debug.Log("Overlaped! : " + hits[0].Entity);
                }
            }
        }

    }

}