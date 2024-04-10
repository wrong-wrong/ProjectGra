using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class AttackExplosionTagComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<AttackExplosionTagComAndAuthoring>
        {
            public override void Bake(AttackExplosionTagComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<AttackNotMovingStraight>(entity);
            }
        }
    }
    public struct AttackNotMovingStraight : IComponentData { }
}