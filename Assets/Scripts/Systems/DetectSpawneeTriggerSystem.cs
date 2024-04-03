using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct DetectSpawneeTriggerSystem : ISystem
    {
        ComponentLookup<EntityHealthPoint> entityHealthPointLookup;
        ComponentLookup<SpawneeCurDamage> spawneeCurDamageLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            entityHealthPointLookup = SystemAPI.GetComponentLookup<EntityHealthPoint>();
            spawneeCurDamageLookup = SystemAPI.GetComponentLookup<SpawneeCurDamage>(true);
        }
        public void OnUpdate(ref SystemState state)
        {
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            entityHealthPointLookup.Update(ref state);
            spawneeCurDamageLookup.Update(ref state);
            var endFixedSimECB = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var triggerJob = new DetectCapabilityTriggerJob
            {
                ecb = endFixedSimECB,
                CurDamageLookup = spawneeCurDamageLookup,
                EntityHealthPointLookup = entityHealthPointLookup
            };

            state.Dependency = triggerJob.Schedule(simulationSingleton, state.Dependency);
        }
    }
    public struct DetectCapabilityTriggerJob : ITriggerEventsJob
    {
        public EntityCommandBuffer ecb;
        public ComponentLookup<EntityHealthPoint> EntityHealthPointLookup;
        [ReadOnly] public ComponentLookup<SpawneeCurDamage> CurDamageLookup;
        public void Execute(TriggerEvent triggerEvent)
        {
            //EntityA tends to be spawnee
            Entity Spawnee;
            Entity Enemy;
            if (CurDamageLookup.HasComponent(triggerEvent.EntityA) && EntityHealthPointLookup.HasComponent(triggerEvent.EntityB))
            {
                
                Spawnee = triggerEvent.EntityA;
                Enemy = triggerEvent.EntityB;
            }else if(CurDamageLookup.HasComponent(triggerEvent.EntityB) && EntityHealthPointLookup.HasComponent(triggerEvent.EntityA))
            {
                Debug.LogError("Rare Situation in DetectSpawneeTriggerSystem, not always that Spawnee is EntityA. This time, Spawnee is EntityB, Entity is EntityA");
                Spawnee = triggerEvent.EntityB;
                Enemy = triggerEvent.EntityA;
            }
            else
            {
                return;
            }
            ecb.DestroyEntity(Spawnee);
            var refHP = EntityHealthPointLookup.GetRefRW(Enemy);
            if ((refHP.ValueRW.HealthPoint -= CurDamageLookup[Spawnee].damage) <= 0)
            {
                ecb.RemoveComponent<PhysicsCollider>(Enemy);
                ecb.SetComponent(Enemy, new EntityStateMachine { CurrentState = EntityState.Dead });    
            }
        }
    }
}