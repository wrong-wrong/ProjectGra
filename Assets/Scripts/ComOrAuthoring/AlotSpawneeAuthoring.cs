using Unity.Entities;
using UnityEngine;

namespace RealTesting
{
    public class AlotSpawneeAuthoring : MonoBehaviour
    {
        public GameObject alotSpawnee;
        public class Baker : Baker<AlotSpawneeAuthoring>
        {
            public override void Bake(AlotSpawneeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<AlotSpawneePrafabCom>(entity, new AlotSpawneePrafabCom { Prefab = GetEntity(authoring.alotSpawnee, TransformUsageFlags.Dynamic) });
            }
        }
    }
    public struct AlotSpawneePrafabCom : IComponentData
    {
        public Entity Prefab;
    }
}