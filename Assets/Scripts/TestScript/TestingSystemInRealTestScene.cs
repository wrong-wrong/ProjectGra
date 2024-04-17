using ProjectGra;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
[DisableAutoCreation]
public partial struct TestingSystemInRealTestScene : ISystem
{
    private Random random;
    public void OnCreate(ref SystemState state)
    {

        // effect request test
        //EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(PopupTextManager.Instance.MaxPopupTextCount, AudioManager.Instance.MaxAudioSourceCount, ParticleManager.Instance.MaxParticleCount);
        random = Random.CreateFromIndex(0);
    }
    public void OnUpdate(ref SystemState state) 
    {
        //if (!Input.GetKeyUp(KeyCode.Space)) return;

        #region effect request test
        //var popText_disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        //var popText_posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
        //var popText_valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        //var audioPosList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList;
        //var audioEnumList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList;
        //var particlePosList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList;
        //var particleEnumList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList;
        //foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TestingEntityTag>().WithEntityAccess())
        //{
        //    var pos = transform.ValueRO.Position;
        //    var disSq = math.distancesq(transform.ValueRO.Position, float3.zero);
        //    var randomInt = random.NextInt(77, 777);
        //    popText_posList.Add(pos);
        //    popText_disSqList.Add(disSq);
        //    popText_valList.Add(randomInt);
        //    //Debug.Log("Adding : " + disSq + " - "+ pos + " - " +  randomInt + "Entity Idx" + entity.Index);
        //    audioPosList.Add(pos);
        //    audioEnumList.Add(AudioEnum.NormalShoot);
        //    particlePosList.Add(pos);
        //    particleEnumList.Add(ParticleEnum.Default);

        //}
        //EffectRequestSharedStaticBuffer.SharedValue.Data.SortPopupText();
        #endregion

        #region flashing com multiple color test


        var deltatime = SystemAPI.Time.DeltaTime;
        foreach (var (flash, flashBit, basecolor) in SystemAPI.Query<RefRW<FlashingCom>, EnabledRefRW<FlashingCom>, RefRW<URPMaterialPropertyBaseColor>>())
        {
            var ratio = flash.ValueRO.AccumulateTimer / flash.ValueRO.CycleTime;
            basecolor.ValueRW.Value.xyz = 1f - flash.ValueRO.FlashColorDifference * math.sin(math.radians(ratio * 180));//1-ratio;
            if ((flash.ValueRW.AccumulateTimer += deltatime) < flash.ValueRO.Duration) continue;
            flash.ValueRW.AccumulateTimer = 0;
            flash.ValueRW.Duration = 1f;
            flash.ValueRW.CycleTime = 1f;
            flashBit.ValueRW = false;
            basecolor.ValueRW.Value = new float4(1, 1, 1, 1);

        }

        if (!Input.GetKeyUp(KeyCode.Space)) return;
        float4 customColor = TestMonoLaucher.Instance.mycustomColor;
        foreach(var (flashingCom, flashingBit) in SystemAPI.Query<RefRW<FlashingCom>, EnabledRefRW<FlashingCom>>()
            .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState))
        {
            flashingCom.ValueRW.FlashColorDifference = new float3(1 - customColor.x, 1 - customColor.y, 1 - customColor.z);
            flashingBit.ValueRW = true;
        }
        #endregion
    }
}