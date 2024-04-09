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
        float previousSetCooldown;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (hp, exp, mat,wpState) in SystemAPI.Query<RefRO<EntityHealthPoint>, RefRO<PlayerExperienceAndLevel>, RefRO<PlayerMaterialCount>, RefRO<MainWeaponState>>())
            {
                if(wpState.ValueRO.RealCooldown > 0)
                {
                    CanvasMonoSingleton.Instance.IngameUIWeaponCooldownFilling(1 - wpState.ValueRO.RealCooldown / wpState.ValueRO.Cooldown);
                    previousSetCooldown = wpState.ValueRO.RealCooldown;
                }
                else if(previousSetCooldown > 0)
                {
                    CanvasMonoSingleton.Instance.IngameUIWeaponCooldownFilling(1);
                }
                if (mat.ValueRO.Count != lastMaterialCount || hp.ValueRO.HealthPoint != lastHealthPoint || exp.ValueRO.Exp != lastExperience )
                {
                    lastHealthPoint = hp.ValueRO.HealthPoint;
                    lastExperience = exp.ValueRO.Exp;
                    lastMaterialCount = mat.ValueRO.Count;
                    CanvasMonoSingleton.Instance.IngameUIUpdataPlayerStats(lastHealthPoint, lastExperience, lastMaterialCount);
                }
            }
        }
    }
}