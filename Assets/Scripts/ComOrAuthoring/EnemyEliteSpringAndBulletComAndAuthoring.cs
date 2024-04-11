using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyEliteSprintAndBulletComAndAuthoring : MonoBehaviour
    {
        public float Speed;
        public class Baker : Baker<EnemyEliteSprintAndBulletComAndAuthoring>
        {
            public override void Bake(EnemyEliteSprintAndBulletComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyEliteSprintAndBulletCom { Speed = authoring.Speed});
            }
        }
    }
    public struct EnemyEliteSprintAndBulletCom : IComponentData 
    {
        public float Speed;
        public float3 TargetDirNormalized;
        public float MovingTimer;
    }
}