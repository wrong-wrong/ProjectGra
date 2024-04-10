using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class SpawneeScalingComAndAuthoing : MonoBehaviour
    {
        public float Timer;
        public float MaxScale;
        public class Baker : Baker<SpawneeScalingComAndAuthoing>
        {
            public override void Bake(SpawneeScalingComAndAuthoing authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SpawneeScalingCom
                {
                    RealTimer = 0f,
                    Timer = authoring.Timer,
                    MaxScaleMinusOne = authoring.MaxScale,
                });
            }
        }
    }
    public struct SpawneeScalingCom : IComponentData
    {
        public float RealTimer;
        public float Timer;
        public float MaxScaleMinusOne;
    }

}