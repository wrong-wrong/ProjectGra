using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(PlayerInputSystem))]
    public partial struct PlayerMoveSystem : ISystem, ISystemStartStop
    {
        //TODO config : speed; sprint multiplier;
        float playerBasicSpeed;
        float playerOriginalSpeed;
        float playerSprintMultiplier;
        float CamXSensitivity;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<GameControllNotPaused>();
        }
        public void OnStartRunning(ref SystemState state) 
        {
            var configCom = SystemAPI.GetSingleton<PlayerConfigComponent>();
            playerOriginalSpeed = configCom.PlayerBasicMoveSpeedValue;
            playerSprintMultiplier = configCom.PlayerSprintMultiplierValue;
            CamXSensitivity = configCom.CamXSensitivity;
            var playerAttribute = SystemAPI.GetSingleton<PlayerAttributeMain>();
            Debug.Log("Attribute related - Speed modified with percentage : " + (playerAttribute.SpeedPercentage + 1));
            playerBasicSpeed = playerOriginalSpeed * (1 + playerAttribute.SpeedPercentage);
        }
        public void OnStopRunning(ref SystemState state) { }
        public void OnUpdate(ref SystemState state) 
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach(var(localtransform, moveandlook, sprint) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveAndLookInput>, EnabledRefRO<SprintInput>>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                float sprintMultiplier = sprint.ValueRO ? playerSprintMultiplier : 1;
                var moveVector = new float3(moveandlook.ValueRO.moveVal.x, 0, moveandlook.ValueRO.moveVal.y) * deltaTime * playerBasicSpeed * sprintMultiplier;
                moveVector = math.mul(localtransform.ValueRO.Rotation, moveVector);
                localtransform.ValueRW.Position += moveVector;
                var targetRotationVal = moveandlook.ValueRO.lookVal.x * deltaTime * CamXSensitivity;
                localtransform.ValueRW = localtransform.ValueRW.RotateY(targetRotationVal);

            }
        }
    }
}