using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
public class TestScriptInSampleScene : MonoBehaviour
{
    public float radiusOne;
    public float radiusTwo;
    public float radiusThree;
    public NativeArray<float2> directionOffsets;
    private Random random;
    private Vector3 tmpPos;
    private float2 tmpf2;
    private NativeArray<float2> outList;
    private bool flip;
    private void Awake()
    {
        directionOffsets = new NativeArray<float2>(16, Allocator.Persistent);
        directionOffsets[0] = new float2(0, 1);
        directionOffsets[1] = new float2(1, 0);
        directionOffsets[2] = new float2(-1, 0);
        directionOffsets[3] = new float2(0, -1);

        directionOffsets[4] = new float2(0.7071f, 0.7071f);
        directionOffsets[5] = new float2(0.7071f, -0.7071f);
        directionOffsets[6] = new float2(-0.7071f, 0.7071f);
        directionOffsets[7] = new float2(-0.7071f, -0.7071f);

        directionOffsets[8] = new float2(0.866f, 0.5f);
        directionOffsets[9] = new float2(0.866f, -0.5f);
        directionOffsets[10] = new float2(-0.866f, 0.5f);
        directionOffsets[11] = new float2(-0.866f, -0.5f);

        directionOffsets[12] = new float2(0.5f, 0.866f);
        directionOffsets[13] = new float2(0.5f, -0.866f);
        directionOffsets[14] = new float2(-0.5f, 0.866f);
        directionOffsets[15] = new float2(-0.5f, -0.866f);
        random = Random.CreateFromIndex((uint)DateTime.Now.Ticks);
        outList = new NativeArray<float2>(7, Allocator.Persistent);
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            //var count = GetRandomPointOnCircleWithRadiusWithCount(radiusOne,5,ref outList);
            //for(int i = 0; i < count; ++i)
            //{
            //    tmpPos.x = outList[i].x;
            //    tmpPos.z = outList[i].y;
            //    GameObject.CreatePrimitive(PrimitiveType.Sphere).gameObject.transform.position = tmpPos;
            //}

            //var f2pos = GetRandomPointOnRadius(radiusTwo);
            //tmpPos.x = f2pos.x;
            //tmpPos.z = f2pos.y;
            //GameObject.CreatePrimitive(PrimitiveType.Sphere).gameObject.transform.position = tmpPos;
            for (int i = 0; i < 1000; i++)
            {
                tmpf2 = GetRandomPointWithRangeRadius(3f, 5f);
                tmpPos.x = tmpf2.x;
                tmpPos.y = tmpf2.y;
                GameObject.CreatePrimitive(PrimitiveType.Sphere).gameObject.transform.position = tmpPos;
            }
        }
    }

    private float2 GetRandomPointWithRangeRadius(float minRadius, float maxRadius)
    {

        //var oriPos = GetRandomPointOnRadius(minRadius);
        //oriPos.x += oriPos.x < 0 ? random.NextFloat(minRadius - maxRadius, 0) : random.NextFloat(0, maxRadius - minRadius);
        //oriPos.y += oriPos.y < 0 ? random.NextFloat(minRadius - maxRadius, 0) : random.NextFloat(0, maxRadius - minRadius);
        //return oriPos;

        var oriPos = GetRandomPointOnRadius(minRadius);
        oriPos.x *= random.NextFloat(1, maxRadius / minRadius);
        oriPos.y *= random.NextFloat(1, maxRadius / minRadius);
        //flip = !flip;
        //if (flip)
        //{
        //    oriPos = oriPos.yx;
        //}
        return oriPos;
    }
    private float2 GetRandomPointOnRadius(float radius)
    {
        var pos = gameObject.transform.position;
        var oriPos = new float2(pos.x, pos.z);
        float2 f2pos;
        f2pos.x = random.NextFloat(oriPos.x - radius, oriPos.x + radius);
        f2pos.y = oriPos.y + math.sqrt((radius + f2pos.x - oriPos.x) * (radius - f2pos.x + oriPos.x)) * (random.NextBool() ? -1 : 1);
        return f2pos;
    }
    private int GetRandomPointOnCircleWithRadiusWithCount(float radius, int count, ref NativeArray<float2> list)
    {
        var pos = gameObject.transform.position;
        var oriPos = new float2(pos.x, pos.z);
        float2 f2pos;
        for (int i = 0; i < count; ++i)
        {
            var x = random.NextFloat(oriPos.x - radius, oriPos.x + radius);
            var y = oriPos.y + math.sqrt((radius + x - oriPos.x) * (radius - x + oriPos.x)) * (random.NextBool() ? -1 : 1);
            f2pos.x = x;
            f2pos.y = y;
            list[i] = f2pos;
        }
        return count;
    }
    private void OnDestroy()
    {
        directionOffsets.Dispose();
        outList.Dispose();
    }
}
