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
        private bool isDoingSkill;
        //private Entity eggPrefab;
        private Random random;
        private float spawnTimer;
        private float spawnIntervalTimer;
        private float spawnIntervalRealTimer;
        private int spawnCount;
        private int spawneeShootOutCount;

        private Entity EggPrefab;
        private Entity NormalSpawneePrefab;

        private float4 spawneePosOffsetX;
        private float4 spawneePosOffsetY;
        private float2 negtiveOne;
        private float2 postiveOne;
        private float3 offsetMin;
        private float3 offsetMax;
        private int currentSpawnPosIdx;

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
            var config = SystemAPI.GetSingleton<PrefabContainerCom>();
            NormalSpawneePrefab = config.NormalEnemySpawneePrefab;

            Debug.LogWarning("using fixed number in EliteEggAndShoot to get egg prefab");
            EggPrefab = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>()[3].Prefab;

        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {


            //if (isDoingSkill)
            //{

            //}
            //if (Input.GetKeyUp(KeyCode.Space)) // use random timer to replace GetKeyUp
            //{
            //    isDoingSkill = true;
            //    spawnCount = 5;
            //    spawnIntervalTimer = 0.5f;
            //    spawnIntervalRealTimer = 0f;
            //}

            var elite = SystemAPI.GetSingletonEntity<EnemyEliteEggAndShootCom>();
            var eliteCom = SystemAPI.GetSingletonRW<EnemyEliteEggAndShootCom>();
            var eliteTransformRW = SystemAPI.GetComponentRW<LocalTransform>(elite);
            var hp = SystemAPI.GetComponent<EntityHealthPoint>(elite);
            var deltatime = SystemAPI.Time.DeltaTime;
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>());
            if (hp.HealthPoint > 500f)
            {
                //state related code
                eliteCom.ValueRW.previousHp = hp.HealthPoint;

                //move logic
                eliteTransformRW.ValueRW.Position += eliteCom.ValueRO.TargetDirNormalized * deltatime * eliteCom.ValueRO.Speed;
                eliteTransformRW.ValueRW.Rotation = quaternion.LookRotation(eliteCom.ValueRO.TargetDirNormalized, math.up());
                //whether to get a new moving target 
                if ((eliteCom.ValueRW.MovingRandomIntervalTimer -= deltatime) < 0f)
                {

                    var tardir = playerTransform.Position - eliteTransformRW.ValueRO.Position;
                    tardir.xz += random.NextFloat2(negtiveOne, postiveOne);
                    var distance = math.distance(playerTransform.Position, eliteTransformRW.ValueRO.Position);
                    eliteCom.ValueRW.TargetDirNormalized = math.normalize(tardir);
                    eliteCom.ValueRW.MovingRandomIntervalTimer = distance / eliteCom.ValueRO.Speed + random.NextFloat(1f, 3f);
                }

                //whether to do skill
                if ((eliteCom.ValueRW.SkillShootRandomIntervalTimer -= deltatime) < 0f)
                {
                    //var deltatime = SystemAPI.Time.DeltaTime;
                    isDoingSkill = true;
                }

                // update skill logic if isDoingSkill is true at this frame
                if (isDoingSkill)
                {
                    if ((spawnTimer -= deltatime) < 0f)
                    {
                        spawnTimer = eliteCom.ValueRO.stateOneInSkillShootingInterval;
                        var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                        var spawnee = ecb.Instantiate(NormalSpawneePrefab);
                        //var elite = SystemAPI.GetSingletonEntity<EnemyEliteShooterCom>();
                        //var eliteTransform = SystemAPI.GetComponent<LocalTransform>(elite);
                        var spawnPos = eliteTransformRW.ValueRO.Position + eliteTransformRW.ValueRO.Up() * spawneePosOffsetY[currentSpawnPosIdx] + eliteTransformRW.ValueRO.Forward() * spawneePosOffsetX[currentSpawnPosIdx];
                        var tarDir = playerTransform.Position - spawnPos;
                        tarDir.y += 1f;
                        //tarDir += random.NextFloat3(offsetMin, offsetMax);
                        ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(tarDir, math.up()) });
                        currentSpawnPosIdx = (currentSpawnPosIdx + 1) % 4;
                        if (++spawneeShootOutCount > eliteCom.ValueRO.stageOneSkillShootCount)
                        {
                            spawneeShootOutCount = 0;
                            currentSpawnPosIdx = 0;
                            isDoingSkill = false;
                            spawnTimer = 0f;
                            eliteCom.ValueRW.SkillShootRandomIntervalTimer = eliteCom.ValueRO.stateOneInSkillShootingInterval * eliteCom.ValueRO.stageOneSkillShootCount + random.NextFloat(2f, 3f);
                        }
                    }
                }


            }
            else
            {
                //state related code
                if (eliteCom.ValueRO.previousHp > 500f)
                {
                    eliteCom.ValueRW.previousHp = 213;
                    eliteCom.ValueRW.SkillShootRandomIntervalTimer = 0f;
                }

                // moving logic
                var tarDir = playerTransform.Position - eliteTransformRW.ValueRO.Position;
                var disSq = math.csum(tarDir * tarDir);
                if (disSq > 4f)
                {
                    eliteTransformRW.ValueRW.Position -= eliteCom.ValueRO.Speed * math.normalize(tarDir) * deltatime;
                    eliteTransformRW.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
                }

                // whether to 
                if ((eliteCom.ValueRW.SkillShootRandomIntervalTimer -= deltatime) < 0f)
                { 
                    isDoingSkill = true;
                    spawnCount = eliteCom.ValueRO.spawnEggSkillspawnCount;
                    spawnIntervalTimer = eliteCom.ValueRO.spawnEggSkillSpawningInterval;
                    spawnIntervalRealTimer = 0f;
                    eliteCom.ValueRW.SkillShootRandomIntervalTimer = eliteCom.ValueRO.spawnEggSkillspawnCount * eliteCom.ValueRO.spawnEggSkillSpawningInterval + random.NextFloat(3f,5f);
                }

                // update skill logic if isDoingSkill
                if (isDoingSkill)
                {
                    //#region old skill logic
                    //if ((spawnTimer -= deltatime) < 0f)
                    //{
                    //    spawnTimer = eliteCom.ValueRO.stateTwoInSkillShootingInterval;
                    //    var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                    //    var spawnee = ecb.Instantiate(NormalSpawneePrefab);
                    //    //var elite = SystemAPI.GetSingletonEntity<EnemyEliteShooterCom>();
                    //    //var eliteTransform = SystemAPI.GetComponent<LocalTransform>(elite);
                    //    var spawnPos = eliteTransformRW.ValueRO.Position + eliteTransformRW.ValueRO.Up() * spawneePosOffsetY[currentSpawnPosIdx] + eliteTransformRW.ValueRO.Forward() * spawneePosOffsetX[currentSpawnPosIdx];
                    //    var spawneeTarDir = playerTransform.Position - spawnPos;
                    //    spawneeTarDir.y += 1f;
                    //    spawneeTarDir += random.NextFloat3(offsetMin, offsetMax);
                    //    ecb.SetComponent(spawnee, new LocalTransform { Position = spawnPos, Scale = 1f, Rotation = quaternion.LookRotation(spawneeTarDir, math.up()) });
                    //    currentSpawnPosIdx = (currentSpawnPosIdx + 1) % 4;
                    //    if (++spawneeShootOutCount > eliteCom.ValueRO.stageTwoSkillShootCount)
                    //    {
                    //        spawneeShootOutCount = 0;
                    //        currentSpawnPosIdx = 0;
                    //        isDoingSkill = false;
                    //        spawnTimer = 0f;
                    //        eliteCom.ValueRW.SkillShootRandomIntervalTimer = eliteCom.ValueRO.stageTwoSkillShootCount * eliteCom.ValueRO.stateTwoInSkillShootingInterval + random.NextFloat(2f, 3f);
                    //    }
                    //}
                    //#endregion
                    #region skill code
                    var buffer = SystemAPI.GetSingletonBuffer<EnemyEliteFlyingEggBuffer>();
                    //var deltatime = SystemAPI.Time.DeltaTime;

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
                                isDoingSkill = false;
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
                    if ((spawnIntervalRealTimer -= deltatime) < 0f && spawnCount-- > 0)
                    {
                        spawnIntervalRealTimer = spawnIntervalTimer;
                        var eliteEntity = SystemAPI.GetSingletonEntity<EnemyEliteEggAndShootCom>();
                        var eliteTransform = SystemAPI.GetComponent<LocalTransform>(eliteEntity);
                        var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                        var enemybuffer = SystemAPI.GetSingletonBuffer<AllEnemyPrefabBuffer>();
                        var eggPrefab = enemybuffer[enemybuffer.Length - 1].Prefab;
                        var originalPos = new float3 { x = eliteTransform.Position.x, z = eliteTransform.Position.z, y = 1f };

                        var egg = ecb.Instantiate(eggPrefab);
                        ecb.SetComponent(eggPrefab, new LocalTransform { Position = originalPos, Scale = 1f, Rotation = quaternion.identity });
                        ecb.AppendToBuffer<EnemyEliteFlyingEggBuffer>(eliteEntity, new EnemyEliteFlyingEggBuffer
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

                // if set dead by TriggerJob
                if (SystemAPI.GetComponent<EntityStateMachine>(elite).CurrentState == EntityState.Dead)
                {
                    state.EntityManager.DestroyEntity(elite);
                }
            }
        }
    }
}