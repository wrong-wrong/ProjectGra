using OOPExperiment;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpUpdateAfterPhysicsSysGrp))]
    public partial struct DetectSpawneeTriggerSystem : ISystem
    {
        ComponentLookup<EntityHealthPoint> entityHealthPointLookup;
        ComponentLookup<AttackCurDamage> spawneeCurDamageLookup;
        ComponentLookup<EntityStateMachine> entityStateMachineLookup;
        ComponentLookup<LocalTransform> localTransformLookup;
        ComponentLookup<AttackPierceTag> attackPierceTagLookup;
        ComponentLookup<AttackKnockBackTag> attackKnockBackTagLookup;
        ComponentLookup<AttackExplosiveCom> attackExplosiveTagLookup;
        ComponentLookup<EntityKnockBackCom> entityKnockBackComLookup;
        BufferLookup<HitBuffer> hitBufferLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            spawneeCurDamageLookup = SystemAPI.GetComponentLookup<AttackCurDamage>(true);
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            attackPierceTagLookup = SystemAPI.GetComponentLookup<AttackPierceTag>(true);
            attackKnockBackTagLookup = SystemAPI.GetComponentLookup<AttackKnockBackTag>(true);
            attackExplosiveTagLookup = SystemAPI.GetComponentLookup<AttackExplosiveCom>(true);
            entityKnockBackComLookup = SystemAPI.GetComponentLookup<EntityKnockBackCom>();  
            entityHealthPointLookup = SystemAPI.GetComponentLookup<EntityHealthPoint>();
            entityStateMachineLookup = SystemAPI.GetComponentLookup<EntityStateMachine>();
            hitBufferLookup = SystemAPI.GetBufferLookup<HitBuffer>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            entityHealthPointLookup.Update(ref state);
            spawneeCurDamageLookup.Update(ref state);
            entityStateMachineLookup.Update(ref state);
            entityKnockBackComLookup.Update(ref state);
            attackExplosiveTagLookup.Update(ref state);
            attackKnockBackTagLookup.Update(ref state);
            attackPierceTagLookup.Update(ref state);
            localTransformLookup.Update(ref state);
            hitBufferLookup.Update(ref state); 
            var endFixedSimECB = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var normalSpawneeTriggerJob = new DetectNormalSpawneeTriggerJob
            {
                ecb = endFixedSimECB,
                CurDamageLookup = spawneeCurDamageLookup,
                EntityHealthPointLookup = entityHealthPointLookup,
                EntityStateMachineLookup = entityStateMachineLookup,
                EntityKnockBackLookup = entityKnockBackComLookup,
                AttackExplosiveLookup = attackExplosiveTagLookup,
                AttackKnockBackLookup = attackKnockBackTagLookup,
                AttackPierceLookup = attackPierceTagLookup,
                LocalTransformLookup = localTransformLookup,
            };
            var pierceSpawneeTriggerJob = new DetectPierceSpawneeTriggerJob
            {
                ecb = endFixedSimECB,
                CurDamageLookup = spawneeCurDamageLookup,
                EntityHealthPointLookup = entityHealthPointLookup,
                EntityStateMachineLookup = entityStateMachineLookup,
                EntityKnockBackLookup = entityKnockBackComLookup,
                AttackExplosiveLookup = attackExplosiveTagLookup,
                AttackKnockBackLookup = attackKnockBackTagLookup,
                AttackPierceLookup = attackPierceTagLookup,
                LocalTransformLookup = localTransformLookup,
                HitBufferLookup = hitBufferLookup,
            };
            state.Dependency = normalSpawneeTriggerJob.Schedule(simulationSingleton, state.Dependency);
            state.Dependency = pierceSpawneeTriggerJob.Schedule(simulationSingleton, state.Dependency);
        }
    }
    
    public struct DetectNormalSpawneeTriggerJob : ITriggerEventsJob
    {
        public EntityCommandBuffer ecb;
        public ComponentLookup<EntityKnockBackCom> EntityKnockBackLookup;
        public ComponentLookup<EntityStateMachine> EntityStateMachineLookup;
        public ComponentLookup<EntityHealthPoint> EntityHealthPointLookup;
        [ReadOnly] public ComponentLookup<AttackCurDamage> CurDamageLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<AttackPierceTag> AttackPierceLookup;
        [ReadOnly] public ComponentLookup<AttackKnockBackTag> AttackKnockBackLookup;
        [ReadOnly] public ComponentLookup<AttackExplosiveCom> AttackExplosiveLookup;
        public void Execute(TriggerEvent triggerEvent)
        {
            //EntityA tends to be spawnee
            Entity Spawnee;
            Entity Enemy;
            if (AttackPierceLookup.HasComponent(triggerEvent.EntityA)) return;
            //Potential Risk caused by not checking EntityB.
            if (CurDamageLookup.HasComponent(triggerEvent.EntityA) && EntityHealthPointLookup.HasComponent(triggerEvent.EntityB))
            {
                Spawnee = triggerEvent.EntityA;
                Enemy = triggerEvent.EntityB;
            }
            else if (CurDamageLookup.HasComponent(triggerEvent.EntityB) && EntityHealthPointLookup.HasComponent(triggerEvent.EntityA))
            {
                Debug.LogError("Rare Situation in DetectSpawneeTriggerSystem, not always that Spawnee is EntityA. This time, Spawnee is EntityB, Entity is EntityA");
                Spawnee = triggerEvent.EntityB;
                Enemy = triggerEvent.EntityA;
            }
            else
            {
                return;
            }

            //Handling knockback;
            if (AttackKnockBackLookup.HasComponent(Spawnee))
            {
                ecb.SetComponentEnabled<EntityKnockBackCom>(Enemy, true);
            }
            //Handling explosive;
            if (AttackExplosiveLookup.TryGetComponent(Spawnee, out var explosiveCom))
            {
                var explosion = ecb.Instantiate(explosiveCom.ExplosionPrefab);
                ecb.SetComponent<LocalTransform>(explosion, LocalTransformLookup[Spawnee]);
            }
            

            ecb.DestroyEntity(Spawnee);
            var refHP = EntityHealthPointLookup.GetRefRW(Enemy);
            //Debug.Log("NormalSpawneeJob");
            if ((refHP.ValueRW.HealthPoint -= CurDamageLookup[Spawnee].damage) <= 0)
            {
                ecb.RemoveComponent<PhysicsCollider>(Enemy);
                //both way setting state machine works
                //ecb.SetComponent(Enemy, new EntityStateMachine { CurrentState = EntityState.Dead });    
                EntityStateMachineLookup.GetRefRW(Enemy).ValueRW.CurrentState = EntityState.Dead;
            }
        }
    }
    public struct DetectPierceSpawneeTriggerJob : ITriggerEventsJob
    {
        public EntityCommandBuffer ecb;
        public ComponentLookup<EntityKnockBackCom> EntityKnockBackLookup;
        public ComponentLookup<EntityStateMachine> EntityStateMachineLookup;
        public ComponentLookup<EntityHealthPoint> EntityHealthPointLookup;
        public BufferLookup<HitBuffer> HitBufferLookup;
        [ReadOnly] public ComponentLookup<AttackCurDamage> CurDamageLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<AttackPierceTag> AttackPierceLookup;
        [ReadOnly] public ComponentLookup<AttackKnockBackTag> AttackKnockBackLookup;
        [ReadOnly] public ComponentLookup<AttackExplosiveCom> AttackExplosiveLookup;
        public void Execute(TriggerEvent triggerEvent)
        {
            //EntityA tends to be spawnee
            //Entity Spawnee;
            //Entity Enemy;
            //Debug.Log("Pierced Job Execute");
            if (!AttackPierceLookup.HasComponent(triggerEvent.EntityA)) return;
            HitBufferLookup.TryGetBuffer(triggerEvent.EntityA, out var hitBuffer);
            //Debug.Log(hitBuffer.Length);
            //if (hitBuffer.Length == 0) return;
            for(int i = 0, n = hitBuffer.Length; i < n; i++)
            {
                if (hitBuffer[i].HitEntity == triggerEvent.EntityB) {return; }
            }
            var bufferEcb = ecb.SetBuffer<HitBuffer>(triggerEvent.EntityA);
            bufferEcb.Add(new HitBuffer { HitEntity = triggerEvent.EntityB });

            if (AttackKnockBackLookup.HasComponent(triggerEvent.EntityA))
            {
                ecb.SetComponentEnabled<EntityKnockBackCom>(triggerEvent.EntityB, true);
            }
            //Handling explosive;
            if (AttackExplosiveLookup.TryGetComponent(triggerEvent.EntityA, out var explosiveCom))
            {
                var explosion = ecb.Instantiate(explosiveCom.ExplosionPrefab);
                ecb.SetComponent<LocalTransform>(explosion, LocalTransformLookup[triggerEvent.EntityA]);
            }

            var refHP = EntityHealthPointLookup.GetRefRW(triggerEvent.EntityB);
            //Debug.Log("NormalSpawneeJob");
            if ((refHP.ValueRW.HealthPoint -= CurDamageLookup[triggerEvent.EntityA].damage) <= 0)
            {
                ecb.RemoveComponent<PhysicsCollider>(triggerEvent.EntityB);
                //both way setting state machine works
                //ecb.SetComponent(Enemy, new EntityStateMachine { CurrentState = EntityState.Dead });    
                EntityStateMachineLookup.GetRefRW(triggerEvent.EntityB).ValueRW.CurrentState = EntityState.Dead;
            }
            //Debug.Log("PierceSpawnee TriggerJob");
        }
    }
}