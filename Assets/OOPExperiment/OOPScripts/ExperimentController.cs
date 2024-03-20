using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentController : MonoBehaviour
{
    public bool isCameraEnable;
    public Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            isCameraEnable = !isCameraEnable;
            mainCamera.enabled = isCameraEnable;
        }
    }
}
