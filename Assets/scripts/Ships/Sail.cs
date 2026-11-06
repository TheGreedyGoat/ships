using UnityEngine;
using UnityEngine.InputSystem;

public class Sail : MonoBehaviour
{
    public Transform sailTransform;

    float localAngleLimitDeg = 80;
    WeatherManager environment => WeatherManager.Instance;

    Transform ship;

    [SerializeField]
    float rotationSpeedDegPSec = 10;

    // -1 => reverse
    // 0 => no sail
    // 1 => half sail
    // 2 => full sail
    private float _sailState = 0;

    public float sailState
    {
        get => _sailState;
        set
        {
            _sailState = Mathf.Clamp(value, -1, 2);
            ApplySailsState();
        }
    }
    public float sailFactor
    {
        get => _sailState * 0.5f;
    }

    private float targetAngle;
    private int frameCounter = 0;
    private int framesPerUpdate = 10;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ship = transform.parent;
        CalculateRotation();
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCounter >= framesPerUpdate)
        {
            CalculateRotation();
            frameCounter = 0;
        }

        Rotate();

        frameCounter++;
    }

    void CalculateRotation()
    {
        Vector3 wind = environment.currentWind;
        float rawAngle = Vector3.SignedAngle(ship.forward, wind, Vector3.up);
        targetAngle = Mathf.Clamp(
            rawAngle,
            -localAngleLimitDeg,
            localAngleLimitDeg
        );

        // transform.localEulerAngles = new Vector3(0, clampedAngle, 0);
    }

    void Rotate()
    {
        float y = Mathf.MoveTowardsAngle(
            transform.localEulerAngles.y,
            targetAngle,
            rotationSpeedDegPSec * Time.deltaTime
        );
        transform.localRotation = Quaternion.Euler(0f, y, 0f);
    }

    void ApplySailsState()
    {
        sailTransform.localScale = new(
            1,
            Mathf.Clamp(Mathf.InverseLerp(0, 2, sailState), 0.05f, 1),
            1
        );
    }

    public void SailsUpDown(float value)
    {
        sailState +=
            value * GlobalValues.Instance.SailStateChangeRate * Time.deltaTime;
    }

    // public void AddSail(InputAction.CallbackContext _)
    // {
    //     sailState += GlobalValues.Instance.SailStateChangeRate;
    // }

    // public void RemoveSail(InputAction.CallbackContext _)
    // {
    //     sailState -= GlobalValues.Instance.SailStateChangeRate;
    // }
}
