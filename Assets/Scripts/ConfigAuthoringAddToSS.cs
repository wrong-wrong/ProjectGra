using JetBrains.Annotations;
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
    }

}