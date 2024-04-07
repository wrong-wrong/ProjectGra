using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyNormalRangedSystem : ISystem, ISystemStartStop
    {
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
            var config = SystemAPI.GetSingleton<NormalRangedConfigCom>();
            followSpeed = config.FollowSpeed;
            attackDistanceSq = config.AttackDistance * config.AttackDistance;
            attackCooldown = config.AttackCooldown;
            deathCountdown = config.DeathCountdown;
            fleeDistanceSq = config.FleeDistance * config.FleeDistance;
            fleeSpeed = config.FleeSpeed;
            spawneePrefab = config.SpawneePrefab;
            attackVal = config.AttackVal;
            state.EntityManager.SetComponentData(spawneePrefab, new SpawneeTimer { Value = config.SpawneeTimer });
            state.EntityManager.SetComponentData(spawneePrefab, new SpawneeCurDamage { damage = attackVal });
            var container = SystemAPI.GetSingleton<PrefabContainerCom>();
            MaterialPrefab = container.MaterialPrefab;
            ItemPrefab = container.ItemPrefab;

        }
        public void OnStopRunning(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();

            foreach (var(localTransform, attack, stateMachine, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalRangedAttack>, RefRW<EntityStateMachine>>()
                .WithEntityAccess())
            {
                var curstate = stateMachine.ValueRO.CurrentState;
                var tarDir = playerLocalTransform.Position - localTransform.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
                if(disSq == 0)
                {
                    continue;
                }
                if (curstate == EntityState.Follow)
                {
                    localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                    localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                    if(disSq < attackDistanceSq)
                    {
                        stateMachine.ValueRW.CurrentState = EntityState.RangedAttack;
                    }
                }else if(curstate == EntityState.RangedAttack)
                {
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

                }
                else if(curstate == EntityState.Flee)
                {
                    localTransform.ValueRW.Position -= math.normalize(tarDir) * fleeSpeed * deltatime;
                    if(disSq > fleeDistanceSq * 1.6)
                    {
                        stateMachine.ValueRW.CurrentState = EntityState.RangedAttack;
                    }
                }
                else if(curstate == EntityState.Dead)
                {
                    if (random.NextFloat() < lootChance)
                    {
                        var material = ecb.Instantiate(MaterialPrefab);
                        ecb.SetComponent<LocalTransform>(material
                            , localTransform.ValueRO);
                    }
                    ecb.DestroyEntity(entity);
                }
            }

        }
    }
}