using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    public partial struct EnemyNormalRangedSystem : ISystem, ISystemStartStop
    {
        float followSpeed;
        float attackDistanceSq;
        float attackCooldown;
        float deathCountdown;

        float fleeDistanceSq;
        float fleeSpeed;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
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
        }
        public void OnStopRunning(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            //var playerDamageRecord = SystemAPI.GetComponentRW<PlayerDamagedRecordCom>(playerEntity);
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
                        //TODO Spawn
                        Debug.Log("NormalRangedEnemy - Should shoot spawnee");
                        attack.ValueRW.AttackCooldown = attackCooldown;
                    }

                }else if(curstate == EntityState.Flee)
                {
                    localTransform.ValueRW.Position -= math.normalize(tarDir) * fleeSpeed * deltatime;
                    if(disSq > fleeDistanceSq * 1.6)
                    {
                        stateMachine.ValueRW.CurrentState = EntityState.RangedAttack;
                    }
                }
                else if(curstate == EntityState.Dead)
                {
                    ecb.DestroyEntity(entity);
                }
            }

        }
    }
}