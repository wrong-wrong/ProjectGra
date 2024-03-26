using ProjectGra.PlayerController;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(CameraTargetFollowSystem))]
    public partial struct PlayerSpawnSpawneeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            //need cur main weapon position,  cur spawnee prefab, player input, real cooldown, 
            foreach(var (mainWeaponState, inputState) in SystemAPI.Query<RefRW<MainWeaponState>,EnabledRefRO<ShootInput>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                 ref var realcd = ref mainWeaponState.ValueRW.RealCooldown;
                realcd -= deltatime;
                if(realcd > 0) { continue; }
                if(inputState.ValueRO == false) { continue; }
                realcd = mainWeaponState.ValueRO.Cooldown;
                var spawnee = state.EntityManager.Instantiate(mainWeaponState.ValueRO.SpawneePrefab);
                state.EntityManager.SetComponentData(spawnee, mainWeaponState.ValueRO.mainWeaponLocalTransform);
            }
        }
    }
}