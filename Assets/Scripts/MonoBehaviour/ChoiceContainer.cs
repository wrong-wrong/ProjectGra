using UnityEngine;

public class ChoiceContainer : MonoBehaviour
{
    [SerializeField] RectTransform containerRect;

    public void RePosition(float posX)
    {
        containerRect.localPosition = new Vector3(posX, containerRect.localPosition.y);
    }
    //public void Start()
    //{
    //    RePosition();
    //}
    //public void Update()
    //{
    //    if(Input.GetKeyUp(KeyCode.Space))
    //    {
    //        RePosition();
    //    }
    //}
}
