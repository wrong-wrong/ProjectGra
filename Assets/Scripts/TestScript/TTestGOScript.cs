using UnityEngine;
using UnityEngine.UI;

public class TTestGOScript : MonoBehaviour
{
    [SerializeField] GameObject canvasGO;
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log(gameObject.name);
        var list = gameObject.GetComponentsInChildren<Image>();
        for (int i = 0; i < list.Length; i++)
        {
            Debug.Log(list[i].name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
