using UnityEngine;

public class NSCShip : MonoBehaviour
{
    [SerializeField]
    Transform targetTransform;

    [SerializeField]
    float propulsion = 2;

    [SerializeField]
    float yawGain = 0.5f;

    [SerializeField, Range(0, 100)]
    float DesiredDistance = 0.5f;

    [SerializeField, Range(0, 100)]
    float DistTolerance = 20;

    float distToTarget =>
        (transform.position - targetTransform.position).magnitude;
    bool isWithinTolerance
    {
        get
        {
            var dist = distToTarget;
            return dist > DesiredDistance - DistTolerance
                && dist < DesiredDistance + DistTolerance;
        }
    }

    [SerializeField]
    Ship ship;

    [SerializeField]
    float maxYawSpeed;

    [SerializeField, Min(1)]
    float rateErrorForFullSteer;

    ShipMovement movement;
    Sail sail;

    Rigidbody body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        movement = GetComponent<ShipMovement>();
        sail = GetComponentInChildren<Sail>();
        ship = GetComponent<Ship>();
        body = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (sail == null)
            sail = GetComponentInChildren<Sail>();
        if (movement == null)
            movement = GetComponentInChildren<ShipMovement>();

        if (isWithinTolerance)
        {
            SteerTowards(targetTransform.forward);
        }
        else
        {
            Vector3 toTargetCenter =
                targetTransform.position - transform.position;
            SteerTowards(toTargetCenter.normalized * DesiredDistance);
        }
    }

    void SteerTowards(Vector3 target)
    {
        Vector2 toTarged2D = new(target.x, target.z);
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

        float yawError = (desiredYawSpeed - currentYawSpeed);

        float steerValue = Mathf.Clamp(yawError / rateErrorForFullSteer, -1, 1);
        Steer(steerValue);

        Debug.DrawLine(transform.position, transform.position + target);
    }

    void SetSailState(int state)
    {
        sail.sailState = state;
    }

    void Steer(float direction)
    {
        movement.Steer(direction);
    }

    void OnDrawGizmos()
    {
        if (targetTransform == null)
            return;

        Gizmos.color = isWithinTolerance ? Color.green : Color.red;
        Gizmos.DrawWireSphere(
            targetTransform.position,
            DesiredDistance - DistTolerance
        );
        Gizmos.DrawWireSphere(targetTransform.position, DesiredDistance);
        Gizmos.DrawWireSphere(
            targetTransform.position,
            DesiredDistance + DistTolerance
        );
    }
}
