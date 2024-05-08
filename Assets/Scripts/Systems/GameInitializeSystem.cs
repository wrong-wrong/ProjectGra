using ProjectGra.PlayerController;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp),OrderFirst = true)]
    public partial struct GameInitializeSystem : ISystem
    {
        bool IsInitialized;
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SuperSingletonTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            state.RequireForUpdate<PlayerConfigComponent>();
            IsInitialized = false;
        }
        public void OnUpdate(ref SystemState state)
        {
            if(!IsInitialized)
            {
                IsInitialized = true;
                var superSingleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
                var configCom = SystemAPI.GetSingleton<PlayerConfigComponent>();

                // Init camera com
                state.EntityManager.SetComponentData(superSingleton, new CameraTargetReference
                {
                    cameraTarget = CameraTargetMonoSingleton.instance.CameraTargetTransform,
                    ghostPlayer = CameraTargetMonoSingleton.instance.transform
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
                        for (int i = 0, n = weaponConfigBuffer.Length; i < n; i++)
                        {
                            wpHashMp[weaponConfigBuffer[i].WeaponIndex] = weaponConfigBuffer[i];
                            ecb.RemoveComponent<LinkedEntityGroup>(weaponConfigBuffer[i].WeaponPrefab);
                            if (!weaponConfigBuffer[i].IsMeleeWeapon) ecb.RemoveComponent<LinkedEntityGroup>(weaponConfigBuffer[i].SpawneePrefab);
                        }

                        state.EntityManager.AddComponent<WeaponIdxToWpDataConfigCom>(superSingleton);
                        var mpCom = new WeaponIdxToWpDataConfigCom { wpNativeHashMap = wpHashMp };
                        state.EntityManager.SetComponentData(superSingleton, mpCom);

                        //Set config to mono
                        SOConfigSingleton.Instance.WeaponMapCom = mpCom;
                        if (SystemAPI.ManagedAPI.TryGetSingleton<WeaponManagedAndMonoOnlyConfigCom>(out var managedConfig))
                        {
                            SOConfigSingleton.Instance.WeaponManagedConfigCom = managedConfig;
                        }
                        SOConfigSingleton.Instance.InitWeaponSOSingleton();

                        //Setting Weapon state should be take over by pause system
                        //but can do some initial work here , remove LEG for example
                    }
                    Debug.Log("GameInitializeSystem - Done Game Data init");

                }

                // Register meshes
                var entitesGraphicsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
                var MeshContainer = SystemAPI.ManagedAPI.GetSingleton<MeshContainerCom>();
                ecb.AddComponent(superSingleton, new BatchMeshIDContainer
                {
                    EnemyEggMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyEggMesh),
                    EnemyEliteEggAndShootMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyEliteEggAndShootMesh),
                    EnemyEliteShootMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyEliteShootMesh),
                    EnemyEliteSprintAndShootMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyEliteSprintAndShootMesh),
                    EnemyNormalMeleeMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyNormalMeleeMesh),
                    EnemyNormalRangedMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyNormalRangedMesh),
                    EnemyNormalSprintMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemyNormalSprintMesh),
                    EnemySummonerMeshID = entitesGraphicsSystem.RegisterMesh(MeshContainer.EnemySummonerMesh),
                });

                // Create EffectRequestSharedStaticBuffer
                Debug.LogWarning("Using number to give native list in buffer a initial size won't limit the max number of element in native list");
                EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(PopupTextManager.Instance.MaxPopupTextCount, AudioManager.Instance.MaxAudioSourceCount, ParticleManager.Instance.MaxParticleCount);
                //Debug.LogError("Using fix number to create effect request buffer");
                // these Manager check EffectRequestBuffer every update
                PopupTextManager.Instance.enabled = true;
                AudioManager.Instance.enabled = true;
                ParticleManager.Instance.enabled = true;
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }

            if(MonoGameManagerSingleton.Instance.IsPresetChoosingDone)
            {
                Debug.Log("GameInitializeSystem - DonePresetChoosing");
                state.Enabled = false;
                MonoGameManagerSingleton.Instance.IsPresetChoosingDone = false;
                var configCom = SystemAPI.GetSingleton<PlayerConfigComponent>();
                var superSingleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();

                //Initializing Player with config
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                var presetMain = PlayerDataModel.Instance.GetMainAttribute();
                var presetDamage = PlayerDataModel.Instance.GetDamageAttribute();
                var calculatedMainAttribute = new PlayerAttributeMain
                {
                    MaxHealthPoint = configCom.MaxHealthPoint + presetMain.MaxHealthPoint,
                    HealthRegain = configCom.HealthRegain + presetMain.HealthRegain,
                    Armor = configCom.Armor + presetMain.Armor,
                    SpeedPercentage = configCom.SpeedPercentage + presetMain.SpeedPercentage,
                    Range = configCom.Range + presetMain.Range,
                    LifeSteal = configCom.LifeSteal + presetMain.LifeSteal,
                    Dodge = configCom.Dodge + presetMain.Dodge,
                };
                var calculatedDamageAttribute = new PlayerAtttributeDamageRelated
                {
                    MeleeRangedElementAttSpd = new float4(configCom.MeleeDamage + presetDamage.MeleeRangedElementAttSpd.x,
                    configCom.RangedDamage + presetDamage.MeleeRangedElementAttSpd.y,
                    configCom.ElementDamage + presetDamage.MeleeRangedElementAttSpd.z,
                    configCom.AttackSpeed + presetDamage.MeleeRangedElementAttSpd.w),
                    CriticalHitChance = configCom.CriticalHitChance + presetDamage.CriticalHitChance,
                    DamagePercentage = configCom.DamagePercentage + presetDamage.DamagePercentage,
                };
                state.EntityManager.SetComponentData(playerEntity, calculatedMainAttribute);
                state.EntityManager.SetComponentData(playerEntity, new EntityHealthPoint { HealthPoint = configCom.MaxHealthPoint + presetMain.MaxHealthPoint });
                state.EntityManager.SetComponentData(playerEntity, calculatedDamageAttribute);
                PlayerDataModel.Instance.SetAttributeWithStruct(calculatedMainAttribute, calculatedDamageAttribute);




                // Setting Spawning Config
                var random = Random.CreateFromIndex(0);
                var spawnConfig = SystemAPI.GetSingleton<EnemySpawningConfig>();
                var SpawningConfigSO = MonoGameManagerSingleton.Instance.SpawningSOList[MonoGameManagerSingleton.Instance.CurrentDifficulty];
                var spawnConfigBuffer = state.EntityManager.GetBuffer<SpawningConfigBuffer>(superSingleton);
                spawnConfigBuffer.Clear();
                for (int i = 0, n = SpawningConfigSO.IsHordeOrElite.Count; i < n; i++)
                {
                    if (!SpawningConfigSO.IsHordeOrElite[i])
                    {
                        spawnConfigBuffer.Add(new SpawningConfigBuffer
                        {
                            SpawnCooldown = SpawningConfigSO.SpawningCooldown[i],
                            PointSpawnChance = SpawningConfigSO.PointSpawnChance[i],
                            GroupSpawnCooldown = SpawningConfigSO.GroupSpawnCooldown[i],
                            GroupSpawnCount = SpawningConfigSO.GroupSpawnCount[i],
                        });
                    }
                    else
                    {
                        bool isElite = random.NextFloat() < spawnConfig.EliteChanceInSpecialWave;
                        int modifier = isElite ? -1 : 1;
                        spawnConfigBuffer.Add(new SpawningConfigBuffer
                        {
                            SpawnCooldown = -SpawningConfigSO.SpawningCooldown[i],
                            PointSpawnChance = SpawningConfigSO.PointSpawnChance[i],
                            GroupSpawnCooldown = SpawningConfigSO.GroupSpawnCooldown[i],
                            GroupSpawnCount = SpawningConfigSO.GroupSpawnCount[i] * modifier,
                        });
                        CanvasMonoSingleton.Instance.AddCodingWaveAndIsElite(i, isElite);
                        //Debug.LogError("asd");
                    }
                }
                //Debug.Log("GameInitializeSystem - SpawningConfigBuffer.Length : " + spawnConfigBuffer.Length);
                state.EntityManager.SetComponentData(superSingleton,
                    new EnemyHpAndDmgModifierWithDifferentDifficulty { DamageModifier = SpawningConfigSO.EnemyDamageModifier, HealthPointModifier = SpawningConfigSO.EnemyHealthPointModifier });
                var waveControllSysHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<GameWaveControlSystem>();
                //state.EntityManager.AddComponent<GameControllMonoDataApplied>(superSingleton);
                state.EntityManager.AddComponent<GameControllMonoDataApplied>(waveControllSysHandle);

                //Init in-game UI
                CanvasMonoSingleton.Instance.IngameUIInit(calculatedMainAttribute.MaxHealthPoint, 16);
                CanvasMonoSingleton.Instance.InitOtherUI();
                //CanvasMonoSingleton.Instance.IngameUIUpdataPlayerStats(playerHp.HealthPoint, 0, playerMaterialsCount.Count);
                CanvasMonoSingleton.Instance.HideShop();
                CanvasMonoSingleton.Instance.ShowInGameUI();
                CanvasMonoSingleton.Instance.HidePresetChoosingCanvasGroup();
            }

        }

    }

}