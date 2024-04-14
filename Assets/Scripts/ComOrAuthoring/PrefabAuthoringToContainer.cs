using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class PrefabAuthoringToContainer : MonoBehaviour
    {
        public GameObject MaterialPrefab;
        public GameObject ItemPrefab;
        public GameObject SummonExplosionPrefab;
        public GameObject NormalEnemySpawneePrefab;
        public GameObject ScalingSpawneePrefab;
        public class Baker : Baker<PrefabAuthoringToContainer>
        {
            public override void Bake(PrefabAuthoringToContainer authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PrefabContainerCom
                {
                    MaterialPrefab = GetEntity(authoring.MaterialPrefab, TransformUsageFlags.Renderable),
                    ItemPrefab = GetEntity(authoring.ItemPrefab, TransformUsageFlags.Renderable),
                    SummonExplosionPrefab = GetEntity(authoring.SummonExplosionPrefab, TransformUsageFlags.Dynamic),
                    NormalEnemySpawneePrefab = GetEntity(authoring.NormalEnemySpawneePrefab, TransformUsageFlags.Dynamic),
                    ScalingSpawneePrefab = GetEntity(authoring.ScalingSpawneePrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
    public struct PrefabContainerCom : IComponentData
    {
        public Entity MaterialPrefab;
        public Entity ItemPrefab;
        public Entity SummonExplosionPrefab;
        public Entity NormalEnemySpawneePrefab;
        public Entity ScalingSpawneePrefab;
    }
}