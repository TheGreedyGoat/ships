using UnityEngine;
using UnityEngine.InputSystem;

public class Sail : MonoBehaviour
{

    public MeshRenderer sailRenderer;
    public Transform sailTransform;

    float localAngleLimitDeg = 80;
    WeatherManager environment;
    [SerializeField]
    Transform ship;

    [SerializeField]
    float rotationSpeedDegPSec = 10;

    // -1 => reverse
    // 0 => no sail
    // 1 => half sail
    // 2 => full sail
    private int _sailState = 0;

    public int sailState
    {
        get => _sailState;
        set
        {
            _sailState = Mathf.Clamp(value, -1, 2);
            ApplySailsState();
        }
    }
    public float sailFactor { get => _sailState * 0.5f; }

    private float targetAngle;
    private int frameCounter = 0;
    private int framesPerUpdate = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        environment = FindAnyObjectByType<WeatherManager>();
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
        targetAngle = Mathf.Clamp(rawAngle, -localAngleLimitDeg, localAngleLimitDeg);

        // transform.localEulerAngles = new Vector3(0, clampedAngle, 0);
    }

    void Rotate()
    {
        float y = Mathf.MoveTowardsAngle(transform.localEulerAngles.y, targetAngle, rotationSpeedDegPSec * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(0f, y, 0f);
    }



    void ApplySailsState()
    {
        sailRenderer.gameObject.SetActive(true);
        switch (_sailState)
        {
            case 0:
            case -1:
                sailRenderer.gameObject.SetActive(false);
                break;
            case 1:

                sailTransform.localScale = new Vector3(10f, 0.3f, 0.5f);
                break;
            case 2:
                sailTransform.localScale = new Vector3(10f, 0.6f, 0.5f);
                break;
            default:
                break;
        }

    }


    public void AddSail(InputAction.CallbackContext _)
    {
        sailState++;
    }

    public void RemoveSail(InputAction.CallbackContext _)
    {
        sailState--;
    }



}
