using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyNormalSprintSystem : ISystem,ISystemStartStop
    {
        float followSpeed;
        float deathTimer;

        float attackCoolDown;
        float sprintSpeed;
        float startSprintDistanceSq;
        float sprintDistance;
        float hitDistanceSq;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
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
        }
        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerDamageRecord = SystemAPI.GetComponentRW<PlayerDamagedRecordCom>(playerEntity);

            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();
            foreach (var (localTransform, attack,stateMachine,entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalSprintAttack>, RefRW<NormalSprintStateMachine>>()
                .WithEntityAccess())
            {
                var tarDir = playerTransform.Position - localTransform.ValueRO.Position;
                var tarDirSq = math.csum(tarDir * tarDir);
                if (stateMachine.ValueRO.CurrentState == EnemyState.Follow)
                {
                    attack.ValueRW.AttackCooldown -= deltatime;
                    if (tarDirSq < 0.001f) continue;
                    localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                    localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                    if (tarDirSq < startSprintDistanceSq && attack.ValueRO.AttackCooldown < 0f)
                    {
                        Debug.Log("Setting state to Sprint");
                        stateMachine.ValueRW.CurrentState = EnemyState.SprintAttack;
                        attack.ValueRW.SprintDirNormalized = math.normalize(tarDir);
                        attack.ValueRW.AttackCooldown = attackCoolDown;
                        attack.ValueRW.SprintTimer = 1.5f * sprintDistance / sprintSpeed; // magic number trying to let the enemy sprint for a longer distance
                    }

                }else if(stateMachine.ValueRO.CurrentState == EnemyState.SprintAttack)
                {
                    localTransform.ValueRW.Position += attack.ValueRO.SprintDirNormalized * sprintSpeed * deltatime;
                    if (tarDirSq < hitDistanceSq)
                    {
                        Debug.Log("tarDirsq < hitDistanceSq - Setting state to Sprint");
                        playerDamageRecord.ValueRW.damagedThisFrame += attack.ValueRO.AttackVal;
                        stateMachine.ValueRW.CurrentState = EnemyState.Follow;
                    }
                    if ((attack.ValueRW.SprintTimer -= deltatime)< 0f)
                    {
                        Debug.Log("Timer out - Setting state to Sprint");
                        stateMachine.ValueRW.CurrentState = EnemyState.Follow;
                    }
                    
                }else if(stateMachine.ValueRO.CurrentState == EnemyState.Dead)
                {
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}