using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public class ConfigAuthoringAddToSS : MonoBehaviour
    {
        [SerializeField] private float3 mainWpOffset;
        [SerializeField] private float3 leftAutoWpOffset;
        [SerializeField] private float3 rightAutoWpOffset;
        [SerializeField] private float3 midAutoWpOffset;
        [SerializeField] private float ghostPlayerHeightOffset;

        [SerializeField] private float CameraXSensitivity = 1f;
        [SerializeField] private float CameraYSensitivity = 1f;
        [SerializeField] private float PlayerBasicMoveSpeed = 1f;
        [SerializeField] private float PlayerSprintMultiplier = 1f;

        [SerializeField] private int MaxHealthPoint;
        [SerializeField] private int HealthRegain;
        [SerializeField] private int Armor;
        [SerializeField] private float SpeedPercentage;



        [SerializeField] private float MeleeDamage;
        [SerializeField] private float RangedDamage;
        [SerializeField] private float ElementDamage;
        [SerializeField] private float AttackSpeed;

        [SerializeField] private float DamagePercentage;
        [SerializeField] private float CriticalHitChance;
        [SerializeField] private float Range;

        [SerializeField] private float LifeSteal;
        [SerializeField] private float Dodge;

        [Header("Enemy Spawning Config")]
        public float2 MapRightUpperPointF2;

        public float SpawningCooldown;
        public float minRadius;
        public float maxRadius;
        public float EliteChanceInSpecialWave;
        [Header("Game Wave Time Config")]
        public float BeginWaveTimeSet;
        public float InWaveTimeSet;
        [Header("Material Config")]
        public float MaterialSpeed;
        public float MaterialTotalTimer;

        public class Baker : Baker<ConfigAuthoringAddToSS>
        {
            public override void Bake(ConfigAuthoringAddToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerConfigComponent
                {
                    ghostPlayerHeightOffset = authoring.ghostPlayerHeightOffset,
                    mainWpOffset = authoring.mainWpOffset,
                    midAutoWpOffset = authoring.midAutoWpOffset,
                    leftAutoWpOffset = authoring.leftAutoWpOffset,
                    rightAutoWpOffset = authoring.rightAutoWpOffset,
                    CamXSensitivity = authoring.CameraXSensitivity,
                    CamYSensitivity = authoring.CameraYSensitivity,
                    PlayerBasicMoveSpeedValue = authoring.PlayerBasicMoveSpeed,
                    PlayerSprintMultiplierValue = authoring.PlayerSprintMultiplier,

                    MaxHealthPoint = authoring.MaxHealthPoint,
                    HealthRegain = authoring.HealthRegain,
                    Armor = authoring.Armor,
                    SpeedPercentage = authoring.SpeedPercentage,

                    MeleeDamage = authoring.MeleeDamage,
                    RangedDamage = authoring.RangedDamage,
                    ElementDamage = authoring.ElementDamage,
                    AttackSpeed = authoring.AttackSpeed,

                    CriticalHitChance = authoring.CriticalHitChance,
                    DamagePercentage = authoring.DamagePercentage,
                    Range = authoring.Range,

                    LifeSteal = authoring.LifeSteal,
                    Dodge = authoring.Dodge,
                });
                AddComponent(entity, new EnemySpawningConfig
                {
                    MapRightUpperPointF2 = authoring.MapRightUpperPointF2,
                    SpawningCooldown = authoring.SpawningCooldown,
                    maxRadius = authoring.maxRadius,
                    minRadius = authoring.minRadius,
                    EliteChanceInSpecialWave = authoring.EliteChanceInSpecialWave,
                });
                //AddComponent(entity, new GameWaveTimeConfig { InWaveTime = authoring.InWaveTimeSet, BeginWaveTime = authoring.BeginWaveTimeSet });
                AddComponent(entity, new MaterialConfig { Speed = authoring.MaterialSpeed, TotalTimer = authoring.MaterialTotalTimer });
            }
        }
    }
    public struct MaterialConfig : IComponentData
    {
        public float Speed;
        public float TotalTimer;
    }
    //public struct GameWaveTimeConfig : IComponentData
    //{
    //    public float BeginWaveTime;
    //    public float InWaveTime;
    //}
    public struct EnemySpawningConfig : IComponentData
    {
        public float2 MapRightUpperPointF2;
        public float SpawningCooldown;
        public float minRadius;
        public float maxRadius;
        public float EliteChanceInSpecialWave;
    }
    public struct PlayerConfigComponent : IComponentData
    {
        public float3 mainWpOffset;
        public float3 leftAutoWpOffset;
        public float3 rightAutoWpOffset;
        public float3 midAutoWpOffset;

        public float ghostPlayerHeightOffset;

        public float CamXSensitivity;
        public float CamYSensitivity;
        public float PlayerBasicMoveSpeedValue;
        public float PlayerSprintMultiplierValue;

        public int MaxHealthPoint;
        public int HealthRegain;
        public int Armor;
        public float SpeedPercentage;

        public float MeleeDamage;
        public float RangedDamage;
        public float ElementDamage;
        public float AttackSpeed;

        public float DamagePercentage;
        public float CriticalHitChance;
        public float Range;

        public float LifeSteal;
        public float Dodge;
    }

}