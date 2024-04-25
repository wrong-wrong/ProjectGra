using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(EnemySummonerSystem))]
    public partial struct EnemyNormalSprintSystem : ISystem, ISystemStartStop
    {

        private Entity ColliderPrefab;
        private BatchMeshID RealMeshId;

        int _HealthPoint;
        int _HpIncreasePerWave;
        int _BasicDamage;
        int _Damage;
        float _DmgIncreasePerWave;
        float _Speed;
        int _MaterialsDropped;
        float _LootCrateDropRate;
        float _ConsumableDropate;
        bool isInit;

        float3 flashColorDifference;
        float sprintWaitTimerSetting;
        float deathTimer;

        float attackCoolDown;
        float sprintSpeed;
        float startSprintDistanceSq;
        float sprintDistance;
        float hitDistanceSq;

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
            if (!isInit)
            {
                isInit = true;
                var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
                RealMeshId = batchMeshIDContainer.EnemyNormalSprintMeshID;
                ColliderPrefab = SystemAPI.GetSingleton<RealColliderPrefabContainerCom>().EnemyNormalSprintCollider;

                var config = SystemAPI.GetSingleton<NormalSprintConfigCom>();
                var basicAttribute = config.BasicAttribute;
                _HealthPoint = basicAttribute.HealthPoint;
                _HpIncreasePerWave = basicAttribute.HpIncreasePerWave;
                _BasicDamage = basicAttribute.Damage;
                _DmgIncreasePerWave = basicAttribute.DmgIncreasePerWave;
                _Speed = basicAttribute.Speed;
                _MaterialsDropped = basicAttribute.MaterialsDropped;
                _LootCrateDropRate = basicAttribute.LootCrateDropRate;
                _ConsumableDropate = basicAttribute.ConsumableDropate;

                flashColorDifference = config.FlashColorDifference;
                deathTimer = config.DeathCountdown;
                attackCoolDown = config.AttackCooldown;
                sprintSpeed = config.SprintSpeed;
                startSprintDistanceSq = config.AttackDistance * config.AttackDistance;
                sprintDistance = config.AttackDistance;
                hitDistanceSq = config.HitDistance * config.HitDistance;
                sprintWaitTimerSetting = config.SprintWaitTimerSetting;
                var container = SystemAPI.GetSingleton<PrefabContainerCom>();
                MaterialPrefab = container.MaterialPrefab;
                ItemPrefab = container.ItemPrefab;
            }

            var shouldUpdate = SystemAPI.GetSingleton<GameControllShouldUpdateEnemy>();
            // Per Wave Update
            //      Needs to set EnemyPrefab's HealthPoint, update System's damage, and attribute of enemy's projectile 
            if (shouldUpdate.Value)
            {
                _HealthPoint += _HpIncreasePerWave;
                _Damage = _BasicDamage + (int)(shouldUpdate.CodingWave * _DmgIncreasePerWave);
                var prefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                SystemAPI.SetComponent(prefabBuffer[2].Prefab, new EntityHealthPoint { HealthPoint = _HealthPoint });
            }

        }
        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerHealthPoint = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var realCollider = state.EntityManager.GetComponentData<PhysicsCollider>(ColliderPrefab);
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
                //collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                collider.ValueRW = realCollider;
                stateMachine.ValueRW.CurrentState = EntityState.Follow;
            }
            foreach (var (transform, attack, stateMachine, flashCom, knockbackBit, knockbackCom, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<NormalSprintAttack>, RefRW<EntityStateMachine>
                , RefRW<FlashingCom>
                , EnabledRefRW<EntityKnockBackCom>
                , RefRW<EntityKnockBackCom>>()
                .WithEntityAccess()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                var tarDir = playerTransform.Position - transform.ValueRO.Position;
                var tarDirSq = math.csum(tarDir * tarDir);

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
                        if(attack.ValueRO.SprintTimer > 0f)
                        {
                            attack.ValueRW.SprintTimer -= deltatime;
                            transform.ValueRW.Position += attack.ValueRO.SprintDirNormalizedMulSpeed * deltatime;
                            continue;
                        }
                        if ((attack.ValueRW.AttackCooldown -= deltatime) < 0f && tarDirSq < startSprintDistanceSq * transform.ValueRO.Scale)//if (tarDirSq < startSprintDistanceSq && attack.ValueRO.AttackCooldown < 0f)
                        {
                            //Debug.Log("Setting state to Sprint");
                            stateMachine.ValueRW.CurrentState = EntityState.SprintAttack;
                            attack.ValueRW.SprintDirNormalizedMulSpeed = normalizedDir * sprintSpeed * transform.ValueRO.Scale; // by multiplying scale, trying to enhance the enemies hatched from the egg
                            attack.ValueRW.AttackCooldown = attackCoolDown;
                            attack.ValueRW.SprintTimer = 1.5f * sprintDistance / sprintSpeed; // magic number trying to let the enemy sprint for a longer distance
                            attack.ValueRW.SprintWaitTimer = sprintWaitTimerSetting;
                            ecb.SetComponentEnabled<FlashingCom>(entity,true);
                            flashCom.ValueRW.FlashColorDifference = flashColorDifference;
                            flashCom.ValueRW.Duration = 1f;
                            flashCom.ValueRW.CycleTime = 0.2f;
                        }
                        else
                        {
                            transform.ValueRW.Rotation = quaternion.LookRotation(tarDir, up);
                            if (tarDirSq < 4f) continue;
                            transform.ValueRW.Position += normalizedDir * _Speed * deltatime;
                        }
                        break;
                    case EntityState.SprintAttack:
                        if ((attack.ValueRW.SprintWaitTimer -= deltatime) > 0f) continue;
                        transform.ValueRW.Position += attack.ValueRO.SprintDirNormalizedMulSpeed * deltatime;
                        if (tarDirSq < hitDistanceSq)
                        {
                            //Debug.Log("tarDirsq < hitDistanceSq - Setting state to Sprint");
                            playerHealthPoint.ValueRW.HealthPoint -= _Damage;
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        }
                        if ((attack.ValueRW.SprintTimer -= deltatime) < 0f)
                        {
                            //Debug.Log("Timer out - Setting state to Sprint");
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        }
                        break;
                    case EntityState.Dead:
                        if (random.NextFloat() < _LootCrateDropRate)
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