using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class EnemyEliteSprintAndShootComAndAuthoring : MonoBehaviour
    {
        public class Baker : Baker<EnemyEliteSprintAndShootComAndAuthoring>
        {
            public override void Bake(EnemyEliteSprintAndShootComAndAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<EnemyEliteSprintAndShootCom>(entity);
            }
        }
    }
    public struct EnemyEliteSprintAndShootCom : IComponentData
    {
        public quaternion RightSpawnPosQuaternion;
        public quaternion LeftSpawnPosQuaternion;
        public float spawnIntervalRealTimer;
        public int spawneeLeftCount;
        public float skillCooldown;
        public float sprintCountdown;
        public float2 tarDirNormalizedMulSpeed;
        public int currentSprintDamage;
        public float movementResetCountdown;
        //public bool isDoingSkill;
    }
}