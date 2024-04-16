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
        EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(PopupTextManager.Instance.MaxPopupTextCount);
        random = Random.CreateFromIndex(0);
    }
    public void OnUpdate(ref SystemState state) 
    {
        if (!Input.GetKeyUp(KeyCode.Space)) return;
        var disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        var posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
        var valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TestingEntityTag>().WithEntityAccess())
        {
            var pos = transform.ValueRO.Position;
            var disSq = math.distancesq(transform.ValueRO.Position, float3.zero);
            var randomInt = random.NextInt(77, 777);
            posList.Add(pos);
            disSqList.Add(disSq);
            valList.Add(randomInt);
            //Debug.Log("Adding : " + disSq + " - "+ pos + " - " +  randomInt + "Entity Idx" + entity.Index);
        }
        EffectRequestSharedStaticBuffer.SharedValue.Data.SortPopupText();
    }
}