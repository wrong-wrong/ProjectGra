using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(EnemySummonerSystem))]
    public partial struct EnemyNormalRangedSystem : ISystem, ISystemStartStop
    {
        private Entity ColliderPrefab;
        private BatchMeshID RealMeshId;
        float followSpeed;
        float attackDistanceSq;
        float attackCooldown;
        float deathCountdown;

        float fleeDistanceSq;
        float fleeSpeed;

        int attackVal;

        Entity spawneePrefab;

        float lootChance;
        Random random;
        Entity MaterialPrefab;
        Entity ItemPrefab;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            random = Random.CreateFromIndex(0);

        }

        public void OnStartRunning(ref SystemState state) 
        {
            var prefabContainerCom = SystemAPI.GetSingleton<PrefabContainerCom>();
            spawneePrefab = prefabContainerCom.NormalEnemySpawneePrefab;
            var config = SystemAPI.GetSingleton<NormalRangedConfigCom>();
            followSpeed = config.FollowSpeed;
            attackDistanceSq = config.AttackDistance * config.AttackDistance;
            attackCooldown = config.AttackCooldown;
            deathCountdown = config.DeathCountdown;
            fleeDistanceSq = config.FleeDistance * config.FleeDistance;
            fleeSpeed = config.FleeSpeed;
            attackVal = config.AttackVal;
            state.EntityManager.SetComponentData(spawneePrefab, new SpawneeTimer { Value = config.SpawneeTimer });
            state.EntityManager.SetComponentData(spawneePrefab, new AttackCurDamage { damage = attackVal });
            var container = SystemAPI.GetSingleton<PrefabContainerCom>();
            MaterialPrefab = container.MaterialPrefab;
            ItemPrefab = container.ItemPrefab;
            var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
            RealMeshId = batchMeshIDContainer.EnemyNormalRangedMeshID;
            ColliderPrefab = SystemAPI.GetSingleton<RealColliderPrefabContainerCom>().EnemyNormalRangedCollider;

        }
        public void OnStopRunning(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();
            var realCollider = state.EntityManager.GetComponentData<PhysicsCollider>(ColliderPrefab);

            foreach (var (spawnTimer, spawnTimerBit, materialAndMesh, collider, stateMachine) in SystemAPI.Query<RefRW<SpawningTimer>, EnabledRefRW<SpawningTimer>
                , RefRW<MaterialMeshInfo>
                , RefRW<PhysicsCollider>
                , RefRW<EntityStateMachine>>()
                .WithAll<NormalRangedAttack>())
            {
                if ((spawnTimer.ValueRW.time -= deltatime) > 0f) continue;
                spawnTimerBit.ValueRW = false;
                materialAndMesh.ValueRW.MeshID = RealMeshId;
                //collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                collider.ValueRW = realCollider;
                stateMachine.ValueRW.CurrentState = EntityState.Follow;
            }
            foreach (var(transform, attack, stateMachine, knockbackBit, knockbackCom, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalRangedAttack>, RefRW<EntityStateMachine>
                , EnabledRefRW<EntityKnockBackCom>
                , RefRW<EntityKnockBackCom>>()
                .WithEntityAccess()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {

                var tarDir = playerLocalTransform.Position - transform.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
                if(disSq == 0)
                {
                    continue;
                }
                var normalizedDir = math.normalize(tarDir);
                if (knockbackBit.ValueRO)
                {
                    transform.ValueRW.Position += normalizedDir * -10 * deltatime;
                    if ((knockbackCom.ValueRW.Timer -= deltatime) < 0)
                    {
                        knockbackBit.ValueRW = false;
                        knockbackCom.ValueRW.Timer = 0.3f;
                    }
                }
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.Follow:
                        transform.ValueRW.Position += normalizedDir * followSpeed * deltatime;
                        transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                        if (disSq < attackDistanceSq * transform.ValueRO.Scale)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.RangedAttack;
                        }
                        break;
                    case EntityState.Flee:
                        transform.ValueRW.Position -= normalizedDir * fleeSpeed * deltatime;
                        if (disSq > fleeDistanceSq * 1.6)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.RangedAttack;
                        }
                        break;
                    case EntityState.RangedAttack:
                        if (disSq < fleeDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Flee;
                        }
                        else if (disSq > attackDistanceSq * 1.2f * transform.ValueRO.Scale)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        }
                        if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f)
                        {

                            transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                            attack.ValueRW.AttackCooldown = attackCooldown;
                            var spawnee = ecb.Instantiate(spawneePrefab);
                            ecb.SetComponent(spawnee, transform.ValueRO);
                        }
                        break;
                    case EntityState.Dead:
                        if (random.NextFloat() < lootChance)
                        {
                            var material = ecb.Instantiate(MaterialPrefab);
                            ecb.SetComponent<LocalTransform>(material
                                , transform.ValueRO);
                        }
                        ecb.DestroyEntity(entity);
                        // request particle
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList.Add(transform.ValueRO.Position);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList.Add(ParticleEnum.Default);
                        break;
                }
            }

        }
    }
}