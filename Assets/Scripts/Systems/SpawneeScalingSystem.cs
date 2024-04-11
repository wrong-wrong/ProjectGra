using Unity.Entities;
using Unity.Transforms;

namespace ProjectGra
{
    public partial struct SpawneeScalingSystem : ISystem
    {
        //private LocalTransform playerTmpTransform;
        //private bool isValidThisUpdate;
        //private Component
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
                scaling.ValueRW.Ratio = realtimer / scaling.ValueRO.Timer;
                if (scaling.ValueRO.Ratio < 1f)
                {
                    transform.ValueRW.Scale = scaling.ValueRO.BasicScale +scaling.ValueRO.OffsetScale * scaling.ValueRO.Ratio;
                }
                //transform.ValueRW.Scale = realtimer < scaling.ValueRO.Timer ? scaling.ValueRO.MaxScale * realtimer/scaling.ValueRO.Timer : scaling.ValueRO.MaxScale;
                    //1 + (realtimer < scaling.ValueRO.Timer ? (scaling.ValueRO.MaxScale - 1) * realtimer / scaling.ValueRO.Timer : (scaling.ValueRO.MaxScale - 1));
            }
        }
        
    }
}