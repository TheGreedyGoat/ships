using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public static class PreAim
{
    public static Vector3 PredictPreaimPosition(
        Transform shooter,
        Transform target,
        Vector3 targetVelocity,
        float bulletSpeed,
        int iterations
    )
    {
        Vector3 futureTargetPos = target.transform.position;
        if (targetVelocity != Vector3.zero)
            for (int i = 0; i < iterations; i++)
            {
                float timeToTarget =
                    Vector3.Distance(futureTargetPos, shooter.position)
                    / bulletSpeed;
                futureTargetPos =
                    timeToTarget * targetVelocity + target.transform.position;
            }

        return futureTargetPos;
    }
}
