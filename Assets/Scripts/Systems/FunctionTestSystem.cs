//using Unity.Entities;
//using UnityEngine;

//namespace ProjectGra
//{
//    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
//    public partial struct FunctionTestSystem : ISystem, ISystemStartStop
//    {
//        bool ComOneInit;
//        bool ComTwoInit;
//        bool FirstUpdate;
//        public void OnCreate(ref SystemState state)
//        {
//            Debug.Log("FunctionTestSystem OnCreate");
//            ComOneInit = false;
//            //ComTwoInit = false;
//            FirstUpdate = false;
//            //state.RequireForUpdate<GameControllNotPaused>();
//            //state.RequireForUpdate<TestSceneExecuteTag>();
//        }

//        public void OnStartRunning(ref SystemState state)
//        {
//            Debug.Log("FuntionTestSystem - OnStartRunning");
//        }

//        public void OnStopRunning(ref SystemState state)
//        {
//            Debug.Log("FuntionTestSystem - OnStopRunning");
//        }

//        public void OnUpdate(ref SystemState state)
//        {

//            if(!FirstUpdate)
//            {
//                Debug.Log("FunctionTestSystem First Update");
//                FirstUpdate = !FirstUpdate;
//            }
//            if(ComOneInit == false)
//            {
//                if(SystemAPI.TryGetSingleton<ConfigComponent>(out var config))
//                {
//                    ComOneInit = true;
//                    Debug.Log("FunctionTestSystem OnUpdate - ConfigComponent - Exist");
//                }
//                else
//                {
//                    Debug.Log("FunctionTestSystem OnUpdate - ConfigComponent - NotExistYet");
//                }
//            }
//            if(!ComTwoInit)
//            {
//                if (SystemAPI.TryGetSingletonEntity<GameControllNotPaused>(out var config))
//                {
//                    ComTwoInit = true;
//                    Debug.Log("FunctionTestSystem OnUpdate - GameControllNotPaused - Exist");
//                }
//                else
//                {
//                    Debug.Log("FunctionTestSystem OnUpdate - GameControllNotPaused - NotExistYet");
//                }
//            }

//        }
//    }
//}