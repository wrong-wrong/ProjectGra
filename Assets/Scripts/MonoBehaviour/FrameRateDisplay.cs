using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class FrameRateDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI fpsText;
    [SerializeField] float updateInterval;
    private StringBuilder strBuilder;
    private int frameCounter;
    private float timer;
    private void Awake()
    {
        strBuilder = new StringBuilder(9);
    }
    // Update is called once per frame
    void Update()
    {
        ++frameCounter;
        if((timer += Time.deltaTime) > updateInterval)
        {
            timer = 0;
            int fps = (int)(frameCounter / updateInterval);
            fpsText.text = strBuilder.Append(fps).ToString();
            strBuilder.Clear();
            frameCounter = 0;
        }
    }
}
