
using System.Threading;
using NUnit.Framework;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class WeatherManager : MonoSingleton<WeatherManager>
{
    // public static WeatherManager instance;
    float windChangeSeconds = 5;
    bool isTurning = false;
    float timePassed = 100;
    float minWindSpeed = 0;
    float maxWindSpeed = 6;


    [SerializeField]
    float maxAngularWindSpeed = 10f;
    [SerializeField] float maxWindStrengthDelta = 10f;
    [SerializeField]
    bool constantWind = false;




    private float t = 0;

    private float currentAngle = 0;
    private float currentStrength = 3;


    public Vector3 currentWind
    {
        get
        {
            return Quaternion.Euler(new(0, currentAngle, 0)) * Vector3.forward * currentStrength;
        }
    }


    private Vector3 targetWind
    {
        get
        {
            return Quaternion.Euler(new(0, targetAngle, 0)) * Vector3.forward * targetStrength;
        }
    }
    private float targetAngle;
    private float targetStrength;
    void Start()
    {
        ReRollWind();
    }

    void ReRollWind()
    {
        targetAngle = Random.Range(-180, 180);
        targetStrength = RollForWindStrength();

    }

    void Update()
    {
        if (isTurning)
        {
            UpdateWind();
        }
        else
        {
            CountDown();
        }
    }

    void UpdateWind()
    {
        isTurning = false;
        float angleDelta = targetAngle - currentAngle;

        if (Mathf.Abs(angleDelta) >= 0.001)
        {

            float maxDelta = maxAngularWindSpeed * Time.deltaTime;
            currentAngle += Mathf.Clamp(angleDelta, -maxDelta, maxDelta);
            isTurning = true;
        }
        float strengthDelta = targetStrength - currentStrength;
        if (Mathf.Abs(strengthDelta) >= 0.001)
        {
            float maxDelta = maxWindStrengthDelta * Time.deltaTime;
            float appliedDelta = Mathf.Clamp(strengthDelta, -maxDelta, maxDelta);
            currentStrength += appliedDelta;
            isTurning = true;
        }
    }

    void CountDown()
    {
        timePassed += Time.deltaTime;
        if (timePassed >= windChangeSeconds)
        {
            timePassed = 0;
            isTurning = true;
            ReRollWind();
        }
    }


    float RollForWindStrength()
    {
        return (Random.Range(maxWindSpeed, maxWindSpeed) + Random.Range(minWindSpeed, maxWindSpeed)) * 0.5f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.teal;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + currentWind + Vector3.up);
        Gizmos.color = Color.mediumVioletRed;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + targetWind + Vector3.up);
    }

}
