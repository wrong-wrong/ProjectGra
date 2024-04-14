using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemyEliteSprintAndShootSystem : ISystem, ISystemStartStop
    {
        //private Entity eggPrefab;
        private Random random;

        private Entity normalSpawneePrefab;
        private float3 rightSpawnPosOffset;
        private float3 leftSpawnPosOffset;
        private float spawnYAxisRotation;
        
        private int spawnCount;
        private float spawnInterval;
        private float sprintDistanceSq;
        private float sprintDistance;
        private float speed;
        private float sprintSpeed;
        private float collideDistanceSq;
        private float skillCooldown;
        private int sprintDamage;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<EnemyEliteSprintAndShootCom>();
            random = Random.CreateFromIndex(0);
            unsafe
            {
                Debug.Log(sizeof(quaternion));
            }
        }
        public void OnStartRunning(ref SystemState state)
        {
            var prefabContainer = SystemAPI.GetSingleton<PrefabContainerCom>();
            normalSpawneePrefab = prefabContainer.NormalEnemySpawneePrefab;
            var config = SystemAPI.GetSingleton<EliteSprintAndShootConfigCom>();
            rightSpawnPosOffset = config.EliteSprintAndShootRightSpawnPosOffset;
            leftSpawnPosOffset = config.EliteSprintAndShootLeftSpawnPosOffset;
            spawnYAxisRotation = math.radians(config.EliteSprintAndShootSpawnYAxisRotation);
            spawnCount = config.EliteSprintAndShootSpawnCount;
            spawnInterval = config.EliteSprintAndShootSpawnInterval;
            sprintDistanceSq = config.EliteSprintAndShootSprintDistance * config.EliteSprintAndShootSprintDistance;
            sprintDistance = config.EliteSprintAndShootSprintDistance;
            speed = config.EliteSprintAndShootSpeed;
            sprintSpeed = config.EliteSprintAndShootSprintSpeed;
            collideDistanceSq = config.EliteSprintAndShootCollideDistance * config.EliteSprintAndShootCollideDistance;
            skillCooldown = config.EliteSprintAndShootSkillCooldown;
            sprintDamage = config.EliteSprintAndShootSprintDamage;
        }
        public void OnUpdate(ref SystemState state)
        {
            //if(Input.GetKeyUp(KeyCode.Space))
            //{
            //    Debug.Log("Elite KeyUp Space");
            //    var transformRW = SystemAPI.GetComponentRW<LocalTransform>(SystemAPI.GetSingletonEntity<EnemyEliteSprintAndShootCom>());
            //    transformRW.ValueRW.Rotation = math.mul(transformRW.ValueRO.Rotation,quaternion.RotateY(15)); // left ,
            //    //quaternion.
            //}
            // only one stage, one kind of skill
            // shooting spawnee while sprint

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            var playerHealthPoint = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var deltatime = SystemAPI.Time.DeltaTime;
            foreach(var(elite, transform, stateMachine, entity) in SystemAPI.Query<RefRW<EnemyEliteSprintAndShootCom>, RefRW<LocalTransform>, RefRW<EntityStateMachine>>()
                .WithEntityAccess())
            {
                //if (Input.GetKeyUp(KeyCode.Alpha1))
                //{
                //    elite.ValueRW.LeftSpawnPosQuaternion = math.mul(transform.ValueRW.Rotation, quaternion.RotateY(math.radians(-15))); // for right
                //    Debug.Log("1");
                //}
                //if (Input.GetKeyUp(KeyCode.Alpha2))
                //{
                //    elite.ValueRW.LeftSpawnPosQuaternion = math.mul(quaternion.RotateY(math.radians(45)), transform.ValueRW.Rotation);
                //    Debug.Log("2");

                //}
                //if (Input.GetKeyUp(KeyCode.Alpha3))
                //{
                //    elite.ValueRW.LeftSpawnPosQuaternion = math.mul(transform.ValueRW.Rotation, quaternion.RotateY(math.radians(-90)));
                //    Debug.Log("3");
                //}
                //if (Input.GetKeyUp(KeyCode.Space))
                //{
                //    var tmp = transform.ValueRO.Position + transform.ValueRO.Forward() * leftSpawnPosOffset.z
                //        + transform.ValueRO.Up() * leftSpawnPosOffset.y;
                //    float3 leftSpawnPos = tmp + transform.ValueRO.Right() * leftSpawnPosOffset.x;
                //    var left = ecb.Instantiate(normalSpawneePrefab);
                //    ecb.SetComponent<LocalTransform>(left, new LocalTransform { Position = leftSpawnPos, Rotation = elite.ValueRO.LeftSpawnPosQuaternion, Scale = 0.5f });
                //}


                var tarDir = playerTransform.Position - transform.ValueRO.Position;
                tarDir.y = 0;
                var distanceSq = math.lengthsq(tarDir.xz);


                switch (stateMachine.ValueRO.CurrentState)
                {
                    case EntityState.Init:
                        // set tarDir and go to Follow
                        stateMachine.ValueRW.CurrentState = EntityState.Follow;
                        tarDir.xz = math.normalize(random.NextFloat2(float2.zero, tarDir.xz));
                        transform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
                        elite.ValueRW.tarDirNormalizedMulSpeed = tarDir.xz * speed;
                        break;
                    case EntityState.DoingSkill:
                        // update pos using tarDirNormalizedMulSpeed
                        transform.ValueRW.Position.xz += elite.ValueRO.tarDirNormalizedMulSpeed * deltatime;
                        // update and check spawnIntervalRealTimer & shoot out count
                        if (elite.ValueRO.spawneeLeftCount > 0 && (elite.ValueRW.spawnIntervalRealTimer -= deltatime) < 0f)
                        {
                            //      if underzero, set to timer and spawn two spawnee at both left and right side
                            --elite.ValueRW.spawneeLeftCount;
                            elite.ValueRW.spawnIntervalRealTimer = spawnInterval;
                            var tmp = transform.ValueRO.Position + transform.ValueRO.Forward() * leftSpawnPosOffset.z
                                + transform.ValueRO.Up() * leftSpawnPosOffset.y;
                            float3 leftSpawnPos = tmp + transform.ValueRO.Right() * leftSpawnPosOffset.x;
                            float3 rightSpawnPos = tmp + transform.ValueRO.Right() * rightSpawnPosOffset.x;
                            var left = ecb.Instantiate(normalSpawneePrefab);
                            //ecb.SetComponent<LocalTransform>(left, new LocalTransform { Position = leftSpawnPos, Rotation = elite.ValueRO.LeftSpawnPosQuaternion, Scale = 0.5f });
                            ecb.SetComponent<LocalTransform>(left, new LocalTransform { Position = leftSpawnPos, Rotation = elite.ValueRO.LeftSpawnPosQuaternion, Scale = 0.5f });
                            var right = ecb.Instantiate(normalSpawneePrefab);
                            ecb.SetComponent<LocalTransform>(right, new LocalTransform { Position = rightSpawnPos, Rotation = elite.ValueRO.RightSpawnPosQuaternion, Scale = 0.5f });
                        }
                        // check distance with player to decide whether the enemy could be considered having a collision with the player,
                        if (elite.ValueRO.currentSprintDamage > 0 && distanceSq < collideDistanceSq)
                        {
                            //      if so, dealing damage and set isDoingSkill to false, we want the damage only be dealed once, but the elite keep sprinting, so we set the curDamage to 0  
                            playerHealthPoint.ValueRW.HealthPoint -= elite.ValueRO.currentSprintDamage;
                            elite.ValueRW.currentSprintDamage = 0;
                        }
                        if ((elite.ValueRW.sprintCountdown -= deltatime) < 0f)
                        {
                            stateMachine.ValueRW.CurrentState = EntityState.Follow;
                            tarDir.xz = math.normalize(random.NextFloat2(float2.zero, tarDir.xz));
                            transform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
                            elite.ValueRW.tarDirNormalizedMulSpeed = tarDir.xz * speed;
                        }

                        break;
                    case EntityState.Follow:
                        // update pos using tarDirNormalizedMulSpeed * deltatime
                        transform.ValueRW.Position.xz += elite.ValueRO.tarDirNormalizedMulSpeed * deltatime;
                        if ((elite.ValueRW.movementResetCountdown -= deltatime) < 0f)
                        {
                            elite.ValueRW.movementResetCountdown = 2f;
                            tarDir.xz = math.normalize(random.NextFloat2(float2.zero, tarDir.xz));
                            transform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
                            elite.ValueRW.tarDirNormalizedMulSpeed = tarDir.xz * speed;
                        }
                        // update and check skill cooldown and distance; 
                        //      if true, set tarDir, LookRotation, isDoingSkill, skillTimer, left&rightQuaternion
                        if ((elite.ValueRW.skillCooldown -= deltatime) < 0f && distanceSq < sprintDistanceSq)
                        {
                            elite.ValueRW.currentSprintDamage = sprintDamage;
                            elite.ValueRW.sprintCountdown = 1.4f * sprintDistance / sprintSpeed;
                            elite.ValueRW.skillCooldown = skillCooldown;
                            elite.ValueRW.spawneeLeftCount = spawnCount;
                            stateMachine.ValueRW.CurrentState = EntityState.DoingSkill;
                            elite.ValueRW.tarDirNormalizedMulSpeed = math.normalize(tarDir.xz) * sprintSpeed;
                            transform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
                            elite.ValueRW.LeftSpawnPosQuaternion = math.mul(transform.ValueRW.Rotation, quaternion.RotateY(-spawnYAxisRotation));
                            elite.ValueRW.RightSpawnPosQuaternion = math.mul(transform.ValueRW.Rotation, quaternion.RotateY(spawnYAxisRotation));
                        }
                        break;
                    case EntityState.Dead:
                        ecb.DestroyEntity(entity);
                        break;
                }
            }
        }



        public void OnStopRunning(ref SystemState state)
        {
            
        }
    }
}