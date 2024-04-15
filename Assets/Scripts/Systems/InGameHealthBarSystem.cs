using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGra
{
    [UpdateInGroup(typeof(MySysGrpAfterFixedBeforeTransform))]
    public partial struct InGameHealthBarSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameControllInGame>();
            state.RequireForUpdate<GameControllNotPaused>();
            state.RequireForUpdate<TestSceneExecuteTag>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<MyECBSystemBeforeTransform.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            // instatiate health bar for whom need it
            foreach(var (barUIOffset, transform, entityHp,eliteHp, stateMachine, entity) in SystemAPI.Query<RefRO<HealthBatUIOffset>, RefRO<LocalTransform>, RefRO<EntityHealthPoint>
                ,RefRW<EliteMaxHealthPoint>
                ,RefRO<EntityStateMachine>>()
                .WithEntityAccess()
                .WithNone<HealthBarUICleanupCom>())
            {
                if (stateMachine.ValueRO.CurrentState == EntityState.SpawnEffect) continue;
                var healthBarPrefab = SystemAPI.ManagedAPI.GetSingleton<GOPrefabManagedContainer>().HealthBarUIPrefab;
                var spawnPos = transform.ValueRO.Position + barUIOffset.ValueRO.OffsetValue;
                var healthBar = Object.Instantiate(healthBarPrefab, spawnPos, transform.ValueRO.Rotation);
                var imgList = healthBar.GetComponentsInChildren<Image>();
                var fillingImg = imgList[imgList.Length - 1];
                eliteHp.ValueRW.MaxHealthPoint = entityHp.ValueRO.HealthPoint;
                eliteHp.ValueRW.PreviousHealthPoint = entityHp.ValueRO.HealthPoint;
                Debug.Log(imgList.Length);
                Debug.Log(fillingImg.name);
                fillingImg.fillAmount = 1;
                Debug.LogWarning("Using fixed index to get filling img from the Image List - GetComponentsInChildren");
                ecb.AddComponent(entity, new HealthBarUICleanupCom { HealthBarGO = healthBar, BarFillingImage = fillingImg});
            }

            // destory health bar GO whose entity has been provisionally destoryed
            foreach(var (healthBarManagedCom, entity) in SystemAPI.Query<HealthBarUICleanupCom>()
                .WithEntityAccess()
                .WithNone<HealthBatUIOffset>())
            {
                Object.Destroy(healthBarManagedCom.HealthBarGO);
                ecb.RemoveComponent<HealthBarUICleanupCom>(entity);
            }

            // update health bar positon & rotation
            foreach(var (healthBarManagedCom, transform, barOffset) in SystemAPI.Query<HealthBarUICleanupCom,RefRO<LocalTransform>,RefRO<HealthBatUIOffset>>())
            {
                healthBarManagedCom.HealthBarGO.transform.position = transform.ValueRO.Position + barOffset.ValueRO.OffsetValue;
                healthBarManagedCom.HealthBarGO.transform.rotation = transform.ValueRO.Rotation;
            }

            // update bar filling amount 
            foreach(var (entityHp, eliteHp, healthBarManagedCom) in SystemAPI.Query<RefRO<EntityHealthPoint>, RefRW<EliteMaxHealthPoint>, HealthBarUICleanupCom>())
            {
                if(entityHp.ValueRO.HealthPoint != eliteHp.ValueRO.PreviousHealthPoint)
                {
                    healthBarManagedCom.BarFillingImage.fillAmount = (float)entityHp.ValueRO.HealthPoint / eliteHp.ValueRO.MaxHealthPoint;
                    eliteHp.ValueRW.PreviousHealthPoint = entityHp.ValueRO.HealthPoint;
                    //Debug.Log("Setting filling amount");
                }
            }
        }
    }
}