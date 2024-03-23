using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp),OrderFirst = true)]
    public partial struct PauseSystem : ISystem
    {
        bool IsPause;
        public void OnCreate(ref SystemState state)
        {
            IsPause = false;
        }
        public void OnUpdate(ref SystemState state)
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
                if (IsPause)
                {
                    state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
                }   
                else
                {
                    state.EntityManager.RemoveComponent<GameControllNotPaused>(singleton);
                }
                IsPause = !IsPause;
            }
        }
    }

    public struct GameControllNotPaused:IComponentData { }
}