using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyCommonComAndAuthoring : MonoBehaviour
    {
        public int HealthPoint;
        public float FollowSpeed;
        public float StopDistance;
        public class Baker : Baker<EnemyCommonComAndAuthoring>
        {
            public override void Bake(EnemyCommonComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyHealthPoint { HealthPoint = authoring.HealthPoint });
                AddComponent(entity, new EnemyFollowState { FollowSpeed = authoring.FollowSpeed , StopDistance = authoring.StopDistance});
            }
        }
    }

    public struct EnemyHealthPoint : IComponentData, IEnableableComponent
    {
        public int HealthPoint;
    }
    public struct EnemyFollowState : IComponentData, IEnableableComponent
    {
        public float FollowSpeed;
        public float StopDistance;
    }

    

}