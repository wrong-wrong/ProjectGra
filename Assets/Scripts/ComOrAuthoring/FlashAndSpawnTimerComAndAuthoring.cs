using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class FlashAndSpawnTimerComAndAuthoring : MonoBehaviour
    {
        [Header("Duration override by spawning timer")]
        // Duration would be overrided by spawning timer,  hit flash duration and cycle time should config at config and set to system
        //public float Duration;
        //public float AccumulateTimer;
        public float FlashingCycleTime;

        public float SpawningTimerAndFlashDuration;
        public class Baker : Baker<FlashAndSpawnTimerComAndAuthoring>
        {
            public override void Bake(FlashAndSpawnTimerComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FlashingCom
                {
                    Duration = authoring.SpawningTimerAndFlashDuration,
                    //AccumulateTimer = authoring.AccumulateTimer,
                    CycleTime = authoring.FlashingCycleTime,
                    FlashColorDifference = new float3(0,1,1), //  float3(1,1,1) - red.xyz
                });
                AddComponent(entity, new SpawningTimer { time = authoring.SpawningTimerAndFlashDuration });
            }
        }
    }
    public struct FlashingCom : IComponentData, IEnableableComponent
    {
        public float Duration;
        public float AccumulateTimer;
        public float CycleTime;
        public float3 FlashColorDifference; // equals to float3(1,1,1) - the color.xyz of the color you want
    }
    public struct SpawningTimer : IComponentData, IEnableableComponent
    {
        public float time;
    }
}