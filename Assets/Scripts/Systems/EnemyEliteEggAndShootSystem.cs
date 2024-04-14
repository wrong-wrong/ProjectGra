using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyEliteEggAndShootSystem : ISystem, ISystemStartStop
    {
        private Random random;
        private float Speed;
        private float StageOneInSkillShootingInterval;
        private float SpawnEggSkillSpawningInterval;
        private int StageOneSkillShootCount;
        private int SpawnEggSkillSpawnCount;

        private Entity EggPrefab;
        private Entity NormalSpawneePrefab;

        private float4 spawneePosOffsetX;
        private float4 spawneePosOffsetY;
        private float2 negtiveOne;
        private float2 postiveOne;
        private float3 offsetMin;
        private float3 offsetMax;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<EnemyEliteEggAndShootCom>();
            random = Random.CreateFromIndex(0);
            spawneePosOffsetX = new float4(-3, -1.5f, 1.5f, 3f);
            spawneePosOffsetY = new float4(2, 3.5f, 3.5f, 2);
            negtiveOne = new float2(-1, -1);
            postiveOne = new float2(1, 1);
            offsetMin = new float3(-8, 0, -8);
            offsetMax = new float3(8, 2, 8);
        }

        public void OnStartRunning(ref SystemState state)
        {
            var prefabContainerCom = SystemAPI.GetSingleton<PrefabContainerCom>();
            NormalSpawneePrefab = prefabContainerCom.NormalEnemySpawneePrefab;

            Debug.LogWarning("using fixed number in EliteEggAndShoot to get egg prefab");
            EggPrefab = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>()[3].Prefab;
            
            var config = SystemAPI.GetSingleton<EliteEggAndShootConfig>();
            Speed = config.Speed;
            StageOneInSkillShootingInterval = config.StageOneInSkillShootingInterval;
            SpawnEggSkillSpawnCount = config.SpawnEggSkillspawnCount;
            StageOneSkillShootCount = config.StageOneSkillShootCount;
            SpawnEggSkillSpawningInterval = config.SpawnEggSkillSpawningInterval;
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (eliteCom, transform, stateMachine, hp, buffer,entity) in SystemAPI.Query<RefRW<EnemyEliteEggAndShootCom>, RefRW<LocalTransform>, RefRW<EntityStateMachine>
                ,RefRO<EntityHealthPoint>
                ,DynamicBuffer<EnemyEliteFlyingEggBuffer>>()
                .WithEntityAccess())
            {
                var tardir = playerTransform.Position - transform.ValueRO.Position;
                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.StageOne:
                        if(hp.ValueRO.HealthPoint < 500)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.StageTwo;
                            eliteCom.ValueRW.MovingRandomIntervalCooldown = 0f;
                            eliteCom.ValueRW.SkillCooldownRealTimer = 0f;
                            continue;
                        }
                        transform.ValueRW.Position += eliteCom.ValueRO.TargetDirNormalizedMulSpeed * deltatime ;
                        //whether to get a new moving target 
                        if ((eliteCom.ValueRW.MovingRandomIntervalCooldown -= deltatime) < 0f)
                        {

                            tardir.xz += random.NextFloat2(negtiveOne, postiveOne);
                            transform.ValueRW.Rotation = quaternion.LookRotation(tardir, math.up());
                            var distance = math.length(tardir);
                            eliteCom.ValueRW.TargetDirNormalizedMulSpeed = math.normalize(tardir) * Speed;
                            eliteCom.ValueRW.MovingRandomIntervalCooldown = distance / Speed + random.NextFloat(1f, 3f);
                        }

                        //whether to do skill
                        if ((eliteCom.ValueRW.SkillCooldownRealTimer -= deltatime) < 0f)
                        {
                            //var deltatime = SystemAPI.Time.DeltaTime;
                            eliteCom.ValueRW.SkillCooldownRealTimer = StageOneInSkillShootingInterval * StageOneSkillShootCount + random.NextFloat(2f, 3f);
                            eliteCom.ValueRW.IsDoingSkill = true;
                        }

                        // update skill logic if isDoingSkill is true at this frame
                        if (eliteCom.ValueRW.IsDoingSkill)
                        {
                            if ((eliteCom.ValueRW.InSkillIntervalRealTimer -= deltatime) < 0f)
                            {
                                eliteCom.ValueRW.InSkillIntervalRealTimer = StageOneInSkillShootingInterval;
                                //var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                                var spawnee = ecb.Instantiate(NormalSpawneePrefab);
                                //var elite = SystemAPI.GetSingletonEntity<EnemyEliteShooterCom>();
                                //var eliteTransform = SystemAPI.GetComponent<LocalTransform>(elite);
                                var spawnPos = transform.ValueRO.Position + transform.ValueRO.Up() * spawneePosOffsetY[eliteCom.ValueRW.CurrentSpawnPosIdx] + transform.ValueRO.Forward() * spawneePosOffsetX[eliteCom.ValueRW.CurrentSpawnPosIdx];
                                var spawnDir = playerTransform.Position - spawnPos;
                                spawnDir.y += 1f;
                                //tarDir += random.NextFloat3(offsetMin, offsetMax);
                                ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(spawnDir, math.up()) });
                                eliteCom.ValueRW.CurrentSpawnPosIdx = (eliteCom.ValueRW.CurrentSpawnPosIdx + 1) % 4;
                                if (--eliteCom.ValueRW.SkillCountLeft == 0)
                                {
                                    eliteCom.ValueRW.SkillCountLeft = StageOneSkillShootCount;
                                    eliteCom.ValueRW.CurrentSpawnPosIdx = 0;
                                    eliteCom.ValueRW.IsDoingSkill = false;
                                    //eliteCom.ValueRW.SkillCooldownRealTimer = StageOneInSkillShootingInterval * StageOneSkillShootCount + random.NextFloat(2f, 3f);
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
                            eliteCom.ValueRW.TargetDirNormalizedMulSpeed = -math.normalize(tardir) * Speed;
                            eliteCom.ValueRW.MovingRandomIntervalCooldown = distance / Speed + random.NextFloat(1f, 3f);
                        }

                        transform.ValueRW.Position += eliteCom.ValueRO.TargetDirNormalizedMulSpeed * deltatime;
                        // whether to do skill
                        if ((eliteCom.ValueRW.SkillCooldownRealTimer -= deltatime) < 0f)
                        {
                            eliteCom.ValueRW.IsDoingSkill = true;
                            eliteCom.ValueRW.SkillCountLeft = SpawnEggSkillSpawnCount;
                            eliteCom.ValueRW.InSkillIntervalRealTimer = 0f;
                            eliteCom.ValueRW.SkillCooldownRealTimer = SpawnEggSkillSpawnCount * SpawnEggSkillSpawningInterval + random.NextFloat(3f, 5f);
                        }

                        // update skill logic if isDoingSkill
                        if (eliteCom.ValueRW.IsDoingSkill)
                        {
                            #region skill code

                            //moving egg
                            for (int i = 0, n = buffer.Length; i < n; i++)
                            {
                                ref var singleEgg = ref buffer.ElementAt(i);
                                var realTimer = (singleEgg.EggTimer += deltatime) / 2f;
                                //Debug.Log("EggTimer" + singleEgg.EggTimer);
                                //Debug.Log("realTimer" + realTimer);
                                if (realTimer > 1f)
                                {
                                    if (i == n - 1)
                                    {
                                        eliteCom.ValueRW.IsDoingSkill = false;
                                        buffer.Clear();
                                    }
                                }
                                else
                                {
                                    if (!state.EntityManager.Exists(singleEgg.EggInstance)) continue;
                                    var eggTransformRW = SystemAPI.GetComponentRW<LocalTransform>(singleEgg.EggInstance);
                                    eggTransformRW.ValueRW.Position.xz = math.lerp(singleEgg.StartPos, singleEgg.EndPos, realTimer);
                                    eggTransformRW.ValueRW.Position.y = math.sin(math.radians(realTimer * 270)) * 2 + 2;
                                }
                            }
                            if ((eliteCom.ValueRW.InSkillIntervalRealTimer -= deltatime) < 0f && eliteCom.ValueRW.SkillCountLeft-- > 0)
                            {
                                eliteCom.ValueRW.InSkillIntervalRealTimer = SpawnEggSkillSpawningInterval;

                                var originalPos = new float3 { x = transform.ValueRO.Position.x, z = transform.ValueRO.Position.z, y = 1f };

                                var egg = ecb.Instantiate(EggPrefab);
                                ecb.SetComponent(EggPrefab, new LocalTransform { Position = originalPos, Scale = 1f, Rotation = quaternion.identity });
                                ecb.AppendToBuffer<EnemyEliteFlyingEggBuffer>(entity, new EnemyEliteFlyingEggBuffer
                                {
                                    EggInstance = egg,
                                    StartPos = originalPos.xz,
                                    EndPos = originalPos.xz + random.NextFloat2(new float2(-6, -6), new float2(6, 6))
                                    ,
                                    EggTimer = 0f
                                });

                            }
                            #endregion
                        }
                        break;
                    case EntityState.Dead:
                        ecb.DestroyEntity(entity);
                        break;
                }
            }
        }
    }
}