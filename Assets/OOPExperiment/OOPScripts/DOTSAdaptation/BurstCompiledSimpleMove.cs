using OOPExperiment;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
namespace OOPExperiment
{
    [BurstCompile(CompileSynchronously = true)]
    public partial struct SimpleSpawneeMoveJob : IJobEntity
    {
        public float deltatime;
        public void Execute(ref LocalTransform transform, in ExperiSpawneeTarDirMulSpeed tarDirMulSpeed)
        {
            transform.Position += tarDirMulSpeed.Value * deltatime;
        }
    }   
}