using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TestingConfigComAndAuthoring : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float2 groupSpawnRangeOffset;
    public float spawnCooldown;
    public int groupSpawnCount;
    public float groupSpawnTimer;
    public float minRadius;
    public float maxRadius;
    public class Baker : Baker<TestingConfigComAndAuthoring> 
    {
        public override void Bake(TestingConfigComAndAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ForTestGroupSpawn
            {
                EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic),
                groupSpawnRangeOffset = authoring.groupSpawnRangeOffset,
                groupSpawnCount = authoring.groupSpawnCount,
                spawnCooldown = authoring.spawnCooldown,
                groupSpawnTimer = authoring.groupSpawnTimer,
                minRadius = authoring.minRadius,
                maxRadius = authoring.maxRadius
            });
        }
    }

    public struct ForTestGroupSpawn : IComponentData
    {
        public Entity EnemyPrefab;
        public float2 groupSpawnRangeOffset;
        public float spawnCooldown;
        public int groupSpawnCount;
        public float groupSpawnTimer;
        public float minRadius;
        public float maxRadius;
    }
}
