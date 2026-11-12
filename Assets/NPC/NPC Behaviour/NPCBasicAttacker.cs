using System;
using UnityEngine;

class BasicAttacker : NPCBehaviour
{
    [SerializeField, Range(0, 1)]
    float toleranceDeviationPercentace;

    Vector3 AimTarget
    {
        get
        {
            var tBody = target.GetComponent<Rigidbody>();
            Vector3 velocity = Vector3.zero;
            if (tBody != null)
            {
                velocity = tBody.linearVelocity;
            }
            return PreAim.PredictPreaimPosition(
                transform,
                target,
                velocity,
                Ship.CannonStats.CannonBallSpeed,
                3
            );
        }
    }

    float AimTargetDistance => Vector3.Distance(AimTarget, transform.position);

    protected override BehaviourState UpdateState()
    {
        BehaviourState nextState = state switch
        {
            BehaviourState.ApproachDistance => OnApproach(),
            BehaviourState.PrepareToFire => OnPerpareFire(),
            _ => BehaviourState.ApproachDistance,
        };

        if (nextState != state)
        {
            Debug.Break();
        }
        return nextState;
    }

    BehaviourState OnApproach()
    {
        print("Im approaching my target");
        desiredHeading = AimTarget - transform.position;
        return AimTargetDistance < Ship.Data.MaxFireRange
            ? BehaviourState.PrepareToFire
            : BehaviourState.ApproachDistance;
    }

    BehaviourState OnPerpareFire()
    {
        print("In Range. Preparing fire");
        if (AimTargetDistance > Ship.Data.MaxFireRange * 2)
        {
            print("Got out ouf range, need to approach again");
            return BehaviourState.ApproachDistance;
        }
        var aimTarget = AimTarget;
        CannonController.AimAndFire(aimTarget);
        desiredHeading = GetHeadingForClosestBroadSide(aimTarget);
        //todo check fire
        return BehaviourState.PrepareToFire;
    }
}
