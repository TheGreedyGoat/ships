using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class CameraMovement : MonoSingleton<CameraMovement>
{
    public bool StayEven = false;
    Transform yaw
    {
        get => transform;
    }
    Transform pitch;
    Transform zoom;

    Player player => Player.Instance;
    Transform playerTransform
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
    float minPitchAngleDeg3rd = 0;

    [SerializeField]
    [Range(45, 90)]
    float maxPitchAngleDeg3rd = 85;

    [SerializeField]
    [Range(0, 45)]
    float minPitchAngleDegAim = -15;

    [SerializeField]
    [Range(45, 90)]
    float maxPitchAngleDegAim = 5;

    [SerializeField]
    float pitchRotationSpeed = 1;

    bool IsAiming => player.IsAiming;
    bool wasAimingLastFrame = false;
    private bool isInTransition = false;

    public float CurrentYaw => yaw.eulerAngles.y;

    public float CurrentPitch => pitch.localEulerAngles.x;
    public float CurrentZoom => zoom.localPosition.z;

    private float zoomCache;

    private float transitionPitchTarget = 0;
    private float transitionZoomTarget = 0;

    [SerializeField, Range(0.1f, 360)]
    private float transitionPitchSpeed = 1;

    [SerializeField, Range(10, 1000)]
    private float transitionZoomSpeed = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        pitch = transform.GetChild(0).transform;
        zoom = pitch.GetComponentInChildren<Camera>().transform;
    }

    void Update()
    {
        var playerShip = Player.Instance.Ship;
        if (playerShip == null)
            return;

        if (StayEven && !IsAiming)
            ResetPitchAndRoll();
        if (isInTransition)
            Transition();
        if (IsAiming)
            OnAiming();
        else
            OnNotAiming();

        wasAimingLastFrame = IsAiming;
    }

    void Transition()
    {
        float zoomDeltaMax = transitionZoomSpeed * Time.deltaTime;
        float remainingZoomDelta = transitionZoomTarget - CurrentZoom;
        Vector3 targetPosition = new(
            0,
            0,
            CurrentZoom
                + Mathf.Clamp(remainingZoomDelta, -zoomDeltaMax, zoomDeltaMax)
        );
        zoom.localPosition = targetPosition;

        float pitchDeltaMax = transitionPitchSpeed * Time.deltaTime;
        float remainingPitchDelta = transitionPitchTarget - CurrentPitch;

        Vector3 targetEulers = new(
            CurrentPitch
                + Mathf.Clamp(
                    remainingPitchDelta,
                    -pitchDeltaMax,
                    pitchDeltaMax
                ),
            0,
            0
        );

        pitch.localEulerAngles = targetEulers;

        print($"{CurrentPitch}, {transitionPitchTarget}");

        if (
            Mathf.Abs(CurrentZoom - transitionZoomTarget) < 0.01f
            && Mathf.Abs(CurrentPitch - transitionPitchTarget) < 0.01f
        )
        {
            isInTransition = false;
        }
    }

    void OnAiming()
    {
        if (!wasAimingLastFrame)
        {
            zoomCache = CurrentZoom;

            transitionZoomTarget = 0;
            transitionPitchTarget = 0;

            isInTransition = true;
        }
    }

    void OnNotAiming()
    {
        if (wasAimingLastFrame)
        {
            transitionZoomTarget = zoomCache;
            transitionPitchTarget = CurrentPitch;

            isInTransition = true;
        }
    }

    void ResetPitchAndRoll()
    {
        // transform.eulerAngles = new(0, transform.eulerAngles.y, 0);
    }

    public void RecieveRotationInput(Vector2 value)
    {
        if (isInTransition)
            return;
        Yaw(value.x);
        Pitch(value.y);
    }

    public void RecieveZoomInput(float value)
    {
        if (IsAiming || isInTransition)
            return;
        Zoom(value);
    }

    void Yaw(float value)
    {
        yaw.Rotate(new(0, value * yawRotationSpeed * Time.deltaTime, 0));
    }

    void Pitch(float delta)
    {
        float minPitchAngle = IsAiming
            ? minPitchAngleDegAim
            : minPitchAngleDeg3rd;
        float maxPitchAngle = IsAiming
            ? maxPitchAngleDegAim
            : maxPitchAngleDeg3rd;

        float newPitch = Mathf.Clamp(
            CurrentPitch + delta * pitchRotationSpeed * Time.deltaTime,
            minPitchAngle,
            maxPitchAngle
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
