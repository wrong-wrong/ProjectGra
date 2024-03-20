using Unity.Entities;
using UnityEngine;

namespace OOPExperiment
{
    public class ExperiConfigAuthoring : MonoBehaviour
    {
        public GameObject ExperiPalumonPrefab;
        public int count;
        public GameObject ExperiMovingTargetPrefab;
        public class Baker : Baker<ExperiConfigAuthoring>
        {
            public override void Bake(ExperiConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ExperiSpawnConfig
                {
                    count = authoring.count,
                    ExperiMovingTargetPrefab = GetEntity(authoring.ExperiMovingTargetPrefab, TransformUsageFlags.Dynamic),
                    ExperiPalumonEntityPrefab = GetEntity(authoring.ExperiPalumonPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
    public struct ExperiSpawnConfig : IComponentData
    {
        public Entity ExperiPalumonEntityPrefab;
        public Entity ExperiMovingTargetPrefab;
        public int count;
    }
}