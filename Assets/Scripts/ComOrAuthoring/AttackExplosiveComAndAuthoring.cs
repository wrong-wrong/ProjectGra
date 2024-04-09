using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class AttackExplosiveComAndAuthoring : MonoBehaviour
    {
        public GameObject ExplosiveEntityPrefab;
        public class Baker : Baker<AttackExplosiveComAndAuthoring>
        {
            public override void Bake(AttackExplosiveComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AttackExplosiveCom
                {
                    ExplosionPrefab = GetEntity(authoring.ExplosiveEntityPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
    public struct AttackExplosiveCom : IComponentData
    {
        public Entity ExplosionPrefab;
    }
}