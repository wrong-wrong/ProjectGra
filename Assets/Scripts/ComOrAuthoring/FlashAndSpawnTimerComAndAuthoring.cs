using Unity.Entities;
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
    }
    public struct SpawningTimer : IComponentData, IEnableableComponent
    {
        public float time;
    }
}