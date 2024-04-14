using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class FlashingComAndAuthoring : MonoBehaviour
    {
        public float Duration;
        //public float AccumulateTimer;
        public float CycleTime;
        public class Baker : Baker<FlashingComAndAuthoring>
        {
            public override void Bake(FlashingComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FlashingCom
                {
                    Duration = authoring.Duration,
                    //AccumulateTimer = authoring.AccumulateTimer,
                    CycleTime = authoring.CycleTime,
                });
            }
        }
    }
    public struct FlashingCom : IComponentData, IEnableableComponent
    {
        public float Duration;
        public float AccumulateTimer;
        public float CycleTime;
    }
}