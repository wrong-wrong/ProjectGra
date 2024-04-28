using OOPExperiment;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
        ComponentLookup<AttackPierce> attackPierceTagLookup;
        ComponentLookup<AttackKnockBackTag> attackKnockBackTagLookup;
        ComponentLookup<AttackExplosiveCom> attackExplosiveTagLookup;
        ComponentLookup<EntityKnockBackCom> entityKnockBackComLookup;
        ComponentLookup<FlashingCom> flashingComLookup;
        ComponentLookup<PlayerSuccessfulAttackCount> attackCountLookup;
        BufferLookup<HitBuffer> hitBufferLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
            spawneeCurDamageLookup = SystemAPI.GetComponentLookup<AttackCurDamage>(true);
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            attackPierceTagLookup = SystemAPI.GetComponentLookup<AttackPierce>(true);
            attackKnockBackTagLookup = SystemAPI.GetComponentLookup<AttackKnockBackTag>(true);
            attackExplosiveTagLookup = SystemAPI.GetComponentLookup<AttackExplosiveCom>(true);
            entityKnockBackComLookup = SystemAPI.GetComponentLookup<EntityKnockBackCom>();  
            entityHealthPointLookup = SystemAPI.GetComponentLookup<EntityHealthPoint>();
            entityStateMachineLookup = SystemAPI.GetComponentLookup<EntityStateMachine>();
            hitBufferLookup = SystemAPI.GetBufferLookup<HitBuffer>();
            flashingComLookup = SystemAPI.GetComponentLookup<FlashingCom>();
            attackCountLookup = SystemAPI.GetComponentLookup<PlayerSuccessfulAttackCount>();
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
            flashingComLookup.Update(ref state);
            attackCountLookup.Update(ref state);
            var beginSimECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            var playerTransform = localTransformLookup[playerEntity];
            var normalSpawneeTriggerJob = new DetectNormalSpawneeTriggerJob
            {
                attackCountLookup = attackCountLookup,
                PlayerEntity = playerEntity,
                PlayerPosition = playerTransform.Position,
                ecb = beginSimECB,
                CurDamageLookup = spawneeCurDamageLookup,
                EntityHealthPointLookup = entityHealthPointLookup,
                EntityStateMachineLookup = entityStateMachineLookup,
                EntityKnockBackLookup = entityKnockBackComLookup,
                AttackExplosiveLookup = attackExplosiveTagLookup,
                AttackKnockBackLookup = attackKnockBackTagLookup,
                AttackPierceLookup = attackPierceTagLookup,
                LocalTransformLookup = localTransformLookup,
                FlashingComLookup = flashingComLookup,
                posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList,
                disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList,
                valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList,
                
            };
            var pierceSpawneeTriggerJob = new DetectPierceSpawneeTriggerJob
            {
                attackCountLookup = attackCountLookup,
                PlayerEntity = playerEntity,
                PlayerPosition = playerTransform.Position,
                ecb = beginSimECB,
                CurDamageLookup = spawneeCurDamageLookup,
                EntityHealthPointLookup = entityHealthPointLookup,
                EntityStateMachineLookup = entityStateMachineLookup,
                EntityKnockBackLookup = entityKnockBackComLookup,
                AttackExplosiveLookup = attackExplosiveTagLookup,
                AttackKnockBackLookup = attackKnockBackTagLookup,
                AttackPierceLookup = attackPierceTagLookup,
                LocalTransformLookup = localTransformLookup,
                HitBufferLookup = hitBufferLookup,
                FlashingComLookup = flashingComLookup,
                posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList,
                disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList,
                valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList,
            };
            //EffectRequestSharedStaticBuffer.SharedValue.Data.PlayerPosition = playerTransform.Position;
            EffectRequestSharedStaticBuffer.SharedValue.Data.SortPopupText();
            state.Dependency = normalSpawneeTriggerJob.Schedule(simulationSingleton, state.Dependency);
            state.Dependency = pierceSpawneeTriggerJob.Schedule(simulationSingleton, state.Dependency);
            state.Dependency.Complete();
        }
    }
    
    public struct DetectNormalSpawneeTriggerJob : ITriggerEventsJob
    {
        public ComponentLookup<PlayerSuccessfulAttackCount> attackCountLookup;
        public Entity PlayerEntity;
        public float3 PlayerPosition;
        public EntityCommandBuffer ecb;
        public ComponentLookup<FlashingCom> FlashingComLookup;
        public ComponentLookup<EntityKnockBackCom> EntityKnockBackLookup;
        public ComponentLookup<EntityStateMachine> EntityStateMachineLookup;
        public ComponentLookup<EntityHealthPoint> EntityHealthPointLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<AttackCurDamage> CurDamageLookup;
        //[ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<AttackPierce> AttackPierceLookup;
        [ReadOnly] public ComponentLookup<AttackKnockBackTag> AttackKnockBackLookup;
        [ReadOnly] public ComponentLookup<AttackExplosiveCom> AttackExplosiveLookup;

        public NativeList<float> disSqList;
        public NativeList<float3> posList;
        public NativeList<int> valList;
        public void Execute(TriggerEvent triggerEvent)
        {
            //EntityA tends to be spawnee
            Entity Spawnee;
            Entity GetHitEntity;
            if (AttackPierceLookup.HasComponent(triggerEvent.EntityA)) return;
            //Potential Risk caused by not checking EntityB.
            if (CurDamageLookup.HasComponent(triggerEvent.EntityA) && EntityHealthPointLookup.HasComponent(triggerEvent.EntityB))
            {
                Spawnee = triggerEvent.EntityA;
                GetHitEntity = triggerEvent.EntityB;
            }
            else if (CurDamageLookup.HasComponent(triggerEvent.EntityB) && EntityHealthPointLookup.HasComponent(triggerEvent.EntityA))
            {
                Debug.LogError("Rare Situation in DetectSpawneeTriggerSystem, not always that Spawnee is EntityA. This time, Spawnee is EntityB, Entity is EntityA");
                Spawnee = triggerEvent.EntityB;
                GetHitEntity = triggerEvent.EntityA;
            }
            else
            {
                return;
            }

            var spawneeLocalTransform = LocalTransformLookup[Spawnee];
            //Handling knockback;
            if (AttackKnockBackLookup.HasComponent(Spawnee))
            {
                ecb.SetComponentEnabled<EntityKnockBackCom>(GetHitEntity, true);
            }
            //Handling explosive;
            if (AttackExplosiveLookup.TryGetComponent(Spawnee, out var explosiveCom))
            {
                var explosion = ecb.Instantiate(explosiveCom.ExplosionPrefab);
                ecb.SetComponent<LocalTransform>(explosion, spawneeLocalTransform);
            }

            //Debug.Log("Destory Spawnee using ecb in NormalSpawneeJob");
            ecb.DestroyEntity(Spawnee);
            var damage = CurDamageLookup[Spawnee].damage;
            if (GetHitEntity == PlayerEntity)
            {
                ecb.AppendToBuffer<PlayerDamagedRecordBuffer>(PlayerEntity, new PlayerDamagedRecordBuffer { Value = damage }); // Negtive value represents get damage
                return;
            }
            //ecb.AppendToBuffer<PlayerDamagedRecordBuffer>(PlayerEntity, new PlayerDamagedRecordBuffer { Value = damage });
            
            var refHP = EntityHealthPointLookup.GetRefRW(GetHitEntity);
            //Debug.Log("NormalSpawneeJob");
            if ((refHP.ValueRW.HealthPoint -= damage) <= 0)
            {
                ecb.RemoveComponent<PhysicsCollider>(GetHitEntity);
                //both way setting state machine works
                //ecb.SetComponent(Enemy, new EntityStateMachine { CurrentState = EntityState.Dead });    
                EntityStateMachineLookup.GetRefRW(GetHitEntity).ValueRW.CurrentState = EntityState.Dead;
            }
            else
            {
                //ecb.SetComponentEnabled<FlashingCom>(Enemy, true);
                FlashingComLookup.SetComponentEnabled(GetHitEntity, true);
                //FlashingComLookup.
            }
            attackCountLookup.GetRefRW(PlayerEntity).ValueRW.Value++;

            var disSq = math.distancesq(PlayerPosition, spawneeLocalTransform.Position);
            var pos = spawneeLocalTransform.Position;
            posList.Add(pos);
            disSqList.Add(disSq);
            valList.Add(damage);
        }
    }
    public struct DetectPierceSpawneeTriggerJob : ITriggerEventsJob
    {
        public ComponentLookup<PlayerSuccessfulAttackCount> attackCountLookup;
        public Entity PlayerEntity;
        public float3 PlayerPosition;
        public EntityCommandBuffer ecb;
        public ComponentLookup<FlashingCom> FlashingComLookup;
        public ComponentLookup<EntityKnockBackCom> EntityKnockBackLookup;
        public ComponentLookup<EntityStateMachine> EntityStateMachineLookup;
        public ComponentLookup<EntityHealthPoint> EntityHealthPointLookup;
        public BufferLookup<HitBuffer> HitBufferLookup;
        [ReadOnly] public ComponentLookup<AttackCurDamage> CurDamageLookup;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<AttackPierce> AttackPierceLookup;
        [ReadOnly] public ComponentLookup<AttackKnockBackTag> AttackKnockBackLookup;
        [ReadOnly] public ComponentLookup<AttackExplosiveCom> AttackExplosiveLookup;
        public NativeList<float> disSqList;
        public NativeList<float3> posList;
        public NativeList<int> valList;

        public void Execute(TriggerEvent triggerEvent)
        {
            //triggerCount.ValueRW.Value += 1;
            //EntityA tends to be spawnee
            Entity Spawnee = triggerEvent.EntityA;
            Entity GetHitEntity = triggerEvent.EntityB;
            if (!AttackPierceLookup.TryGetComponent(Spawnee,out AttackPierce pierceCom)) return;
            //Debug.Log("Pierced Job Executing");
            HitBufferLookup.TryGetBuffer(Spawnee, out var hitBuffer);
            //Debug.Log(hitBuffer.Length);
            //if (hitBuffer.Length == 0) return;
            for(int i = 0, n = hitBuffer.Length; i < n; i++)
            {
                if (hitBuffer[i].HitEntity == GetHitEntity) {return; }
            }
            var spawneeLocalTransform = LocalTransformLookup[Spawnee];

            //var bufferEcb = ecb.SetBuffer<HitBuffer>(Spawnee);
            //bufferEcb.Add(new HitBuffer { HitEntity = Enemy });

            ecb.AppendToBuffer<HitBuffer>(Spawnee, new HitBuffer { HitEntity = GetHitEntity });

            if (AttackKnockBackLookup.HasComponent(Spawnee))
            {
                ecb.SetComponentEnabled<EntityKnockBackCom>(GetHitEntity, true);
            }
            //Handling explosive;
            if (AttackExplosiveLookup.TryGetComponent(Spawnee, out var explosiveCom))
            {
                var explosion = ecb.Instantiate(explosiveCom.ExplosionPrefab);
                ecb.SetComponent<LocalTransform>(explosion, LocalTransformLookup[Spawnee]);
            }
            if (hitBuffer.Length + 1 >= pierceCom.MaxPierceCount)
            {
                ecb.DestroyEntity(Spawnee);
            }
            var damage = CurDamageLookup[Spawnee].damage;
            if (GetHitEntity == PlayerEntity)
            {
                ecb.AppendToBuffer<PlayerDamagedRecordBuffer>(PlayerEntity, new PlayerDamagedRecordBuffer { Value = damage }); //
                return;
            }



            var refHP = EntityHealthPointLookup.GetRefRW(GetHitEntity);
            //Debug.Log("NormalSpawneeJob");
            //Debug.Log("Pierced Job - Hurting");
            if ((refHP.ValueRW.HealthPoint -= damage) <= 0)
            {
                //ecb.RemoveComponent<PhysicsCollider>(GetHitEntity);
                //both way setting state machine works
                //ecb.SetComponent(Enemy, new EntityStateMachine { CurrentState = EntityState.Dead });    
                EntityStateMachineLookup.GetRefRW(GetHitEntity).ValueRW.CurrentState = EntityState.Dead;
            }
            else
            {
                FlashingComLookup.SetComponentEnabled(GetHitEntity, true);  
            }
            attackCountLookup.GetRefRW(PlayerEntity).ValueRW.Value++;


            var disSq = math.distancesq(PlayerPosition, spawneeLocalTransform.Position);
            var pos = spawneeLocalTransform.Position;
            posList.Add(pos);
            disSqList.Add(disSq);
            valList.Add(damage);
            //Debug.Log("PierceSpawnee TriggerJob");

        }
    }
}