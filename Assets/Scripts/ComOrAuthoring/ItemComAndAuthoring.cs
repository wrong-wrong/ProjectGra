using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class ItemComAndAuthoring : MonoBehaviour
    {
        public bool IsLegendary;
        public class Baker : Baker<ItemComAndAuthoring>
        {
            public override void Bake(ItemComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<ItemTag>(entity);
                if(authoring.IsLegendary)
                {
                    SetComponentEnabled<ItemTag>(entity, true);
                }
                else
                {
                    SetComponentEnabled<ItemTag>(entity, false);
                }
            }
        }
    }
    public struct ItemTag : IComponentData, IEnableableComponent { }
}