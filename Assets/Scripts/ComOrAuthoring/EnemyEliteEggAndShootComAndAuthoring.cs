using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyEliteEggAndShootComAndAuthoring : MonoBehaviour
    {
        public float Speed;
        public float stateOneInSkillShootingInterval;
        public float spawnEggSkillSpawningInterval;

        public int stageOneSkillShootCount;
        public int spawnEggSkillspawnCount;
        public class Baker : Baker<EnemyEliteEggAndShootComAndAuthoring>
        {
            public override void Bake(EnemyEliteEggAndShootComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyEliteEggAndShootCom
                {
                    Speed = authoring.Speed
                ,
                    stageOneSkillShootCount = authoring.stageOneSkillShootCount
                ,
                    spawnEggSkillspawnCount = authoring.spawnEggSkillspawnCount
                ,
                    stateOneInSkillShootingInterval = authoring.stateOneInSkillShootingInterval
                ,
                    spawnEggSkillSpawningInterval = authoring.spawnEggSkillSpawningInterval
                });
                AddBuffer<EnemyEliteFlyingEggBuffer>(entity);
            }
        }
    }
    public struct EnemyEliteEggAndShootCom : IComponentData
    {
        public float Speed;
        public float3 TargetDirNormalized;
        public float MovingTimer;
        public float MovingRandomIntervalTimer;
        public float SkillShootRandomIntervalTimer;

        public float previousHp;
        public float stateOneInSkillShootingInterval;
        public float spawnEggSkillSpawningInterval;

        public int stageOneSkillShootCount;
        public int spawnEggSkillspawnCount;
    }
    [InternalBufferCapacity(0)]
    public struct EnemyEliteFlyingEggBuffer : IBufferElementData
    {
        public float EggTimer;
        public Entity EggInstance;
        public float2 StartPos;
        public float2 EndPos;
    }
}