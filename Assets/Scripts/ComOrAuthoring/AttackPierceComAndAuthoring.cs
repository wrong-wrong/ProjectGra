using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class AttackPierceComAndAuthoring : MonoBehaviour
    {
        public int MaxPierceCount;
        public class Baker : Baker<AttackPierceComAndAuthoring>
        {
            public override void Bake(AttackPierceComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AttackPierce { MaxPierceCount = authoring.MaxPierceCount });
                AddBuffer<HitBuffer>(entity);
            }
        }
    }

    public struct AttackPierce : IComponentData 
    {
        public int MaxPierceCount;
    }
    [InternalBufferCapacity(0)]
    public struct HitBuffer : IBufferElementData 
    {
        public Entity HitEntity;
    }

}