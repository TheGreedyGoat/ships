using System;
using UnityEngine;

public abstract class NPCBehaviour : MonoBehaviour
{
    protected enum BehaviourState
    {
        Idle,
        ApproachDistance,
        MatchHeading,
        Disengage,
    }

    protected BehaviourState state = BehaviourState.Idle;

    [SerializeField]
    public DistanceRange DesiredRange;

    [SerializeField]
    protected Transform target;

    void Start()
    {
        if (target == null)
            target = Player.Instance.ship.transform;
    }

    public virtual Vector3 GetDesiredHeading()
    {
        if (target == null)
            return transform.right;
        switch (state)
        {
            case BehaviourState.ApproachDistance:
                return Approach();
            case BehaviourState.MatchHeading:
                return target.forward;
            case BehaviourState.Disengage:
                return transform.position - target.position;
            default:
                return transform.forward;
        }
    }

    protected abstract BehaviourState GetNextState();

    void Update()
    {
        state = GetNextState();
    }

    protected Vector3 Approach()
    {
        var targetPoint =
            target.position
            + (transform.position - target.position).normalized
                * DesiredRange.DesiredDistance;

        return (targetPoint - transform.position).normalized;
    }
}
