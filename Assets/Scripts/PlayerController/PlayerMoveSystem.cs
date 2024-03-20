using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectGra
{
    public partial struct PlayerMoveSystem : ISystem
    {   
        //TODO config : speed; sprint multiplier;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state) 
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach(var(localtransform, moveandlook, sprint) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveAndLookInput>, EnabledRefRO<SprintInput>>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                var moveVector = new float3(moveandlook.ValueRO.moveVal.x, 0, moveandlook.ValueRO.moveVal.y) * deltaTime;
                moveVector = math.mul(localtransform.ValueRO.Rotation, moveVector);
                localtransform.ValueRW = localtransform.ValueRO.Translate(moveVector);
                localtransform.ValueRW = localtransform.ValueRW.RotateY(moveandlook.ValueRO.lookVal.x *  deltaTime);
            }
        }
    }
}