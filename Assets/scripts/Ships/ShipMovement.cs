using System;
using UnityEngine;

[RequireComponent(typeof(Ship))]
public class ShipMovement : MonoBehaviour
{


    [SerializeField]
    WeatherManager environment;
    [SerializeField]
    float reverseForce = 0.2f;
    private Rigidbody rb;

    Sail sail;
    int sailState { get => sail.sailState; }

    ShipStatBlock stats { get => GetComponent<Ship>().stats; }




    [Header("Debug")]
    // Debug
    [SerializeField]
    float vectorScaleFactor = 100;
    [SerializeField]
    private Vector3 totalForce;

    [SerializeField]
    [Range(5, 30)]
    float minPropulsionAngle = 15;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        sail = GetComponentInChildren<Sail>();
        transform.position = new(transform.position.x, WaveValues.Instance.GetHeight(transform.position), transform.position.z);


    }


    void FixedUpdate()
    {
        totalForce = Vector3.zero;
        Friction();
        Move();

        Debug.DrawLine(transform.position, transform.position + totalForce * 3, Color.crimson);

        DrawPropulsionCircle();

    }



    void Move()
    {
        switch (sailState)
        {
            case -1:
                AddForce(-transform.forward * reverseForce);
                return;
            case 0:
                return;
            default:
                break;
        }
        Vector3 wind = environment.currentWind;
        Vector3 heading = transform.forward;

        float relativeAngle = Vector3.SignedAngle(heading, wind, Vector3.up);
        float perc = Mathf.Clamp(Mathf.InverseLerp(a: 180f, b: minPropulsionAngle, value: relativeAngle), 0.0f, 1.0f);
        Vector3 accelerationForce = heading * sail.sailFactor * stats.acceleration * perc;

        AddForce(accelerationForce);
    }



    void Friction()
    {

        Vector3 velocity = rb.linearVelocity;
        Vector3 heading = transform.forward;
        Vector3 projection = Vector3.Project(velocity, heading);
        Vector3 lateralForce = -(velocity - projection) * stats.lateralFrictionFactor;
        Vector3 longitudalForce = -projection * stats.longitudalFrictionFactor;

        AddForce(lateralForce + longitudalForce);
        Debug.DrawLine(transform.position, transform.position + velocity * vectorScaleFactor, Color.red);
    }

    public void Steer(float steerValue)
    {
        rb.AddTorque(new Vector3(0, ApplySpeedToTorque(steerValue * stats.turningAgility) * (1.5f - sail.sailFactor), 0));

    }

    float ApplySpeedToTorque(float raw)
    {
        float sqrSpeed = rb.linearVelocity.magnitude;
        return raw * (Mathf.Sqrt(sqrSpeed) + 0.1f);
    }

    void AddForce(Vector3 force)
    {
        totalForce += force;
        rb.AddForce(force);
    }


    void DrawPropulsionCircle()
    {
        float radius = 5;
        float totaldegrees = 360f - 2 * minPropulsionAngle;
        float singleStep = totaldegrees / 30;


        float angleOffset = 90 - transform.eulerAngles.y;


        for (float a = minPropulsionAngle + angleOffset; a <= 360 - minPropulsionAngle + angleOffset; a += singleStep)
        {
            Vector3 start = transform.position
            + radius * new Vector3(Mathf.Cos(a / 180 * Mathf.PI), 0, Mathf.Sin(a / 180 * Mathf.PI));

            Vector3 end = transform.position
            + radius * new Vector3(Mathf.Cos((a + singleStep) / 180 * Mathf.PI), 0, Mathf.Sin((a + singleStep) / 180 * Mathf.PI));
            Debug.DrawLine(start, end, Color.red);

        }
    }

}
