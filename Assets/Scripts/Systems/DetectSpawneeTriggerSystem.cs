using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct DetectSpawneeTriggerSystem : ISystem
    {
        ComponentLookup<EnemyHealthPoint> enemyHealthPointLookup;
        ComponentLookup<SpawneeCurDamage> spawneeCurDamageLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            enemyHealthPointLookup = SystemAPI.GetComponentLookup<EnemyHealthPoint>();
            spawneeCurDamageLookup = SystemAPI.GetComponentLookup<SpawneeCurDamage>(true);
        }
        public void OnUpdate(ref SystemState state)
        {
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            enemyHealthPointLookup.Update(ref state);
            spawneeCurDamageLookup.Update(ref state);
            var endFixedSimECB = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var triggerJob = new DetectCapabilityTriggerJob
            {
                ecb = endFixedSimECB,
                CurDamageLookup = spawneeCurDamageLookup,
                EnemyHealthPointLookup = enemyHealthPointLookup
            };

            state.Dependency = triggerJob.Schedule(simulationSingleton, state.Dependency);
        }
    }
    public struct DetectCapabilityTriggerJob : ITriggerEventsJob
    {
        public EntityCommandBuffer ecb;
        public ComponentLookup<EnemyHealthPoint> EnemyHealthPointLookup;
        [ReadOnly] public ComponentLookup<SpawneeCurDamage> CurDamageLookup;
        public void Execute(TriggerEvent triggerEvent)
        {
            //EntityA tends to be spawnee
            Entity Spawnee;
            Entity Enemy;
            Debug.Log("TriggerJob");
            if (CurDamageLookup.HasComponent(triggerEvent.EntityA) && EnemyHealthPointLookup.HasComponent(triggerEvent.EntityB))
            {
                
                Spawnee = triggerEvent.EntityA;
                Enemy = triggerEvent.EntityB;
            }else if(CurDamageLookup.HasComponent(triggerEvent.EntityB) && EnemyHealthPointLookup.HasComponent(triggerEvent.EntityA))
            {
                Debug.LogError("Rare Situation in DetectSpawneeTriggerSystem, not always that Spawnee is EntityA. This time, Spawnee is EntityB, Enemy is EntityA");
                Spawnee = triggerEvent.EntityB;
                Enemy = triggerEvent.EntityA;
            }
            else
            {
                return;
            }
            ecb.DestroyEntity(Spawnee);
            var refHP = EnemyHealthPointLookup.GetRefRW(Enemy);
            if ((refHP.ValueRW.HealthPoint -= CurDamageLookup[Spawnee].damage) <= 0)
            {
                ecb.RemoveComponent<PhysicsCollider>(Enemy);
                ecb.SetComponent(Enemy, new NormalMeleeStateMachine { CurrentState = EnemyState.Dead });    
            }
        }
    }
}