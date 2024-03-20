using Unity.Entities;
using UnityEngine;

namespace OOPExperiment
{
    public struct MovingTargetSpeed : IComponentData
    {
        public float speed;
    }
    public class MovingTargetAuthoring : MonoBehaviour
    {
        public float speed;
        public class Baker : Baker<MovingTargetAuthoring>
        {
            public override void Bake(MovingTargetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MovingTargetSpeed { speed = authoring.speed });
            }
        }
    }

}