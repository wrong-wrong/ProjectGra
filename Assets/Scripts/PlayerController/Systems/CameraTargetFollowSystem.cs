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
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach(var (localtransform, moveandlook) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveAndLookInput>>())
            {
                _cameraPitch -= moveandlook.ValueRO.lookVal.y * CamYSensitivity;
                _cameraPitch = clampAngle(_cameraPitch);
                //Debug.Log(_cameraPitch);
                camReference.ghoshPlayer.position = localtransform.ValueRO.Position;
                camReference.ghoshPlayer.rotation = localtransform.ValueRO.Rotation;
                camReference.cameraTarget.localRotation = quaternion.Euler(math.radians(_cameraPitch), 0f, 0f);
                //cameraTarget.rotation = math.mul(localtransform.ValueRO.Rotation, quaternion.RotateX(_cameraPitch));
                //cameraTarget.localRotation = quaternion.Euler(_cameraPitch, 0f, 0f); //should be child of the ghost player


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