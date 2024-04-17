using ProjectGra;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StressTestScript : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        #region reference stress test Awake
        int capacity = 64;
        EffectRequestSharedStaticBuffer.SharedValue.Data = new EffectRequestSharedStaticBuffer(capacity, capacity, capacity);
        for (int i = 0; i < capacity; ++i)
        {
            EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList.Add(0);
        }
        #endregion
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
    }
}
