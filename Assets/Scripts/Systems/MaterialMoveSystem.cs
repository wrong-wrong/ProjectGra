using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectGra
{
    public partial struct LootMoveSystem : ISystem, ISystemStartStop
    {
        private float speed;
        private float totalTimer;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }

        public void OnStartRunning(ref SystemState state)
        {   
            var config = SystemAPI.GetSingleton<MaterialConfig>();
            speed = config.Speed;
            totalTimer = config.TotalTimer;
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state) 
        {
            var speedMulDeltaTimer = speed * SystemAPI.Time.DeltaTime;
            var deltatime = SystemAPI.Time.DeltaTime;
            foreach(var (transform, materialMoveTag, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<LootMoveCom>>().WithEntityAccess())
            {
                transform.ValueRW.Position.xz += materialMoveTag.ValueRO.tarDir * speedMulDeltaTimer;
                var ratio = (materialMoveTag.ValueRW.accumulateTimer += deltatime) / totalTimer;
                if(ratio > 1f)
                {
                    SystemAPI.SetComponentEnabled<LootMoveCom>(entity, false);
                }
                else
                {
                    transform.ValueRW.Position.y = math.sin(math.radians(ratio * 180f));
                }
            }
        }

    }
}