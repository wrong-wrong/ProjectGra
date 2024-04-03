using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct PauseSystem : ISystem, ISystemStartStop
    {
        public bool IsPause;
        private NativeArray<int> idxList;
        public void OnCreate(ref SystemState state)
        {
            //state.EntityManager.AddComponent(state.SystemHandle, new PauseSystemData { IsPause = true });     // API oversight
            state.EntityManager.AddComponent<PauseSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new PauseSystemData { IsPause = false });
            IsPause = false;
        }
        public void OnStartRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnContinueButtonClicked += UnpauseOnContinueButtonCallback;
            idxList = new NativeArray<int>(3,Allocator.Persistent);
        }
        public void OnStopRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnContinueButtonClicked -= UnpauseOnContinueButtonCallback;
            idxList.Dispose();
        }
        public void UnpauseOnContinueButtonCallback()
        {
            
            ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<PauseSystem>();
            Unpause(ref state);
            //IsPause = !IsPause;
        }

        private void PopulateWeaponStateWithWeaponIdx(ref SystemState state, int mainWeaponIdx,ref NativeArray<int> wpIdxList)
        {
            //Get configBuffer info from 
            
            //Get playerAttribute 
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerAttibute = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var playerRange = SystemAPI.GetSingleton<PlayerAttributeMain>().Range;
            var mainWeaponstate = SystemAPI.GetSingleton<MainWeaponState>();
            var autoWeaponBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponState>();
            var wpHashMapWrapperCom = SystemAPI.GetSingleton<AllWeaponMap>();
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            //destory model anyway;
            //need to destory earlier model entity if existed and instantiate new one;
            if (state.EntityManager.Exists(mainWeaponstate.WeaponModel))
            {
                ecb.DestroyEntity(mainWeaponstate.WeaponModel);
            }
            if (mainWeaponIdx == -1)
            {
                mainWeaponstate.WeaponIndex = -1;
            }
            else
            {
                //calculate the state info with playerAttribute and config buffer info
                var config = wpHashMapWrapperCom.wpNativeHashMap[mainWeaponIdx];

                var newWpModel = ecb.Instantiate(config.WeaponPrefab);
                var calculatedDamageAfterBonus = (int)((1 + playerAttibute.DamagePercentage)
                    * (config.BasicDamage + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance;
                var calculatedCooldown = config.Cooldown * math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f,2f);
                var calculatedRange = playerRange + config.Range;   //used to set spawnee's timer

                //using ecb or set directly
                ecb.SetComponent(playerEntity, new MainWeaponState
                {
                    WeaponIndex = mainWeaponIdx,
                    WeaponModel = newWpModel,
                    WeaponPositionOffset = config.WeaponPositionOffset,
                    RealCooldown = 0f,
                    Cooldown = calculatedCooldown,
                    DamageAfterBonus = calculatedDamageAfterBonus,
                    WeaponCriticalHitChance = calculatedCritHitChance,
                    WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                    SpawneePrefab = config.SpawneePrefab,
                });
                ecb.SetComponent(config.SpawneePrefab, new SpawneeCurDamage { damage = calculatedDamageAfterBonus });
                //todo maybe divide range by config.spawneeSpeed;
                ecb.SetComponent(config.SpawneePrefab, new SpawneeTimer { Value = calculatedRange / 20f });
            }
            //Destory previous model
            for(int i = 0; i < 3; ++i)
            {
                ref var autoWp = ref autoWeaponBuffer.ElementAt(i);
                if (state.EntityManager.Exists(autoWp.WeaponModel))
                {
                    ecb.DestroyEntity(autoWp.WeaponModel);
                }
            }
            //record operation on the buffer
            var autoWpEcb = ecb.SetBuffer<AutoWeaponState>(playerEntity);
            autoWpEcb.Clear();
            for(int i = 0; i < 3; ++i)
            {
                var config = wpHashMapWrapperCom.wpNativeHashMap[wpIdxList[i]];

                var newWpModel = ecb.Instantiate(config.WeaponPrefab);
                var calculatedDamageAfterBonus = (int)((1 + playerAttibute.DamagePercentage)
                    * (config.BasicDamage + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance;
                var calculatedCooldown = config.Cooldown * math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
                var calculatedRange = playerRange + config.Range;   //used to set spawnee's timer

                autoWpEcb.Add(new AutoWeaponState
                {
                    WeaponIndex = wpIdxList[i],
                    WeaponModel = newWpModel,
                    WeaponPositionOffset = config.WeaponPositionOffset,
                    RealCooldown = 0f,
                    Cooldown = calculatedCooldown,
                    DamageAfterBonus = calculatedDamageAfterBonus,
                    WeaponCriticalHitChance = calculatedCritHitChance,
                    WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                    SpawneePrefab = config.SpawneePrefab,
                }); 
                ecb.SetComponent(config.SpawneePrefab, new SpawneeCurDamage { damage = calculatedDamageAfterBonus });
                //todo maybe divide range by config.spawneeSpeed;
                ecb.SetComponent(config.SpawneePrefab, new SpawneeTimer { Value = calculatedRange / 20f });
            }

        }
        private void Unpause(ref SystemState state)
        {
            idxList[0] = 1;
            idxList[1] = 2;
            idxList[2] = 3;
            PopulateWeaponStateWithWeaponIdx(ref state, 0, ref idxList);

            //flip isPause
            var com = SystemAPI.GetComponentRW<PauseSystemData>(state.SystemHandle);
            com.ValueRW.IsPause = !com.ValueRW.IsPause;

            //Unpause
            var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerMaterialsCount = SystemAPI.GetComponent<PlayerMaterialCount>(playerEntity);
            var playerHp = SystemAPI.GetComponent<EntityHealthPoint>(playerEntity);
            CanvasMonoSingleton.Instance.HideShop();
            CanvasMonoSingleton.Instance.UpdateInGameUI(playerHp.HealthPoint, 0.5f, playerMaterialsCount.Count);
            CanvasMonoSingleton.Instance.ShowInGameUI();
            Cursor.lockState = CursorLockMode.Locked;

        }
        private void Pause(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            state.EntityManager.RemoveComponent<GameControllNotPaused>(singleton);

            var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
            var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var weaponinfo = SystemAPI.GetSingleton<MainWeaponState>();
            CanvasMonoSingleton.Instance.ShowShop(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom, weaponinfo);
            CanvasMonoSingleton.Instance.HideInGameUI();
            //show Cursor
            Cursor.lockState = CursorLockMode.None;
        }
        public void OnUpdate(ref SystemState state)
        {

            if (Input.GetKeyUp(KeyCode.P))
            {
                var com = SystemAPI.GetComponentRW<PauseSystemData>(state.SystemHandle);
                if (com.ValueRO.IsPause)   //unpausing 
                {
                    //com.ValueRW.IsPause = !com.ValueRW.IsPause; //!!!!!!!!   flip in the unpause function
                    Unpause(ref state);
                    
                }
                else  // pausing 
                {
                    com.ValueRW.IsPause = !com.ValueRW.IsPause;
                    Pause(ref state);
                    
                }
            }
        }

    }
    public struct PauseSystemData : IComponentData
    {
        public bool IsPause;
    }
    public struct GameControllNotPaused : IComponentData { }
}






//hide UI
//var canvasGrpCom = SystemAPI.ManagedAPI.GetSingleton<MyCanvasGroupManagedCom>();
//if (canvasGrpCom != null)
//{
//    canvasGrpCom.canvasGroup.alpha = 0f;
//    canvasGrpCom.canvasGroup.interactable = false;
//    canvasGrpCom.canvasGroup.blocksRaycasts = false;
//}


//show UI
//var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
//var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
//var canvasGrpCom = SystemAPI.ManagedAPI.GetSingleton<MyCanvasGroupManagedCom>();
//if (canvasGrpCom != null)
//{
//    CanvasMonoSingleton.instance.UpdatePlayerAttribute(PlayerAttibuteCom,PlayerDamagedRelatedAttributeCom);

//    canvasGrpCom.canvasGroup.alpha = 1f;
//    canvasGrpCom.canvasGroup.interactable = true;
//    canvasGrpCom.canvasGroup.blocksRaycasts = true;
//}