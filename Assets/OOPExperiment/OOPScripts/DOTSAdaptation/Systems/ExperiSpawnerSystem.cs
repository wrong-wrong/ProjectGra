using Unity.Entities;
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
            foreach(var configCom in SystemAPI.Query<RefRO<ExperiSpawnConfig>>())
            {
                var movingTargetPrefab = configCom.ValueRO.ExperiMovingTargetPrefab;
                var palumonPrefab = configCom.ValueRO.ExperiPalumonEntityPrefab;
                var count = configCom.ValueRO.count;
                var targetEtt = state.EntityManager.Instantiate(movingTargetPrefab);
                var palumonList = state.EntityManager.Instantiate(palumonPrefab,count,state.WorldUpdateAllocator);
                for(int i = 0; i < count; i++)
                {
                    state.EntityManager.SetComponentData(palumonList[i], new ExperiPalumonTargetEntity { targetEntity = targetEtt });
                }
            }
        }
    }

}