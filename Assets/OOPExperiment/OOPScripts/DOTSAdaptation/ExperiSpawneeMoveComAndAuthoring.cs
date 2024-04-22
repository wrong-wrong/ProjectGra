using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace OOPExperiment
{
    public class ExperiSpawneeMoveComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<ExperiSpawneeMoveComAndAuthoring>
        {
            public override void Bake(ExperiSpawneeMoveComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new ExperiSpawneeTarDirMulSpeed { Value = new float3(1,0,0) });
            }
        }
    }
    public struct ExperiSpawneeTarDirMulSpeed : IComponentData
    {
        public float3 Value;
    }
}