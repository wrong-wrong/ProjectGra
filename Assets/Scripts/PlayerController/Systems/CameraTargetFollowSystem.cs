using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using static UnityEngine.Rendering.DebugUI;
namespace ProjectGra.PlayerController
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(PlayerMoveSystem))]
    public partial struct CameraTargetFollowSystem : ISystem,ISystemStartStop
    {
        private float _cameraPitch;
        private float _topClamp;
        private float _bottomClamp;
        private float CamYSensitivity;
        private float3 mainWpOffset;
        private NativeArray<float3> offsetList;
        //private LocalTransform tmpTransform;
        public void OnCreate(ref SystemState state)
        {
            _cameraPitch = 0.0f;
            _topClamp = 90.0f;
            _bottomClamp = -90.0f;
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<ConfigComponent>();
            state.RequireForUpdate<CameraTargetReference>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<GameControllNotPaused>();
        }
        public void OnStartRunning(ref SystemState state)
        {
            offsetList = new NativeArray<float3>(3, Allocator.Persistent);
            var configCom = SystemAPI.GetSingleton<ConfigComponent>();
            CamYSensitivity = configCom.CamYSensitivity;
            mainWpOffset = configCom.mainWpOffset;
            offsetList[0] = configCom.leftAutoWpOffset;
            offsetList[1] = configCom.midAutoWpOffset;
            offsetList[2] = configCom.rightAutoWpOffset;
            //tmpTransform = LocalTransform.Identity;
        }
        public void OnStopRunning(ref SystemState state) { offsetList.Dispose(); }
        public void OnUpdate(ref SystemState state)
        {
            var camReference= SystemAPI.ManagedAPI.GetSingleton<CameraTargetReference>();
            var ghostPlayer = camReference.ghostPlayer;
            var cameraTarget = camReference.cameraTarget;

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponentRO<LocalTransform>(playerEntity);
            var moveandlook = SystemAPI.GetComponentRO<MoveAndLookInput>(playerEntity);
            var mainWpState = SystemAPI.GetComponentRW<MainWeapon>(playerEntity);
            var autoWpBuffer = SystemAPI.GetBuffer<AutoWeaponBuffer>(playerEntity);

            _cameraPitch -= moveandlook.ValueRO.lookVal.y * CamYSensitivity;
            _cameraPitch = clampAngle(_cameraPitch);
            ghostPlayer.position = playerTransform.ValueRO.Position;
            ghostPlayer.rotation = playerTransform.ValueRO.Rotation;
            cameraTarget.localRotation = quaternion.Euler(math.radians(_cameraPitch), 0f, 0f);
            var camforward = cameraTarget.forward;
            var camup = cameraTarget.up;
            var camright = cameraTarget.right;
            if(mainWpState.ValueRO.WeaponIndex != -1)
            {
                var offset = mainWpState.ValueRO.WeaponPositionOffset + mainWpOffset;

                //mainWeaponTransformRW.ValueRW.Position = (camforward * offset.z + camright * offset.x + camup * offset.y + cameraTarget.position);
                //mainWeaponTransformRW.ValueRW.Rotation = quaternion.LookRotation(camforward, math.up());
                //mainWpState.ValueRW.mainWeaponLocalTransform = mainWeaponTransformRW.ValueRO;

                mainWpState.ValueRW.mainWeaponLocalTransform.Position = (camforward * offset.z + camright * offset.x + camup * offset.y + cameraTarget.position);
                mainWpState.ValueRW.mainWeaponLocalTransform.Rotation = quaternion.LookRotation(camforward, math.up());
                if(!mainWpState.ValueRO.IsMeleeWeapon || mainWpState.ValueRO.WeaponCurrentState == WeaponState.None)//(mainWpState.ValueRO.WeaponCurrentState != WeaponState.Thrust && mainWpState.ValueRO.WeaponCurrentState != WeaponState.Retrieve))
                {
                    SystemAPI.GetComponentRW<LocalTransform>(mainWpState.ValueRO.WeaponModel).ValueRW = mainWpState.ValueRW.mainWeaponLocalTransform; 
                    //var mainWeaponTransformRW = SystemAPI.GetComponentRW<LocalTransform>(mainWpState.ValueRO.WeaponModel);
                    //mainWeaponTransformRW.ValueRW = mainWpState.ValueRW.mainWeaponLocalTransform;
                }
            }

            var forward = playerTransform.ValueRO.Forward();
            var right = playerTransform.ValueRO.Right();
            for (int i = 0, n = autoWpBuffer.Length; i < n; ++i)
            {
                ref var wp = ref autoWpBuffer.ElementAt(i);
                if (wp.WeaponIndex != -1)
                {
                    var offset = wp.WeaponPositionOffset + offsetList[i];
                    //transformRW.ValueRW.Position = (camforward * offset.z + camright * offset.x + camup * offset.y + cameraTarget.position);
                    wp.autoWeaponLocalTransform.Position = (forward * offset.z + right * offset.x + playerTransform.ValueRO.Position);
                    if (!wp.IsMeleeWeapon || wp.WeaponCurrentState == WeaponState.None)//(wp.WeaponCurrentState != WeaponState.Thrust && wp.WeaponCurrentState != WeaponState.Retrieve))
                    {
                        SystemAPI.GetComponentRW<LocalTransform>(wp.WeaponModel).ValueRW.Position = wp.autoWeaponLocalTransform.Position;

                        //var transformRW = SystemAPI.GetComponentRW<LocalTransform>(wp.WeaponModel);
                        //transformRW.ValueRW.Position = wp.autoWeaponLocalTransform.Position;
                    }
                }
            }

            //foreach (var (localtransform, moveandlook, mainWeaponState) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveAndLookInput>, RefRW<MainWeaponState>>())
            //{
            //    _cameraPitch -= moveandlook.ValueRO.lookVal.y * CamYSensitivity;
            //    _cameraPitch = clampAngle(_cameraPitch);
            //    ghostPlayer.position = localtransform.ValueRO.Position;
            //    ghostPlayer.rotation = localtransform.ValueRO.Rotation;
            //    cameraTarget.localRotation = quaternion.Euler(math.radians(_cameraPitch), 0f, 0f);

            //    var offset = mainWeaponState.ValueRO.WeaponPositionOffset;

            //    //trying   pos + Forward * offset.z + Right * offset.x + Up * offset.y;
            //    //this can make the position fixed at a point relative to the camera
            //    // and LookRotation handling the rotation
            //    var mainWeaponTransformRW = SystemAPI.GetComponentRW<LocalTransform>(mainWeaponState.ValueRO.WeaponModel);
            //    var camforward = cameraTarget.forward;
            //    mainWeaponTransformRW.ValueRW.Position = (float3)(cameraTarget.position + camforward * offset.z + cameraTarget.right * offset.x + cameraTarget.up * offset.y);
            //    //!!!   offset can't be 0,0,0,     or LookRotation would be NaN!!!!
            //    mainWeaponTransformRW.ValueRW.Rotation = quaternion.LookRotation(camforward, math.up());
            //    mainWeaponState.ValueRW.mainWeaponLocalTransform = mainWeaponTransformRW.ValueRO;
            //}

        }
        private float clampAngle(float pitch)
        {
            if (pitch < -360f) pitch += 360f;
            if (pitch > 360f) pitch -= 360f;
            return math.clamp(pitch, _bottomClamp, _topClamp);
        }
    }
}