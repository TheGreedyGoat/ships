using System;
using UnityEngine;
using UnityEngine.Rendering;

public class NPCFollower : NPCBehaviour
{
    void Start()
    {
        var ship = GetComponent<Ship>();
        var playerShip = Player.Instance.ship;
        if (ship != null && playerShip != null)
            // Ensure the NPC can hold speed with the player
            ship.stats = ship.stats.CopyWith(
                acceleration: Mathf.Max(
                    ship.stats.acceleration,
                    playerShip.stats.acceleration
                )
            );
    }

    protected override BehaviourState GetNextState()
    {
        int rangeCheck = DesiredRange.CheckDistance(
            transform.position,
            target.position
        );
        return rangeCheck == 0
            ? BehaviourState.MatchHeading
            : BehaviourState.ApproachDistance;
    }

    public override Vector3 GetDesiredHeading()
    {
        // delta to tolerance limit
        return InterpolateByRangeDeviation(target.forward, Approach());
        // return Vector3.Lerp(
        //     target.forward,
        //     Approach(),
        //     Mathf.Clamp01(
        //         Mathf.InverseLerp(
        //             DesiredRange.Tolerance * 0.5f,
        //             DesiredRange.Tolerance * 1.5f,
        //             delta
        //         )
        //     )
        // );
    }
}
