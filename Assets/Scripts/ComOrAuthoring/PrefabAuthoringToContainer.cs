using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class PrefabAuthoringToContainer : MonoBehaviour
    {
        public GameObject MaterialPrefab;

        public class Baker : Baker<PrefabAuthoringToContainer>
        {
            public override void Bake(PrefabAuthoringToContainer authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new MaterialPrefabCom
                {
                    Prefab = GetEntity(authoring.MaterialPrefab, TransformUsageFlags.Renderable)
                });
            }
        }

        public struct MaterialPrefabCom : IComponentData
        {
            public Entity Prefab;
        }
    }
}