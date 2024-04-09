using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class AttackPierceComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<AttackPierceComAndAuthoring>
        {
            public override void Bake(AttackPierceComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<AttackPierceTag>(entity);
                AddBuffer<HitBuffer>(entity);
            }
        }
    }

    public struct AttackPierceTag : IComponentData { }
    [InternalBufferCapacity(0)]
    public struct HitBuffer : IBufferElementData 
    {
        public Entity HitEntity;
    }

}