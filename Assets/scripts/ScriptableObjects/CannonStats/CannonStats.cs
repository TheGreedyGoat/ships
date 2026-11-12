using UnityEngine;

[CreateAssetMenu(
    fileName = "CannonStats",
    menuName = "Scriptable Objects/CannonStats"
)]
public class CannonStats : ScriptableObject
{
    public Rigidbody CannonBall;

    [Min(1)]
    public float CannonBallSpeed;
    public float ReloadTimeSeconds = 5;

    [Range(0, 90)]
    public float ElevationLimitDeg = 10f;

    [Range(0, 90)]
    public float TraversalLimitDeg = 10f;
}
