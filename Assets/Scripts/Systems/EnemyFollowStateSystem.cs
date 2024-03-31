//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;

//namespace ProjectGra
//{
//    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
//    public partial struct EnemyFollowStateSystem : ISystem
//    {
//        public void OnCreate(ref SystemState state)
//        {
//            state.RequireForUpdate<GameControllNotPaused>();
//            state.RequireForUpdate<TestSceneExecuteTag>();
//        }
//        public void OnUpdate(ref SystemState state)
//        {
//            var playerTransform = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<PlayerTag>());
//            var deltatime = SystemAPI.Time.DeltaTime;
//            foreach(var (enemyTransform, enemyFollow) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyFollowState>>())
//            {
//                var tarDir = playerTransform.Position - enemyTransform.ValueRO.Position;
//                if(math.csum(tarDir * tarDir)<enemyFollow.ValueRO.StopDistance)
//                {
//                    continue;
//                }

//                //if(math.any(tarDir != float3.zero)) // seems this will likely be true,   because 0,0,0 would be continued 
//                //{
//                //    enemyTransform.ValueRW.Position += math.normalize(tarDir) * enemyFollow.ValueRO.FollowSpeed * deltatime;
//                //    enemyTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
//                //}

//                //potentially dangerous
//                enemyTransform.ValueRW.Position += math.normalize(tarDir) * enemyFollow.ValueRO.FollowSpeed * deltatime;
//                enemyTransform.ValueRW.Rotation = quaternion.LookRotation(tarDir, math.up());
//            }
//        }
//    }
//}