using Unity.Entities;
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
            }
        }
    }
    public struct MaterialTag : IComponentData { }
}