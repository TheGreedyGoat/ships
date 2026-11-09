using System;
using UnityEngine;

[Serializable]
public class DistanceRange
{
    [SerializeField, Min(0f)]
    float desiredDist;

    [SerializeField, Min(0f)]
    float tolerance;

    public float MinDistance => desiredDist + tolerance;
    public float MaxDistance => desiredDist - tolerance;

    public float DesiredDistance => desiredDist;
    public float Tolerance => tolerance;

    // public DistanceRange() { }

    public DistanceRange(float desiredDist, float tolerance)
    {
        this.desiredDist = desiredDist;
        this.tolerance = tolerance;
    }

    /// <summary>
    /// Returns
    /// 0 if the distance between a and b are within this range,
    /// 1 if they are further away,
    /// -1 if they are closer
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public int CheckDistance(Vector3 a, Vector3 b)
    {
        float dSqr = (b - a).sqrMagnitude;
        return dSqr > Mathf.Pow(MinDistance, 2) ? 1
            : dSqr < MathF.Pow(MaxDistance, 2) ? -1
            : 0;
    }
}
