using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyEliteShooterComAndAuthoring : MonoBehaviour
    {
        //public float HealthPoint;
        public float Speed;
        public float stateOneInSkillShootingInterval;
        public float stateTwoInSkillShootingInterval;

        public int stageOneSkillShootCount;
        public int stageTwoSkillShootCount;
        public class Baker : Baker<EnemyEliteShooterComAndAuthoring>
        {
            public override void Bake(EnemyEliteShooterComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyEliteShooterCom { Speed = authoring.Speed
                ,stageOneSkillShootCount = authoring.stageOneSkillShootCount
                ,stageTwoSkillShootCount = authoring.stageTwoSkillShootCount
                ,stateOneInSkillShootingInterval = authoring.stateOneInSkillShootingInterval
                ,stateTwoInSkillShootingInterval = authoring.stateTwoInSkillShootingInterval});
            }
        }
    }
    public struct EnemyEliteShooterCom : IComponentData 
    {
        public float Speed;
        public float3 TargetDirNormalized;
        public float MovingTimer;
        public float MovingRandomIntervalTimer;
        public float SkillShootRandomIntervalTimer;

        public float previousHp;
        public float stateOneInSkillShootingInterval;
        public float stateTwoInSkillShootingInterval;

        public int stageOneSkillShootCount;
        public int stageTwoSkillShootCount;
        //public float HeathPoint;
    }
}