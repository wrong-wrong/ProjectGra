using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace ProjectGra
{
    public partial struct FlashingSystem : ISystem
    {
        private float hitFlashCycleTime;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            hitFlashCycleTime = 0.5f;
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            foreach (var (flash, flashBit, basecolor) in SystemAPI.Query<RefRW<FlashingCom>, EnabledRefRW<FlashingCom>, RefRW<URPMaterialPropertyBaseColor>>())
            {
                var ratio = flash.ValueRO.AccumulateTimer / flash.ValueRO.CycleTime;
                basecolor.ValueRW.Value.xyz = 1f - flash.ValueRO.FlashColorDifference * math.sin(math.radians(ratio * 180));//1-ratio;
                if ((flash.ValueRW.AccumulateTimer += deltatime) < flash.ValueRO.Duration) continue;
                flash.ValueRW.AccumulateTimer = 0;
                flash.ValueRW.Duration = hitFlashCycleTime;
                flash.ValueRW.CycleTime = hitFlashCycleTime;
                flashBit.ValueRW = false;
                basecolor.ValueRW.Value = new float4(1, 1, 1, 1);
                
            }
        }
    }
}