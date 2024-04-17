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
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyNormalRangedSystem : ISystem, ISystemStartStop
    {
        private Entity ColliderPrefab;

        private CollisionFilter enemyCollidesWithRayCastAndPlayerSpawnee;
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
            enemyCollidesWithRayCastAndPlayerSpawnee = new CollisionFilter
            {
                BelongsTo = 1 << 3, // enemy layer
                CollidesWith = 1 << 1 | 1 << 5, // ray cast & player spawnee
            };
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
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

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
            foreach (var(localTransform, attack, stateMachine, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalRangedAttack>, RefRW<EntityStateMachine>>()
                .WithEntityAccess())
            {
                var tarDir = playerLocalTransform.Position - localTransform.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
                if(disSq == 0)
                {
                    continue;
                }
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.Follow:
                        localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                        localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                        if (disSq < attackDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.RangedAttack;
                        }
                        break;
                    case EntityState.Flee:
                        localTransform.ValueRW.Position -= math.normalize(tarDir) * fleeSpeed * deltatime;
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
                        else if (disSq > attackDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        }
                        if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f)
                        {

                            localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                            attack.ValueRW.AttackCooldown = attackCooldown;
                            var spawnee = ecb.Instantiate(spawneePrefab);
                            ecb.SetComponent(spawnee, localTransform.ValueRO);
                        }
                        break;
                    case EntityState.Dead:
                        if (random.NextFloat() < lootChance)
                        {
                            var material = ecb.Instantiate(MaterialPrefab);
                            ecb.SetComponent<LocalTransform>(material
                                , localTransform.ValueRO);
                        }
                        ecb.DestroyEntity(entity);
                        // request particle
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList.Add(localTransform.ValueRO.Position);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList.Add(ParticleEnum.Default);
                        break;
                }
            }

        }
    }
}