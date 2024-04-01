using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyNormalMeleeSystem : ISystem, ISystemStartStop
    {
        float followSpeed;
        float attackDistanceSq;
        float cooldown;
        float deathCountDown;
        int attackVal;

        float lootChance;
        Random random;
        Entity MaterialPrefab;
        public void OnCreate(ref SystemState state)
        {
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
        }
        public void OnStopRunning(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerHealthPoint = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var deltatime = SystemAPI.Time.DeltaTime;
            var up = math.up();
            foreach (var (localTransform, stateMachine, attack, death, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<EntityStateMachine>, RefRW<NormalMeleeAttack>
                , RefRW<NormalMeleeDeath>>()
                .WithEntityAccess())
            {
                var tarDir = playerLocalTransform.Position - localTransform.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
                if (stateMachine.ValueRO.CurrentState == EntityState.Follow)
                {
                    if (disSq < attackDistanceSq)
                    {
                        stateMachine.ValueRW.CurrentState = EntityState.MeleeAttack;
                    }
                    else
                    {
                        localTransform.ValueRW.Position += math.normalize(tarDir) * followSpeed * deltatime;
                        localTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                    }
                }
                else if (stateMachine.ValueRO.CurrentState == EntityState.MeleeAttack)
                {
                    if (disSq > attackDistanceSq)
                    {
                        stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        attack.ValueRW.AttackCooldown = cooldown;
                    }
                    else if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f)
                    {
                        //Debug.Log("Player should take damage");
                        playerHealthPoint.ValueRW.HealthPoint -= attackVal;
                        attack.ValueRW.AttackCooldown = cooldown;
                    }

                }
                else if (stateMachine.ValueRO.CurrentState == EntityState.Dead)
                {
                    if ((death.ValueRW.timer -= deltatime) > 0f)
                    {
                        localTransform.ValueRW.Scale = death.ValueRO.timer / deathCountDown;
                    }
                    else
                    {
                        ecb.DestroyEntity(entity);
                        if (random.NextFloat() < lootChance)
                        {
                            var material = ecb.Instantiate(MaterialPrefab);
                            ecb.SetComponent<LocalTransform>(material
                                , localTransform.ValueRO);
                        }
                    }
                }
            }
        }
    }
}