using NUnit.Framework.Constraints;
using UnityEngine;

public class NPCCore : MonoBehaviour
{
    [SerializeField]
    float propulsion = 2;

    [SerializeField]
    float yawGain = 0.5f;

    [SerializeField]
    Ship ship;

    [SerializeField]
    float maxYawSpeed;

    [SerializeField, Min(1)]
    float rateErrorForFullSteer;

    ShipMovement movement;
    Sail sail;

    Rigidbody body;

    NPCBehaviour Behaviour;

    NPCCannons CannonController => GetComponentInChildren<NPCCannons>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movement = GetComponent<ShipMovement>();
        sail = GetComponentInChildren<Sail>();
        ship = GetComponent<Ship>();
        body = GetComponent<Rigidbody>();
        if (CannonController == null)
        {
            GetComponentInChildren<ShipCannons>()
                .gameObject.AddComponent<NPCCannons>();
        }
    }

    void FixedUpdate()
    {
        if (sail == null)
            sail = GetComponentInChildren<Sail>();
        if (movement == null)
            movement = GetComponentInChildren<ShipMovement>();
        if (Behaviour == null)
        {
            Behaviour = GetComponent<NPCBehaviour>();
            if (Behaviour == null)
                return;
        }
        var desHeading = Behaviour.GetDesiredHeading();
        SteerTowardsHeading(desHeading);

        Debug.DrawLine(transform.position, transform.position + desHeading * 5);
    }

    void SteerTowardsHeading(Vector3 targetHeading)
    {
        Vector2 toTarged2D = new(targetHeading.x, targetHeading.z);
        Vector2 forward = new(transform.forward.x, transform.forward.z);

        float deltaAngle = Vector2.SignedAngle(forward, toTarged2D);
        int sailState = Mathf.Abs(deltaAngle) > 30 ? 1 : 2;
        SetSailState(sailState);

        float currentYawSpeed = Mathf.Rad2Deg * body.angularVelocity.y;
        float desiredYawSpeed = -Mathf.Clamp(
            deltaAngle * yawGain,
            -maxYawSpeed,
            maxYawSpeed
        );

        float yawError = desiredYawSpeed - currentYawSpeed;

        float steerValue = Mathf.Clamp(yawError / rateErrorForFullSteer, -1, 1);
        Steer(steerValue);
    }

    void SetSailState(int state)
    {
        sail.sailState = state;
    }

    void Steer(float direction)
    {
        movement.Steer(direction);
    }

    public void UnassignNPC()
    {
        if (Behaviour != null)
        {
            ObjectDestroyer.Destroy(Behaviour);
        }
        ObjectDestroyer.Destroy(this);
    }
}
