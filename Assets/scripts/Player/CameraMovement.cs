using UnityCommunity.UnitySingleton;
using UnityEngine;

public class CameraMovement : MonoSingleton<CameraMovement>
{
    Transform yaw
    {
        get => transform;
    }
    Transform pitch;
    Transform zoom;

    Transform player
    {
        get => Player.Instance.transform;
    }

    [SerializeField]
    float minZoom = 6;

    [SerializeField]
    float maxZoom = 25;

    [SerializeField]
    float zoomSpeed = 2;

    [SerializeField]
    float yawRotationSpeed = 20;

    [SerializeField]
    [Range(0, 45)]
    float minPitchAngleDeg = 10;

    [SerializeField]
    [Range(45, 90)]
    float maxPitchAngleDeg = 85;

    [SerializeField]
    float pitchRotationSpeed = 1;

    public float CurrentYaw => yaw.eulerAngles.y;

    public float CurrentPitch => pitch.localEulerAngles.x;

    public Vector3 CurrentEulers
    {
        get { return new(CurrentPitch, CurrentYaw, 0); }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        pitch = transform.GetChild(0).transform;
        zoom = pitch.GetComponentInChildren<Camera>().transform;
    }

    void Update()
    {
        var player = Player.Instance.ship;
        if (player == null)
            return;
        transform.position =
            player.transform.position + new Vector3(0, player.CannonDecks, 0);
    }

    public void RecieveRotationInput(Vector2 value)
    {
        Yaw(value.x);
        Pitch(value.y);
    }

    public void RecieveZoomInput(float value) => Zoom(value);

    void Yaw(float value)
    {
        yaw.Rotate(new(0, value * yawRotationSpeed * Time.deltaTime, 0));
    }

    void Pitch(float value)
    {
        float currentPitch = pitch.localEulerAngles.x;
        float newPitch = Mathf.Clamp(
            currentPitch + value * pitchRotationSpeed * Time.deltaTime,
            minPitchAngleDeg,
            maxPitchAngleDeg
        );
        pitch.transform.localEulerAngles = new(newPitch, 0, 0);
    }

    void Zoom(float value)
    {
        float currentZoom = zoom.localPosition.z;
        float newZoom = Mathf.Clamp(
            currentZoom + value * zoomSpeed * Time.deltaTime,
            -maxZoom,
            -minZoom
        );
        zoom.localPosition = new(0, 0, newZoom);
    }
}
