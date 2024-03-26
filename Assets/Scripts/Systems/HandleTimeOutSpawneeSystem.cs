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
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }

        //destory or recycle  spawnee with disabled Timer
        public void OnUpdate(ref SystemState state)
        {
            var eq = SystemAPI.QueryBuilder().WithDisabled<SpawneeTimer>().Build().ToEntityArray(state.WorldUpdateAllocator);
            state.EntityManager.DestroyEntity(eq);
        }
    }
}