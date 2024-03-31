using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform),OrderLast = true)]
    public partial struct DamageApplyingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach(var (attribute, damageRecord) in SystemAPI.Query<RefRW<PlayerAttributeMain>, RefRW<PlayerDamagedRecordCom>>())
            {
                if(damageRecord.ValueRO.damagedThisFrame > 0f)
                {
                    attribute.ValueRW.CurrentHealthPoint -= damageRecord.ValueRO.damagedThisFrame;
                    damageRecord.ValueRW.damagedThisFrame = 0;
                }
            }
        }
    }
}