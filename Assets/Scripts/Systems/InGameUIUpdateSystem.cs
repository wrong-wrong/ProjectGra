using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct InGameUIUpdateSystem : ISystem
    {
        int lastHealthPoint;
        float lastExperience;
        int lastMaterialCount;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (hp, exp, mat) in SystemAPI.Query<RefRO<EntityHealthPoint>, RefRO<PlayerExperience>, RefRO<PlayerMaterialCount>>())
            {
                if (mat.ValueRO.Count != lastMaterialCount || hp.ValueRO.HealthPoint != lastHealthPoint || exp.ValueRO.Value != lastExperience )
                {
                    lastHealthPoint = hp.ValueRO.HealthPoint;
                    lastExperience = exp.ValueRO.Value;
                    lastMaterialCount = mat.ValueRO.Count;
                    CanvasMonoSingleton.Instance.UpdateInGameUI(lastHealthPoint, lastExperience, lastMaterialCount);
                }
            }
        }
    }
}