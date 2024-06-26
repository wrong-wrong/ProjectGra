using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(PlayerMainWeaponSystem))]
    public partial struct AutoWeaponUpdateSystem : ISystem
    {
        private CollisionFilter emptyCollisionFilter;
        private CollisionFilter playerSpawneeCollidesWithEnemyLayer;
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            emptyCollisionFilter = new CollisionFilter();
            playerSpawneeCollidesWithEnemyLayer = new CollisionFilter
            {
                BelongsTo = 1 << 5,// player spawnee
                CollidesWith = 1 << 3, // enemy layer
            };
            //Debug.Log(math.radians(90));    // 1.570796
            //Debug.Log(math.degrees(90));    // 5156.62
            //Debug.Log(math.sin(90));        // 0.89399
            //Debug.Log(math.sin(math.radians(90)));  // 1
            //Debug.Log(math.sin(math.degrees(90)));  // -0.954
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            var wpBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponBuffer>();
            //var wpStateMachineBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponStateMachineBuffer>();
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            for (int i = 0, n = wpBuffer.Length; i < n; ++i)
            {
                ref var wp = ref wpBuffer.ElementAt(i);
                if (wp.WeaponIndex == -1) continue;
                if ((wp.RealCooldown -= deltatime) < 0f && wp.DamageAfterBonus >= 0)
                {
                    if (!wp.IsMeleeWeapon) // Ranged Weapon
                    {
                        var spawnee = ecb.Instantiate(wp.SpawneePrefab);
                        ecb.SetComponent(spawnee, wp.autoWeaponLocalTransform);
                        wp.RealCooldown = wp.Cooldown;

                        // request audio
                        EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList.Add(wp.autoWeaponLocalTransform.Position);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList.Add(AudioEnum.NormalShoot);
                    }
                    else // MeleeWeapon
                    {
                        switch (wp.WeaponCurrentState)
                        {
                            case WeaponState.None:
                                state.EntityManager.GetComponentData<PhysicsCollider>(wp.WeaponModel).Value.Value.SetCollisionFilter(playerSpawneeCollidesWithEnemyLayer);
                                wp.MeleeRealShootingTimer = 0f;

                                if (!wp.IsMeleeSweep)
                                {
                                    //just shoot, because it would be targeting something if code execute up here
                                    wp.MeleeTargetPosition = wp.autoWeaponLocalTransform.Forward() * wp.Range + wp.autoWeaponLocalTransform.Position;
                                    wp.MeleeOriginalPosition = wp.autoWeaponLocalTransform.Position;
                                    wp.WeaponCurrentState = WeaponState.Thrust;
                                    EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList.Add(wp.autoWeaponLocalTransform.Position);
                                    EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList.Add(AudioEnum.NormalShoot);
                                    //change collisionFilter to begin trigger
                                    //Debug.Log("AutoMeleeWeapon start Thrust");
                                }
                                else
                                {
                                    // using MeleeTargetPosition as forwardMulRange
                                    wp.MeleeTargetPosition = wp.autoWeaponLocalTransform.Forward() * wp.Range;
                                    wp.MeleeSweepRightMulHalfWidth = wp.autoWeaponLocalTransform.Right() * wp.SweepHalfWidth;
                                    wp.MeleeOriginalPosition = wp.autoWeaponLocalTransform.Position;
                                    wp.WeaponCurrentState = WeaponState.Sweep;
                                    // request audio
                                    EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList.Add(wp.autoWeaponLocalTransform.Position);
                                    EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList.Add(AudioEnum.NormalShoot);
                                }

                                break;
                            case WeaponState.Thrust:
                                var lerpAmount = (wp.MeleeRealShootingTimer += deltatime) / wp.MeleeShootingTimer;
                                if (lerpAmount > 1f)
                                {
                                    wp.WeaponCurrentState = WeaponState.Retrieve;
                                    state.EntityManager.GetComponentData<PhysicsCollider>(wp.WeaponModel).Value.Value.SetCollisionFilter(emptyCollisionFilter);
                                    //change collisionFilter when start retrieve
                                    wp.MeleeOriginalPosition = wp.MeleeTargetPosition;
                                    wp.MeleeRealShootingTimer = 0f;
                                    var hitBuffer = ecb.SetBuffer<HitBuffer>(wp.WeaponModel);
                                    hitBuffer.Clear();
                                    //Debug.Log("AutoMeleeWeapon start Retrieve");
                                    // In Retrieve using wp.autoWeaponLocalTransform instead of wp.MeleeTargetPosition = wp.auto
                                }
                                else
                                {
                                    //TODO : Modify lerpAmount with  sine function? to make a uneven movement
                                    SystemAPI.GetComponentRW<LocalTransform>(wp.WeaponModel).ValueRW.Position = math.lerp(wp.MeleeOriginalPosition, wp.MeleeTargetPosition, lerpAmount);
                                }
                                break;
                            case WeaponState.Sweep:
                                var ratio = (wp.MeleeRealShootingTimer += deltatime) / wp.MeleeShootingTimer;
                                if (ratio < 1f)
                                {
                                    // radians(x) == x * 0.0174532925f,   radians( ratio * 180) == ratio * 0.0174532925f * 180 == ratio * pi
                                    SystemAPI.GetComponentRW<LocalTransform>(wp.WeaponModel).ValueRW.Position =
                                        wp.MeleeOriginalPosition
                                        + wp.MeleeTargetPosition * math.sin(ratio * math.PI) // using meleeTargetPosition as forwardMulRange
                                        + wp.MeleeSweepRightMulHalfWidth * math.cos(ratio * math.PI);
                                }
                                else
                                {
                                    wp.WeaponCurrentState = WeaponState.None;
                                    wp.RealCooldown = wp.Cooldown;
                                    state.EntityManager.GetComponentData<PhysicsCollider>(wp.WeaponModel).Value.Value.SetCollisionFilter(emptyCollisionFilter);
                                    state.EntityManager.GetBuffer<HitBuffer>(wp.WeaponModel).Clear();
                                }
                                break;
                            case WeaponState.Retrieve:
                                lerpAmount = (wp.MeleeRealShootingTimer += deltatime) / wp.MeleeShootingTimer;
                                if (lerpAmount > 1f)
                                {
                                    wp.WeaponCurrentState = WeaponState.None;
                                    wp.RealCooldown = wp.Cooldown;
                                    //Debug.Log("AutoMeleeWeapon back to None");
                                }
                                else
                                {
                                    SystemAPI.GetComponentRW<LocalTransform>(wp.WeaponModel).ValueRW.Position = math.lerp(wp.MeleeOriginalPosition, wp.autoWeaponLocalTransform.Position, lerpAmount);
                                }
                                break;
                        }
                        //Debug.Log("melee weapon need to fire");
                    }
                }
                //var something = state.EntityManager.GetComponentData<PhysicsCollider>(wp.SpawneePrefab).Value.Value.
            }
        }

    }
}