using System;
using UnityEngine;

class BasicAttacker : NPCBehaviour
{
    [SerializeField, Range(0, 1)]
    float toleranceDeviationPercentace;

    protected override BehaviourState GetNextState()
    {
        return BehaviourState.ApproachDistance;
    }

    public override Vector3 GetDesiredHeading()
    {
        return InterpolateByRangeDeviation(
            ShowClosestBroadSide(),
            Approach(),
            toleranceDeviationPercentace
        );
    }
}
