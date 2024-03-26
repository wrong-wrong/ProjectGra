using ProjectGra.PlayerController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp),OrderFirst = true)]
    public partial struct InitializeSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { 
            state.RequireForUpdate<SuperSingletonTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<ConfigComponent>();
        }
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            state.EntityManager.AddComponentObject(singleton, new CameraTargetReference { 
                cameraTarget = CameraTargetMonoSingleton.instance.CameraTargetTransform, 
                ghostPlayer = CameraTargetMonoSingleton.instance.transform});
            state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
            state.EntityManager.AddComponentObject(singleton, new MyCanvasGroupManagedCom { canvasGroup = CanvasMonoSingleton.instance.canvasGroup });

            var configCom = SystemAPI.GetSingleton<ConfigComponent>();

            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            state.EntityManager.SetComponentData(playerEntity, new PlayerAttributeMain
            {
                MaxHealthPoint = configCom.MaxHealthPoint,
                HealthRegain = configCom.HealthRegain,
                Armor = configCom.Armor,
                SpeedPercentage = configCom.SpeedPercentage,
                Range = configCom.Range,
            });
            if(SystemAPI.TryGetSingletonBuffer<WeaponTypeList>(out var weaponTypeList))
            {
                if(weaponTypeList.Length == 0)
                {
                    Debug.Log("weaponTypeList.Length is zero");
                }
                else
                {
                    var firstWeapon = weaponTypeList[0];
                    var weaponInstance = state.EntityManager.Instantiate(firstWeapon.WeaponModel);
                    state.EntityManager.SetComponentData(playerEntity, new MainWeaponState
                    {
                        WeaponModel = weaponInstance,
                        SpawneePrefab = firstWeapon.SpawneePrefab,
                        DamageBonus = firstWeapon.DamageBonus,
                        BasicDamage = firstWeapon.BasicDamage,
                        WeaponCriticalHitChance = firstWeapon.WeaponCriticalHitChance,
                        WeaponCriticalHitRatio = firstWeapon.WeaponCriticalHitRatio,
                        Cooldown = firstWeapon.Cooldown,
                        Range = firstWeapon.Range,
                        RealCooldown = 0,
                        WeaponPositionOffset = firstWeapon.WeaponPositionOffset,
                        DamageAfterBonus = firstWeapon.BasicDamage
                    });
                    state.EntityManager.SetComponentData(firstWeapon.SpawneePrefab, new SpawneeTimer
                    {
                        Value = 3f
                    });
                    state.EntityManager.RemoveComponent<LinkedEntityGroup>(firstWeapon.SpawneePrefab);
                }
            }
            state.EntityManager.SetComponentData(playerEntity, new PlayerAtttributeDamageRelated
            {
                MeleeRangedElementAttSpd = new float4(configCom.MeleeDamage, configCom.RangedDamage, configCom.ElementDamage, configCom.AttackSpeed),
                CriticalHitChange = configCom.CriticalHitChance,
                DamagePercentage = configCom.DamagePercentage,
            });
        }
    }

}