using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyEliteShooterComAndAuthoring : MonoBehaviour
    {
        //public float HealthPoint;

        public class Baker : Baker<EnemyEliteShooterComAndAuthoring>
        {
            public override void Bake(EnemyEliteShooterComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyEliteShooterCom { });
            }
        }
    }
    public struct EnemyEliteShooterCom : IComponentData 
    {
        public float MovingRandomIntervalCooldown;
        public float SkillCooldownRealTimer;
        public float3 TargetDirNormalizedMulSpeed;
        public bool IsDoingSkill;
        public float InSkillIntervalRealTimer;
        public int CurrentSpawnPosIdx;
        public int ShootLeftCount;
    }
}