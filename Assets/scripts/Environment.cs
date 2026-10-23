
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager instance;
    float windChangeSeconds = 5;
    float targetAngle = 60f;
    bool isTurning = false;
    float timePassed = 0;
    float minWindSpeed = 0;
    float maxWindSpeed = 6;

    float windStrength = 3;

    [SerializeField]
    float windchangeSpeed = 0.01f;
    [SerializeField]
    bool constantWind = false;


    [Header("Wave Settings")]


    [SerializeField]
    [Range(0.1f, 50f)]
    float waveLengthA = 10;
    [SerializeField]
    [Range(0, 2)]
    float amplitudeA = 1;

    [SerializeField]
    [Range(0.1f, 15f)]
    float waveLengthB = 10;
    [SerializeField]
    [Range(0, 1)]
    float amplitudeB = 1;


    [SerializeField]
    [Range(0.1f, 5f)]
    float waveLengthC = 10;
    [SerializeField]
    [Range(0, 0.5f)]
    float amplitudeC = 1;

    [SerializeField]
    float positionWeight = 50;


    float offsetA;
    float offsetB;
    float offsetC;

    public Vector3 wind = Vector3.zero;
    const float Tau = Mathf.PI * 2;


    void Start()
    {
        instance = this;
        UpdateWind();
        offsetA = Random.Range(0, Tau);
        offsetB = Random.Range(0, Tau);
        offsetC = Random.Range(0, Tau);
    }
    // Update is called once per frame
    void Update()
    {
        if (constantWind) return;
        if (isTurning)
        {
            UpdateRotation();
        }
        else
        {

            timePassed += Time.deltaTime;
            if (timePassed >= windChangeSeconds)
            {
                targetAngle = Random.value * 360f - 180f;
                isTurning = true;
            }
        }
    }
    void UpdateRotation()
    {
        float deltaAngle = targetAngle - transform.eulerAngles.y - 180f;
        if (deltaAngle > 180) { deltaAngle -= 360f; }
        if (deltaAngle < -180) { deltaAngle += 360f; }

        float maxStep = Time.deltaTime * windchangeSpeed;


        transform.Rotate(new Vector3(0, Mathf.Clamp(deltaAngle, -maxStep, maxStep), 0));
        UpdateWind();
        if (Mathf.Abs(deltaAngle) <= 0.1)
        {
            isTurning = false;
            timePassed = 0;
            Debug.Log(transform.eulerAngles);
        }
    }

    void UpdateWind()
    {
        wind = transform.forward * windStrength;
    }


    public float GetWaterLevel(Vector3 position)
    {

        return GetSine(amplitudeA, waveLengthA, offsetA + position.x * 1 / positionWeight);
    }

    public float GetSine(float ampl, float wL, float offset)
    {
        return Mathf.Sin(offset + Time.time * 1 / wL) * ampl;
    }
}
