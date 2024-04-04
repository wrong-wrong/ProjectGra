using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct PauseSystem : ISystem, ISystemStartStop
    {
        //public bool IsPause;
        //private NativeArray<int> idxList;
        public void OnCreate(ref SystemState state)
        {
            //state.EntityManager.AddComponent(state.SystemHandle, new PauseSystemData { IsPause = true });     // API oversight
            var l = new NativeArray<int>(3, Allocator.Persistent);
            l[0] = 1; l[1] = 2; l[2] = 3;
            state.EntityManager.AddComponent<PauseSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new PauseSystemData { IsPause = false , idxList = l});
            state.RequireForUpdate<PlayerTag>();
            //IsPause = false;
        }
        public void OnStartRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnContinueButtonClicked += UnpauseOnContinueButtonCallback;
            var idxList = SystemAPI.GetComponent<PauseSystemData>(state.SystemHandle).idxList;
            PopulateWeaponStateWithWeaponIdx(ref state, 0, ref idxList);
        }
        public void OnStopRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnContinueButtonClicked -= UnpauseOnContinueButtonCallback;
        }
        public void UnpauseOnContinueButtonCallback()
        {
            
            ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<PauseSystem>();
            Unpause(ref state);
        }

        private void PopulateWeaponStateWithWeaponIdx(ref SystemState state, int mainWeaponIdx,ref NativeArray<int> wpIdxList)
        {
            //Get configBuffer info from 
            float tmpRange = 0;
            //Get playerAttribute 
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerAttibute = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var playerRange = SystemAPI.GetSingleton<PlayerAttributeMain>().Range;
            var mainWeaponstate = SystemAPI.GetSingleton<MainWeaponState>();
            var autoWeaponBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponState>();
            var wpHashMapWrapperCom = SystemAPI.GetSingleton<AllWeaponMap>();
            var overlapRadiusCom = SystemAPI.GetSingleton<PlayerOverlapRadius>();
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
                if (wpIdxList[i] == -1)
                {
                    autoWpEcb.Add(new AutoWeaponState { WeaponIndex = -1 });
                    continue;
                }

                var config = wpHashMapWrapperCom.wpNativeHashMap[wpIdxList[i]];

                var newWpModel = ecb.Instantiate(config.WeaponPrefab);
                var calculatedDamageAfterBonus = (int)((1 + playerAttibute.DamagePercentage)
                    * (config.BasicDamage + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance;
                var calculatedCooldown = config.Cooldown * math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
                var calculatedRange = playerRange + config.Range;   //used to set spawnee's timer
                tmpRange = math.max(tmpRange, config.Range);
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
                    Range = calculatedRange,
                }); 
                ecb.SetComponent(config.SpawneePrefab, new SpawneeCurDamage { damage = calculatedDamageAfterBonus });
                //todo maybe divide range by config.spawneeSpeed;
                ecb.SetComponent(config.SpawneePrefab, new SpawneeTimer { Value = calculatedRange / 20f });
            }
            if(tmpRange == 0)
            {
                ecb.SetComponent(playerEntity, new PlayerOverlapRadius { Value = 0f });
            }
            else
            {
                ecb.SetComponent(playerEntity, new PlayerOverlapRadius { Value = tmpRange + playerRange });
            }
        }
        private void Unpause(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.GetSlowWeaponIdx(out int idx, out int idx1, out int idx2, out int idx3);
            var sysData = SystemAPI.GetComponentRW<PauseSystemData>(state.SystemHandle);
            sysData.ValueRW.idxList[0] = idx1;
            sysData.ValueRW.idxList[1] = idx2;
            sysData.ValueRW.idxList[2] = idx3;
            PopulateWeaponStateWithWeaponIdx(ref state, idx, ref sysData.ValueRW.idxList);

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
            var mainWpstate= SystemAPI.GetSingleton<MainWeaponState>();
            var autoWpBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponState>();
            CanvasMonoSingleton.Instance.SetSlotWeaponIdx(mainWpstate.WeaponIndex, autoWpBuffer[0].WeaponIndex, autoWpBuffer[1].WeaponIndex, autoWpBuffer[2].WeaponIndex);

            CanvasMonoSingleton.Instance.ShowShop(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom, mainWpstate);
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
        public NativeArray<int> idxList;
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