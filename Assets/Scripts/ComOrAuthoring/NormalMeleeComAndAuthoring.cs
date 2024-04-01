using JetBrains.Annotations;
using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class NormalMeleeComAndAuthoring : MonoBehaviour
    {
        public float MeleeAttackCooldown;
        //public int AttackVal;
        public float deathTimer;
        public class Baker : Baker<NormalMeleeComAndAuthoring>
        {
            public override void Bake(NormalMeleeComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                //AddComponent(entity, new NormalMeleeStateMachine
                //{
                //    CurrentState = EnemyState.Follow
                //});
                AddComponent(entity, new NormalMeleeAttack
                {
                    AttackCooldown = authoring.MeleeAttackCooldown,
                    //AttackVal = authoring.AttackVal,
                });
                AddComponent(entity, new NormalMeleeDeath
                {
                    timer = authoring.deathTimer,
                });
            }
        }
    }
    public struct NormalMeleeDeath : IComponentData
    {
        public float timer;
    }
    //public struct NormalMeleeStateMachine : IComponentData
    //{
    //    public EnemyState CurrentState;
    //}
    public struct NormalMeleeAttack : IComponentData
    {
        //public int AttackVal;
        public float AttackCooldown;
    }
}