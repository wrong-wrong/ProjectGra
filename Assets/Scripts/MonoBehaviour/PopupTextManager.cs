using ProjectGra;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PopupTextManager : MonoBehaviour
{
    public int MaxPopupTextCount;
    [SerializeField] float maxShowingPopupTextDistance;
    //[SerializeField] int val;
    [SerializeField] GameObject singlePopupTextPrefab;
    [SerializeField] Transform Canvas;
    //public Image TestImage;
    private List<PopupTextSingleUnit> popupTextRingList;
    private int ringListHead;
    private int ringListTail;
    private int ringListLength;
    public static PopupTextManager Instance;
    private Camera mainCam;
    //private Transform camTransform;
    private float maxDistanceSq;
    private void Awake()
    {
        maxDistanceSq = maxShowingPopupTextDistance * maxShowingPopupTextDistance;
        mainCam = Camera.main;
        //camTransform = mainCam.transform;
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        popupTextRingList = new List<PopupTextSingleUnit>(MaxPopupTextCount);
        for (int i = 0; i < MaxPopupTextCount; ++i)
        {
            var com = Object.Instantiate(singlePopupTextPrefab, Canvas).GetComponent<PopupTextSingleUnit>();
            com.DoHide();
            popupTextRingList.Add(com);
        }
        ringListLength = popupTextRingList.Count;
        ringListHead = 0;
        ringListTail = 0;
    }
    void Update()
    {
        var disSqList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextDistanceSqList;
        if (disSqList.Length != 0)
        {
            var posList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextWorldPosList;
            var valList = EffectRequestSharedStaticBuffer.SharedValue.Data.PopupTextValueList;
            //Debug.Log("Buffer length :" + disSqList.Length);
            for (int i = 0, n = disSqList.Length; i < n; ++i)
            {
                if (disSqList[i] > maxDistanceSq)
                {
                    //Debug.Log("Too Far for text");
                    break;
                }
                if (!RequirePopupTextAt(posList[i], valList[i], disSqList[i]))
                {
                    //Debug.Log("Break At requesting popup text");
                    break;
                }
            }
            disSqList.Clear();
            posList.Clear();
            valList.Clear();
        }
        if (ringListHead == ringListTail) return;
        //Debug.Log(ringListHead + " - " + ringListTail);
        var deltatime = Time.deltaTime;
        for (int i = ringListHead, j = ringListTail; i != j; i = (i + 1) % ringListLength)
        {
            //Debug.Log("ad");
            if (!popupTextRingList[i].DoUpdateAndCheckIsActive(deltatime))
            {
                ringListHead = (ringListHead + 1) % ringListLength;
            }
        }

    }
    public bool RequirePopupTextAt(Vector3 pos, int val, float disSq)
    {
        //Debug.Log("RequirePopupTextAt - called at popManager");
        if ((ringListTail + 1) % ringListLength != ringListHead )
        {
            float fontsize = 60f;
            if (disSq > 64f) fontsize = 32f;
            var screenPos = mainCam.WorldToScreenPoint(pos);
            popupTextRingList[ringListTail].DoInitAtScreenPos(screenPos, val, fontsize);
            ringListTail = (ringListTail + 1) % ringListLength;
            return true;
        }
        else
        {
            //Debug.Log("No available popup text");
            return false;
        }
    }
}
