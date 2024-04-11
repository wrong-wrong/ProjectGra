using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct EnemySummonedExplosionSystem : ISystem, ISystemStartStop
    {


        //should only need by the system

        //for flying state
        //public float StartSpeed;
        //public float SpeedVariation;
        //public float StopSpeed;
        //scaling to set in every state,  basic scale should be previous scale,
        public float3 ScaleToSetInLaterThreeState;
        public float3 TimerToSetInLaterThreeState;

        // sin (1.57079632) == 1 ,
        // 1.256637 == 0.8f * 1.57079632;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<SummonedExplosionCom>();
            //Debug.Log(math.sin(1.57079632));
            //Debug.Log(math.sin(1.57079632 * 0.8f) + " - 0.8f * 1.57079632" + 1.57079632 * 0.8f);// 0.951056
            //Debug.Log(math.sin(1.57079632 * 0.7f) + " - 0.7f * 1.57079632" + 1.57079632 * 0.7f);// 0.891006 -  1.1f
            //Debug.Log(math.sin(1.57079632 * 0.6f) + " - 0.6f * 1.57079632" + 1.57079632 * 0.6f);// 0.809017 == sin(1.57079632 * 0.6f)

        }

        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<SummonedExplosionSystemConfigCom>();
            ScaleToSetInLaterThreeState = config.ScaleToSetInLaterThreeState;
            TimerToSetInLaterThreeState = config.TimerToSetInLaterThreeState;
        }

        public void OnStopRunning(ref SystemState state)
        {
            //throw new System.NotImplementedException();
        }

        public void OnUpdate(ref SystemState state)
        {
            //Debug.Log("SommonedExplosion System");
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            foreach (var (summon, scaling, transform, entity) in SystemAPI.Query<RefRW<SummonedExplosionCom>, RefRW<SpawneeScalingCom>, RefRW<LocalTransform>>().WithEntityAccess())
            {
                switch (summon.ValueRO.CurrentState)
                {
                    case SummonExplosionState.Summoning:
                        if (scaling.ValueRO.Ratio > 1f)   // should change state
                        {
                            //set state
                            summon.ValueRW.CurrentState = SummonExplosionState.Flying;
                            summon.ValueRW.FollowingEntity = playerEntity;
                            summon.ValueRW.OriginalPosition = transform.ValueRO.Position;

                            //set scaling
                            scaling.ValueRW.BasicScale = transform.ValueRW.Scale;
                            scaling.ValueRW.OffsetScale = -transform.ValueRO.Scale + ScaleToSetInLaterThreeState[0];    // offsetScale would be applied to the basic scale
                            scaling.ValueRW.Timer = TimerToSetInLaterThreeState[0];
                            scaling.ValueRW.RealTimer = 0f;
                        }
                        else
                        {
                            var tarTransform = SystemAPI.GetComponent<LocalTransform>(summon.ValueRO.FollowingEntity);
                            tarTransform.Position.y += 2.5f;
                            transform.ValueRW.Position = tarTransform.Position;
                        }
                        break;
                    case SummonExplosionState.Flying:

                        if (scaling.ValueRO.Ratio > 1f)   // should change state
                        {
                            //set state
                            summon.ValueRW.CurrentState = SummonExplosionState.Explode;
                            //set scaling
                            scaling.ValueRW.BasicScale = transform.ValueRW.Scale;
                            scaling.ValueRW.OffsetScale = -transform.ValueRO.Scale + ScaleToSetInLaterThreeState[1];
                            scaling.ValueRW.Timer = TimerToSetInLaterThreeState[1];
                            scaling.ValueRW.RealTimer = 0f;

                        }
                        else
                        {

                            // sin (1.57079632) == 1 ,
                            // 1.256637 == 0.8f * 1.57079632;
                            transform.ValueRW.Position = math.lerp(summon.ValueRO.OriginalPosition, playerTransform.Position, math.sin(scaling.ValueRO.Ratio * 1.1f));
                            //using lerp follow player

                        }
                        break;
                    case SummonExplosionState.Explode:

                        if (scaling.ValueRO.Ratio > 1f)   // should change state
                        {
                            //set state
                            summon.ValueRW.CurrentState = SummonExplosionState.Collapse;

                            //set scaling
                            scaling.ValueRW.BasicScale = transform.ValueRW.Scale;
                            scaling.ValueRW.OffsetScale = -transform.ValueRO.Scale + ScaleToSetInLaterThreeState[2];
                            scaling.ValueRW.Timer = TimerToSetInLaterThreeState[2];
                            scaling.ValueRW.RealTimer = 0f;
                        }

                        break;
                    case SummonExplosionState.Collapse:
                        //Debug.Log("Collapse");
                        if (scaling.ValueRO.Ratio > 1f)   // should change state
                        {
                            ecb.DestroyEntity(entity);
                        }
                        break;
                }
            }
        }
    }
}