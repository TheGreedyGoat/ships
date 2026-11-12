using UnityEngine;

public class NPCCannons : MonoBehaviour
{
    NPCCore NPC => GetComponentInParent<NPCCore>();
    ShipCannons Cannons => GetComponent<ShipCannons>();

    bool CheckPreAim(int side, out RaycastHit hitInfo)
    {
        var aimPoints = Cannons.GetShotPrediction(side);
        hitInfo = default;

        for (int i = 0; i < aimPoints.Length - 1; i++)
        {
            Vector3 curr = aimPoints[i];
            Vector3 next = aimPoints[i + 1];
            Vector3 dir = next - curr;

            float len = dir.magnitude;

            if (Physics.Raycast(aimPoints[i], dir.normalized, out hitInfo, len))
            {
                return true;
            }
        }
        return false;
    }
}
