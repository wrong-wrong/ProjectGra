using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyNormalSprintSystem : ISystem, ISystemStartStop
    {
        private CollisionFilter enemyCollidesWithRayCastAndPlayerSpawnee;
        private BatchMeshID RealMeshId;
        float followSpeed;
        float deathTimer;

        int attackVal;
        float attackCoolDown;
        float sprintSpeed;
        float startSprintDistanceSq;
        float sprintDistance;
        float hitDistanceSq;

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
            var config = SystemAPI.GetSingleton<NormalSprintConfigCom>();
            followSpeed = config.FollowSpeed;
            deathTimer = config.DeathCountdown;
            attackCoolDown = config.AttackCooldown;
            sprintSpeed = config.SprintSpeed;
            startSprintDistanceSq = config.AttackDistance * config.AttackDistance;
            sprintDistance = config.AttackDistance;
            hitDistanceSq = config.HitDistance * config.HitDistance;
            lootChance = config.LootChance;
            attackVal = config.AttackVal;
            var container = SystemAPI.GetSingleton<PrefabContainerCom>();
            MaterialPrefab = container.MaterialPrefab;
            ItemPrefab = container.ItemPrefab;
            var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
            RealMeshId = batchMeshIDContainer.EnemyNormalSprintMeshID;
        }
        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerHealthPoint = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);

            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();
            foreach (var (spawnTimer, spawnTimerBit, materialAndMesh, collider, stateMachine) in SystemAPI.Query<RefRW<SpawningTimer>, EnabledRefRW<SpawningTimer>
                , RefRW<MaterialMeshInfo>
                , RefRW<PhysicsCollider>
                , RefRW<EntityStateMachine>>()
                .WithAll<NormalSprintAttack>())
            {
                if ((spawnTimer.ValueRW.time -= deltatime) > 0f) continue;
                spawnTimerBit.ValueRW = false;
                materialAndMesh.ValueRW.MeshID = RealMeshId;
                collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                stateMachine.ValueRW.CurrentState = EntityState.Follow;
            }
            foreach (var (localTransform, attack, stateMachine, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalSprintAttack>, RefRW<EntityStateMachine>>()
                .WithEntityAccess())
            {
                var tarDir = playerTransform.Position - localTransform.ValueRO.Position;
                var tarDirSq = math.csum(tarDir * tarDir);
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.Follow:

                        if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f && tarDirSq < startSprintDistanceSq)//if (tarDirSq < startSprintDistanceSq && attack.ValueRO.AttackCooldown < 0f)
                        {
                            //Debug.Log("Setting state to Sprint");
                            stateMachine.ValueRW.CurrentState = EntityState.SprintAttack;
                            attack.ValueRW.SprintDirNormalized = math.normalize(tarDir);
                            attack.ValueRW.AttackCooldown = attackCoolDown;
                            attack.ValueRW.SprintTimer = 1.5f * sprintDistance / sprintSpeed; // magic number trying to let the enemy sprint for a longer distance
                        }
                        else
                        {
                            localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                            if (tarDirSq < 4f) continue;
                            localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                        }
                        break;
                    case EntityState.SprintAttack:
                        localTransform.ValueRW.Position += attack.ValueRO.SprintDirNormalized * sprintSpeed * deltatime;
                        if (tarDirSq < hitDistanceSq)
                        {
                            //Debug.Log("tarDirsq < hitDistanceSq - Setting state to Sprint");
                            playerHealthPoint.ValueRW.HealthPoint -= attackVal;
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        }
                        if ((attack.ValueRW.SprintTimer -= deltatime) < 0f)
                        {
                            //Debug.Log("Timer out - Setting state to Sprint");
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
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
                        break;
                }
            }
        }
    }
}