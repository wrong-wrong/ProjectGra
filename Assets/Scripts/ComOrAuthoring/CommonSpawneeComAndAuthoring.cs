using Unity.Entities;
using UnityEngine;
namespace ProjectGra
{
    public class SpawneeComAndAuthoring : MonoBehaviour
    {
        public float SpawneeTimer;
        public class Baker : Baker<SpawneeComAndAuthoring>
        {
            public override void Bake(SpawneeComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.SpawneeTimer == 0) AddComponent<SpawneeTimer>(entity); // timer would be set , but explosion need to be set manually
                else AddComponent(entity, new SpawneeTimer { Value = authoring.SpawneeTimer });
                //AddComponent<SpawneeTimer>(entity);
                AddComponent<AttackCurDamage>(entity);
            }
        }
    }

    public struct SpawneeTimer : IComponentData, IEnableableComponent
    {
        public float Value;
    }
    public struct AttackCurDamage : IComponentData
    {
        public int damage;
    }


}
