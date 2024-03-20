using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace OOPExperiment
{
    public partial struct ExperiMovingTargetSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExperiExecuteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach(var (localTransform, speed)in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MovingTargetSpeed>>())
            {
                var position = localTransform.ValueRO.Position + new float3(speed.ValueRO.speed * Time.deltaTime,0,0);
                localTransform.ValueRW.Position = position;
            }
        }
    }

}