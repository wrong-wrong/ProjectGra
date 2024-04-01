using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public class ConfigAuthoringAddToSS : MonoBehaviour
    {
        [SerializeField] private float CameraXSensitivity = 1f;
        [SerializeField] private float CameraYSensitivity = 1f;
        [SerializeField] private float PlayerBasicMoveSpeed = 1f;
        [SerializeField] private float PlayerSprintMultiplier = 1f;

        [SerializeField] private int MaxHealthPoint;
        [SerializeField] private int HealthRegain;
        [SerializeField] private float Armor;
        [SerializeField] private float SpeedPercentage;



        [SerializeField] private float MeleeDamage;
        [SerializeField] private float RangedDamage;
        [SerializeField] private float ElementDamage;
        [SerializeField] private float AttackSpeed;

        [SerializeField] private float DamagePercentage;
        [SerializeField] private float CriticalHitChance;
        [SerializeField] private float Range;



        public class Baker : Baker<ConfigAuthoringAddToSS>
        {
            public override void Bake(ConfigAuthoringAddToSS authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ConfigComponent
                {
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
                });
            }
        }
    }
    public struct ConfigComponent : IComponentData
    {
        public float CamXSensitivity;
        public float CamYSensitivity;
        public float PlayerBasicMoveSpeedValue;
        public float PlayerSprintMultiplierValue;

        public int MaxHealthPoint;
        public int HealthRegain;
        public float Armor;
        public float SpeedPercentage;

        public float MeleeDamage;
        public float RangedDamage;
        public float ElementDamage;
        public float AttackSpeed;

        public float DamagePercentage;
        public float CriticalHitChance;
        public float Range;
    }

}