using ProjectGra;
using UnityEngine;

public class TestMovingLittleCube : MonoBehaviour
{
    [SerializeField] int val;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            var worldPos = gameObject.transform.position;
            PopupTextManager.Instance.RequirePopupTextAt(worldPos, val,0);
            AudioManager.Instance.RequireAudioPlayedAt(worldPos);
        }
    }
}
