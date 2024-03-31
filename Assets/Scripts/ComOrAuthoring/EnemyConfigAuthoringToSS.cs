using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyConfigAuthoringToSS : MonoBehaviour
    {
        [Header("NormalMeleeConfig")]
        public float MeleeFollowSpeed;
        public float MeleeAttackDistance;
        public float MeleeAttackCooldown;
        public float MeleeDeathCountdown;
        public class Baker : Baker<EnemyConfigAuthoringToSS>
        {
            public override void Bake(EnemyConfigAuthoringToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new NormalMeleeConfigCom
                {
                    FollowSpeed = authoring.MeleeFollowSpeed,
                    AttackDistance = authoring.MeleeAttackDistance,
                    AttackCooldown = authoring.MeleeAttackCooldown,
                    DeathCountdown = authoring.MeleeDeathCountdown
                });
            }
        }
    }

    public struct NormalMeleeConfigCom : IComponentData
    {
        public float FollowSpeed;
        public float AttackDistance;
        public float AttackCooldown;
        public float DeathCountdown;
    }
}