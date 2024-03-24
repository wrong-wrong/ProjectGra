using ProjectGra.PlayerController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
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
                ghoshPlayer = CameraTargetMonoSingleton.instance.transform});
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
            state.EntityManager.SetComponentData(playerEntity, new PlayerAtttributeDamageRelated
            {
                MeleeRangedElementAttSpd = new float4(configCom.MeleeDamage, configCom.RangedDamage, configCom.ElementDamage, configCom.AttackSpeed),
                CriticalHitChange = configCom.CriticalHitChance,
                DamagePercentage = configCom.DamagePercentage,
            });
        }
    }

}