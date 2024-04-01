using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyCommonComAndAuthoring : MonoBehaviour
    {
        public int HealthPoint;
        //public float FollowSpeed;
        //public float StopDistance;
        public class Baker : Baker<EnemyCommonComAndAuthoring>
        {
            public override void Bake(EnemyCommonComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntityHealthPoint { HealthPoint = authoring.HealthPoint });
                //AddComponent(entity, new EnemyFollowState { FollowSpeed = authoring.FollowSpeed , StopDistance = authoring.StopDistance});
                AddComponent(entity, new EntityStateMachine { CurrentState = EntityState.Follow });
            }
        }
    }

    public struct EntityHealthPoint : IComponentData
    {
        public int HealthPoint;
    }

    public struct EntityStateMachine : IComponentData
    {
        public EntityState CurrentState;
    }
    

}