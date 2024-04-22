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
    public partial struct EnemyNormalMeleeSystem : ISystem, ISystemStartStop
    {
        private Entity ColliderPrefab;
        private BatchMeshID RealMeshId;
        float followSpeed;
        float attackDistanceSq;
        float cooldown;
        float deathCountDown;
        int attackVal;

        float lootChance;
        Random random;
        Entity MaterialPrefab;
        Entity ItemPrefab;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<NormalMeleeConfigCom>();
            random = Random.CreateFromIndex(0);

        }
        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<NormalMeleeConfigCom>();

            followSpeed = config.FollowSpeed;
            attackDistanceSq = config.AttackDistance * config.AttackDistance;
            cooldown = config.AttackCooldown;
            deathCountDown = config.DeathCountdown;
            attackVal = config.AttackVal;
            var prefabContainer = SystemAPI.GetSingleton<PrefabContainerCom>();
            MaterialPrefab = prefabContainer.MaterialPrefab;
            ItemPrefab = prefabContainer.ItemPrefab;
            var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
            RealMeshId = batchMeshIDContainer.EnemyNormalMeleeMeshID;
            ColliderPrefab = SystemAPI.GetSingleton<RealColliderPrefabContainerCom>().EnemyNormalMeleeCollider;
        }
        public void OnStopRunning(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerHealthPoint = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var realCollider = state.EntityManager.GetComponentData<PhysicsCollider>(ColliderPrefab);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();
            foreach (var (spawnTimer, spawnTimerBit, materialAndMesh, collider, stateMachine) in SystemAPI.Query<RefRW<SpawningTimer>, EnabledRefRW<SpawningTimer>
                , RefRW<MaterialMeshInfo>
                , RefRW<PhysicsCollider>
                , RefRW<EntityStateMachine>>()
                .WithAll<NormalMeleeAttack>())
            {
                if ((spawnTimer.ValueRW.time -= deltatime) > 0f) continue;
                spawnTimerBit.ValueRW = false;
                materialAndMesh.ValueRW.MeshID = RealMeshId;
                //collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                collider.ValueRW = realCollider;
                stateMachine.ValueRW.CurrentState = EntityState.Follow;
            }
            foreach (var (transform, stateMachine, attack, death, knockbackBit,knockbackCom, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<EntityStateMachine>, RefRW<NormalMeleeAttack>
                , RefRW<NormalMeleeDeath>
                , EnabledRefRW<EntityKnockBackCom>
                , RefRW<EntityKnockBackCom>>()
                .WithEntityAccess()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                var tarDir = playerLocalTransform.Position - transform.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
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
                        if ((attack.ValueRW.AttackCooldown -= deltatime) < 0 && disSq < attackDistanceSq)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.MeleeAttack;
                            attack.ValueRW.AttackCooldown = cooldown;
                        }
                        else
                        {
                            transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                            if (disSq < 4f) continue;
                            transform.ValueRW.Position += normalizedDir * followSpeed * deltatime;
                        }
                        break;

                    case EntityState.MeleeAttack:
                        stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        attack.ValueRW.AttackCooldown = cooldown;                        
                        Debug.Log("Player attacked by normal melee");
                        playerHealthPoint.ValueRW.HealthPoint -= (int)(attackVal * transform.ValueRO.Scale);
                        break;
                    case EntityState.Dead:
                        if ((death.ValueRW.timer -= deltatime) > 0f)
                        {
                            transform.ValueRW.Scale = death.ValueRO.timer / deathCountDown;
                        }
                        else
                        {
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
                        }
                        break;

                }
                //if (stateMachine.ValueRO.CurrentState == EntityState.Follow)
                //{
                //    if (disSq < attackDistanceSq)
                //    {
                //        stateMachine.ValueRW.CurrentState = EntityState.MeleeAttack;
                //    }
                //    else
                //    {
                //        localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                //        localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                //    }
                //}
                //else if (stateMachine.ValueRO.CurrentState == EntityState.MeleeAttack)
                //{
                //    if (disSq > attackDistanceSq)
                //    {
                //        stateMachine.ValueRW.CurrentState = EntityState.Follow;
                //        attack.ValueRW.AttackCooldown = cooldown;
                //    }
                //    else if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f)
                //    {
                //        //Debug.Log("Player should take damage");
                //        playerHealthPoint.ValueRW.HealthPoint -= attackVal;
                //        attack.ValueRW.AttackCooldown = cooldown;
                //    }

                //}
                //else if (stateMachine.ValueRO.CurrentState == EntityState.Dead)
                //{
                //    if ((death.ValueRW.timer -= deltatime) > 0f)
                //    {
                //        localTransform.ValueRW.Scale = death.ValueRO.timer / deathCountDown;
                //    }
                //    else
                //    {
                //        if (random.NextFloat() < lootChance)
                //        {
                //            var material = ecb.Instantiate(MaterialPrefab);
                //            ecb.SetComponent<LocalTransform>(material
                //                , localTransform.ValueRO);
                //        }
                //        ecb.DestroyEntity(entity);
                //    }
                //}
            }
        }
    }
}