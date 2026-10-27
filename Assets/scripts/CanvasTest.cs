using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Create a Canvas that holds a Text GameObject.

public class CanvasTest : MonoBehaviour
{
    [SerializeField]
    RectTransform cTransform;
    [SerializeField] Transform environment;

    [SerializeField]
    Transform cameraYaw;
    void Update()
    {

        float angle = Vector3.SignedAngle(WeatherManager.Instance.currentWind, cameraYaw.forward, Vector3.down);//(environment.eulerAngles - cameraYaw.eulerAngles).y;

        cTransform.eulerAngles = new Vector3(0, 0, -angle + 180);
    }
}