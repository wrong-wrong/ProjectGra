using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    //[UpdateAfter(typeof(HandleTimeOutSpawneeSystem))]
    public partial struct SpawneeMoveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            //EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            //state.Dependency = new SpawneeMoveJob
            //{
            //    deltatime = deltatime,
            //    ecb = ecb.AsParallelWriter()
            //}.Schedule(state.Dependency);

            //state.Dependency.Complete();
            //ecb.Playback(state.EntityManager);

            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            state.Dependency = new SpawneeMoveJob
            {
                deltatime = deltatime,
                //ecb = ecb.AsParallelWriter()
            }.Schedule(state.Dependency);
        }
    }

    
    [WithAll(typeof(SpawneeMoveTag))]
    public partial struct SpawneeMoveJob : IJobEntity
    {
        [ReadOnly]public float deltatime;
        //public EntityCommandBuffer.ParallelWriter ecb;
        public void Execute(ref LocalTransform localTransform)
        {
            localTransform.Position += localTransform.Forward() * 20f * deltatime;
            //if(timer.Value < 0 ) { ecb.DestroyEntity(index, entity); }
        }
    }
}