using ProjectGra.PlayerController;
using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    public partial struct InitializeSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { 
            state.RequireForUpdate<SuperSingletonTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            state.EntityManager.AddComponentObject(singleton, new CameraTargetReference { 
                cameraTarget = CameraTargetMonoSingleton.instance.CameraTargetTransform, 
                ghoshPlayer = CameraTargetMonoSingleton.instance.transform});
            state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
        }
    }

}