using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyNormalMeleeSystem: ISystem, ISystemStartStop
    {
        float followSpeed;
        float attackDistanceSq;
        float cooldown;
        float deathCountDown;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<NormalMeleeConfigCom>();
        }
        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<NormalMeleeConfigCom>();

            followSpeed = config.FollowSpeed;
            attackDistanceSq = config.AttackDistance * config.AttackDistance;
            cooldown = config.AttackCooldown;
            deathCountDown = config.DeathCountdown;
        }
        public void OnStopRunning(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerDamageRecord = SystemAPI.GetComponentRW<PlayerDamagedRecordCom>(playerEntity);
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();
            foreach(var (localTransform, stateMachine, attack, death,entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalMeleeStateMachine>, RefRW<NormalMeleeAttack>
                , RefRW<NormalMeleeDeath>>()
                .WithEntityAccess())
            {
                var tarDir = playerLocalTransform.Position - localTransform.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
                if (stateMachine.ValueRO.CurrentState == EnemyState.Follow)
                {
                    if(disSq < attackDistanceSq)
                    {
                        stateMachine.ValueRW.CurrentState = EnemyState.MeleeAttack;
                    }
                    else
                    {
                        localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                        localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                    }
                }else if(stateMachine.ValueRO.CurrentState == EnemyState.MeleeAttack) 
                {
                    if(disSq > attackDistanceSq)
                    {
                        stateMachine.ValueRW.CurrentState = EnemyState.Follow;
                        attack.ValueRW.AttackCooldown = cooldown;
                    }
                    else if((attack.ValueRW.AttackCooldown -= deltatime) < 0f)
                    {
                        //Debug.Log("Player should take damage");
                        playerDamageRecord.ValueRW.damagedThisFrame += attack.ValueRO.AttackVal;
                        attack.ValueRW.AttackCooldown = cooldown;
                    }
                    
                }else if(stateMachine.ValueRO.CurrentState == EnemyState.Dead)
                {
                    if((death.ValueRW.timer -= deltatime) > 0f)
                    {
                        localTransform.ValueRW.Scale = death.ValueRO.timer / deathCountDown;
                    }
                    else
                    {
                        ecb.DestroyEntity(entity);
                    }
                }
            }
        }
    }
}