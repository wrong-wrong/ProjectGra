using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
namespace ProjectGra.PlayerController
{
    public partial struct CameraTargetFollowSystem : ISystem,ISystemStartStop
    {
        private float _cameraPitch;
        private float _topClamp;
        private float _bottomClamp;
        private float CamYSensitivity;
        public void OnCreate(ref SystemState state)
        {
            _cameraPitch = 0.0f;
            _topClamp = 90.0f;
            _bottomClamp = -90.0f;
            state.RequireForUpdate<ConfigComponent>();
            state.RequireForUpdate<CameraTargetReference>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<GameControllNotPaused>();
        }
        public void OnStartRunning(ref SystemState state)
        {
            var configCom = SystemAPI.GetSingleton<ConfigComponent>();
            CamYSensitivity = configCom.CamYSensitivity;
        }
        public void OnStopRunning(ref SystemState state) { }
        public void OnUpdate(ref SystemState state)
        {
            var camReference= SystemAPI.ManagedAPI.GetSingleton<CameraTargetReference>();
            var ghostPlayer = camReference.ghostPlayer;
            var cameraTarget = camReference.cameraTarget;
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach(var (localtransform, moveandlook, mainWeaponState) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveAndLookInput>,RefRO<MainWeaponState>>())
            {
                _cameraPitch -= moveandlook.ValueRO.lookVal.y * CamYSensitivity;
                _cameraPitch = clampAngle(_cameraPitch);
                //Debug.Log(_cameraPitch);
                ghostPlayer.position = localtransform.ValueRO.Position;
                ghostPlayer.rotation = localtransform.ValueRO.Rotation;
                cameraTarget.localRotation = quaternion.Euler(math.radians(_cameraPitch), 0f, 0f);


                var offset = mainWeaponState.ValueRO.WeaponPositionOffset;

                //trying   pos + Forward * offset.z + Right * offset.x + Up * offset.y;
                //this can make the position fixed at a point relative to the camera
                // and LookRotation handling the rotation
                var mainWeaponTransformRW = SystemAPI.GetComponentRW<LocalTransform>(mainWeaponState.ValueRO.WeaponModel);
                var camforward = cameraTarget.forward;
                mainWeaponTransformRW.ValueRW.Position = (float3)(cameraTarget.position +camforward* offset.z + cameraTarget.right * offset.x + cameraTarget.up * offset.y);
                //!!!   offset can't be 0,0,0,     or LookRotation would be NaN!!!!
                mainWeaponTransformRW.ValueRW.Rotation = quaternion.LookRotation(camforward, math.up());

            }

        }
        private float clampAngle(float pitch)
        {
            if (pitch < -360f) pitch += 360f;
            if (pitch > 360f) pitch -= 360f;
            return math.clamp(pitch, _bottomClamp, _topClamp);
        }
    }
}