using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class NormalMeleeComAndAuthoring : MonoBehaviour
    {
        public float MeleeAttackCooldown;
        public class Baker : Baker<NormalMeleeComAndAuthoring>
        {
            public override void Bake(NormalMeleeComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NormalMeleeStateMachine
                {
                    CurrentState = EnemyState.Follow
                });
                AddComponent(entity, new NormalMeleeAttack
                {
                    AttackCooldown = authoring.MeleeAttackCooldown
                });
            }
        }
    }

    public struct NormalMeleeStateMachine : IComponentData
    {
        public EnemyState CurrentState;
    }
    public struct NormalMeleeAttack : IComponentData
    {
        public float AttackCooldown;
    }
}