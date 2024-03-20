using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace OOPExperiment
{
    public partial struct ExperiFollowingSystem : ISystem
    {
        private ComponentLookup<LocalTransform> localTransformLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExperiExecuteTag>();
            //localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

            //false for using lookup in job
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(false);

        }

        public void OnUpdate(ref SystemState state)
        {
            // Entity + ComponentLookup to get the position from LocalTransform
            //  2 ways to implement  : 1.fixed Transform for the single moving target // no , real game target should not be the one, target can be any one
            //                         2. ComponentLookup

            //10k 1.85ms   using fixed targetTransform outside the loop    no rotation
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //var targetTransform = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<MovingTargetSpeed
            //foreach (var (palumonTransform, palumonSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>>())
            //{
            //    var tarDir = targetTransform.Position - palumonTransform.ValueRO.Position;
            //    palumonTransform.ValueRW.Position += MyNormalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //}

            //10K  3.70ms              using localTransformLookup       no rotation
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //localTransformLookup.Update(ref state);
            //foreach (var (palumonTransform, palumonSpeed, palumonTarget) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>, RefRO<ExperiPalumonTargetEntity>>())
            //{
            //    var tarDir = localTransformLookup[palumonTarget.ValueRO.targetEntity].Position - palumonTransform.ValueRO.Position;
            //    palumonTransform.ValueRW.Position += MyNormalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //}

            //10K  14.5ms  fixed targetTransform & lookRotation
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //foreach (var targetTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<MovingTargetSpeed>().WithNone<Prefab>())
            //{
            //    foreach (var (palumonTransform, palumonSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>>())
            //    {
            //        var tarDir = targetTransform.ValueRO.Position - palumonTransform.ValueRO.Position;
            //        //using a tmp value to hold localTransform
            //        var transform = palumonTransform.ValueRO;
            //        transform.Position += MyNormalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //        transform.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
            //        palumonTransform.ValueRW = transform;
            //        //directly modifying localTransform     - BASICALLY THE SAME
            //        //palumonTransform.ValueRW.Position += MyNormalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //        //palumonTransform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
            //    }
            //}

            //10K  16.4ms using localTransformLookup  & lookRotation
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //localTransformLookup.Update(ref state);
            //foreach (var (palumonTransform, palumonSpeed, palumonTarget) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>, RefRO<ExperiPalumonTargetEntity>>())
            //{
            //    var tarDir = localTransformLookup[palumonTarget.ValueRO.targetEntity].Position - palumonTransform.ValueRO.Position;
            //    palumonTransform.ValueRW.Position += MyNormalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //    palumonTransform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
            //}

            //10K 14.4ms using LookRotation without safe
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //localTransformLookup.Update(ref state);
            //foreach (var (palumonTransform, palumonSpeed, palumonTarget) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>, RefRO<ExperiPalumonTargetEntity>>())
            //{
            //    var tarDir = localTransformLookup[palumonTarget.ValueRO.targetEntity].Position - palumonTransform.ValueRO.Position;
            //    palumonTransform.ValueRW.Position += MyNormalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //    if (tarDir.x != 0 || tarDir.y != 0 || tarDir.z != 0) palumonTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
            //}

            //10K  14.4ms roughly the same using math.normalize & unsafe lookRotation
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //localTransformLookup.Update(ref state);
            //foreach (var (palumonTransform, palumonSpeed, palumonTarget) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>, RefRO<ExperiPalumonTargetEntity>>())
            //{
            //    var tarDir = localTransformLookup[palumonTarget.ValueRO.targetEntity].Position - palumonTransform.ValueRO.Position;
            //    if (tarDir.x != 0 || tarDir.y != 0 || tarDir.z != 0)
            //    {
            //        palumonTransform.ValueRW.Position += math.normalize(tarDir) * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //        palumonTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
            //    }
            //}

            //10K  18.4ms  even worse  transformLookup & lookRotation first then move forward
            //var deltaTime = SystemAPI.Time.DeltaTime;
            //localTransformLookup.Update(ref state);
            //foreach (var (palumonTransform, palumonSpeed, palumonTarget) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiPalumonSpeed>, RefRO<ExperiPalumonTargetEntity>>())
            //{
            //    var tarDir = localTransformLookup[palumonTarget.ValueRO.targetEntity].Position - palumonTransform.ValueRO.Position;
            //    palumonTransform.ValueRW.Rotation = quaternion.LookRotationSafe(tarDir, math.up());
            //    palumonTransform.ValueRW.Position += palumonTransform.ValueRO.Forward() * palumonSpeed.ValueRO.MoveSpeed * deltaTime;
            //}


            //10K  15.7ms  using math.normalize  and  Schedule running on the main thread
            //localTransformLookup.Update(ref state);
            //var palumonFollowJob = new ExperiPalumonFollowJob { deltaTime = SystemAPI.Time.DeltaTime, localTransformLookup = localTransformLookup };
            //state.Dependency = palumonFollowJob.Schedule(state.Dependency);

            //10k  1.38ms ScheduleParallel unbursted    and 0.13ms for bursted ScheduleParallel
            localTransformLookup.Update(ref state);
            var palumonFollowJob = new ExperiPalumonFollowJob { deltaTime = SystemAPI.Time.DeltaTime, localTransformLookup = localTransformLookup };
            state.Dependency = palumonFollowJob.ScheduleParallel(state.Dependency);

        }
        public static float MyMagnitude(float3 vector)
        {
            return (float)math.sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }
        public static float3 MyNormalize(float3 value)
        {
            float num = MyMagnitude(value);
            if (num > 1E-05f)
            {
                return value / num;
            }

            return float3.zero;
        }
    }


    [BurstCompile]
    public partial struct ExperiPalumonFollowJob : IJobEntity
    {
        public float deltaTime;

        //[ReadOnly]
        [NativeDisableParallelForRestriction]
        public ComponentLookup<LocalTransform> localTransformLookup;
        public void Execute(Entity entity, in ExperiPalumonSpeed speedCom, in ExperiPalumonTargetEntity tarEttCom)
        {
            var localTransform = localTransformLookup[entity];
            var tarDir = localTransformLookup[tarEttCom.targetEntity].Position - localTransform.Position;
            if (tarDir.x != 0 || tarDir.y != 0 || tarDir.z != 0)
            {
                localTransform.Position += math.normalize(tarDir) * speedCom.MoveSpeed * deltaTime;
                localTransform.Rotation = quaternion.LookRotation(tarDir, math.up());
                localTransformLookup[entity] = localTransform;
            }    
        }
    }
}