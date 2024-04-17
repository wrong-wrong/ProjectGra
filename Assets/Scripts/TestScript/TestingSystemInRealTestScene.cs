using ProjectGra;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
[DisableAutoCreation]
public partial struct TestingSystemInRealTestScene : ISystem
{
    private Random random;
    public void OnCreate(ref SystemState state)
    {
        EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(PopupTextManager.Instance.MaxPopupTextCount, AudioManager.Instance.MaxAudioSourceCount, ParticleManager.Instance.MaxParticleCount);
        random = Random.CreateFromIndex(0);
    }
    public void OnUpdate(ref SystemState state) 
    {
        if (!Input.GetKeyUp(KeyCode.Space)) return;
        var popText_disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        var popText_posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
        var popText_valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        var audioPosList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioPosList;
        var audioEnumList = EffectRequestSharedStaticBuffer.SharedValue.Data.AudioEnumList;
        var particlePosList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticlePosList;
        var particleEnumList = EffectRequestSharedStaticBuffer.SharedValue.Data.ParticleEnumList;
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TestingEntityTag>().WithEntityAccess())
        {
            var pos = transform.ValueRO.Position;
            var disSq = math.distancesq(transform.ValueRO.Position, float3.zero);
            var randomInt = random.NextInt(77, 777);
            popText_posList.Add(pos);
            popText_disSqList.Add(disSq);
            popText_valList.Add(randomInt);
            //Debug.Log("Adding : " + disSq + " - "+ pos + " - " +  randomInt + "Entity Idx" + entity.Index);
            audioPosList.Add(pos);
            audioEnumList.Add(AudioEnum.NormalShoot);
            particlePosList.Add(pos);
            particleEnumList.Add(ParticleEnum.Default);

        }
        EffectRequestSharedStaticBuffer.SharedValue.Data.SortPopupText();
        
    }
}