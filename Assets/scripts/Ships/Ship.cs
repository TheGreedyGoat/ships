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

    public void SetNPCBehaviour(NPCBehaviour behaviour)
    {
        if (Player.Instance.Ship == this)
        {
            Debug.LogWarning(
                "Can't assign the player ship as NPC. Un- or reassign player first"
            );
            return;
        }
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

        isBaked = true;
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
}
