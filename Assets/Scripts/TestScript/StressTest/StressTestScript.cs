using ProjectGra;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StressTestScript : MonoBehaviour
{
    // Start is called before the first frame update

    // branch test
    //[SerializeField] int reps;
    //int val;
    //bool flag;
    //int times;
    //long totalticks;
    //long minTicks;
    //long maxTicks;
    private void Awake()
    {
        #region reference stress test Awake
        //int capacity = 64;
        //EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(capacity, capacity, capacity);
        //for (int i = 0; i < capacity; ++i)
        //{
        //    EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList.Add(0);
        //}

        #endregion

        // branch test
        //minTicks = long.MaxValue; maxTicks = long.MinValue;
    }

    // Update is called once per frame
    void Update()
    {
        #region reference stress test Update
        //EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList[0] = 1;
        //Debug.Log(EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList[0]);
        //Stopwatch stopwatch = Stopwatch.StartNew();
        //var list = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        //for (int i = 0; i < 1000; i++)
        //{
        //    for (int j = 0; j < 64; j++)
        //    {
        //        list[j] = 10; // average elapsedTicks - 15000+-
        //        //EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList[j] = 10;   // average elapsedTicks - 24000+-
        //    }
        //}
        //stopwatch.Stop();
        //Debug.Log(stopwatch.ElapsedTicks);
        ////EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList.Clear();
        //Debug.Log(EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList[0]);
        //Debug.Break();
        #endregion

        #region branch test

        //for(int j = 0; j < 100; ++j)
        //{
        //    val = 0;
        //    Stopwatch stopwatch = Stopwatch.StartNew(); 
        //    for (int i = 0; i < reps; ++i)
        //    {
        //        if(flag)
        //        {
        //            ++val;
        //        }
        //        else
        //        {
        //            --val;
        //        }
        //        flag = !flag;
        //    }
        //    stopwatch.Stop();

        //    Debug.Log(stopwatch.ElapsedTicks);
        //    totalticks += stopwatch.ElapsedTicks;
        //    minTicks = math.min(minTicks, stopwatch.ElapsedTicks);
        //    maxTicks = math.max(maxTicks, stopwatch.ElapsedTicks);
        //    ++times;
        //}
        //Debug.Log("Average ticks : " + totalticks / times + "\nTest times : " + times + "\nMaxTick : " + maxTicks + "\nMinTicks : " + minTicks);
        //Debug.Break();
        #endregion
    }
}
