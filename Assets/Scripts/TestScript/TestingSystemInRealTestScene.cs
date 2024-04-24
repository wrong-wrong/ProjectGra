using ProjectGra;
using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;
[DisableAutoCreation]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup),OrderLast = true)]
//[UpdateAfter(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
public partial struct TestingSystemInRealTestScene : ISystem, ISystemStartStop
{
    private Random random;

    // melee weapon sweep test
    //float3 forwardMulRange;
    //float3 rightMulHalfWidth;
    //float sweepAccumulateTimer;
    //float sweepTotalTime;
    //float3 originalPos;

    //// group spawn test
    //Entity EnemyPrefab;
    //float2 groupSpawnFixedRangePointOne;
    //float2 groupSpawnFixedRangePointTwo;
    //float2 groupSpawnRangeOffset;
    //float spawnCooldown;
    //float spawnRealCooldown;
    //int groupSpawnCount;
    //int groupSpawnRealCount;
    //float groupSpawnTimer;
    //float groupSpawnRealTimer;

    //float minRadius;
    //float maxRadius;

    public void OnCreate(ref SystemState state)
    {

        //// effect request test
        //PopupTextManager.Instance.enabled = true;
        //EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(PopupTextManager.Instance.MaxPopupTextCount, PopupTextManager.Instance.MaxPopupTextCount, PopupTextManager.Instance.MaxPopupTextCount);
        //var defaultTransform = new LocalTransform { Position = Vector3.zero };
        //Debug.Log(defaultTransform.Scale); // scale default 0
        Debug.Log("Waiting For TestingEntityTag");
        state.RequireForUpdate<TestingEntityTag>();
        random = Random.CreateFromIndex(0);

    }

    public void OnStartRunning(ref SystemState state)
    {
        // group spawn test
        //var config = SystemAPI.GetSingleton<ForTestGroupSpawn>();
        //EnemyPrefab = config.EnemyPrefab;
        //groupSpawnRangeOffset = config.groupSpawnRangeOffset;
        //spawnCooldown = config.spawnCooldown;
        //groupSpawnCount = config.groupSpawnCount;
        //groupSpawnTimer = config.groupSpawnTimer;
        //minRadius = config.minRadius;
        //maxRadius = config.maxRadius;
        //Debug.Log(groupSpawnTimer);
    }

