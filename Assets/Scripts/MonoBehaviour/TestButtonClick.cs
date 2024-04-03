using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestButtonClick : MonoBehaviour,IPointerEnterHandler,IBeginDragHandler,IEndDragHandler,IDragHandler
{
    public Image image;
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag : " + gameObject.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        image.color = Color.red;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.color = Color.green;
        Debug.Log("OnEndDrag : " + gameObject.name);
    }

    //[SerializeField] Button button;
    //public void Awake()
    //{
    //    button.onClick.AddListener(OnClicked);
    //}

    //public void OnDisable()
    //{
    //    button.onClick.RemoveListener(OnClicked);
    //}
    //public void OnClicked()
    //{
    //    Debug.Log("Button Clicked");
    //}

    public void OnPointerEnter(PointerEventData ptrData)
    {
        Debug.Log(gameObject.name);
    }
}
