using System;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCCore : MonoBehaviour
{
    float yawGain = 0.5f;

    Ship ship => GetComponent<Ship>();

    float maxYawSpeed = 20;

    float rateErrorForFullSteer = 40;

    ShipMovement movement;
    Sail sail => GetComponentInChildren<Sail>();

    Rigidbody body => GetComponent<Rigidbody>();

    NPCBehaviour Behaviour => GetComponent<NPCBehaviour>();

    NPCCannons CannonController => GetComponentInChildren<NPCCannons>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Setup<BehaviourType>()
        where BehaviourType : NPCBehaviour
    {
        movement = GetComponent<ShipMovement>();
        gameObject.AddComponent<BehaviourType>();
        if (CannonController == null)
        {
            GetComponentInChildren<ShipCannons>()
                .gameObject.AddComponent<NPCCannons>();
        }
    }

    void FixedUpdate()
    {
        if (movement == null)
            movement = GetComponentInChildren<ShipMovement>();
        if (Behaviour == null)
        {
            if (Behaviour == null)
                return;
        }

        ShowPreaim(Behaviour.Target);
        SteerTowardsHeading(Behaviour.DesiredHeading);

        // Debug.DrawLine(
        //     transform.position,
        //     transform.position + Behaviour.DesiredHeading * 5
        // );
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

    public void SetSailState(int state)
    {
        sail.sailState = state;
    }

    public void Steer(float direction)
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

    void ShowPreaim(Transform target)
    {
        if (target == null || target.GetComponent<Rigidbody>() == null)
            return;
        print($"Target: {target}");
        print($"CannonStats: {ship.CannonStats}");
        Debug.DrawLine(
            transform.position,
            PreAim.PredictPreaimPosition(
                transform,
                target,
                target.GetComponent<Rigidbody>().linearVelocity,
                ship.CannonStats.CannonBallSpeed,
                5
            ),
            Color.green
        );
    }
}
