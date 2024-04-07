using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class ItemComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<ItemComAndAuthoring>
        {
            public override void Bake(ItemComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ItemTag>(entity);
            }
        }
    }
    public struct ItemTag : IComponentData { }
}