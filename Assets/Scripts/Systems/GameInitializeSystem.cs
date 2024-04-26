using ProjectGra.PlayerController;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

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
                state.Enabled = false;
                MonoGameManagerSingleton.Instance.IsPresetChoosingDone = false;
                var configCom = SystemAPI.GetSingleton<PlayerConfigComponent>();
                var superSingleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();

                //Initializing Player with config
                var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                state.EntityManager.SetComponentData(playerEntity, new PlayerAttributeMain
                {
                    MaxHealthPoint = configCom.MaxHealthPoint,
                    HealthRegain = configCom.HealthRegain,
                    Armor = configCom.Armor,
                    SpeedPercentage = configCom.SpeedPercentage,
                    Range = configCom.Range,
                    LifeSteal = configCom.LifeSteal,
                    Dodge = configCom.Dodge,
                });
                state.EntityManager.SetComponentData(playerEntity, new EntityHealthPoint { HealthPoint = configCom.MaxHealthPoint });
                state.EntityManager.SetComponentData(playerEntity, new PlayerAtttributeDamageRelated
                {
                    MeleeRangedElementAttSpd = new float4(configCom.MeleeDamage, configCom.RangedDamage, configCom.ElementDamage, configCom.AttackSpeed),
                    CriticalHitChance = configCom.CriticalHitChance,
                    DamagePercentage = configCom.DamagePercentage,
                });

                //Init in-game UI
                var playerMaterialsCount = SystemAPI.GetSingleton<PlayerMaterialCount>();
                var playerHp = SystemAPI.GetComponent<EntityHealthPoint>(playerEntity);
                CanvasMonoSingleton.Instance.IngameUISetMaxHpExp(configCom.MaxHealthPoint, 16);
                CanvasMonoSingleton.Instance.IngameUIUpdataPlayerStats(playerHp.HealthPoint, 0, playerMaterialsCount.Count);
                CanvasMonoSingleton.Instance.HideShop();
                CanvasMonoSingleton.Instance.ShowInGameUI();
                CanvasMonoSingleton.Instance.HidePresetChoosingCanvasGroup();


                // Setting Spawning Config
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
                            PointSpawnChance = SpawningConfigSO.PointSpawnChance[i]
                        });
                    }
                    else
                    {
                        spawnConfigBuffer.Add(new SpawningConfigBuffer
                        {
                            SpawnCooldown = -SpawningConfigSO.SpawningCooldown[i],
                            PointSpawnChance = SpawningConfigSO.PointSpawnChance[i]
                        });
                    }
                }
                Debug.Log("GameInitializeSystem - SpawningConfigBuffer.Length : " + spawnConfigBuffer.Length);
                state.EntityManager.SetComponentData(superSingleton,
                    new EnemyHpAndDmgModifierWithDifferentDifficulty { DamageModifier = SpawningConfigSO.EnemyDamageModifier, HealthPointModifier = SpawningConfigSO.EnemyHealthPointModifier });
                var waveControllSysHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<GameWaveControllSystem>();
                //state.EntityManager.AddComponent<GameControllMonoDataApplied>(superSingleton);
                state.EntityManager.AddComponent<GameControllMonoDataApplied>(waveControllSysHandle);


            }

        }

    }

}