    public void OnStopRunning(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {

        #region group spawn test

        //var deltatime = SystemAPI.Time.DeltaTime;
        //// can there be multiple group spawn at the same time ? 
        //// while group spawn count 
        ////      update group spawn exclusive timer, spawn within a fixed range

        //if (groupSpawnRealCount > 0)
        //{
        //    if ((groupSpawnRealTimer -= deltatime) < 0f)
        //    {
        //        --groupSpawnRealCount;
        //        groupSpawnRealTimer = groupSpawnTimer;
        //        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        //        var enemy = ecb.Instantiate(EnemyPrefab);
        //        var posf2 = random.NextFloat2(groupSpawnFixedRangePointOne, groupSpawnFixedRangePointTwo);
        //        ecb.SetComponent(enemy, new LocalTransform { Position = new float3(posf2.x, 0, posf2.y), Scale = 1f });
        //    }
        //}

        //// if spawn timer
        ////      check group spawn count > 0 then random decide point spawn or group spawn
        //if ((spawnRealCooldown -= deltatime) < 0f)
        //{
        //    var playerPosition = SystemAPI.GetComponent<LocalTransform>(SystemAPI.GetSingletonEntity<TestingEntityTag>()).Position;
        //    spawnRealCooldown = spawnCooldown;
        //    //      need to get random point any way
        //    float2 f2pos;
        //    f2pos.x = random.NextFloat(playerPosition.x - minRadius, playerPosition.x + minRadius);
        //    f2pos.y = playerPosition.z + math.sqrt((minRadius + f2pos.x - playerPosition.x) * (minRadius - f2pos.x + playerPosition.x)) * (random.NextBool() ? -1 : 1);
        //    f2pos.x *= random.NextFloat(1, maxRadius /  minRadius);
        //    f2pos.y *= random.NextFloat(1, maxRadius / minRadius);

        //    if (random.NextFloat() > 0.5f || groupSpawnRealCount > 0)//      if point spawn
        //    {
        //        Debug.Log("Point Spawn");
        //        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
        //        var enemy = ecb.Instantiate(EnemyPrefab);
        //        ecb.SetComponent<LocalTransform>(enemy, new LocalTransform { Position = new float3(f2pos.x,0,f2pos.y) , Scale = 1f});
        //    }
        //    else  //          if group spawn then calculate group spawn fixed range
        //    {
        //        Debug.Log("Group Spawn");
        //        groupSpawnRealCount = groupSpawnCount;

        //        var dir = f2pos.yx - playerPosition.zx;
        //        dir.x = -dir.x;
        //        groupSpawnFixedRangePointOne = f2pos + dir + groupSpawnRangeOffset;
        //        groupSpawnFixedRangePointTwo = f2pos - dir - groupSpawnRangeOffset;

        //    }
        //}

        #endregion


        #region melee weapon sweep test
        //var deltatime = SystemAPI.Time.DeltaTime;
        //if((sweepAccumulateTimer += deltatime) < sweepTotalTime)
        //{
        //    var testEntity = SystemAPI.GetSingletonEntity<TestingEntityTag>();
        //    var transformRW = SystemAPI.GetComponentRW<LocalTransform>(testEntity);
        //    var ratio = sweepAccumulateTimer / sweepTotalTime;
        //    transformRW.ValueRW.Position = originalPos + forwardMulRange * math.sin(math.radians(ratio * 180)) + rightMulHalfWidth * math.cos(math.radians(ratio * 180));
        //}


        //if (!Input.GetKeyUp(KeyCode.Space)) return;
        //sweepAccumulateTimer = 0;

        //var playerTransform = TestMonoLaucher.Instance.PlayerModel;
        //originalPos = playerTransform.position;
        //sweepTotalTime = TestMonoLaucher.Instance.ForwardRange / TestMonoLaucher.Instance.ForwardSpeed;
        //forwardMulRange = playerTransform.forward * TestMonoLaucher.Instance.ForwardRange;
        //rightMulHalfWidth = playerTransform.right * TestMonoLaucher.Instance.HalfWidth;


        //// rotation is controlled by cameratargetfollowSystem in real application
        //var testEntity2 = SystemAPI.GetSingletonEntity<TestingEntityTag>();
        //var transformRW2 = SystemAPI.GetComponentRW<LocalTransform>(testEntity2);
        //transformRW2.ValueRW.Rotation = quaternion.LookRotation(playerTransform.forward, math.up());

        #endregion


        #region effect request test
        //var popText_disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        //var popText_posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
        //var popText_valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        ////var audioPosList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList;
        ////var audioEnumList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList;
        ////var particlePosList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList;
        ////var particleEnumList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList;
        //foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TestingEntityTag>().WithEntityAccess())
        //{

        //    popText_posList.Add(1);
        //    popText_disSqList.Add(-1);
        //    popText_valList.Add(0);


        //    popText_posList.Add(1);
        //    popText_disSqList.Add(-2);
        //    popText_valList.Add(0);
        //    //var pos = transform.ValueRO.Position;
        //    //var disSq = math.distancesq(transform.ValueRO.Position, float3.zero);
        //    //var randomInt = random.NextInt(77, 777);
        //    //popText_posList.Add(pos);
        //    //popText_disSqList.Add(disSq);
        //    //popText_valList.Add(randomInt);
        //    //Debug.Log("Adding : " + disSq + " - "+ pos + " - " +  randomInt + "Entity Idx" + entity.Index);
        //    //audioPosList.Add(pos);
        //    //audioEnumList.Add(AudioEnum.NormalShoot);
        //    //particlePosList.Add(pos);
        //    //particleEnumList.Add(ParticleEnum.Default);

        //}
        //EffectRequestSharedStaticBuffer.SharedValue.Data.SortPopupText();
        //Debug.Log("TestingSystemExecute");
        #endregion

        #region flashing com multiple color test


        //var deltatime = SystemAPI.Time.DeltaTime;
        //foreach (var (flash, flashBit, basecolor) in SystemAPI.Query<RefRW<FlashingCom>, EnabledRefRW<FlashingCom>, RefRW<URPMaterialPropertyBaseColor>>())
        //{
        //    var ratio = flash.ValueRO.AccumulateTimer / flash.ValueRO.CycleTime;
        //    basecolor.ValueRW.Value.xyz = 1f - flash.ValueRO.FlashColorDifference * math.sin(math.radians(ratio * 180));//1-ratio;
        //    if ((flash.ValueRW.AccumulateTimer += deltatime) < flash.ValueRO.Duration) continue;
        //    flash.ValueRW.AccumulateTimer = 0;
        //    flash.ValueRW.Duration = 1f;
        //    flash.ValueRW.CycleTime = 1f;
        //    flashBit.ValueRW = false;
        //    basecolor.ValueRW.Value = new float4(1, 1, 1, 1);

        //}

        //if (!Input.GetKeyUp(KeyCode.Space)) return;
        //float4 customColor = TestMonoLaucher.Instance.mycustomColor;
        //foreach(var (flashingCom, flashingBit) in SystemAPI.Query<RefRW<FlashingCom>, EnabledRefRW<FlashingCom>>()
        //    .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
        //{
        //    flashingCom.ValueRW.FlashColorDifference = new float3(1 - customColor.x, 1 - customColor.y, 1 - customColor.z);
        //    flashingBit.ValueRW = true;
        //}
        #endregion
    }
}