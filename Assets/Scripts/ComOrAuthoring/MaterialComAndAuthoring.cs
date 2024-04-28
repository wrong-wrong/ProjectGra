using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class MaterialComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<MaterialComAndAuthoring>
        {
            public override void Bake(MaterialComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<MaterialTag>(entity);
                AddComponent<MaterialMoveCom>(entity);
                SetComponentEnabled<MaterialMoveCom>(entity, true);
            }
        }
    }
    public struct MaterialTag : IComponentData { }
    public struct MaterialMoveCom : IComponentData, IEnableableComponent
    {
        public float2 tarDir;
        public float accumulateTimer;
    }
}