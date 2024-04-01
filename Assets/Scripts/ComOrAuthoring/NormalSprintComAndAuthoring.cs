using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class NormalSprintComAndAuthoring : MonoBehaviour
    {
        public int AttackVal;
        public float AttackCooldown;
        public float DeathTimer;
        public class Baker : Baker<NormalSprintComAndAuthoring>
        {
            public override void Bake(NormalSprintComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NormalSprintDeath { Timer = authoring.DeathTimer });
                AddComponent(entity, new NormalSprintAttack { AttackCooldown = authoring.AttackCooldown });
                //AddComponent(entity, new NormalSprintStateMachine { CurrentState = EnemyState.Follow });
            }
        }
    }

    //public struct NormalSprintStateMachine : IComponentData
    //{
    //    public EnemyState CurrentState;
    //}
    public struct NormalSprintDeath : IComponentData
    {
        public float Timer;
    }
    public struct NormalSprintAttack : IComponentData
    {
        //public int AttackVal;
        public float AttackCooldown;
        public float SprintTimer;
        public float3 SprintDirNormalized;
    }
}