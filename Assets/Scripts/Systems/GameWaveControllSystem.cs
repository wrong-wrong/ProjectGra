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

            state.RequireForUpdate<GameControllMonoDataApplied>();

            explosiveLookup = SystemAPI.GetComponentLookup<AttackExplosiveCom>();
            //state.EntityManager.AddComponent(state.SystemHandle, new PauseSystemData { IsPause = true });     // API oversight
            //var l = new NativeArray<int>(3, Allocator.Persistent);
            ////l[0] = 1; l[1] = 2; l[2] = 3;
            //l[0] = -1; l[1] = -1; l[2] = -1;
            state.EntityManager.AddComponent<WaveControllSystemData>(state.SystemHandle);
            //state.EntityManager.SetComponentData(state.SystemHandle, new WaveControllSystemData { tmpWpIdx = default });
            state.EntityManager.AddComponent<GameStateCom>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new GameStateCom { CurrentState = GameControllState.Uninitialized });
            state.EntityManager.AddComponent<GameControllShouldUpdateEnemy>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new GameControllShouldUpdateEnemy { Value = true });
            timer = 3f;
        }
        public void OnStartRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnShopContinueButtonClicked += ShopContinueButtonCallback;
            //CanvasMonoSingleton.Instance.OnPauseContinueButtonClicked += PauseContinueButtonCallback;
            //tmp test code
            //var idxList = SystemAPI.GetComponent<WaveControllSystemData>(state.SystemHandle).idxList;
            var config = SystemAPI.GetSingleton<GameWaveTimeConfig>();
            beginWaveTimeSet = config.BeginWaveTime;
            inWaveTimeSet = config.InWaveTime;
            var sysData = SystemAPI.GetSingletonRW<WaveControllSystemData>();
            var tmpI4 = new int4(MonoGameManagerSingleton.Instance.CurrentWeaponPresetIdx, -1, -1, -1);
            sysData.ValueRW.tmpWpIdx = tmpI4;
            sysData.ValueRW.tmpWpLevel = int4.zero;
            //playerPrefab = SystemAPI.GetSingleton<PrefabContainerCom>().PlayerPrefab;
            PopulateWeaponStateWithWeaponIdx(ref state, tmpI4);
        }
        public void OnStopRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnShopContinueButtonClicked -= ShopContinueButtonCallback;
            //CanvasMonoSingleton.Instance.OnPauseContinueButtonClicked -= PauseContinueButtonCallback;
        }
        public void ShopContinueButtonCallback()
        {

            ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<GameWaveControllSystem>();
            ExitShopState(ref state);
        }

        //public void PauseContinueButtonCallback()
        //{
        //    ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<GameWaveControllSystem>();
        //    UnpauseReal(ref state);
        //}

        private void PopulateWeaponStateWithWeaponIdx(ref SystemState state, int4 weaponIdx = default, int4 weaponLevel = default)
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
                var calculatedDamageAfterBonus = (int)((1f + playerAttibute.DamagePercentage)
                    * (config.BasicDamage[weaponLevel[0]] + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance[weaponLevel[0]];

                var cooldownModifier = math.clamp(1f - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
                var calculatedCooldown = config.Cooldown[weaponLevel[0]] * cooldownModifier;
                var calculatedRange = playerRange + config.Range[weaponLevel[0]];   //used to set spawnee's timer
                Debug.Log("Attribute related - All Weapon Cooldown modified with : " + cooldownModifier);
                if (!config.IsMeleeWeapon) // Ranged Weapon
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
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio[weaponLevel[0]],
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
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio[weaponLevel[0]],
                        MeleeShootingTimer = calculatedRange / config.MeleeForwardSpeed,
                        IsMeleeWeapon = config.IsMeleeWeapon,
                        IsMeleeSweep = config.IsMeleeSweep,
                        Range = calculatedRange,
                        //SpawneePrefab = Entity.Null
                        SweepHalfWidth = config.SweepHalfWidth
                    });
                    //if (!config.IsMeleeSweep)
                    //{
                    //    ecb.SetComponent(playerEntity, new MainWeapon
                    //    {
                    //        WeaponIndex = weaponIdx[0],
                    //        WeaponModel = newWpModel,
                    //        WeaponPositionOffset = config.WeaponPositionOffset,
                    //        mainWeaponLocalTransform = ModelTransform,
                    //        RealCooldown = 0f,
                    //        Cooldown = calculatedCooldown,
                    //        DamageAfterBonus = calculatedDamageAfterBonus, // setting to negtive to indicate its not targeting something
                    //        WeaponCriticalHitChance = calculatedCritHitChance,
                    //        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio,
                    //        MeleeShootingTimer = calculatedRange / 20f,
                    //        IsMeleeWeapon = config.IsMeleeWeapon,
                    //        IsMeleeSweep = config.IsMeleeSweep,
                    //        Range = calculatedRange,
                    //        //SpawneePrefab = Entity.Null
                    //    });
                    //}
                    //else
                    //{

                    //}

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
                    * (config.BasicDamage[weaponLevel[i + 1]] + math.csum(config.DamageBonus * playerAttibute.MeleeRangedElementAttSpd)));
                var calculatedCritHitChance = playerAttibute.CriticalHitChance + config.WeaponCriticalHitChance[weaponLevel[i + 1]];
                var calculatedCooldown = config.Cooldown[weaponLevel[i + 1]] * math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f);
                //Debug.Log("Attribute related - Auto wp " + i + 1 + "Cooldown modified with percentage:" + math.clamp(1 - playerAttibute.MeleeRangedElementAttSpd.w, 0.2f, 2f));
                var calculatedRange = playerRange + config.Range[weaponLevel[i + 1]];   //used to set spawnee's timer
                tmpRange = math.max(tmpRange, config.Range[weaponLevel[i + 1]]);
                if (!config.IsMeleeWeapon) // Ranged Weapon
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
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio[weaponLevel[i + 1]],
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
                else // melee weapon
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
                        WeaponCriticalHitRatio = config.WeaponCriticalHitRatio[weaponLevel[i + 1]],
                        Range = calculatedRange,
                        MeleeShootingTimer = calculatedRange / config.MeleeForwardSpeed,
                        IsMeleeWeapon = config.IsMeleeWeapon,
                        IsMeleeSweep = config.IsMeleeSweep,
                        SweepHalfWidth = config.SweepHalfWidth,
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
    
            //Setting player data in ECS
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            //var playerMaterialsCount = SystemAPI.GetComponentRW<PlayerMaterialCount>(playerEntity);
            //var playerHp = SystemAPI.GetComponentRW<EntityHealthPoint>(playerEntity);
            //playerMaterialsCount.ValueRW.Count = PlayerDataModel.Instance.playerMaterialCount;
            //playerHp.ValueRW.HealthPoint = PlayerDataModel.Instance.GetMaxHealthPoint();
            SystemAPI.SetComponent(playerEntity, new PlayerMaterialCount { Count = PlayerDataModel.Instance.playerMaterialCount });
            SystemAPI.SetComponent(playerEntity, new EntityHealthPoint { HealthPoint = PlayerDataModel.Instance.GetMaxHealthPoint() });
            SystemAPI.SetComponent(playerEntity, PlayerDataModel.Instance.GetDamageAttribute());
            SystemAPI.SetComponent(playerEntity, PlayerDataModel.Instance.GetMainAttribute());
            state.EntityManager.AddComponentData(playerEntity, state.EntityManager.GetComponentData<PhysicsCollider>(SystemAPI.GetSingleton<PrefabContainerCom>().PlayerPrefab));


            //Update weaponstate
            var newSysData = new WaveControllSystemData
            {
                tmpWpIdx = CanvasMonoSingleton.Instance.GetSlotWeaponIdxInShop(),
                tmpWpLevel = CanvasMonoSingleton.Instance.GetSlotWeaponLevelInShop(),
                //tmpIsMeleeWp = CanvasMonoSingleton.Instance.GetSlowWeaponIsMeleeInShop(),
            };
            SystemAPI.SetComponent(state.SystemHandle, newSysData);

            Debug.Log(newSysData.tmpWpIdx);
            PopulateWeaponStateWithWeaponIdx(ref state, newSysData.tmpWpIdx, newSysData.tmpWpLevel);
            //CanvasMonoSingleton.Instance.HideShop();
            //CanvasMonoSingleton.Instance.ShowInGameUI();
            Cursor.lockState = CursorLockMode.Locked;


            //set spawning system
            var spawningConfig = SystemAPI.GetSingletonRW<EnemySpawningConfig>();
            spawningConfig.ValueRW.SpawningCooldown = 1f;
            Debug.Log("spawningCooldown after set" + spawningConfig.ValueRO.SpawningCooldown);
            //set gamestate
            var gameState = SystemAPI.GetSingletonRW<GameStateCom>();
            gameState.ValueRW.CurrentState = GameControllState.BeforeWave;
            Debug.Log("InShop to BeforeWave!");

            //set should enemy update
            var updateEnemyCom = SystemAPI.GetSingletonRW<GameControllShouldUpdateEnemy>();
            updateEnemyCom.ValueRW.Value = true;
            updateEnemyCom.ValueRW.CodingWave = CanvasMonoSingleton.Instance.CodingWave;
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
            //var sysData = SystemAPI.GetSingleton<WaveControllSystemData>();
            //TODO maybe not necessary to setWeaponIdx When Pause,  if we set the weapon in initialize system, that's when we are able to select out role and init weapon
            //CanvasMonoSingleton.Instance.SetSlotWeaponIdxInShop(sysData.tmpWpIdx, sysData.tmpWpLevel);
            CanvasMonoSingleton.Instance.ShowShopAndOtherUI(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom, 1, materialCount.Count);
            Debug.Log("Using Test Fixed number for itemCount");

            //CanvasMonoSingleton.Instance.HideInGameUI();
            //show Cursor
            Cursor.lockState = CursorLockMode.None;
            //set gamestate
            var gameState = SystemAPI.GetSingletonRW<GameStateCom>();
            Debug.Log("Pausing at state : " + gameState.ValueRW.CurrentState);
            gameState.ValueRW.PreviousState = gameState.ValueRO.CurrentState;
            gameState.ValueRW.CurrentState = GameControllState.InShop;
        }

        private void Pause(ref SystemState state)
        {
            state.EntityManager.RemoveComponent<GameControllNotPaused>(state.SystemHandle);

            CanvasMonoSingleton.Instance.ShowPauseCanvasGroup(true);
            Cursor.lockState = CursorLockMode.None;
            var gameState = SystemAPI.GetSingletonRW<GameStateCom>();
            Debug.Log("Real Pausing at state : " + gameState.ValueRW.CurrentState);
            gameState.ValueRW.PreviousState = gameState.ValueRO.CurrentState;
            gameState.ValueRW.CurrentState = GameControllState.Paused;

            SystemAPI.GetSingletonRW<GameControllShouldUpdateEnemy>().ValueRW.Value = false;

        }
        private void Unpause(ref SystemState state)
        {

            state.EntityManager.AddComponent<GameControllNotPaused>(state.SystemHandle);
            //Unpause
            CanvasMonoSingleton.Instance.HidePauseCanvasGroup();
            Cursor.lockState = CursorLockMode.Locked;
            //set gamestate
            var gameState = SystemAPI.GetSingletonRW<GameStateCom>();
            gameState.ValueRW.CurrentState = gameState.ValueRO.PreviousState;
        }
        public void OnUpdate(ref SystemState state)
        {

            var gameState = SystemAPI.GetSingletonRW<GameStateCom>();
            var deltatime = SystemAPI.Time.DeltaTime;
            switch (gameState.ValueRO.CurrentState)
            {
                case GameControllState.BeforeWave:
                    if ((timer -= deltatime) < 0f)   //state change
                    {
                        var wave = SystemAPI.GetSingleton<GameControllShouldUpdateEnemy>().CodingWave;

                        timer = math.clamp(20 + wave * 5, 20, 60) + 1; // setting in wave time;
                        gameState.ValueRW.PreviousState = GameControllState.BeforeWave;
                        gameState.ValueRW.CurrentState = GameControllState.InWave;

                        state.EntityManager.AddComponent<GameControllInGame>(state.SystemHandle);
                        CanvasMonoSingleton.Instance.StartCountdownTimer(timer);
                        Debug.Log("BeforeWave to InWave!");
                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        Pause(ref state);
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
                        CanvasMonoSingleton.Instance.StopCountdown();
                        Debug.Log("InWave to AfterWave!");

                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        Pause(ref state);
                    }
                    if (Input.GetKeyUp(KeyCode.T))
                    {
                        EnterShopState(ref state);
                    }
                    break;
                case GameControllState.AfterWave:
                    if (!SystemAPI.HasComponent<GameControllWaveCleanup>(state.SystemHandle))  
                    {
                        timer = beginWaveTimeSet; // setting begin wave time;!!!!      
                        gameState.ValueRW.CurrentState = GameControllState.InShop;
                        EnterShopState(ref state);
                        Debug.Log("AfterWave to InShop!");
                    }
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        Pause(ref state);
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
                    var sysData = SystemAPI.GetSingleton<WaveControllSystemData>();
                    CanvasMonoSingleton.Instance.SetSlotWeaponIdxInShop(sysData.tmpWpIdx, sysData.tmpWpLevel);

                    Debug.Log("Uninitialized to BeforeWave!");

                    break;
                case GameControllState.Paused:
                    if (Input.GetKeyUp(KeyCode.P))
                    {
                        Unpause(ref state);
                        Debug.Log("Unpaused");
                        return;
                    }
                    // nothing happeds when setting clicked
                    var buttonEnum = PauseUIManager.Instance.buttonClickedEnum;
                    // check if continue clicked
                    switch (buttonEnum)
                    {
                        
                        case ButtonClickedEnum.Continue:
                            PauseUIManager.Instance.buttonClickedEnum = ButtonClickedEnum.None;
                            Unpause(ref state);
                            break;
                        case ButtonClickedEnum.Restart:
                        case ButtonClickedEnum.MainMenu:
                            PauseUIManager.Instance.buttonClickedEnum = ButtonClickedEnum.None;
                            gameState.ValueRW.CurrentState = GameControllState.Uninitialized;
                            gameState.ValueRW.PreviousState = GameControllState.Uninitialized;
                            // just do wave clean on my own
                            //      destory enemy, spawnee, item, material
                            //      destory healthBar in cleanup Component
                            DoWaveCleanup(ref state);
                            // delete GameControllMonoDataApplied component
                            state.EntityManager.RemoveComponent<GameControllMonoDataApplied>(state.SystemHandle);
                            // delete other relevent component
                            //      not in shop, not paused, in game ,, waveclean is not add at all
                            state.EntityManager.RemoveComponent<GameControllNotInShop>(state.SystemHandle);
                            state.EntityManager.RemoveComponent<GameControllNotPaused>(state.SystemHandle);
                            state.EntityManager.RemoveComponent<GameControllInGame>(state.SystemHandle);

                            // enable initSystem
                            state.WorldUnmanaged.GetExistingSystemState<GameInitializeSystem>().Enabled = true;
                            break;
                    }
                    // check if restart clicked
                    // check if main menu clicked
                    break;
                case GameControllState.Gameover:

                    //// first let the wave clean system updates
                    ////      check flag , whether has added WaveCleanComponent and removed GCInGame
                    //// if not add and remove
                    //// else check if the WaveCleanCom has been removed by WaveCleanSystem
                    ////      if so do the rest
                    ///

                    // set current state before any structural change
                    gameState.ValueRW.CurrentState = GameControllState.Uninitialized;
                    gameState.ValueRW.PreviousState = GameControllState.Uninitialized;
                    // just do wave clean on my own
                    //      destory enemy, spawnee, item, material
                    //      destory healthBar in cleanup Component
                    DoWaveCleanup(ref state);
                    // delete GameControllMonoDataApplied component
                    state.EntityManager.RemoveComponent<GameControllMonoDataApplied>(state.SystemHandle);
                    // delete other relevent component
                    //      not in shop, not paused, in game ,, waveclean is not add at all
                    state.EntityManager.RemoveComponent<GameControllNotInShop>(state.SystemHandle);
                    state.EntityManager.RemoveComponent<GameControllNotPaused>(state.SystemHandle);
                    state.EntityManager.RemoveComponent<GameControllInGame>(state.SystemHandle);

                    // enable initSystem
                    var initSystem = state.WorldUnmanaged.GetExistingSystemState<GameInitializeSystem>();
                    initSystem.Enabled = true;
                    // show pause UI
                    CanvasMonoSingleton.Instance.ShowPauseCanvasGroup(false);
                    //      hide continue button
                    break;
            }
        }
        private void DoWaveCleanup(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var enemyList = SystemAPI.QueryBuilder().WithAll<EnemyTag>().Build().ToEntityArray(state.WorldUpdateAllocator);
            ecb.DestroyEntity(enemyList);
            var spawneeList = SystemAPI.QueryBuilder().WithAll<SpawneeTimer>().Build().ToEntityArray(state.WorldUpdateAllocator);
            ecb.DestroyEntity(spawneeList);
            //TODO destroy unpicked item
            //TODO maybe need to destory material to
            // destory health bar GO whose entity has been provisionally destoryed
            foreach (var (healthBarManagedCom, entity) in SystemAPI.Query<HealthBarUICleanupCom>()
                .WithEntityAccess()
                .WithNone<HealthBatUIOffset>())
            {
                Object.Destroy(healthBarManagedCom.HealthBarGO);
                ecb.RemoveComponent<HealthBarUICleanupCom>(entity);
            }
        }
    }
    public struct WaveControllSystemData : IComponentData
    {
        //public bool IsPause;
        //public NativeArray<int> idxList;
        public int4 tmpWpIdx;
        public int4 tmpWpLevel;
        //public bool4 tmpIsMeleeWp;
    }
    public struct GameControllNotPaused : IComponentData { }
    public struct GameControllInGame : IComponentData { }
    public struct GameControllNotInShop : IComponentData { }
    public struct GameControllWaveCleanup : IComponentData { }
    public struct GameControllGameOver : IComponentData { }
    public struct GameControllMonoDataApplied : IComponentData { }
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
    public struct GameControllShouldUpdateEnemy : IComponentData
    {
        public bool Value;
        public int CodingWave;
    }
}


