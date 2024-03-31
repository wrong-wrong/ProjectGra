using Unity.Entities;
using UnityEngine;
namespace ProjectGra
{
    public class SpawneeComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<SpawneeComAndAuthoring>
        {
            public override void Bake(SpawneeComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SpawneeTimer>(entity);
                AddComponent<SpawneeCurDamage>(entity);
            }
        }
    }

    public struct SpawneeTimer : IComponentData, IEnableableComponent
    {
        public float Value;
    }
    public struct SpawneeCurDamage : IComponentData
    {
        public int damage;
    }


}
