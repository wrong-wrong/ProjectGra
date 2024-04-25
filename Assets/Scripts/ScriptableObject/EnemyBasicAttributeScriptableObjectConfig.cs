using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyEnemyBasicAttibuteSO")]
    public class EnemyBasicAttributeScriptableObjectConfig : ScriptableObject
    {
        public string EnemyTypeName;
        public GameObject EnemyPrefab;
        public int AppearCodingWave;
        public int HealthPoint;
        public int HPIncreasePerWave;
        public int Damage;
        public float DmgIncreasePerWave;
        public float Speed;
        public int MaterialsDropped;
        public float LootCrateDropRate;
        public float ConsumableDropRate;
    }
}