using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class NPCBehaviour : MonoBehaviour
{
    protected enum BehaviourState
    {
        Idle,
        ApproachDistance,
        MatchHeading,
        Disengage,
        PrepareToFire,
    }

    protected Ship Ship => GetComponent<Ship>();

    protected BehaviourState state = BehaviourState.Idle;

    NPCCore npc => GetComponent<NPCCore>();

    protected NPCCannons CannonController =>
        GetComponentInChildren<NPCCannons>();

    [SerializeField]
    public DistanceRange DesiredRange;

    [SerializeField]
    protected Transform target;

    public Transform Target => target;

    protected Vector3 desiredHeading = Vector3.forward;

    public Vector3 DesiredHeading => desiredHeading;

    void Start()
    {
        if (target == null)
            target = Player.Instance.Ship.transform;
    }

    // public abstract Vector3 GetDesiredHeading();

    protected abstract BehaviourState UpdateState();

    void Update()
    {
        state = UpdateState();
        Debug.DrawLine(
            transform.position,
            transform.position + DesiredHeading,
            Color.antiqueWhite
        );
    }

    protected Vector3 InterpolateByRangeDeviation(
        Vector3 onClose,
        Vector3 onFar,
        float deviation = 0.5f
    )
    {
        float delta = Mathf.Abs(
            Vector3.Distance(transform.position, target.position)
                - DesiredRange.DesiredDistance
        );
        return Vector3.Lerp(
            onClose,
            onFar,
            Mathf.Sqrt(
                Mathf.Clamp01(
                    Mathf.InverseLerp(
                        DesiredRange.Tolerance * (1 - deviation),
                        DesiredRange.Tolerance * (1 + deviation),
                        delta
                    )
                )
            )
        );
    }

    protected Vector3 Approach()
    {
        var targetPoint =
            target.position
            + (transform.position - target.position).normalized
                * DesiredRange.DesiredDistance;

        return (targetPoint - transform.position).normalized;
    }

    protected Vector3 GetHeadingForClosestBroadSide(Vector3 aimTarget)
    {
        var dirToTarget = (aimTarget - transform.position).normalized;
        var tDotSelf = Vector3.Dot(dirToTarget, transform.right);

        return Quaternion.Euler(0f, 90f * Mathf.Sign(tDotSelf), 0f)
            * (transform.position - aimTarget).normalized;
    }

    //
    //  ######   #### ######## ##     ##  #######   ######
    // ##    ##   ##       ##  ###   ### ##     ## ##    ##
    // ##         ##      ##   #### #### ##     ## ##
    // ##   ####  ##     ##    ## ### ## ##     ##  ######
    // ##    ##   ##    ##     ##     ## ##     ##       ##
    // ##    ##   ##   ##      ##     ## ##     ## ##    ##
    //  ######   #### ######## ##     ##  #######   ######
    //

    // void OnDrawGizmos()
    // {
    //     if (target == null)
    //         return;
    //     int appState = DesiredRange.CheckDistance(
    //         transform.position,
    //         target.position
    //     );

    //     float step = 2 * Mathf.PI / 10;
    //     float maxR = DesiredRange.MaxDistance;
    //     float minR = DesiredRange.MinDistance;
    //     for (int i = 0; i <= 10; i++)
    //     {
    //         float currentAngle = i * step;
    //         float nextAngle = (i + 1) * step;
    //         float cosC = Mathf.Cos(currentAngle);
    //         float sinC = Mathf.Sin(currentAngle);

    //         float cosN = Mathf.Cos(nextAngle);
    //         float sinN = Mathf.Sin(nextAngle);

    //         Vector3 start = new(cosC, 0, sinC);
    //         Vector3 end = new(cosN, 0, sinN);

    //         Gizmos.color = appState == 0 ? Color.green : Color.red;
    //         Gizmos.DrawLine(
    //             target.position + start * minR + Vector3.up,
    //             end * minR + target.position + Vector3.up
    //         );
    //         Gizmos.DrawLine(
    //             target.position + start * maxR + Vector3.up,
    //             end * maxR + target.position + Vector3.up
    //         );

    //         Gizmos.color = Color.blue;

    //         Gizmos.DrawLine(
    //             target.position
    //                 + start * DesiredRange.DesiredDistance
    //                 + Vector3.up,
    //             end * DesiredRange.DesiredDistance
    //                 + target.position
    //                 + Vector3.up
    //         );
    //     }
    // }
}
