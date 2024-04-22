using Unity.Entities;
using UnityEngine;

namespace OOPExperiment
{
    public class ExperiConfigAuthoring : MonoBehaviour
    {
        //public GameObject ExperiPalumonPrefab;
        //public int count;
        //public GameObject ExperiMovingTargetPrefab;
        public int count;
        public GameObject ExperiSpawneePrefab;
        public class Baker : Baker<ExperiConfigAuthoring>
        {
            public override void Bake(ExperiConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                //AddComponent(entity, new ExperiSpawnConfig
                //{
                //    count = authoring.count,
                //    ExperiMovingTargetPrefab = GetEntity(authoring.ExperiMovingTargetPrefab, TransformUsageFlags.Dynamic),
                //    ExperiPalumonEntityPrefab = GetEntity(authoring.ExperiPalumonPrefab, TransformUsageFlags.Dynamic),
                //});

                AddComponent(entity, new ExperiSpawnConfig
                {
                    count = authoring.count,
                    ExperiSpawneePrefab = GetEntity(authoring.ExperiSpawneePrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
    //public struct ExperiSpawnConfig : IComponentData
    //{
    //    public Entity ExperiPalumonEntityPrefab;
    //    public Entity ExperiMovingTargetPrefab;
    //    public int count;
    //}
    public struct ExperiSpawnConfig : IComponentData
    {
        public Entity ExperiSpawneePrefab;
        public int count;
    }
}