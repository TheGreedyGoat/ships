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
    float minPitchAngleDegFP = -15;

    [SerializeField]
    [Range(45, 90)]
    float maxPitchAngleDegFP = 5;

    [SerializeField]
    float pitchRotationSpeed = 1;

    public bool IsFirstPerson => player.IsAiming;
    bool wasAimingLastFrame = false;
    private bool isInTransition = false;

    public float CurrentYaw => yaw.localEulerAngles.y;

    public float CurrentPitch => pitch.localEulerAngles.x;
    public float CurrentZoom => zoom.localPosition.z;

    private float zoomCache;

    private float transitionPitchTarget = 0;
    private float transitionZoomTarget = 0;

    [SerializeField, Range(0.1f, 360)]
    private float transitionPitchSpeed = 1;

    [SerializeField, Range(10, 1000)]
    private float transitionZoomSpeed = 1;

    private const float Inv180 = 1 / 180f;
    public int FacedBroadSide => 1 - Mathf.FloorToInt(CurrentYaw * Inv180);

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

        if (StayEven && !IsFirstPerson)
            ResetPitchAndRoll();
        if (isInTransition)
            Transition();
        if (IsFirstPerson)
            OnFirstPerson();
        else
            OnThirdPerson();

        wasAimingLastFrame = IsFirstPerson;
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

    void OnFirstPerson()
    {
        if (!wasAimingLastFrame)
        {
            zoomCache = CurrentZoom;

            transitionZoomTarget = 0;
            transitionPitchTarget = 0;

            isInTransition = true;
        }
        player.PlayerCannons.SetAimVisibility(FacedBroadSide, true);
        player.PlayerCannons.SetAimVisibility(1 - FacedBroadSide, false);
    }

    void OnThirdPerson()
    {
        if (wasAimingLastFrame)
        {
            player.PlayerCannons.SetAimVisibility(0, false);
            player.PlayerCannons.SetAimVisibility(1, false);
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
        if (IsFirstPerson || isInTransition)
            return;
        Zoom(value);
    }

    void Yaw(float value)
    {
        yaw.Rotate(new(0, value * yawRotationSpeed * Time.deltaTime, 0));
    }

    void Pitch(float delta)
    {
        float minPitchAngle = IsFirstPerson
            ? minPitchAngleDegFP
            : minPitchAngleDeg3rd;
        float maxPitchAngle = IsFirstPerson
            ? maxPitchAngleDegFP
            : maxPitchAngleDeg3rd;
        float desiredPitch = IsFirstPerson
            ? -player.Cannons.CurrentCannonPitch
            : CurrentPitch + delta * pitchRotationSpeed * Time.deltaTime;

        float newPitch = Mathf.Clamp(
            desiredPitch,
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
