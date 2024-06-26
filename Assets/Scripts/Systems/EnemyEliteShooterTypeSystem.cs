using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Rendering;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(EnemyEliteEggAndShootSystem))]
    public partial struct EnemyEliteShooterSystem : ISystem, ISystemStartStop
    {
        private Entity ColliderPrefab;
        private BatchMeshID RealMeshId;
        private float4 spawneePosOffsetX;
        private float4 spawneePosOffsetY;
        public float StageOneSpeed;
        public float StageTwoSpeed;
        public float StageOneInSkillShootingInterval;
        public float StageTwoInSkillShootingInterval;
        public int StageOneSkillShootCount;
        public int StageTwoSkillShootCount;


        int _HealthPoint;
        int _HpIncreasePerWave;
        int _BasicDamage;
        int _Damage;
        float _DmgIncreasePerWave;
        float _Speed;
        int _MaterialsDropped;
        bool isInit;
        Entity ItemPrefab;
        Entity MaterialPrefab;
        //private float spawnTimer;
        //private bool isDoingSkill;
        private Entity eliteScalingSpawneePrefab;
        //private Entity summonExplosionPrefab;
        private Random random;
        private float2 negtiveOne;
        private float2 postiveOne;
        private float3 offsetMin;
        private float3 offsetMax;
        //private bool IsResetSpawneePrefab;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<EnemyEliteShooterCom>();
            spawneePosOffsetX = new float4(-3, -1.5f, 1.5f, 3f);
            spawneePosOffsetY = new float4(2, 3.5f, 3.5f, 2);
            negtiveOne = new float2(-1, -1);
            postiveOne = new float2(1, 1);
            offsetMin = new float3(-8, 0, -8);
            offsetMax = new float3(8, 2, 8);
            random = Random.CreateFromIndex(0);

        }

        public void OnStartRunning(ref SystemState state)
        {
            if(!isInit)
            {
                var batchMeshIDContainer = SystemAPI.GetSingleton<BatchMeshIDContainer>();
                RealMeshId = batchMeshIDContainer.EnemyEliteShootMeshID;
                ColliderPrefab = SystemAPI.GetSingleton<RealColliderPrefabContainerCom>().EnemyEliteShootCollider;

                var prefabContainer = SystemAPI.GetSingleton<PrefabContainerCom>();
                eliteScalingSpawneePrefab = prefabContainer.ScalingSpawneePrefab;

                var config = SystemAPI.GetSingleton<EliteShooterConfigCom>();
                StageOneSpeed = config.StageOneSpeed;
                StageTwoSpeed = config.StageTwoSpeed;
                StageOneSkillShootCount = config.StageOneSkillShootCount;
                StageOneInSkillShootingInterval = config.StageOneInSkillShootingInterval;
                StageTwoSkillShootCount = config.StageTwoSkillShootCount;
                StageTwoInSkillShootingInterval = config.StageTwoInSkillShootingInterval;
                var basicAttribute = config.BasicAttribute;
                _HealthPoint = basicAttribute.HealthPoint;
                _HpIncreasePerWave = basicAttribute.HpIncreasePerWave;
                _BasicDamage = basicAttribute.Damage;
                _DmgIncreasePerWave = basicAttribute.DmgIncreasePerWave;
                _Speed = basicAttribute.Speed;
                _MaterialsDropped = basicAttribute.MaterialsDropped;
                ItemPrefab = prefabContainer.NormalCratePrefab;
                MaterialPrefab = prefabContainer.MaterialPrefab;
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
                SystemAPI.SetComponent(prefabBuffer[6].Prefab, new EntityHealthPoint { HealthPoint = _HealthPoint });
            }

        }

        public void OnStopRunning(ref SystemState state)
        {

        }

        public void OnUpdate(ref SystemState state)
        {
            var realCollider = state.EntityManager.GetComponentData<PhysicsCollider>(ColliderPrefab);

            var deltatime = SystemAPI.Time.DeltaTime;
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (spawnTimer, spawnTimerBit, materialAndMesh, collider, stateMachine) in SystemAPI.Query<RefRW<SpawningTimer>, EnabledRefRW<SpawningTimer>
                , RefRW<MaterialMeshInfo>
                , RefRW<PhysicsCollider>
                , RefRW<EntityStateMachine>>()
                .WithAll<EnemyEliteShooterCom>())
            {
                if ((spawnTimer.ValueRW.time -= deltatime) > 0f) continue;
                spawnTimerBit.ValueRW = false;
                materialAndMesh.ValueRW.MeshID = RealMeshId;
                //collider.ValueRW.Value.Value.SetCollisionFilter(enemyCollidesWithRayCastAndPlayerSpawnee);
                collider.ValueRW = realCollider;
                stateMachine.ValueRW.CurrentState = EntityState.StageOne;
            }
            foreach (var (eliteCom, transform, stateMachine, hp, entity) in SystemAPI.Query<RefRW<EnemyEliteShooterCom>, RefRW<LocalTransform>, RefRW<EntityStateMachine>
                , RefRO<EntityHealthPoint>>()
                .WithEntityAccess())
            {
                var tardir = playerTransform.Position - transform.ValueRO.Position;
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.StageOne:
                        if (hp.ValueRO.HealthPoint < 500)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.StageTwo;
                            eliteCom.ValueRW.MovingRandomIntervalCooldown = 0f;
                            eliteCom.ValueRW.SkillCooldownRealTimer = 0f;
                            continue;
                        }
                        transform.ValueRW.Position += eliteCom.ValueRO.TargetDirNormalizedMulSpeed * deltatime;
                        //whether to get a new moving target 
                        if ((eliteCom.ValueRW.MovingRandomIntervalCooldown -= deltatime) < 0f)
                        {

                            tardir.xz += random.NextFloat2(negtiveOne, postiveOne);
                            transform.ValueRW.Rotation = quaternion.LookRotation(tardir, math.up());
                            var distance = math.length(tardir);
                            eliteCom.ValueRW.TargetDirNormalizedMulSpeed = math.normalize(tardir) * StageOneSpeed;
                            eliteCom.ValueRW.MovingRandomIntervalCooldown = distance / StageOneSpeed + random.NextFloat(1f, 3f);
                        }

                        //whether to do skill
                        if ((eliteCom.ValueRW.SkillCooldownRealTimer -= deltatime) < 0f)
                        {
                            eliteCom.ValueRW.IsDoingSkill = true;
                            eliteCom.ValueRW.ShootLeftCount = StageOneSkillShootCount;
                            eliteCom.ValueRW.InSkillIntervalRealTimer = 0f;
                            eliteCom.ValueRW.SkillCooldownRealTimer = StageOneSkillShootCount * StageOneInSkillShootingInterval + random.NextFloat(3f, 5f);
                        }

                        // update skill logic if isDoingSkill is true at this frame
                        if (eliteCom.ValueRW.IsDoingSkill)
                        {
                            if ((eliteCom.ValueRW.InSkillIntervalRealTimer -= deltatime) < 0f)
                            {
                                eliteCom.ValueRW.InSkillIntervalRealTimer = StageOneInSkillShootingInterval;
                                var spawnee = ecb.Instantiate(eliteScalingSpawneePrefab);
                                //var elite = SystemAPI.GetSingletonEntity<EnemyEliteShooterCom>();
                                //var eliteTransform = SystemAPI.GetComponent<LocalTransform>(elite);
                                var spawnPos = transform.ValueRO.Position + transform.ValueRO.Up() * spawneePosOffsetY[eliteCom.ValueRO.CurrentSpawnPosIdx] + transform.ValueRO.Forward() * spawneePosOffsetX[eliteCom.ValueRO.CurrentSpawnPosIdx];
                                var tarDir = playerTransform.Position - spawnPos;
                                tarDir.y += 1f;
                                //tarDir += random.NextFloat3(offsetMin, offsetMax);
                                ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(tarDir, math.up()) });
                                eliteCom.ValueRW.CurrentSpawnPosIdx = (eliteCom.ValueRO.CurrentSpawnPosIdx + 1) % 4;
                                if (eliteCom.ValueRW.ShootLeftCount-- <= 0)
                                {
                                    eliteCom.ValueRW.IsDoingSkill = false;
                                }
                            }
                        }
                        break;
                    case EntityState.StageTwo:
                        //todo using tarDirNormalizedMulSpeed
                        if ((eliteCom.ValueRW.MovingRandomIntervalCooldown -= deltatime) < 0f)
                        {

                            tardir.xz += random.NextFloat2(negtiveOne, postiveOne) * 3;
                            transform.ValueRW.Rotation = quaternion.LookRotation(-tardir, math.up());
                            var distance = math.length(tardir);
                            eliteCom.ValueRW.TargetDirNormalizedMulSpeed = -math.normalize(tardir) * StageTwoSpeed;
                            eliteCom.ValueRW.MovingRandomIntervalCooldown = distance / StageTwoSpeed + random.NextFloat(1f, 3f);
                        }

                        transform.ValueRW.Position += eliteCom.ValueRO.TargetDirNormalizedMulSpeed * deltatime;
                        // whether to do skill
                        if ((eliteCom.ValueRW.SkillCooldownRealTimer -= deltatime) < 0f)
                        {
                            eliteCom.ValueRW.IsDoingSkill = true;
                            eliteCom.ValueRW.ShootLeftCount = StageTwoSkillShootCount;
                            eliteCom.ValueRW.InSkillIntervalRealTimer = 0f;
                            eliteCom.ValueRW.SkillCooldownRealTimer = StageTwoSkillShootCount * StageTwoInSkillShootingInterval + random.NextFloat(3f, 5f);
                        }

                        // update skill logic if isDoingSkill
                        if (eliteCom.ValueRW.IsDoingSkill)
                        {
                            if ((eliteCom.ValueRW.InSkillIntervalRealTimer -= deltatime) < 0f)
                            {
                                eliteCom.ValueRW.InSkillIntervalRealTimer = StageTwoInSkillShootingInterval;
                                var spawnee = ecb.Instantiate(eliteScalingSpawneePrefab);
                                var spawnPos = transform.ValueRO.Position + transform.ValueRO.Up() * spawneePosOffsetY[eliteCom.ValueRW.CurrentSpawnPosIdx] + transform.ValueRO.Forward() * spawneePosOffsetX[eliteCom.ValueRW.CurrentSpawnPosIdx];
                                var spawneeTarDir = playerTransform.Position - spawnPos;
                                spawneeTarDir.y += 1f;
                                spawneeTarDir += random.NextFloat3(offsetMin, offsetMax);
                                ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(spawneeTarDir, math.up()) });
                                eliteCom.ValueRW.CurrentSpawnPosIdx = (eliteCom.ValueRW.CurrentSpawnPosIdx + 1) % 4;
                                if (eliteCom.ValueRW.ShootLeftCount-- <= 0)
                                {
                                    eliteCom.ValueRW.IsDoingSkill = false;
                                }
                            }
                        }
                        break;
                    case EntityState.Dead:
                        var item = ecb.Instantiate(ItemPrefab);
                        ecb.SetComponent<LocalTransform>(item, transform.ValueRO);

                        for (int i = 0; i < _MaterialsDropped; ++i)
                        {
                            var material = ecb.Instantiate(MaterialPrefab);
                            ecb.SetComponent(material, new LootMoveCom { tarDir = random.NextFloat2Direction(), accumulateTimer = 0f });
                            ecb.SetComponent<LocalTransform>(material, transform.ValueRO);
                        }
                        ecb.DestroyEntity(entity);
                        break;
                }
            }

            #region Test code
            ////updates spawnTimer< 0->set to 1, spawn prefab at pos; if PosIdx >= 4,-> set not doing skill, posIdx set to 0, spawnTimer set to 0;
            ////updates scaling timer, if timer > setting, -> look rotation player need to fire, set moveTag to true;

            //// random move test code
            ////var eliteDataCom = SystemAPI.GetSingletonRW<EnemyEliteSprintAndBulletCom>();
            ////var deltatime = SystemAPI.Time.DeltaTime;
            ////var eliteEntity = SystemAPI.GetSingletonEntity<EnemyEliteSprintAndBulletCom>();
            ////if ((eliteDataCom.ValueRW.MovingTimer -= deltatime) > 0f)
            ////{
            ////    var eliteTransform = SystemAPI.GetComponentRW<LocalTransform>(eliteEntity);
            ////    eliteTransform.ValueRW.Position += eliteDataCom.ValueRO.TargetDirNormalized * deltatime * eliteDataCom.ValueRO.Speed;
            ////}

            //if (isDoingSkill)
            //{
            //    // random move test code
            //    //var playerPos = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>());
            //    //playerPos.Position.xz += random.NextFloat2(negtiveOne,postiveOne)*5;
            //    //var eliteTransform = SystemAPI.GetComponentRW<LocalTransform>(eliteEntity);
            //    //var tarDir = playerPos.Position - eliteTransform.ValueRO.Position;
            //    //eliteDataCom.ValueRW.TargetDirNormalized = math.normalize(tarDir);
            //    //eliteDataCom.ValueRW.MovingTimer = math.length(tarDir) / eliteDataCom.ValueRO.Speed;
            //    //isDoingSkill = false;

            //    //random shoot test code
            //    //ShootingSkill(ref state, 0.1f, 16, offsetMin, offsetMax);

            //    //sommoned explosion test code
            //    //var summonedExplosion = SystemAPI.GetSingletonEntity<SommonedExplosionCom>();
            //    //var transformRW = SystemAPI.GetComponentRW<LocalTransform>(summonedExplosion);
            //    //transformRW.ValueRW.Scale += 1f;
            //    //isDoingSkill = false;

            //    //sommon explosion full test
            //    var eliteEntity = SystemAPI.GetSingletonEntity<EnemyEliteShooterCom>();
            //    //var eliteTransform = SystemAPI.GetComponentRW<LocalTransform>(eliteEntity);
            //    var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            //    // instantiate at next frame. This can let the Sommon system handle the LocalTransform of the explosion;
            //    var explosion = ecb.Instantiate(summonExplosionPrefab);
            //    ecb.SetComponent<SummonedExplosionCom>(explosion, new SummonedExplosionCom { FollowingEntity = eliteEntity ,CurrentState = SummonExplosionState.Summoning});
            //    ecb.SetComponent(explosion, new AttackCurDamage { damage = 100 });
            //    isDoingSkill = false;
            //}
            //if (Input.GetKeyUp(KeyCode.Space))
            //{
            //    isDoingSkill = true;

            //    //random shoot test code
            //    //state.EntityManager.SetComponentData(eliteSpawnee, new SpawneeScalingCom { MaxScale = 0.5f });
            //}
            #endregion
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void ShootingSkill(ref SystemState state, float spawnInterval, int count, float3 offsetmin, float3 offsetmax) // four spawnee spawned at 1second interval
        //{
        //    var deltatime = SystemAPI.Time.DeltaTime;
        //    if ((spawnTimer -= deltatime) < 0f)
        //    {
        //        spawnTimer = spawnInterval;
        //        var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        //        var spawnee = ecb.Instantiate(eliteScalingSpawneePrefab);
        //        var elite = SystemAPI.GetSingletonEntity<EnemyEliteShooterCom>();
        //        var eliteTransform = SystemAPI.GetComponent<LocalTransform>(elite);
        //        var spawnPos = eliteTransform.Position + eliteTransform.Up() * spawneePosOffsetY[CurrentSpawnPosIdx] + eliteTransform.Forward() * spawneePosOffsetX[CurrentSpawnPosIdx];
        //        var tarDir = SystemAPI.GetComponentRO<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>()).ValueRO.Position - spawnPos;
        //        tarDir.y += 1f;
        //        tarDir += random.NextFloat3(offsetmin, offsetmax);
        //        ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(tarDir, math.up()) });
        //        CurrentSpawnPosIdx = (CurrentSpawnPosIdx + 1) % 4;
        //        if (++spawneeShootOutCount > count)
        //        {
        //            spawneeShootOutCount = 0;
        //            CurrentSpawnPosIdx = 0;
        //            isDoingSkill = false;
        //            spawnTimer = 0f;
        //        }
        //    }
        //}

    }

    //public struct EnemyEliteSprintAndBulletStateCom : IComponentData
    //{

    //}


}