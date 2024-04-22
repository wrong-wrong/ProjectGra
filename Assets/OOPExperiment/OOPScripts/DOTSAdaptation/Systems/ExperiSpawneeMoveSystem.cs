using Unity.Entities;
using Unity.Transforms;

namespace OOPExperiment
{
    public partial struct ExperiSpawneeMoveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExperiSpawneeTarDirMulSpeed>();
        }
        public void OnUpdate(ref SystemState state)
        {

            //var deltatime = SystemAPI.Time.DeltaTime;
            //var job = new SimpleSpawneeMoveJob { deltatime = deltatime};
            //state.Dependency = job.Schedule(state.Dependency);
            //state.Dependency.Complete();
            var deltatime = SystemAPI.Time.DeltaTime;
            foreach (var (transform, tarDirMulSpeed) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ExperiSpawneeTarDirMulSpeed>>())
            {
                transform.ValueRW.Position += tarDirMulSpeed.ValueRO.Value * deltatime;
            }
        }
    }
}