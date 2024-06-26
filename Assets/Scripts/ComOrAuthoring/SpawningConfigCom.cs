using Unity.Entities;

namespace ProjectGra
{

    [InternalBufferCapacity(0)]
    public struct SpawningConfigBuffer : IBufferElementData
    {
        public float PointSpawnChance;
        // set SpawnCooldown to negtive to indicate this wave involves horde or elite
        public float SpawnCooldown;
        public float GroupSpawnCooldown;
        public int GroupSpawnCount;
    }
    public struct EnemyHpAndDmgModifierWithDifferentDifficulty : IComponentData
    {
        public float HealthPointModifier;
        public float DamageModifier;
    }
}