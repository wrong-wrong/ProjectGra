using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class PrefabAuthoringToContainer : MonoBehaviour
    {
        public GameObject MaterialPrefab;
        public GameObject ItemPrefab;
        public class Baker : Baker<PrefabAuthoringToContainer>
        {
            public override void Bake(PrefabAuthoringToContainer authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PrefabContainerCom
                {
                    MaterialPrefab = GetEntity(authoring.MaterialPrefab, TransformUsageFlags.Renderable),
                    ItemPrefab = GetEntity(authoring.ItemPrefab, TransformUsageFlags.Renderable),
                });
            }
        }
    }
    public struct PrefabContainerCom : IComponentData
    {
        public Entity MaterialPrefab;
        public Entity ItemPrefab;
    }
}