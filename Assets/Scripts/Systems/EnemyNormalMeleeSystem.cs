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

        float attackDistanceSq;
        float cooldown;
        float deathCountDown;

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
            isInit = false;
        }
        public void OnStartRunning(ref SystemState state)
        {
            // Initialization
            if (!isInit)
            {
                isInit = true;
                var config = SystemAPI.GetSingleton<NormalMeleeConfigCom>();
                var basicAttribute = config.BasicAttribute;
                _HealthPoint = basicAttribute.HealthPoint;
                _HpIncreasePerWave = basicAttribute.HpIncreasePerWave;
                _BasicDamage = basicAttribute.Damage;
                _DmgIncreasePerWave = basicAttribute.DmgIncreasePerWave;
                _Speed = basicAttribute.Speed;
                _MaterialsDropped = basicAttribute.MaterialsDropped;
                _LootCrateDropRate = basicAttribute.LootCrateDropRate;
                _ConsumableDropate = basicAttribute.ConsumableDropate;
                var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
                RealMeshId = batchMeshIDContainer.EnemyNormalMeleeMeshID;
                ColliderPrefab = SystemAPI.GetSingleton<RealColliderPrefabContainerCom>().EnemyNormalMeleeCollider;

                // Enemy Specific Initialization
                attackDistanceSq = config.AttackDistance * config.AttackDistance;
                cooldown = config.AttackCooldown;
                deathCountDown = config.DeathCountdown;

                // Might put aside
                var prefabContainer = SystemAPI.GetSingleton<PrefabContainerCom>();
                MaterialPrefab = prefabContainer.MaterialPrefab;
                ItemPrefab = prefabContainer.NormalCratePrefab;
            }
            var shouldUpdate = SystemAPI.GetSingleton<GameControllShouldUpdateEnemy>();
            // Per Wave Update
            //      Needs to set EnemyPrefab's HealthPoint, update System's damage, and attribute of enemy's projectile 
            if (shouldUpdate.Value)
            {
                _HealthPoint += _HpIncreasePerWave;
                _Damage = _BasicDamage + (int)(shouldUpdate.CodingWave * _DmgIncreasePerWave);
                var attModifier = SystemAPI.GetSingleton<EnemyHpAndDmgModifierWithDifferentDifficulty>();
                _HealthPoint = (int)(_HealthPoint * attModifier.HealthPointModifier);
                _Damage = math.max((int)(_Damage * attModifier.DamageModifier), 1);
                var prefabBuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                SystemAPI.SetComponent(prefabBuffer[0].Prefab, new EntityHealthPoint { HealthPoint = _HealthPoint });
            }


        }
        public void OnStopRunning(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            //var playerHealthPoint = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
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
            foreach (var (transform, stateMachine, attack, death, knockbackBit, knockbackCom, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<EntityStateMachine>, RefRW<NormalMeleeAttack>
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
                            transform.ValueRW.Position += normalizedDir * _Speed * deltatime;
                        }
                        break;

                    case EntityState.MeleeAttack:
                        stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        attack.ValueRW.AttackCooldown = cooldown;
                        //Debug.Log("Player attacked by normal melee");
                        //playerHealthPoint.ValueRW.HealthPoint -= (int)(_Damage * transform.ValueRO.Scale);
                        ecb.AppendToBuffer<PlayerDamagedRecordBuffer>(playerEntity, new PlayerDamagedRecordBuffer { Value = (int)(_Damage * transform.ValueRO.Scale) });
                        break;
                    case EntityState.Dead:

                        if (random.NextFloat() < _LootCrateDropRate)
                        {
                            var item = ecb.Instantiate(ItemPrefab);
                            ecb.SetComponent<LocalTransform>(item
                                , transform.ValueRO);
                        }
                        for (int i = 0; i < _MaterialsDropped; ++i)
                        {
                            var material = ecb.Instantiate(MaterialPrefab);
                            ecb.SetComponent(material, new LootMoveCom { tarDir = random.NextFloat2Direction(), accumulateTimer = 0f });
                            ecb.SetComponent<LocalTransform>(material, transform.ValueRO);
                        }
                        ecb.DestroyEntity(entity);
                        // request particle
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList.Add(transform.ValueRO.Position);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList.Add(ParticleEnum.Default);

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