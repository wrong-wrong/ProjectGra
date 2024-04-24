using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
    public partial struct PlayerRelatedDamageSettlingSystem : ISystem, ISystemStartStop
    {
        private Random random;
        private float accumulateHp;
        private float hpRegenerationSpeed;
        private float oneSecondTimer;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            random = Random.CreateFromIndex(0);
        }

        public void OnStartRunning(ref SystemState state)
        {
            var playerAttribute = SystemAPI.GetSingletonRW<PlayerAttributeMain>();
            accumulateHp = 0f;
            hpRegenerationSpeed = 0f;
            if(playerAttribute.ValueRO.HealthRegain > 0)
            {
                hpRegenerationSpeed = 0.2f + (playerAttribute.ValueRO.HealthRegain - 1) * 0.089f;
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state) 
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            if((oneSecondTimer += deltatime) > 1f)
            {
                oneSecondTimer = 0f;
                if((accumulateHp += hpRegenerationSpeed) > 1f)
                {
                    var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                    var playerHpRW = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
                    ++playerHpRW.ValueRW.HealthPoint;
                    accumulateHp = 0f;
                    Debug.Log("Attribute related - Health Point Regenerated!");
                }

            }
            var playerAttribute = SystemAPI.GetSingletonRW<PlayerAttributeMain>();
            var playerSuccessHitCount = SystemAPI.GetSingletonRW<PlayerSuccessfulAttackCount>();
            var damageBuffer = SystemAPI.GetSingletonBuffer<PlayerDamagedRecordBuffer>();
            if(damageBuffer.Length > 0)
            {
                int damageGet = 0;
                for (int i = 0, n = damageBuffer.Length; i < n; i++)
                {
                    if (random.NextFloat() >= playerAttribute.ValueRO.Dodge) damageGet += damageBuffer[i].Value;
                    else
                    {
                        Debug.Log("Attribute related - Dodged!");
                        EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList.Add(0);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList.Add(-2);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList.Add(0);
                    }
                }
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerHpRW = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
                playerHpRW.ValueRW.HealthPoint -= (int)(damageGet * (1f - playerAttribute.ValueRO.Armor));
                damageBuffer.Clear();
            }

            if(playerSuccessHitCount.ValueRO.Value > 0 && playerAttribute.ValueRO.LifeSteal > 0f)
            {
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var playerHpRW = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
                // TODO : apply life steal logic
                for (int i = 0, n = playerSuccessHitCount.ValueRO.Value; i < n; i++)
                {
                    if(random.NextFloat()<playerAttribute.ValueRW.LifeSteal)
                    {
                        ++playerHpRW.ValueRW.HealthPoint;
                        Debug.Log("Attribute related - Life Steal!");
                        EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList.Add(0);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList.Add(-1);
                        EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList.Add(0);
                    }
                }
                playerSuccessHitCount.ValueRW.Value = 0;
            }
        }
    }
}