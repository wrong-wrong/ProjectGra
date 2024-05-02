using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct InGameUIUpdateSystem : ISystem
    {
        int lastHealthPoint;
        int lastExperience;
        int lastMaterialCount;
        int lastNormalCrateCount;
        int lastLegendaryCrateCount;
        float previousSetCooldown;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (hp, exp, mat, item, wpState) in SystemAPI.Query<EntityHealthPoint, PlayerExperience, PlayerMaterialCount, PlayerItemCount, MainWeapon>())
            {
                if(wpState.RealCooldown > 0)
                {
                    CanvasMonoSingleton.Instance.IngameUIWeaponCooldownFilling(1 - wpState.RealCooldown / wpState.Cooldown);
                    previousSetCooldown = wpState.RealCooldown;
                }
                else if(previousSetCooldown > 0)
                {
                    CanvasMonoSingleton.Instance.IngameUIWeaponCooldownFilling(1);
                }
                if (mat.Count != lastMaterialCount)
                {
                    lastMaterialCount = mat.Count;
                    CanvasMonoSingleton.Instance.IngameUIUpdatePlayerMaterial(lastMaterialCount);
                }
                if(hp.HealthPoint != lastHealthPoint)
                {
                    lastHealthPoint = hp.HealthPoint;
                    CanvasMonoSingleton.Instance.IngameUIUpdatePlayerHp(lastHealthPoint);
                }
                if(exp.Exp != lastExperience)
                {
                    lastExperience = exp.Exp;
                    CanvasMonoSingleton.Instance.IngameUIUpdatePlayerExp(lastExperience);
                }
                if(item.Normal != lastNormalCrateCount)
                {
                    lastNormalCrateCount = item.Normal;
                    CanvasMonoSingleton.Instance.IngameUIAddNormalCrateIcon(lastNormalCrateCount);
                }
                if(item.Legendary != lastLegendaryCrateCount)
                {
                    lastLegendaryCrateCount = item.Legendary;
                    CanvasMonoSingleton.Instance.IngameUIAddLegendaryCrateIcon(lastLegendaryCrateCount);
                }
            }
        }
    }
}