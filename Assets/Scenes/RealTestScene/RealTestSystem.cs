using ProjectGra;
using Unity.Entities;
using UnityEngine;

namespace RealTesting
{
    [DisableAutoCreation]
    public partial struct RealTestSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AlotSpawneePrafabCom>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<AlotSpawneePrafabCom>();
            var tmp = state.EntityManager.Instantiate(config.Prefab);
            state.EntityManager.AddComponent<PlayerTag>(tmp);
        }

        public void OnStopRunning(ref SystemState state)
        {
            
        }

        public void OnUpdate(ref SystemState state)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<PlayerTag>());   
            }
            
        }
    }
}