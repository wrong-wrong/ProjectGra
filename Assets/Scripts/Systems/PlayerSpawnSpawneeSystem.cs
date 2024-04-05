using ProjectGra.PlayerController;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    [UpdateAfter(typeof(CameraTargetFollowSystem))]
    public partial struct PlayerSpawnSpawneeSystem : ISystem
    {
        private Random random;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            random = Random.CreateFromIndex(0);
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            //need cur main weapon position,  cur spawnee prefab, player input, real cooldown, 
            foreach (var (mainWeaponState, inputState, playerAttribute) in SystemAPI.Query<RefRW<MainWeaponState>, EnabledRefRO<ShootInput>, RefRO<PlayerAtttributeDamageRelated>>()
                    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                if (mainWeaponState.ValueRO.WeaponIndex == -1) continue;
                ref var realcd = ref mainWeaponState.ValueRW.RealCooldown;
                realcd -= deltatime;
                if (realcd > 0.001f) { continue; }
                if (inputState.ValueRO == false) { continue; }
                realcd = mainWeaponState.ValueRO.Cooldown;
                var spawnee = state.EntityManager.Instantiate(mainWeaponState.ValueRO.SpawneePrefab);
                state.EntityManager.SetComponentData(spawnee, mainWeaponState.ValueRO.mainWeaponLocalTransform);
                if (random.NextFloat() < mainWeaponState.ValueRO.WeaponCriticalHitChance + playerAttribute.ValueRO.CriticalHitChance)
                {
                    state.EntityManager.SetComponentData(spawnee, new SpawneeCurDamage
                    {
                        damage = (int)(mainWeaponState.ValueRO.DamageAfterBonus * mainWeaponState.ValueRO.WeaponCriticalHitRatio)
                    });
                }
            }
        }
    }
}