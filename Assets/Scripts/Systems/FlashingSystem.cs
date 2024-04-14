using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace ProjectGra
{
    public partial struct FlashingSystem : ISystem
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
            foreach (var (flash, flashBit, basecolor) in SystemAPI.Query<RefRW<FlashingCom>, EnabledRefRW<FlashingCom>, RefRW<URPMaterialPropertyBaseColor>>())
            {
                var ratio = flash.ValueRO.AccumulateTimer / flash.ValueRO.CycleTime;
                basecolor.ValueRW.Value.yz = math.sin(math.radians(ratio * 180));//1-ratio;
                if ((flash.ValueRW.AccumulateTimer += deltatime) < flash.ValueRO.Duration) continue;
                flashBit.ValueRW = false;
                basecolor.ValueRW.Value = new float4(1, 1, 1, 1);
                
            }
        }
    }
}