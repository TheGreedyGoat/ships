
using UnityEngine;
using UnityEngine.Video;

public class EnvironmentManager : MonoBehaviour
{
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

    public Vector3 wind = Vector3.zero;

    void Start()
    {
        UpdateWind();
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

}
