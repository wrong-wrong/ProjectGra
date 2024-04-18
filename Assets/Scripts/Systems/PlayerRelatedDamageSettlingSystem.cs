using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
    public partial struct PlayerRelatedDamageSettlingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state) 
        {

            var playerSuccessHitCount = SystemAPI.GetSingletonRW<PlayerSuccessfulAttackCount>();
            var damageBuffer = SystemAPI.GetSingletonBuffer<PlayerDamagedRecordBuffer>();
            if(damageBuffer.Length > 0)
            {
                int damageGet = 0;
                for (int i = 0, n = damageBuffer.Length; i < n; i++)
                {
                    damageGet += damageBuffer[i].Value;
                }
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerHpRW = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
                playerHpRW.ValueRW.HealthPoint -= damageGet;
                damageBuffer.Clear();
            }

            if(playerSuccessHitCount.ValueRO.Value > 0)
            {
                // TODO : apply life steal logic
                playerSuccessHitCount.ValueRW.Value = 0;
            }
        }
    }
}