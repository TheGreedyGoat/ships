using UnityEngine;


[RequireComponent(typeof(ShipMovement))]
[RequireComponent(typeof(Buoyancy))]
class Ship : MonoBehaviour
{

    [SerializeField] public ShipStatBlock stats;

    void Awake()
    {
        Cannons c = GetComponentInChildren<Cannons>();
        if (c != null)
        {
            c.reloadTime = stats.reloadTimeSeconds;
        }
    }
}
