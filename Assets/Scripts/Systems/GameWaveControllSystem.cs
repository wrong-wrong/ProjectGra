using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct GameWaveControllSystem : ISystem, ISystemStartStop
    {
        private float timer;
        private ComponentLookup<AttackExplosiveCom> explosiveLookup;
        private float beginWaveTimeSet;
        private float inWaveTimeSet;
        //private Entity playerPrefab;
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<PlayerTag>(); // equal to Initialized , since playerTag is added through baking

            state.RequireForUpdate<GameControllInitialized>();

            explosiveLookup = SystemAPI.GetComponentLookup<AttackExplosiveCom>();
            //state.EntityManager.AddComponent(state.SystemHandle, new PauseSystemData { IsPause = true });     // API oversight
            //var l = new NativeArray<int>(3, Allocator.Persistent);
            ////l[0] = 1; l[1] = 2; l[2] = 3;
            //l[0] = -1; l[1] = -1; l[2] = -1;
            state.EntityManager.AddComponent<WaveControllSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new WaveControllSystemData { tmpIsMeleeWp = default, tmpWpIdx = default });
            state.EntityManager.AddComponent<GameStateCom>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new GameStateCom { CurrentState = GameControllState.Uninitialized });
            timer = 3f;
        }
        public void OnStartRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnShopContinueButtonClicked += ShopContinueButtonCallback;
            CanvasMonoSingleton.Instance.OnPauseContinueButtonClicked += PauseContinueButtonCallback;
            //tmp test code
            //var idxList = SystemAPI.GetComponent<WaveControllSystemData>(state.SystemHandle).idxList;
            var config = SystemAPI.GetSingleton<GameWaveTimeConfig>();
            beginWaveTimeSet = config.BeginWaveTime;
            inWaveTimeSet = config.InWaveTime;
            //playerPrefab = SystemAPI.GetSingleton<PrefabContainerCom>().PlayerPrefab;
            PopulateWeaponStateWithWeaponIdx(ref state);
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

        private void PopulateWeaponStateWithWeaponIdx(ref SystemState state, int4 weaponIdx = default, bool4 isMeleeWeapon = default)
        {
            //Get configBuffer info from 
            float tmpRange = 0;
            //Get playerAttribute 
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerAttibute = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var playerRange = SystemAPI.GetSingleton<PlayerAttributeMain>().Range;
            var mainWeaponstate = SystemAPI.GetSingletonRW<MainWeapon>();
            var autoWeaponBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponBuffer>();
            var wpHashMapWrapperCom = SystemAPI.GetSingleton<WeaponIdxToWpDataConfigCom>();
            //var overlapRadiusCom = SystemAPI.GetSingleton<PlayerOverlapRadius>();
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            explosiveLookup.Update(ref state);
            //destory model anyway;
            //need to destory earlier model entity if existed and instantiate new one;
            if (state.EntityManager.Exists(mainWeaponstate.ValueRO.WeaponModel))
            {
                ecb.DestroyEntity(mainWeaponstate.ValueRO.WeaponModel);
            }
            if (weaponIdx[0] == -1)
            {
                mainWeaponstate.ValueRW.WeaponIndex = -1;
            }
            else
            {
                //calculate the state info with playerAttribute and config buffer info
                var config = wpHashMapWrapperCom.wpNativeHashMap[weaponIdx[0]];

                var newWpModel = ecb.Instantiate(config.WeaponPrefab);
                var ModelTransform = SystemAPI.GetComponent<LocalTransform>(config.WeaponPrefab);
                var calculatedDamageAfterBonus = (int)((1 + playerAttibute.DamagePercentage)
                    * (config.BasicDamage + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance;
                var calculatedCooldown = config.Cooldown * math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
                var calculatedRange = playerRange + config.Range;   //used to set spawnee's timer

                if (!isMeleeWeapon[0]) // Ranged Weapon
                {
                    //using ecb or set directly
                    ecb.SetComponent(playerEntity, new MainWeapon
                    {
                        WeaponIndex = weaponIdx[0],
                        WeaponModel = newWpModel,
                        WeaponPositionOffset = config.WeaponPositionOffset,
                        mainWeaponLocalTransform = ModelTransform,
                        RealCooldown = 0f,
                        Cooldown = calculatedCooldown,
                        DamageAfterBonus = calculatedDamageAfterBonus,
                        WeaponCriticalHitChance = calculatedCritHitChance,
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                        SpawneePrefab = config.SpawneePrefab,
                        IsMeleeWeapon = config.IsMeleeWeapon,
                        Range = calculatedRange,

                    });
                    ecb.SetComponent(config.SpawneePrefab, new AttackCurDamage { damage = calculatedDamageAfterBonus });
                    //todo maybe divide range by config.spawneeSpeed;
                    ecb.SetComponent(config.SpawneePrefab, new SpawneeTimer { Value = calculatedRange / 20f });
                    if (explosiveLookup.TryGetComponent(config.SpawneePrefab, out AttackExplosiveCom explosive))
                    {
                        ecb.SetComponent<AttackCurDamage>(explosive.ExplosionPrefab, new AttackCurDamage { damage = calculatedDamageAfterBonus });
                        //sucks
                    }
                }
                else // Melee Weapon
                {
                    ecb.SetComponent(playerEntity, new MainWeapon
                    {
                        WeaponIndex = weaponIdx[0],
                        WeaponModel = newWpModel,
                        WeaponPositionOffset = config.WeaponPositionOffset,
                        mainWeaponLocalTransform = ModelTransform,
                        RealCooldown = 0f,
                        Cooldown = calculatedCooldown,
                        DamageAfterBonus = calculatedDamageAfterBonus, // setting to negtive to indicate its not targeting something
                        WeaponCriticalHitChance = calculatedCritHitChance,
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                        MeleeShootingTimer = calculatedRange / 20f,
                        IsMeleeWeapon = config.IsMeleeWeapon,
                        Range = calculatedRange,
                        //SpawneePrefab = Entity.Null
                    });
                    ecb.SetComponent(newWpModel, new AttackCurDamage { damage = calculatedDamageAfterBonus });
                }

            }
            //Destory previous model
            for (int i = 0; i < 3; ++i)
            {
                ref var autoWp = ref autoWeaponBuffer.ElementAt(i);
                if (state.EntityManager.Exists(autoWp.WeaponModel))
                {
                    ecb.DestroyEntity(autoWp.WeaponModel);
                }
            }
            //record operation on the buffer
            var autoWpEcb = ecb.SetBuffer<AutoWeaponBuffer>(playerEntity);
            autoWpEcb.Clear();
            for (int i = 0; i < 3; ++i)
            {
                if (weaponIdx[i + 1] == -1)
                {
                    autoWpEcb.Add(new AutoWeaponBuffer { WeaponIndex = -1 });
                    continue;
                }

                var config = wpHashMapWrapperCom.wpNativeHashMap[weaponIdx[i + 1]];

                var newWpModel = ecb.Instantiate(config.WeaponPrefab);
                var ModelTransform = SystemAPI.GetComponent<LocalTransform>(config.WeaponPrefab);
                var calculatedDamageAfterBonus = (int)((1 + playerAttibute.DamagePercentage)
                    * (config.BasicDamage + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance;
                var calculatedCooldown = config.Cooldown * math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
                var calculatedRange = playerRange + config.Range;   //used to set spawnee's timer
                tmpRange = math.max(tmpRange, config.Range);
                if (!isMeleeWeapon[i + 1]) // Ranged Weapon
                {
                    autoWpEcb.Add(new AutoWeaponBuffer
                    {
                        WeaponIndex = weaponIdx[i + 1],
                        WeaponModel = newWpModel,
                        WeaponPositionOffset = config.WeaponPositionOffset,
                        autoWeaponLocalTransform = ModelTransform,
                        RealCooldown = 0f,
                        Cooldown = calculatedCooldown,
                        DamageAfterBonus = calculatedDamageAfterBonus * -1, // setting to negtive to indicate its not targeting something
                        WeaponCriticalHitChance = calculatedCritHitChance,
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                        SpawneePrefab = config.SpawneePrefab,
                        Range = calculatedRange,
                        IsMeleeWeapon = config.IsMeleeWeapon,
                    });
                    ecb.SetComponent(config.SpawneePrefab, new AttackCurDamage { damage = calculatedDamageAfterBonus });
                    //todo maybe divide range by config.spawneeSpeed;
                    ecb.SetComponent(config.SpawneePrefab, new SpawneeTimer { Value = calculatedRange / 20f });
                    if (explosiveLookup.TryGetComponent(config.SpawneePrefab, out AttackExplosiveCom explosive))
                    {
                        ecb.SetComponent<AttackCurDamage>(explosive.ExplosionPrefab, new AttackCurDamage { damage = calculatedDamageAfterBonus });
                        //sucks
                    }
                }
                else
                {
                    autoWpEcb.Add(new AutoWeaponBuffer
                    {
                        WeaponIndex = weaponIdx[i + 1],
                        WeaponModel = newWpModel,
                        WeaponPositionOffset = config.WeaponPositionOffset,
                        autoWeaponLocalTransform = ModelTransform,
                        RealCooldown = 0f,
                        Cooldown = calculatedCooldown,
                        DamageAfterBonus = calculatedDamageAfterBonus * -1, // setting to negtive to indicate its not targeting something
                        WeaponCriticalHitChance = calculatedCritHitChance,
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                        Range = calculatedRange,
                        MeleeShootingTimer = calculatedRange / 20f,
                        IsMeleeWeapon = config.IsMeleeWeapon,
                        //SpawneePrefab = Entity.Null
                    });
                    //SETTING AttackCurDamage TO THE NEWWEAPONMODEL  !!!
                    ecb.SetComponent(newWpModel, new AttackCurDamage { damage = calculatedDamageAfterBonus });
                    //todo maybe divide range by config.spawneeSpeed;
                }

            }
            if (tmpRange == 0)
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
            var sysData = SystemAPI.GetComponentRW<WaveControllSystemData>(state.SystemHandle);
            sysData.ValueRW.tmpWpIdx = CanvasMonoSingleton.Instance.GetSlotWeaponIdxInShop();
            sysData.ValueRW.tmpIsMeleeWp = CanvasMonoSingleton.Instance.GetSlowWeaponIsMeleeInShop();

            Debug.Log(sysData.ValueRW.tmpWpIdx);
            Debug.Log(sysData.ValueRW.tmpIsMeleeWp);
            PopulateWeaponStateWithWeaponIdx(ref state, sysData.ValueRW.tmpWpIdx, sysData.ValueRW.tmpIsMeleeWp);

            //Setting player data in ECS
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerMaterialsCount = SystemAPI.GetComponentRW<PlayerMaterialCount>(playerEntity);
            var playerHp = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
            playerMaterialsCount.ValueRW.Count = PlayerDataModel.Instance.playerMaterialCount;
            playerHp.ValueRW.HealthPoint = PlayerDataModel.Instance.GetMaxHealthPoint();
            SystemAPI.SetComponent(playerEntity, PlayerDataModel.Instance.GetDamageAttribute());
            SystemAPI.SetComponent(playerEntity, PlayerDataModel.Instance.GetMainAttribute());
            state.EntityManager.AddComponentData(playerEntity, state.EntityManager.GetComponentData<PhysicsCollider>(SystemAPI.GetSingleton<PrefabContainerCom>().PlayerPrefab));
            //CanvasMonoSingleton.Instance.HideShop();
            //CanvasMonoSingleton.Instance.ShowInGameUI();
            Cursor.lockState = CursorLockMode.Locked;


            //set spawning system
            var spawningConfig = SystemAPI.GetSingletonRW<EnemySpawningConfig>();
            spawningConfig.ValueRW.SpawningCooldown = 1f;
            //Debug.Log("spawningCooldown after set" + spawningConfig.ValueRO.spawningCooldown);
            //set gamestate
            var gameState = SystemAPI.GetComponentRW<GameStateCom>(state.SystemHandle);
            gameState.ValueRW.CurrentState = GameControllState.BeforeWave;
            Debug.Log("InShop to BeforeWave!");
        }
        private void EnterShopState(ref SystemState state)
        {

            state.EntityManager.RemoveComponent<GameControllNotInShop>(state.SystemHandle);
            state.EntityManager.RemoveComponent<GameControllNotPaused>(state.SystemHandle);

            var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
            var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var playerItemCount = SystemAPI.GetSingleton<PlayerItemCount>();
            var playerExpCom = SystemAPI.GetSingleton<PlayerExperienceAndLevel>();
            var mainWpstate = SystemAPI.GetSingleton<MainWeapon>();
            var autoWpBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponBuffer>();
            var materialCount = SystemAPI.GetSingleton<PlayerMaterialCount>();
            var sysData = SystemAPI.GetComponentRW<WaveControllSystemData>(state.SystemHandle);
            //TODO maybe not necessary to setWeaponIdx When Pause,  if we set the weapon in initialize system, that's when we are able to select out role and init weapon
            CanvasMonoSingleton.Instance.SetSlotWeaponIdxInShop(sysData.ValueRW.tmpWpIdx, sysData.ValueRW.tmpIsMeleeWp);
            CanvasMonoSingleton.Instance.ShowShopAndOtherUI(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom, 1, 1, materialCount.Count);
            Debug.Log("Using Test Fixed number for itemCount and level up");

            //CanvasMonoSingleton.Instance.HideInGameUI();
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

            CanvasMonoSingleton.Instance.ShowPauseCanvasGroup();
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
            CanvasMonoSingleton.Instance.HidePauseCanvasGroup();
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
                        timer = inWaveTimeSet; // setting in wave time;
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
                        EnterShopState(ref state);
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
                        EnterShopState(ref state);
                    }
                    break;
                case GameControllState.AfterWave:
                    if (!SystemAPI.HasComponent<GameControllWaveCleanup>(state.SystemHandle))  // TODO need wave clean up System
                    {
                        timer = beginWaveTimeSet; // setting begin wave time;!!!!      
                        gameState.ValueRW.CurrentState = GameControllState.InShop;
                        EnterShopState(ref state);
                        Debug.Log("AfterWave to InShop!");
                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        PauseReal(ref state);
                    }
                    break;
                case GameControllState.InShop:
                    //if (Input.GetKeyUp(KeyCode.T))
                    //{
                    //    ExitShopState(ref state);
                    //}
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
        //public NativeArray<int> idxList;
        public int4 tmpWpIdx;
        public bool4 tmpIsMeleeWp;
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


