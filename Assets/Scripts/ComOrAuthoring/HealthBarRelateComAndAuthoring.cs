using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    public class HealthBarRelateComAndAuthoring : MonoBehaviour
    {
        public float3 HealthBarOffsetValue;
        public class Baker : Baker<HealthBarRelateComAndAuthoring>
        {
            public override void Bake(HealthBarRelateComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HealthBatUIOffset { OffsetValue = authoring.HealthBarOffsetValue });
                AddComponent<EliteMaxHealthPoint>(entity);
            }
        }
    }
    public class HealthBarUICleanupCom : ICleanupComponentData
    {
        public GameObject HealthBarGO;
        public Image BarFillingImage;
    }

    public struct HealthBatUIOffset : IComponentData
    {
        public float3 OffsetValue;
    }
    public struct EliteMaxHealthPoint : IComponentData
    {
        public int MaxHealthPoint;
        public int PreviousHealthPoint;
    }
}