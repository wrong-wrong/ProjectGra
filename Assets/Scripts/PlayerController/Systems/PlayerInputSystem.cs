using Unity.Entities;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateBeforeFixedStepSysGrp))]
    //[UpdateAfter(typeof(GameWaveControllSystem))]
    public partial struct PlayerInputSystem : ISystem
    {
        public class Singleton : IComponentData
        {
            public MyInputAction Value;
        }

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotInShop>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            var inputActions = new MyInputAction();
            inputActions.Enable();
            state.EntityManager.AddComponentObject(state.SystemHandle, new Singleton { Value = inputActions });
        }

        public void OnUpdate(ref SystemState state)
        {

            var inputActionMap = SystemAPI.ManagedAPI.GetComponent<Singleton>(state.SystemHandle).Value.DefaultMap;

            foreach (var(moveAndLook, sprint, shoot) in SystemAPI.Query<RefRW<MoveAndLookInput>
                , EnabledRefRW<SprintInput>, EnabledRefRW<ShootInput>>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
            {
                moveAndLook.ValueRW.moveVal = inputActionMap.PlayerMove.ReadValue<Vector2>();
                moveAndLook.ValueRW.lookVal = inputActionMap.PlayerLook.ReadValue<Vector2>();
                sprint.ValueRW = inputActionMap.PlayerSprint.IsPressed();
                shoot.ValueRW = inputActionMap.PlayerShoot.IsPressed();
            }
        }
    }
}