using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class PopupTextSingleUnit : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] float disappearScale;
    [SerializeField] float showingTime;
    private float timer;
    public bool IsActived;


    public void DoInitAtScreenPos(Vector3 screenPos, int val, float fontSize)
    {
        rectTransform.position = screenPos;
        rectTransform.localScale = Vector3.one;
        DoInit(val,fontSize);
    }
    private void DoInit(int val, float fontSize)
    {
        text.text = val.ToString();
        text.fontSize = fontSize;
        timer = showingTime;
        IsActived = true;
    }
    public bool DoUpdateAndCheckIsActive(float deltatime)
    {
        if ((timer -= deltatime) > 0f)
        {
            rectTransform.localScale = Vector3.one * (disappearScale + timer / showingTime * (1 - disappearScale));
            //rectTransform.position += Vector3.one * math.sin(math.radians(ratio * 90));
            rectTransform.position += Vector3.one;
            return true;
        }
        DoHide();
        return false;
    }
    public void DoHide()
    {
        rectTransform.localScale = Vector3.zero;
        IsActived = false;
    }
    
}
