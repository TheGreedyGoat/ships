using UnityEngine;

public class NPCCannons : MonoBehaviour
{
    NPCCore NPC => GetComponentInParent<NPCCore>();
    public ShipCannons Cannons => GetComponent<ShipCannons>();

    public void AimAndFire(Vector3 targetWorldPoint)
    {
        Vector3 toTarget = targetWorldPoint - transform.position;
        float angleLeft = Vector3.SignedAngle(
            transform.right,
            toTarget,
            Vector3.up
        );

        float angleRight = Vector3.SignedAngle(
            transform.right,
            toTarget,
            Vector3.up
        );

        Cannons.SetLocalCannonRotation(0, 0, -angleLeft);
        Cannons.SetLocalCannonRotation(1, 0, -angleRight);

        if (angleLeft == 0)
        {
            print("FIRE!!");
            Cannons.Fire(0);
            Debug.Break();
        }
        if (angleRight == 0)
        {
            print("FIRE!!");
            Cannons.Fire(1);
            Debug.Break();
        }
    }
}
