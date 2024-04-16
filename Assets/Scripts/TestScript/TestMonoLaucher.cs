using ProjectGra;
using Unity.Entities;
using UnityEngine;

public class TestMonoLaucher : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var testsystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TestingSystemInRealTestScene>();
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(testsystem);
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (!Input.GetKeyUp(KeyCode.T)) return;

        //var disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        //var posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
        //var valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        //for(int i = 0, n = disSqList.Length; i < n; i++)
        //{
        //    Debug.Log("Getting :" + disSqList[i] + " - " + posList[i] + " - " + valList[i]);
        //}
    }
}
