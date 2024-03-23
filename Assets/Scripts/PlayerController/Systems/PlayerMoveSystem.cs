using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectGra
{
    public partial struct PlayerMoveSystem : ISystem, ISystemStartStop
    {
        //TODO config : speed; sprint multiplier;
        float playerBasicSpeed;
        float playerSprintMultiplier;
        float CamXSensitivity;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<GameControllNotPaused>();
        }
        public void OnStartRunning(ref SystemState state) 
        {
            var configCom = SystemAPI.GetSingleton<ConfigComponent>();
            playerBasicSpeed = configCom.PlayerBasicMoveSpeedValue;
            playerSprintMultiplier = configCom.PlayerSprintMultiplierValue;
            CamXSensitivity = configCom.CamXSensitivity;
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
                localtransform.ValueRW = localtransform.ValueRO.Translate(moveVector);
                localtransform.ValueRW = localtransform.ValueRW.RotateY(moveandlook.ValueRO.lookVal.x *  deltaTime * CamXSensitivity);
            }
        }
    }
}