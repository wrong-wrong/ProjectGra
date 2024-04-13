using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EntityScalingComAndAuthoing : MonoBehaviour
    {
        public float Timer;
        public float BasicScale;
        public float OffsetScale;
        public bool IsEnable;
        public class Baker : Baker<EntityScalingComAndAuthoing>
        {
            public override void Bake(EntityScalingComAndAuthoing authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntityScalingCom
                {
                    RealTimer = 0f,
                    Timer = authoring.Timer,
                    OffsetScale = authoring.OffsetScale,
                    BasicScale = authoring.BasicScale,
                });
                if(authoring.IsEnable)
                {
                    SetComponentEnabled<EntityScalingCom>(entity, true);
                }
                else
                {
                    SetComponentEnabled<EntityScalingCom>(entity, false);
                }
            }
        }
    }
    public struct EntityScalingCom : IComponentData, IEnableableComponent
    {
        public float BasicScale;
        public float RealTimer;
        public float Timer;
        public float OffsetScale;
        
        public float Ratio;
    }

}