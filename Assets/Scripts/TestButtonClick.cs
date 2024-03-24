using UnityEngine;
using UnityEngine.UI;

public class TestButtonClick : MonoBehaviour
{
    [SerializeField] Button button;
    public void Awake()
    {
        button.onClick.AddListener(OnClicked);
    }

    public void OnDisable()
    {
        button.onClick.RemoveListener(OnClicked);
    }
    public void OnClicked()
    {
        Debug.Log("Button Clicked");
    }
}
