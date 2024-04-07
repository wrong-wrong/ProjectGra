using ProjectGra;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public ItemFoundUIManager itemFoundUI;
    public Button buttonOne;
    public Button buttonTwo;
    public bool flag;
    public RectTransform rectTransform;
    public List<WeaponSlot> wpSlots;
    public void Awake()
    {
        buttonOne.onClick.AddListener(RerollItemFound);   
        buttonTwo.onClick.AddListener(InitAllSlot);
    }
    public void OnDestroy()
    {
        buttonOne.onClick.RemoveAllListeners();
        buttonTwo.onClick.RemoveAllListeners();
    }

    private void RerollItemFound()
    {
        itemFoundUI.Reroll();
    }
    //private void FlipActive()
    //{
    //    if(flag)
    //    {
    //        rectTransform.localScale = Vector3.one;
    //    }
    //    else
    //    {
    //        rectTransform.localScale = Vector3.zero;
    //    }
    //    flag = !flag;
    //}


    private void InitAllSlot()
    {
        for(int i = 0, n =  wpSlots.Count; i < n; ++i)
        {
            wpSlots[i].InitSlot();
        }
    }
}
