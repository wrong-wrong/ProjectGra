using ProjectGra.PlayerController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(CameraTargetFollowSystem))]
    public partial struct PlayerAttackSystem : ISystem
    {
        private Random random;
        private CollisionFilter emptyCollisionFilter;
        private CollisionFilter playerSpawneeCollidesWithEnemyLayer;
       
        //private float magicNum;// = 1.5707963;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            random = Random.CreateFromIndex(0);
            emptyCollisionFilter = new CollisionFilter();
            playerSpawneeCollidesWithEnemyLayer = new CollisionFilter
            {
                BelongsTo = 1 << 5,// player spawnee
                CollidesWith = 1 << 3, // enemy layer
            };
            //magicNum = 1.5707963f;
            //Debug.Log(math.radians(90));    // 1.570796
            //Debug.Log(math.degrees(90));    // 5156.62
            //Debug.Log(math.sin(90));        // 0.89399
            //Debug.Log(math.sin(math.radians(90)));  // 1
            //Debug.Log(math.sin(math.degrees(90)));  // -0.954
            //Debug.Log(math.sin(1.570796325));   // 1
            //Debug.Log(math.sin(1.57079632));    // 1
            //Debug.Log(math.sin(1.5707963));     // 1
            //Debug.Log(math.sin(1.570796));      // 0.....
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            //need cur main weapon position,  cur spawnee prefab, player input, real cooldown, 
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (mainWeaponState, inputState, playerAttribute) in SystemAPI.Query<RefRW<MainWeapon>, EnabledRefRO<ShootInput>, RefRO<PlayerAtttributeDamageRelated>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                if (mainWeaponState.ValueRO.WeaponIndex == -1) continue;

                //ref var realcd = ref mainWeaponState.ValueRW.RealCooldown;   // what am i doing here .... 
                //realcd -= deltatime;
                //if (realcd > 0.001f) { continue; }
                //if (inputState.ValueRO == false) { continue; }

                if ((mainWeaponState.ValueRW.RealCooldown -= deltatime) > 0f) { continue; }
                if(mainWeaponState.ValueRO.IsMeleeWeapon)
                {
                    switch (mainWeaponState.ValueRO.WeaponCurrentState)
                    {
                        case WeaponState.None:
                            if (!inputState.ValueRO) return;
                            //just shoot, because it would be targeting something if code execute up here
                            //Debug.Log(mainWeaponState.ValueRO.mainWeaponLocalTransform.Forward());
                            mainWeaponState.ValueRW.MeleeTargetPosition = mainWeaponState.ValueRO.mainWeaponLocalTransform.Forward() * mainWeaponState.ValueRO.Range + mainWeaponState.ValueRO.mainWeaponLocalTransform.Position;
                            mainWeaponState.ValueRW.MeleeOriginalPosition = mainWeaponState.ValueRO.mainWeaponLocalTransform.Position;
                            mainWeaponState.ValueRW.MeleeRealShootingTimer = 0f;
                            mainWeaponState.ValueRW.WeaponCurrentState = WeaponState.Thrust;
                            state.EntityManager.GetComponentData<PhysicsCollider>(mainWeaponState.ValueRW.WeaponModel).Value.Value.SetCollisionFilter(playerSpawneeCollidesWithEnemyLayer);
                            //change collisionFilter to begin trigger
                            //Debug.Log("MainMeleeWeapon start Thrust");

                            // request audio
                            EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList.Add(mainWeaponState.ValueRO.mainWeaponLocalTransform.Position);
                            EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList.Add(AudioEnum.NormalShoot);
                            break;
                        case WeaponState.Thrust:
                            var lerpAmount = (mainWeaponState.ValueRW.MeleeRealShootingTimer += deltatime) / mainWeaponState.ValueRO.MeleeShootingTimer;
                            if (lerpAmount > 1f)
                            {
                                mainWeaponState.ValueRW.WeaponCurrentState = WeaponState.Retrieve;
                                state.EntityManager.GetComponentData<PhysicsCollider>(mainWeaponState.ValueRW.WeaponModel).Value.Value.SetCollisionFilter(emptyCollisionFilter);
                                //change collisionFilter when start retrieve
                                mainWeaponState.ValueRW.MeleeOriginalPosition = mainWeaponState.ValueRO.MeleeTargetPosition;
                                mainWeaponState.ValueRW.MeleeRealShootingTimer = 0f;
                                var hitBuffer = ecb.SetBuffer<HitBuffer>(mainWeaponState.ValueRW.WeaponModel);
                                hitBuffer.Clear();
                                //Debug.Log("MainMeleeWeapon start Retrieve");
                                // In Retrieve using wp.autoWeaponLocalTransform instead of wp.MeleeTargetPosition = wp.auto
                            }
                            else
                            {
                                var val = math.sin(math.radians(lerpAmount *90));
                                //TODO : Modify lerpAmount with  sine function? to make a uneven movement
                                SystemAPI.GetComponentRW<LocalTransform>(mainWeaponState.ValueRW.WeaponModel).ValueRW.Position = 
                                    math.lerp(mainWeaponState.ValueRW.MeleeOriginalPosition, mainWeaponState.ValueRW.MeleeTargetPosition, math.sin(lerpAmount * 1.5707963f));
                            }
                            break;
                        case WeaponState.Retrieve:
                            lerpAmount = 2*(mainWeaponState.ValueRW.MeleeRealShootingTimer += deltatime) / mainWeaponState.ValueRW.MeleeShootingTimer;
                            if (lerpAmount > 1f)
                            {
                                mainWeaponState.ValueRW.WeaponCurrentState = WeaponState.None;
                                mainWeaponState.ValueRW.RealCooldown = mainWeaponState.ValueRW.Cooldown;
                                //Debug.Log("MainMeleeWeapon back to None");
                            }
                            else
                            {
                                SystemAPI.GetComponentRW<LocalTransform>(mainWeaponState.ValueRW.WeaponModel).ValueRW.Position = math.lerp(mainWeaponState.ValueRO.MeleeOriginalPosition, mainWeaponState.ValueRO.mainWeaponLocalTransform.Position, lerpAmount);
                            }
                            break;
                    }
                    //if (inputState.ValueRO)
                    //{
                    //    Debug.Log("MeleeWeapon should fire");
                    //    mainWeaponState.ValueRW.RealCooldown = mainWeaponState.ValueRO.Cooldown;
                    //}
                }
                else if (inputState.ValueRO)
                {
                    mainWeaponState.ValueRW.RealCooldown = mainWeaponState.ValueRO.Cooldown;
                    var spawnee = ecb.Instantiate(mainWeaponState.ValueRO.SpawneePrefab);
                    ecb.SetComponent(spawnee, mainWeaponState.ValueRO.mainWeaponLocalTransform);
                    if (random.NextFloat() < mainWeaponState.ValueRO.WeaponCriticalHitChance + playerAttribute.ValueRO.CriticalHitChance)
                    {
                        ecb.SetComponent(spawnee, new AttackCurDamage
                        {
                            damage = (int)(mainWeaponState.ValueRO.DamageAfterBonus * mainWeaponState.ValueRO.WeaponCriticalHitRatio)
                        });
                    }

                    // request audio
                    EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList.Add(mainWeaponState.ValueRO.mainWeaponLocalTransform.Position);
                    EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList.Add(AudioEnum.NormalShoot);
                }
                //mainWeaponState.ValueRW.RealCooldown = mainWeaponState.ValueRO.Cooldown;
                //var spawnee = state.EntityManager.Instantiate(mainWeaponState.ValueRO.SpawneePrefab);
                //state.EntityManager.SetComponentData(spawnee, mainWeaponState.ValueRO.mainWeaponLocalTransform);
                //if (random.NextFloat() < mainWeaponState.ValueRO.WeaponCriticalHitChance + playerAttribute.ValueRO.CriticalHitChance)
                //{
                //    state.EntityManager.SetComponentData(spawnee, new AttackCurDamage
                //    {
                //        damage = (int)(mainWeaponState.ValueRO.DamageAfterBonus * mainWeaponState.ValueRO.WeaponCriticalHitRatio)
                //    });
                //}
            }
        }
    }
}