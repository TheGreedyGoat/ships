using JetBrains.Annotations;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    public ShipStatBlock stats;

    public CannonStats CannonStats;

    [SerializeField]
    bool isPlayer = false;

    [SerializeField]
    public ShipData Data;
    private bool isBaked = false;

    public bool IsBaked
    {
        get => isBaked;
    }

    void Start()
    {
        Unbake();
        Bake();
        if (isPlayer)
        {
            SetPlayer();
        }
        else
        {
            SetNPCBehaviour();
        }

        transform.position = new(
            transform.position.x,
            WaveField.Instance.GetHeight(transform.position),
            transform.position.z
        );
    }

    public void SetPlayer()
    {
        Player.Instance.SetPlayerShip(this);
    }

    public void SetNPCBehaviour()
    {
        if (Player.Instance.Ship == this)
        {
            Debug.LogWarning(
                "Can't assign the player ship as NPC. Un- or reassign player first"
            );
            return;
        }
        var core = gameObject.AddComponent<NPCCore>();
        core.Setup<BasicAttacker>();
    }

    public void Bake()
    {
        if (isBaked)
        {
            Debug.LogWarning("Ship has already been baked");
            return;
        }

        Shipbaker.Instance.BakeShip(this);
        gameObject.AddComponent<ShipMovement>();
        if (CannonStats == null)
        {
            CannonStats = GlobalValues.Instance.DefaultCannonStats;
        }
        CalculateCannonRange();

        isBaked = true;
    }

    void CalculateCannonRange()
    {
        var cannons = GetComponentInChildren<ShipCannons>();
        int iterations = 50;
        float currentAngle = -CannonStats.ElevationLimitDeg;
        float angleLimit = CannonStats.ElevationLimitDeg;

        float angleStep = Mathf.Abs(angleLimit - currentAngle) / iterations;
        float longestDistance = 0;
        for (; currentAngle <= angleLimit; currentAngle += angleStep)
        {
            cannons.SetLocalCannonRotation(0, currentAngle, 0);
            longestDistance = Mathf.Max(
                cannons.GetDistancePrediction(0, 0),
                longestDistance
            );
        }

        Data.MaxFireRange = longestDistance;
    }

    public void Unbake()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
        foreach (var comp in GetComponents<Component>())
        {
            if (
                comp == null
                || comp is Transform
                || comp is Rigidbody
                || comp is NPCCore
                || comp is NPCBehaviour
                || comp == this
            )
                continue;
            if (Application.isPlaying)
                Destroy(comp);
            else
                DestroyImmediate(comp);
        }
        isBaked = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(
            transform.position + transform.right * Data.MaxFireRange,
            1
        );
        Gizmos.DrawSphere(
            transform.position - transform.right * Data.MaxFireRange,
            1
        );
    }
}
