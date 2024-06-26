using ProjectGra;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;
public class PopupTextManager : MonoBehaviour
{
    public static PopupTextManager Instance;
    //public bool IsInitialized;
    public int MaxPopupTextCount;
    [SerializeField] string DodgeString;
    [SerializeField] Vector3 DodgeStringPos;
    [SerializeField] float DodgeStringFontSize;
    [SerializeField] Vector3 DodgeStringDir;
    [SerializeField] string LifeStealString;
    [SerializeField] Vector3 LifeStealStringPos;
    [SerializeField] float LifeStealStringFontSize;
    [SerializeField] Vector3 LifeStealStringDir;
    [SerializeField] float maxShowingPopupTextDistance;
    //[SerializeField] int val;
    [SerializeField] GameObject singlePopupTextPrefab;
    [SerializeField] Transform Canvas;
    //public Image TestImage;
    private List<PopupTextSingleUnit> popupTextRingList;
    private List<Vector3> posRingList;
    private int ringListHead;
    private int ringListTail;
    private int ringListLength;
    private Camera mainCam;
    //private Transform camTransform;
    private float maxDistanceSq;
    private Random random;
    private void Awake()
    {
        //int tmpVal = 5;
        //Debug.Log(8 * ++tmpVal);
        maxDistanceSq = maxShowingPopupTextDistance * maxShowingPopupTextDistance;
        mainCam = Camera.main;
        //camTransform = mainCam.transform;
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        random = Random.CreateFromIndex(0);
        popupTextRingList = new List<PopupTextSingleUnit>(MaxPopupTextCount);
        posRingList = new List<Vector3>(MaxPopupTextCount);
        for (int i = 0; i < MaxPopupTextCount; ++i)
        {
            var com = Object.Instantiate(singlePopupTextPrefab, Canvas).GetComponent<PopupTextSingleUnit>();
            com.DoHide();
            popupTextRingList.Add(com);
            posRingList.Add(Vector3.zero);
        }
        ringListLength = popupTextRingList.Count;
        ringListHead = 0;
        ringListTail = 0;
        this.enabled = false;
        Debug.LogWarning("Current popup text update logic only works when all text have the same showing time");
        //Debug.Log("Awake");
    }
    //private void OnEnable()
    //{
    //    Debug.Log("OnEnable");
    //}
    //private void Start()
    //{
    //    Debug.Log("Start");
    //}
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
            var screenPos = mainCam.WorldToScreenPoint(posRingList[i]);
            if (!popupTextRingList[i].DoUpdateAndCheckIsActiveWithScreenPos(screenPos, deltatime))
            {
                ringListHead = (ringListHead + 1) % ringListLength;
            }
        }

    }
    public bool RequirePopupTextAt(Vector3 pos, int val, float disSq)
    {
        if ((ringListTail + 1) % ringListLength == ringListHead) return false;
        if (disSq >= 0)
        {
            return RequireDamageText(pos, val, disSq);
        }
        if (disSq == -1)
        {
            RequireLifeStealText();
            return true;
        }
        if (disSq == -2)
        {
            RequireDodgeText();
            return true;
        }
        Debug.LogError("Exception in RequirePopupText - No valid disSq value");
        return true;
    }
    private bool RequireDamageText(Vector3 pos, int val, float disSq)
    {
        var f2 = math.normalize(random.NextFloat2());
        float fontsize = 60f;
        if (disSq > 64f) fontsize = 32f;
        var screenPos = mainCam.WorldToScreenPoint(pos);
        popupTextRingList[ringListTail].DoInitAtScreenPos(screenPos, val, fontsize, new Vector3(f2.x, f2.y, 0));
        posRingList[ringListTail] = pos;
        ringListTail = (ringListTail + 1) % ringListLength;
        return true;
    }
    private void RequireDodgeText()
    {
        popupTextRingList[ringListTail].DoInitShowDodge(DodgeStringPos, DodgeString, DodgeStringFontSize, DodgeStringDir);
        posRingList[ringListTail] = DodgeStringPos;
        ringListTail = (ringListTail + 1) % ringListLength;
    }

    private void RequireLifeStealText()
    {
        popupTextRingList[ringListTail].DoInitShowLifeSteal(LifeStealStringPos, LifeStealString, LifeStealStringFontSize, LifeStealStringDir);
        posRingList[ringListTail] = LifeStealStringPos;
        ringListTail = (ringListTail + 1) % ringListLength;
    }
}
