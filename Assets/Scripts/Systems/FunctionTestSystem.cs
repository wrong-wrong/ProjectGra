using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct FunctionTestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            //state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            if(Input.GetKeyUp(KeyCode.Space))
            {
                var mainWeaponInfo = SystemAPI.GetSingleton<MainWeaponState>();
                CanvasMonoSingleton.Instance.UpdateMainWeaponInfo(mainWeaponInfo);
            }
        }
    }
}