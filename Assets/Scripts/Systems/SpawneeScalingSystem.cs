using Unity.Entities;
using Unity.Transforms;

namespace ProjectGra
{
    public partial struct SpawneeScalingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<SpawneeScalingCom>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            foreach(var (scaling,transform) in SystemAPI.Query<RefRW<SpawneeScalingCom>, RefRW<LocalTransform>>())
            {
                var realtimer = scaling.ValueRW.RealTimer += deltatime;
                transform.ValueRW.Scale = 1 + realtimer < scaling.ValueRO.Timer ? scaling.ValueRO.MaxScaleMinusOne * realtimer: scaling.ValueRO.MaxScaleMinusOne;
            }
        }
        
    }
}