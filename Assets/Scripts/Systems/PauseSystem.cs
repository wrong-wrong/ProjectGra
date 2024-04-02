using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct PauseSystem : ISystem, ISystemStartStop
    {
        public bool IsPause;
        public void OnCreate(ref SystemState state)
        {
            //state.EntityManager.AddComponent(state.SystemHandle, new PauseSystemData { IsPause = true });     // API oversight
            state.EntityManager.AddComponent<PauseSystemData>(state.SystemHandle);
            state.EntityManager.SetComponentData(state.SystemHandle, new PauseSystemData { IsPause = false });
            IsPause = false;
        }
        public void OnStartRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnContinueButtonClicked += UnpauseOnContinueButtonCallback;
        }
        public void OnStopRunning(ref SystemState state)
        {
            CanvasMonoSingleton.Instance.OnContinueButtonClicked -= UnpauseOnContinueButtonCallback;
        }
        public void UnpauseOnContinueButtonCallback()
        {
            
            ref var state = ref World.DefaultGameObjectInjectionWorld.EntityManager.WorldUnmanaged.GetExistingSystemState<PauseSystem>();
            Unpause(ref state);
            //IsPause = !IsPause;
        }
        private void Unpause(ref SystemState state)
        {

            //flip isPause
            var com = SystemAPI.GetComponentRW<PauseSystemData>(state.SystemHandle);
            com.ValueRW.IsPause = !com.ValueRW.IsPause;

            //Unpause
            var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerMaterialsCount = SystemAPI.GetComponent<PlayerMaterialCount>(playerEntity);
            var playerHp = SystemAPI.GetComponent<EntityHealthPoint>(playerEntity);
            CanvasMonoSingleton.Instance.HideShop();
            CanvasMonoSingleton.Instance.UpdateInGameUI(playerHp.HealthPoint, 0.5f, playerMaterialsCount.Count);
            CanvasMonoSingleton.Instance.ShowInGameUI();
            Cursor.lockState = CursorLockMode.Locked;

        }
        private void Pause(ref SystemState state)
        {
            var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
            state.EntityManager.RemoveComponent<GameControllNotPaused>(singleton);

            var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
            var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
            var weaponinfo = SystemAPI.GetSingleton<MainWeaponState>();
            CanvasMonoSingleton.Instance.ShowShop(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom, weaponinfo);
            CanvasMonoSingleton.Instance.HideInGameUI();
            //show Cursor
            Cursor.lockState = CursorLockMode.None;
        }
        public void OnUpdate(ref SystemState state)
        {

            if (Input.GetKeyUp(KeyCode.P))
            {
                var com = SystemAPI.GetComponentRW<PauseSystemData>(state.SystemHandle);
                if (com.ValueRO.IsPause)   //unpausing 
                {
                    //com.ValueRW.IsPause = !com.ValueRW.IsPause; //!!!!!!!!   flip in the unpause function
                    Unpause(ref state);
                    
                }
                else  // pausing 
                {
                    com.ValueRW.IsPause = !com.ValueRW.IsPause;
                    Pause(ref state);
                    
                }
            }
        }

    }
    public struct PauseSystemData : IComponentData
    {
        public bool IsPause;
    }
    public struct GameControllNotPaused : IComponentData { }
}






//hide UI
//var canvasGrpCom = SystemAPI.ManagedAPI.GetSingleton<MyCanvasGroupManagedCom>();
//if (canvasGrpCom != null)
//{
//    canvasGrpCom.canvasGroup.alpha = 0f;
//    canvasGrpCom.canvasGroup.interactable = false;
//    canvasGrpCom.canvasGroup.blocksRaycasts = false;
//}


//show UI
//var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
//var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
//var canvasGrpCom = SystemAPI.ManagedAPI.GetSingleton<MyCanvasGroupManagedCom>();
//if (canvasGrpCom != null)
//{
//    CanvasMonoSingleton.instance.UpdatePlayerAttribute(PlayerAttibuteCom,PlayerDamagedRelatedAttributeCom);

//    canvasGrpCom.canvasGroup.alpha = 1f;
//    canvasGrpCom.canvasGroup.interactable = true;
//    canvasGrpCom.canvasGroup.blocksRaycasts = true;
//}