using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public struct MoveAndLookInput : IComponentData
    {
        public float2 moveVal;
        public float2 lookVal;
    }
    public struct SprintInput : IComponentData, IEnableableComponent { }
    public struct ShootInput : IComponentData, IEnableableComponent { }
    public struct PlayerTag : IComponentData { }
    public class CameraTargetReference : IComponentData
    {
        public Transform ghostPlayer;
        public Transform cameraTarget;
    }

    public class ControllerAuthoring : MonoBehaviour
    {
        public class Baker : Baker<ControllerAuthoring>
        {
            public override void Bake(ControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent<MoveAndLookInput>(entity);
                AddComponent<SprintInput>(entity);
                AddComponent<ShootInput>(entity);
            }
        }
    }

}
