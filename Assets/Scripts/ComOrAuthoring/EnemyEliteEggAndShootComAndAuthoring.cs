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
                AddComponent(entity, new EnemyEliteEggAndShootCom{});
                AddBuffer<EnemyEliteFlyingEggBuffer>(entity);
            }
        }
    }
    public struct EnemyEliteEggAndShootCom : IComponentData
    {
        //public float Speed;
        public float3 TargetDirNormalizedMulSpeed;
        public float MovingRandomIntervalCooldown;
        public float SkillCooldownRealTimer;
        public float InSkillIntervalRealTimer;
        public int SkillCountLeft;
        public int CurrentSpawnPosIdx;
        public bool IsDoingSkill;
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