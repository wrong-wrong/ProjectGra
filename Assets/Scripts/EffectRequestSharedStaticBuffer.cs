using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectGra
{
    public struct EffectRequestSharedStaticBuffer
    {
        public static readonly SharedStatic<EffectRequestSharedStaticBuffer> SharedValue = SharedStatic<EffectRequestSharedStaticBuffer>.GetOrCreate<EffectRequestSharedStaticBuffer>();
        public NativeList<int> PopupTextRequestEntityIdxList;
        public NativeList<float> PopupTextDistanceSqList;
        public NativeList<float3> PopupTextWorldPosList;
        public NativeList<int> PopupTextValueList;
        public NativeList<float3> AudioPosList;
        public NativeList<AudioEnum> AudioEnumList;
        public NativeList<float3> ParticlePosList;
        public NativeList<ParticleEnum> ParticleEnumList;
        //public float3 PlayerPosition;
        public EffectRequestSharedStaticBuffer(int popupTextCapacity, int audioSourceCapacity, int particleCapacity)
        {
            PopupTextDistanceSqList = new NativeList<float>(popupTextCapacity,Allocator.Persistent);
            PopupTextWorldPosList = new NativeList<float3>(popupTextCapacity,Allocator.Persistent);
            PopupTextValueList = new NativeList<int>(popupTextCapacity,Allocator.Persistent);
            PopupTextRequestEntityIdxList = new NativeList<int>(popupTextCapacity, Allocator.Persistent);

            AudioPosList = new NativeList<float3>(audioSourceCapacity,Allocator.Persistent);
            AudioEnumList = new NativeList<AudioEnum>(audioSourceCapacity,Allocator.Persistent);

            ParticlePosList = new NativeList<float3>(particleCapacity, Allocator.Persistent);
            ParticleEnumList= new NativeList<ParticleEnum>(particleCapacity, Allocator.Persistent);
        }    

        public void SortPopupText()
        {
            for(int i = 1, n =  PopupTextDistanceSqList.Length; i < n; i++)
            {
                //Debug.Log("Sorting " + PopupTextDistanceSqList[i]);
                float baseDistanceSq = PopupTextDistanceSqList[i];
                float3 baseWorldPos = PopupTextWorldPosList[i];
                int baseValue = PopupTextValueList[i];
                int j = i - 1;
                while(j >= 0 && PopupTextDistanceSqList[j] > baseDistanceSq)
                {
                    PopupTextDistanceSqList[j + 1] = PopupTextDistanceSqList[j];
                    PopupTextWorldPosList[j + 1] = PopupTextWorldPosList[j];
                    PopupTextValueList[j + 1] = PopupTextValueList[j];
                    --j;
                }
                PopupTextDistanceSqList[j + 1] = baseDistanceSq;
                PopupTextWorldPosList[j + 1] = baseWorldPos;
                PopupTextValueList[j + 1] = baseValue;
            }
            //for(int i = 0, n = PopupTextDistanceSqList.Length;i < n; i++)
            //{
            //    Debug.Log("Sorted Idx:"+i+" - " + PopupTextDistanceSqList[i]);
            //}
        }
    }
}