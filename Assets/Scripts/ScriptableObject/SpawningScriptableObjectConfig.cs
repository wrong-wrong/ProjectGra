using System.Collections.Generic;
using UnityEngine;

namespace ProjectGra
{
    [CreateAssetMenu(menuName = "MyWaveSpawningSO")]
    public class SpawningScriptableObjectConfig : ScriptableObject
    {
        public float EnemyHealthPointModifier;
        public float EnemyDamageModifier;

        public List<bool> IsHordeOrElite;
        public List<float> PointSpawnChance;
        public List<float> SpawningCooldown;
    }
}