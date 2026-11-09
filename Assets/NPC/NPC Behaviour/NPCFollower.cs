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
        float delta = Mathf.Abs(
            Vector3.Distance(transform.position, target.position)
                - DesiredRange.DesiredDistance
        );

        return Vector3.Lerp(
            target.forward,
            Approach(),
            Mathf.Clamp01(
                Mathf.InverseLerp(
                    DesiredRange.Tolerance * 0.5f,
                    DesiredRange.Tolerance * 1.5f,
                    delta
                )
            )
        );
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

    void OnDrawGizmos()
    {
        if (target == null)
            return;
        int appState = DesiredRange.CheckDistance(
            transform.position,
            target.position
        );
        Gizmos.color = appState == 0 ? Color.green : Color.red;

        float step = 2 * Mathf.PI / 10;
        float maxR = DesiredRange.MaxDistance;
        float minR = DesiredRange.MinDistance;
        for (int i = 0; i <= 10; i++)
        {
            float currentAngle = i * step;
            float nextAngle = (i + 1) * step;
            float cosC = Mathf.Cos(currentAngle);
            float sinC = Mathf.Sin(currentAngle);

            float cosN = Mathf.Cos(nextAngle);
            float sinN = Mathf.Sin(nextAngle);

            Vector3 start = new(cosC, 0, sinC);
            Vector3 end = new(cosN, 0, sinN);

            Gizmos.DrawLine(
                target.position + start * minR + Vector3.up,
                end * minR + target.position + Vector3.up
            );
            Gizmos.DrawLine(
                target.position + start * maxR + Vector3.up,
                end * maxR + target.position + Vector3.up
            );
        }
    }
}
