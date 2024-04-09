using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(PlayerSpawnSpawneeSystem))]
    public partial struct HandleTimeOutSpawneeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }

        //destory or recycle  spawnee with disabled Timer
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (timer, entity)in SystemAPI.Query<RefRW<SpawneeTimer>>().WithEntityAccess())
            {
                if ((timer.ValueRW.Value -= deltaTime) < 0f) ecb.DestroyEntity(entity);
            }
        }
    }
}