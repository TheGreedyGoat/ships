using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    Transform yaw { get => transform; }
    Transform pitch;
    Transform zoom;

    Transform player { get => ShipPlayerControls.Instance.transform; }

    [SerializeField] float minZoom = 6;
    [SerializeField] float maxZoom = 25;
    [SerializeField] float zoomSpeed = 2;
    [SerializeField] float yawRotationSpeed = 20;

    [SerializeField][Range(0, 45)] float minPitchAngleDeg = 10;
    [SerializeField][Range(45, 90)] float maxPitchAngleDeg = 85;

    [SerializeField] float pitchRotationSpeed = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        pitch = transform.GetChild(0).transform;
        zoom = pitch.GetComponentInChildren<Camera>().transform;
    }

    void Update()
    {
        transform.position = new(player.position.x, 0, player.position.z);
    }

    public void RecieveRotationInput(Vector2 value)
    {
        Yaw(value.x);
        Pitch(value.y);
    }

    public void RecieveZoomInput(float value)
    => Zoom(value);


    void Yaw(float value)
    {
        yaw.Rotate(new(0, value * yawRotationSpeed * Time.deltaTime, 0));
    }

    void Pitch(float value)
    {
        float currentPitch = pitch.localEulerAngles.x;
        float newPitch = Mathf.Clamp(currentPitch + value * pitchRotationSpeed * Time.deltaTime, minPitchAngleDeg, maxPitchAngleDeg);
        pitch.transform.localEulerAngles = new(newPitch, 0, 0);
    }

    void Zoom(float value)
    {
        float currentZoom = zoom.localPosition.z;
        float newZoom = Mathf.Clamp(currentZoom + value * zoomSpeed * Time.deltaTime, -maxZoom, -minZoom);
        zoom.localPosition = new(0, 0, newZoom);
    }


}
