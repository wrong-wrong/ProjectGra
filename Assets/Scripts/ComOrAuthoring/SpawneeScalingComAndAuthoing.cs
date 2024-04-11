using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class SpawneeScalingComAndAuthoing : MonoBehaviour
    {
        public float Timer;
        public float BasicScale;
        public float OffsetScale;
        public class Baker : Baker<SpawneeScalingComAndAuthoing>
        {
            public override void Bake(SpawneeScalingComAndAuthoing authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SpawneeScalingCom
                {
                    RealTimer = 0f,
                    Timer = authoring.Timer,
                    OffsetScale = authoring.OffsetScale,
                    BasicScale = authoring.BasicScale,
                });
            }
        }
    }
    public struct SpawneeScalingCom : IComponentData
    {
        public float BasicScale;
        public float RealTimer;
        public float Timer;
        public float OffsetScale;
        
        public float Ratio;
    }

}