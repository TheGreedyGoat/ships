using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] Transform follow;
    [SerializeField] float minHeight = 10;
    [SerializeField] float maxHeight = 15;

    [SerializeField] float scrollSpeed;

    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new(follow.position.x, 0, follow.position.z);
    }

    public void onZoom(InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();
        Vector3 camPos = cam.transform.localPosition;
        float y = Mathf.Clamp(camPos.y + value * scrollSpeed, minHeight, maxHeight);

        cam.transform.localPosition = new(camPos.x, y, camPos.z);
    }
}
