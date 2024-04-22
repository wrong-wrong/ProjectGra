using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace OOPExperiment
{
    public partial struct ExperiSpawnerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExperiExecuteTag>();
            state.RequireForUpdate<ExperiSpawnConfig>();
        }
        public void OnUpdate(ref SystemState state) 
        {
            state.Enabled = false;
            //foreach(var configCom in SystemAPI.Query<RefRO<ExperiSpawnConfig>>())
            //{
            //    var movingTargetPrefab = configCom.ValueRO.ExperiMovingTargetPrefab;
            //    var palumonPrefab = configCom.ValueRO.ExperiPalumonEntityPrefab;
            //    var count = configCom.ValueRO.count;
            //    var targetEtt = state.EntityManager.Instantiate(movingTargetPrefab);
            //    state.EntityManager.SetComponentData(targetEtt, new LocalTransform { Position = new float3(3,0,0),Rotation = quaternion.identity, Scale = 1f });
            //    var palumonList = state.EntityManager.Instantiate(palumonPrefab,count,state.WorldUpdateAllocator);
            //    for(int i = 0; i < count; i++)
            //    {
            //        state.EntityManager.SetComponentData(palumonList[i], new ExperiPalumonTargetEntity { targetEntity = targetEtt });
            //    }
            //}
            var configCom = SystemAPI.GetSingleton<ExperiSpawnConfig>();
            var spawneePrefab = configCom.ExperiSpawneePrefab;
            var count = configCom.count;
            state.EntityManager.Instantiate(spawneePrefab, count, state.WorldUpdateAllocator);
        }
    }

}