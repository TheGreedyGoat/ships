using System;
using UnityEngine;
using UnityEngine.Rendering;

public class NPCFollower : NPCBehaviour
{
    void Start()
    {
        var ship = GetComponent<Ship>();
        var playerShip = Player.Instance.Ship;
        if (ship != null && playerShip != null)
            // Ensure the NPC can hold speed with the player
            ship.stats = ship.stats.CopyWith(
                acceleration: Mathf.Max(
                    ship.stats.acceleration,
                    playerShip.stats.acceleration
                )
            );
    }

    protected override BehaviourState UpdateState()
    {
        int rangeCheck = DesiredRange.CheckDistance(
            transform.position,
            target.position
        );
        return rangeCheck == 0
            ? BehaviourState.MatchHeading
            : BehaviourState.ApproachDistance;
    }
}
