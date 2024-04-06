using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct GameWaveControllSystem : ISystem, ISystemStartStop
    {
        private float timer;
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<PlayerTag>(); // equal to Initialized , since playerTag is added through baking

            state.RequireForUpdate<GameControllInitialized>();

            //state.EntityManager.AddComponent(state.SystemHandle, new PauseSystemData { IsPause = true });     // API oversight
            var l = new NativeArray<int>(3, Allocator.Persistent);
            l[0] = 1; l[1] = 2; l[2] = 3;
            state.EntityManager.AddComponent<WaveControllSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new WaveControllSystemData {idxList = l});
            state.EntityManager.AddComponent<GameStateCom>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new GameStateCom { CurrentState = GameControllState.Uninitialized });
            timer = 3f;
        }
        public void OnStartRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnShopContinueButtonClicked += ShopContinueButtonCallback;
            CanvasMonoSingleton.Instance.OnPauseContinueButtonClicked += PauseContinueButtonCallback;
            //tmp test code
            var idxList = SystemAPI.GetComponent<WaveControllSystemData>(state.SystemHandle).idxList;
            PopulateWeaponStateWithWeaponIdx(ref state, 0, ref idxList);
        }
        public void OnStopRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnShopContinueButtonClicked -= ShopContinueButtonCallback;
            CanvasMonoSingleton.Instance.OnPauseContinueButtonClicked -= PauseContinueButtonCallback;
        }
        public void ShopContinueButtonCallback()
        {
            
            ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<GameWaveControllSystem>();
            ExitShopState(ref state);
        }

        public void PauseContinueButtonCallback()
        {
            ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<GameWaveControllSystem>();
            UnpauseReal(ref state);
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
            var wpHashMapWrapperCom = SystemAPI.GetSingleton<WeaponIdxToConfigCom>();
            //var overlapRadiusCom = SystemAPI.GetSingleton<PlayerOverlapRadius>();
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
        private void ExitShopState(ref SystemState state)
        {
            state.EntityManager.AddComponent<GameControllNotPaused>(state.SystemHandle);
            state.EntityManager.AddComponent<GameControllNotInShop>(state.SystemHandle);

            //Update weaponstate
            CanvasMonoSingleton.Instance.GetSlowWeaponIdx(out int idx, out int idx1, out int idx2, out int idx3);
            var sysData = SystemAPI.GetComponentRW<WaveControllSystemData>(state.SystemHandle);
            sysData.ValueRW.idxList[0] = idx1;
            sysData.ValueRW.idxList[1] = idx2;
            sysData.ValueRW.idxList[2] = idx3;
            PopulateWeaponStateWithWeaponIdx(ref state, idx, ref sysData.ValueRW.idxList);

            //Setting player data in ECS
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerMaterialsCount = SystemAPI.GetComponentRW<PlayerMaterialCount>(playerEntity);
            var playerHp = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
            playerMaterialsCount.ValueRW.Count = CanvasMonoSingleton.Instance.playerMaterialCount;
            playerHp.ValueRW.HealthPoint = CanvasMonoSingleton.Instance.mainAttribute.MaxHealthPoint;
            SystemAPI.SetComponent(playerEntity, CanvasMonoSingleton.Instance.damagedAttribute);
            SystemAPI.SetComponent(playerEntity, CanvasMonoSingleton.Instance.mainAttribute);

            CanvasMonoSingleton.Instance.HideShop();
            CanvasMonoSingleton.Instance.ShowInGameUI();
            Cursor.lockState = CursorLockMode.Locked;

            //set gamestate
            var gameState = SystemAPI.GetComponentRW<GameStateCom>(state.SystemHandle);
            gameState.ValueRW.CurrentState = GameControllState.BeforeWave;
            Debug.Log("InShop to BeforeWave!");
        }
        private void ShopState(ref SystemState state)
        {
            //var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();  // used to Remove GCNotPaused from super singleton
            state.EntityManager.RemoveComponent<GameControllNotInShop>(state.SystemHandle);
            state.EntityManager.RemoveComponent<GameControllNotPaused>(state.SystemHandle);
            //var com = SystemAPI.GetComponentRW<WaveControllSystemData>(state.SystemHandle);
            //com.ValueRW.IsPause = !com.ValueRW.IsPause;

            var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
            var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var mainWpstate= SystemAPI.GetSingleton<MainWeaponState>();
            var autoWpBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponState>();
            var materialCount = SystemAPI.GetSingleton<PlayerMaterialCount>();
            //TODO maybe not necessary to setWeaponIdx When Pause,  if we set the weapon in initialize system, that's when we are able to select out role and init weapon
            CanvasMonoSingleton.Instance.SetSlotWeaponIdx(mainWpstate.WeaponIndex, autoWpBuffer[0].WeaponIndex, autoWpBuffer[1].WeaponIndex, autoWpBuffer[2].WeaponIndex);
            CanvasMonoSingleton.Instance.ShowShop(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom, mainWpstate, materialCount.Count);
            CanvasMonoSingleton.Instance.HideInGameUI();
            //show Cursor
            Cursor.lockState = CursorLockMode.None;
            //set gamestate
            var gameState = SystemAPI.GetComponentRW<GameStateCom>(state.SystemHandle);
            Debug.Log("Pausing at state : " + gameState.ValueRW.CurrentState);
            gameState.ValueRW.PreviousState = gameState.ValueRO.CurrentState;
            gameState.ValueRW.CurrentState = GameControllState.InShop;
        }

        private void PauseReal(ref SystemState state)
        {
            state.EntityManager.RemoveComponent<GameControllNotPaused>(state.SystemHandle);

            var playerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
            var playerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            CanvasMonoSingleton.Instance.ShowPause(playerAttibuteCom, playerDamagedRelatedAttributeCom);
            Cursor.lockState = CursorLockMode.None;
            var gameState = SystemAPI.GetComponentRW<GameStateCom>(state.SystemHandle);
            Debug.Log("Real Pausing at state : " + gameState.ValueRW.CurrentState);
            gameState.ValueRW.PreviousState = gameState.ValueRO.CurrentState;
            gameState.ValueRW.CurrentState = GameControllState.Paused;
        }
        private void UnpauseReal(ref SystemState state)
        {
            
            state.EntityManager.AddComponent<GameControllNotPaused>(state.SystemHandle);
            //Unpause
            CanvasMonoSingleton.Instance.HidePause();
            Cursor.lockState = CursorLockMode.Locked;
            //set gamestate
            var gameState = SystemAPI.GetComponentRW<GameStateCom>(state.SystemHandle);
            gameState.ValueRW.CurrentState = gameState.ValueRO.PreviousState;
        }
        public void OnUpdate(ref SystemState state)
        {

            var gameState = SystemAPI.GetComponentRW<GameStateCom>(state.SystemHandle);
            var deltatime = SystemAPI.Time.DeltaTime;
            switch (gameState.ValueRO.CurrentState)
            {
                case GameControllState.BeforeWave:
                    if ((timer -= deltatime) < 0f)   //state change
                    {
                        timer = 10f; // setting in wave time;
                        gameState.ValueRW.CurrentState = GameControllState.InWave;

                        state.EntityManager.AddComponent<GameControllInGame>(state.SystemHandle);
                        Debug.Log("BeforeWave to InWave!");
                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        PauseReal(ref state);
                    }
                    if (Input.GetKeyUp(KeyCode.T))
                    {
                        ShopState(ref state);
                    }
                    break;
                case GameControllState.InWave:
                    if ((timer -= deltatime) < 0f)
                    {
                        gameState.ValueRW.PreviousState = GameControllState.InWave;  // setting only for InShop state, InShop checks previous state to decide if Pause
                        gameState.ValueRW.CurrentState = GameControllState.AfterWave;

                        state.EntityManager.RemoveComponent<GameControllInGame>(state.SystemHandle);
                        state.EntityManager.AddComponent<GameControllWaveCleanup>(state.SystemHandle);
                        Debug.Log("InWave to AfterWave!");

                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        PauseReal(ref state);
                    }
                    if (Input.GetKeyUp(KeyCode.T))
                    {
                        ShopState(ref state);
                    }
                    break;
                case GameControllState.AfterWave:
                    if (!SystemAPI.HasComponent<GameControllWaveCleanup>(state.SystemHandle))  // TODO need wave clean up System
                    {
                        timer = 2f; // setting begin wave time;!!!!      
                        gameState.ValueRW.CurrentState = GameControllState.InShop;
                        ShopState(ref state);
                        Debug.Log("AfterWave to InShop!");
                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        PauseReal(ref state);
                    }
                    break;
                case GameControllState.InShop:
                    if (Input.GetKeyUp(KeyCode.T))
                    {
                        ExitShopState(ref state);
                    }
                    break;
                case GameControllState.Uninitialized:
                    gameState.ValueRW.CurrentState = GameControllState.BeforeWave;
                    state.EntityManager.AddComponent<GameControllNotPaused>(state.SystemHandle);
                    state.EntityManager.AddComponent<GameControllNotInShop>(state.SystemHandle);
                    Debug.Log("Uninitialized to BeforeWave!");

                    break;
                case GameControllState.Paused:
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        UnpauseReal(ref state);
                        Debug.Log("Unpaused");
                    }
                    break;
                case GameControllState.Gameover:
                    break;
            }
        }

    }
    public struct WaveControllSystemData : IComponentData
    {
        //public bool IsPause;
        public NativeArray<int> idxList;
    }
    public struct GameControllNotPaused : IComponentData { }
    public struct GameControllInGame : IComponentData { }
    public struct GameControllNotInShop : IComponentData { }
    public struct GameControllWaveCleanup : IComponentData { }
    public struct GameControllGameOver : IComponentData { }
    public struct GameControllInitialized : IComponentData { }
    public struct GameStateCom : IComponentData
    {
        public GameControllState CurrentState;
        public GameControllState PreviousState;
    }
    public enum GameControllState
    {
        BeforeWave,
        InWave,
        AfterWave,
        InShop,
        Gameover,
        Uninitialized,
        Paused,
    }
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