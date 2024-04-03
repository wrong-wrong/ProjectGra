using ProjectGra.PlayerController;
using Unity.Collections;
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
            var superSingleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            var configCom = SystemAPI.GetSingleton<ConfigComponent>();

            state.EntityManager.AddComponentObject(superSingleton, new CameraTargetReference { 
                cameraTarget = CameraTargetMonoSingleton.instance.CameraTargetTransform, 
                ghostPlayer = CameraTargetMonoSingleton.instance.transform});
            state.EntityManager.AddComponent<GameControllNotPaused>(superSingleton);
            
            //Initializing Player with config
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            state.EntityManager.SetComponentData(playerEntity, new PlayerAttributeMain
            {
                MaxHealthPoint = configCom.MaxHealthPoint,
                HealthRegain = configCom.HealthRegain,
                Armor = configCom.Armor,
                SpeedPercentage = configCom.SpeedPercentage,
                Range = configCom.Range,
            });
            state.EntityManager.SetComponentData(playerEntity, new EntityHealthPoint { HealthPoint = configCom.MaxHealthPoint });
            state.EntityManager.SetComponentData(playerEntity, new PlayerAtttributeDamageRelated
            {
                MeleeRangedElementAttSpd = new float4(configCom.MeleeDamage, configCom.RangedDamage, configCom.ElementDamage, configCom.AttackSpeed),
                CriticalHitChance = configCom.CriticalHitChance,
                DamagePercentage = configCom.DamagePercentage,
            });


            //use baked SO data to construct the AllWeaponMap component,
            //and let the pause system response for weapon state 
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            if (SystemAPI.TryGetSingletonBuffer<WeaponConfigInfoCom>(out var weaponConfigBuffer))
            {
                if (weaponConfigBuffer.Length == 0)
                {
                    Debug.Log("weaponTypeList.Length is zero");
                }
                else
                {
                    Debug.Log("WeaponConfigInfoCom Count : " + weaponConfigBuffer.Length);
                    var wpHashMp = new NativeHashMap<int, WeaponConfigInfoCom>(weaponConfigBuffer.Length, Allocator.Persistent);
                    for(int i = 0, n = weaponConfigBuffer.Length; i < n; i++)
                    {
                        wpHashMp[weaponConfigBuffer[i].WeaponIndex] = weaponConfigBuffer[i];
                        ecb.RemoveComponent<LinkedEntityGroup>(weaponConfigBuffer[i].SpawneePrefab);
                    }
                    state.EntityManager.AddComponent<AllWeaponMap>(superSingleton);
                    var mpCom = new AllWeaponMap { wpNativeHashMap = wpHashMp };
                    state.EntityManager.SetComponentData(superSingleton, mpCom);
                    WeaponSOConfigSingleton.Instance.weaponMap = mpCom;
                    //Setting Weapon state should be take over by pause system
                    //but can do some initial work here , remove LEG for example
                }
            }



            //Init in-game UI
            var playerMaterialsCount = SystemAPI.GetSingleton<PlayerMaterialCount>();
            var playerHp = SystemAPI.GetComponent<EntityHealthPoint>(playerEntity);
            CanvasMonoSingleton.Instance.SetMaxHpExp(configCom.MaxHealthPoint, 10f);
            CanvasMonoSingleton.Instance.UpdateInGameUI(playerHp.HealthPoint, 0.5f, playerMaterialsCount.Count);
            CanvasMonoSingleton.Instance.HideShop();
            CanvasMonoSingleton.Instance.ShowInGameUI();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }


    }

}