using Unity.Entities;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct AutoWeaponUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var deltatime = SystemAPI.Time.DeltaTime;
            var wpBuffer = SystemAPI.GetSingletonBuffer<AutoWeaponState>();
            var ecb = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            for (int i = 0, n = wpBuffer.Length; i < n; ++i) 
            { 
                ref var wp = ref wpBuffer.ElementAt(i);
                if (wp.WeaponIndex == -1) continue;
                if((wp.RealCooldown-= deltatime) < 0f && wp.DamageAfterBonus > 0)
                {
                    var spawnee = ecb.Instantiate(wp.SpawneePrefab);
                    ecb.SetComponent(spawnee, wp.autoWeaponLocalTransform);
                    wp.RealCooldown = wp.Cooldown;
                }
            }
        }

    }
}