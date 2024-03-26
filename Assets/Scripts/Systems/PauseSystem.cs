using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
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
            if(Input.GetKeyUp(KeyCode.P))
            {
                
                var singleton = SystemAPI.GetSingletonEntity<SuperSingletonTag>();

                if (IsPause)
                {

                    state.EntityManager.AddComponent<GameControllNotPaused>(singleton);
                    //hide UI
                    var canvasGrpCom = SystemAPI.ManagedAPI.GetSingleton<MyCanvasGroupManagedCom>();
                    if (canvasGrpCom != null)
                    {
                        canvasGrpCom.canvasGroup.alpha = 0f;
                        canvasGrpCom.canvasGroup.interactable = false;
                        canvasGrpCom.canvasGroup.blocksRaycasts = false;
                    }
                    Cursor.lockState = CursorLockMode.Locked;
                }   
                else
                {
                    state.EntityManager.RemoveComponent<GameControllNotPaused>(singleton);
                    //show UI
                    var PlayerAttibuteCom = SystemAPI.GetSingleton<PlayerAttributeMain>();
                    var PlayerDamagedRelatedAttributeCom = SystemAPI.GetSingleton<PlayerAtttributeDamageRelated>();
                    var canvasGrpCom = SystemAPI.ManagedAPI.GetSingleton<MyCanvasGroupManagedCom>();
                    if (canvasGrpCom != null)
                    {
                        CanvasMonoSingleton.instance.UpdatePlayerAttribute(PlayerAttibuteCom,PlayerDamagedRelatedAttributeCom);

                        canvasGrpCom.canvasGroup.alpha = 1f;
                        canvasGrpCom.canvasGroup.interactable = true;
                        canvasGrpCom.canvasGroup.blocksRaycasts = true;
                    }
                    //show Cursor
                    Cursor.lockState = CursorLockMode.None;
                }
                IsPause = !IsPause;
            }
        }
    }

    public struct GameControllNotPaused:IComponentData { }
}