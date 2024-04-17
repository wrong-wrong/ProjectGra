using ProjectGra;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TestMonoLaucher : MonoBehaviour
{

    public static TestMonoLaucher Instance;
    // Start is called before the first frame update

    //// test melee weapon sweep
    public Transform PlayerModel;
    public float ForwardSpeed;
    public float ForwardRange;
    public float HalfWidth;

    //// test flash color
    //public Color managedColor;
    //public float4 mycustomColor;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        var testsystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TestingSystemInRealTestScene>();
        World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(testsystem);
        
    }

    // Update is called once per frame
    void Update()
    {
        #region custom color test
        //if (!Input.GetKeyUp(KeyCode.Space)) return;
        //Debug.Log(managedColor.r + ", " + managedColor.g + ", " + managedColor.b);
        //mycustomColor = new float4(managedColor.r, managedColor.g, managedColor.b, managedColor.a);
        #endregion

        #region effect request test
        //if (!Input.GetKeyUp(KeyCode.T)) return;

        //var disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        //var posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
        //var valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
        //for(int i = 0, n = disSqList.Length; i < n; i++)
        //{
        //    Debug.Log("Getting :" + disSqList[i] + " - " + posList[i] + " - " + valList[i]);
        //}
        #endregion
    }
}
