using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct InGameUIUpdateSystem : ISystem
    {
        int lastHealthPoint;
        int lastExperience;
        int lastMaterialCount;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (hp, exp, mat) in SystemAPI.Query<RefRO<EntityHealthPoint>, RefRO<PlayerExperienceAndLevel>, RefRO<PlayerMaterialCount>>())
            {
                if (mat.ValueRO.Count != lastMaterialCount || hp.ValueRO.HealthPoint != lastHealthPoint || exp.ValueRO.Exp != lastExperience )
                {
                    lastHealthPoint = hp.ValueRO.HealthPoint;
                    lastExperience = exp.ValueRO.Exp;
                    lastMaterialCount = mat.ValueRO.Count;
                    CanvasMonoSingleton.Instance.UpdateInGameUI(lastHealthPoint, lastExperience, lastMaterialCount);
                }
            }
        }
    }
}