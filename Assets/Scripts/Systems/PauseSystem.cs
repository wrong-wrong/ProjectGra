using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySystemGroupInInitializationSysGrp))]
    public partial struct PauseSystem : ISystem
    {
        bool IsPause;
        public void OnCreate(ref SystemState state)
        {
            IsPause = false;
        }
        public void OnUpdate(ref SystemState state)
        {
            if (Input.GetKeyUp(KeyCode.P))
            {

                var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();
                var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();

                if (IsPause)
                {

                    state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
                    var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
                    var playerMaterialsCount = SystemAPI.GetComponent<PlayerMaterialCount>(playerEntity);
                    var playerHp = SystemAPI.GetComponent<EntityHealthPoint>(playerEntity);
                    CanvasMonoSingleton.Instance.HideShop();
                    CanvasMonoSingleton.Instance.UpdateInGameUI(playerHp.HealthPoint, 0.5f, playerMaterialsCount.Count);
                    CanvasMonoSingleton.Instance.ShowInGameUI();

                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {

                    state.EntityManager.RemoveComponent<GameControllNotPaused>(singleton);
                    var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
                    CanvasMonoSingleton.Instance.ShowShop(PlayerAttibuteCom, PlayerDamagedRelatedAttributeCom);
                    CanvasMonoSingleton.Instance.HideInGameUI();

                    //show Cursor
                    Cursor.lockState = CursorLockMode.None;
                }
                IsPause = !IsPause;
            }
        }

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