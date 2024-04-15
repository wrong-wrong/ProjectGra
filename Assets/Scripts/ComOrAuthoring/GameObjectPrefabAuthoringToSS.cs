using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class GameObjectPrefabAuthoringToSS : MonoBehaviour
    {
        public GameObject HealthBarUIPrefab;
        public class Baker : Baker<GameObjectPrefabAuthoringToSS>
        {
            public override void Bake(GameObjectPrefabAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponentObject(entity, new GOPrefabManagedContainer
                {
                    HealthBarUIPrefab = authoring.HealthBarUIPrefab
                });
            }
        }
    }
    public class GOPrefabManagedContainer : IComponentData
    {
        public GameObject HealthBarUIPrefab;
    }
}