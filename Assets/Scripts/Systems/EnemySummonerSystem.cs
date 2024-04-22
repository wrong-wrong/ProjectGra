using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(EnemyEliteSprintAndShootSystem))]
    public partial struct EnemySummonerSystem : ISystem, ISystemStartStop
    {
        private Entity ColliderPrefab;
        private BatchMeshID RealMeshId;
        private Entity summonExplosionPrefab;
        private Random random;
        private float normalSpeed;
        private float chasingSpeed;
        private float floatingCycleSpeed;
        private float floatingRange;
        private float calculatedMulOfCycleSpeed;
        private float fleeDistanceSq;
        private float summonDistanceSq;
        private float chasingDistanceSq;
        private float explodeDistanceSq;
        private float attackCooldown;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<EnemySummonerDeath>();
            random = Random.CreateFromIndex(0);
        }

        public void OnStartRunning(ref SystemState state)
        {
            var prefabContainer = SystemAPI.GetSingleton<PrefabContainerCom>();
            summonExplosionPrefab = prefabContainer.SummonExplosionPrefab;
            var config = SystemAPI.GetSingleton<EnemySummonerConfigCom>();
            floatingCycleSpeed = config.EnemySummonerFloatingCycleSpeed;
            floatingRange = config.EnemySummonerFloatingRange;
            normalSpeed = config.EnemySummonerSpeed;
            chasingSpeed = config.EnemySummonerChasingSpeed;
            fleeDistanceSq = config.EnemySummonerFleeDistance * config.EnemySummonerFleeDistance;
            chasingDistanceSq = config.EnemySummonerChasingDistance * config.EnemySummonerChasingDistance;
            summonDistanceSq = config.EnemySummonerSummonDistance * config.EnemySummonerSummonDistance;
            attackCooldown = config.EnemySummonerAttackCooldown;
            explodeDistanceSq = config.EnemySummonerExplodeDistance * config.EnemySummonerExplodeDistance;
            //Debug.Log(math.sin(1.5707963));     // 1
            calculatedMulOfCycleSpeed = 360 * 0.0174532925f / floatingCycleSpeed;
            var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
            RealMeshId = batchMeshIDContainer.EnemySummonerMeshID;

            ColliderPrefab = SystemAPI.GetSingleton<RealColliderPrefabContainerCom>().EnemySummonerCollider;
        }

        public void OnStopRunning(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var deltatime = SystemAPI.Time.DeltaTime;
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var realCollider = state.EntityManager.GetComponentData<PhysicsCollider>(ColliderPrefab);
            foreach (var (spawnTimer, spawnTimerBit, materialAndMesh, collider, stateMachine) in SystemAPI.Query<RefRW<SpawningTimer>, EnabledRefRW<SpawningTimer>
                , RefRW<MaterialMeshInfo>
                , RefRW<PhysicsCollider>
                , RefRW<EntityStateMachine>>()
                .WithAll<EnemySummonerMovement>())
            {
                if ((spawnTimer.ValueRW.time -= deltatime) > 0f) continue;
                spawnTimerBit.ValueRW = false;
                materialAndMesh.ValueRW.MeshID = RealMeshId;
                //collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                collider.ValueRW = realCollider;
                stateMachine.ValueRW.CurrentState = EntityState.Init;
            }
            foreach (var (stateMachine, movement, attack, transform, scalingBit, scalingCom, entity) in SystemAPI.Query<RefRW<EntityStateMachine>, RefRW<EnemySummonerMovement>, RefRW<EnemySummonerAttack>
                , RefRW<LocalTransform>, EnabledRefRW<EntityScalingCom>
                , RefRO<EntityScalingCom>>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)
                .WithEntityAccess())
            {
                // handle floating timer and pos.y    math.radians(timer * 360 / cycleSpeed  * deltatime% 360)  
                //movement.ValueRW.floatingTimer += deltatime;
                //transform.ValueRW.Position.y = floatingRange * math.sin(math.radians(movement.ValueRO.floatingTimer * 360 / floatingCycleSpeed));

                // can be modified into 
                // float radians(float x) { return x * 0.0174532925f; }
                // current solution will cause position flash when instatiated . if want to fix this ,summoner should be instantiated at the height of floating range and modified with += floatingRange* math.sin(...)
                transform.ValueRW.Position.y = floatingRange + floatingRange * math.sin((movement.ValueRW.floatingTimer += deltatime) * calculatedMulOfCycleSpeed);
                // handle tarDirMulSpeed * deltatime in different state
                var tarDir = playerTransform.Position - transform.ValueRO.Position;
                tarDir.y = 0;
                var distanceSq = math.lengthsq(tarDir.xz);

                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.Init:
                        // set tarDir and go to Follow
                        stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        tarDir.xz = math.normalize(random.NextFloat2(float2.zero, tarDir.xz));
                        transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
                        movement.ValueRW.tarDirMulSpeed = tarDir.xz * normalSpeed;
                        break;
                    case EntityState.Follow:
                        // update pos using tarDir
                        transform.ValueRW.Position.xz += movement.ValueRO.tarDirMulSpeed * deltatime;
                        // distanceSq < summonDisSq, go to summon state
                        if (distanceSq < summonDistanceSq * transform.ValueRO.Scale)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Summon;
                        }
                        break;
                    case EntityState.Summon:
                        // distanceSq < fleeDisSq, go to flee state, and set random tarDirNormalizeMulSpeed & rotation
                        if (distanceSq < fleeDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Flee;
                            tarDir.xz = -math.normalize(random.NextFloat2(float2.zero, tarDir.xz));
                            transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
                            movement.ValueRW.tarDirMulSpeed = tarDir.xz * normalSpeed;
                            continue;
                        }
                        // distanceSq > summonDisSq, go to follow state, and set random tarDirNormalizeMulSpeed & rotaion
                        if (distanceSq > summonDistanceSq * 1.5f * transform.ValueRO.Scale)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                            tarDir.xz = math.normalize(random.NextFloat2(float2.zero, tarDir.xz));
                            transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
                            movement.ValueRW.tarDirMulSpeed = tarDir.xz * normalSpeed;
                            continue;
                        }
                        // set rotaion every frame
                        transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());

                        // if cooldown under zero, spawn summon explosion and set its component
                        if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f)
                        {
                            var explosion = ecb.Instantiate(summonExplosionPrefab);
                            ecb.SetComponent(explosion, new SummonedExplosionCom
                            {
                                FollowingEntity = entity,
                            });
                            attack.ValueRW.AttackCooldown = attackCooldown;
                        }
                        break;
                    case EntityState.Flee:
                        // distanceSq < chasingDisSq, go to chasing state, 
                        if (distanceSq < chasingDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.ChasingToSelfDestruct;
                            continue;
                        }
                        // distanceSq > fleeDisSq * 1.5f, go to summon state, set rotation,
                        if (distanceSq > fleeDistanceSq * 1.5f)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Summon;
                            continue;
                        }
                        // update pos using tarDir
                        transform.ValueRW.Position.xz += movement.ValueRO.tarDirMulSpeed * deltatime;
                        break;
                    case EntityState.ChasingToSelfDestruct:
                        // update position and rotation every frame 
                        if (distanceSq < explodeDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Dead;
                            continue;
                        }
                        transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
                        transform.ValueRW.Position += transform.ValueRO.Forward() * chasingSpeed * deltatime;
                        // distanceSq < explodeDisSq, go to dead state
                        break;
                    case EntityState.Dead:
                        // set scaling component and go to explode state
                        scalingBit.ValueRW = true;
                        stateMachine.ValueRW.CurrentState = EntityState.Explode;
                        break;
                    case EntityState.Explode:
                        // check scaling ratio to decide whether to destory self
                        if (scalingCom.ValueRO.Ratio > 1f)
                        {
                            ecb.DestroyEntity(entity);

                            EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList.Add(transform.ValueRO.Position);
                            EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList.Add(AudioEnum.Explode);
                            var explosion = ecb.Instantiate(summonExplosionPrefab);
                            ecb.SetComponent<LocalTransform>(explosion, new LocalTransform { Position = transform.ValueRO.Position , Scale = 1f, Rotation = quaternion.identity});
                        }
                        break;
                }


            }
        }

    }
}