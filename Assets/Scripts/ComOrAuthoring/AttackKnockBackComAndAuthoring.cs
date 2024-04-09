using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class AttackKnockBackComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<AttackKnockBackComAndAuthoring>
        {
            public override void Bake(AttackKnockBackComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<AttackKnockBackTag>(entity);
            }
        }
    }
    public struct AttackKnockBackTag : IComponentData
    {

    }
}