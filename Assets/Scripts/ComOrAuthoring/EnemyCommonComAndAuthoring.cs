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
                AddComponent(entity, new EntityStateMachine { CurrentState = EntityState.Follow });
                AddComponent<EnemyTag>(entity);
                AddComponent(entity, new EntityKnockBackCom { Timer = 0.5f});
                SetComponentEnabled<EntityKnockBackCom>(entity, false); 
            }
        }
    }
    public struct EnemyTag : IComponentData { }
    public struct EntityHealthPoint : IComponentData
    {
        public int HealthPoint;
    }

    public struct EntityStateMachine : IComponentData
    {
        public EntityState CurrentState;
    }
    public struct EntityKnockBackCom : IComponentData, IEnableableComponent 
    {
        public float Timer;
    }

}