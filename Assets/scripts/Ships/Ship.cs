using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    public ShipStatBlock stats;

    [SerializeField]
    public int CannonDecks;

    [SerializeField]
    public int CannonsPerDeck;

    [SerializeField]
    bool isPlayer = false;
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
    }

    public void SetPlayer()
    {
        Player.Instance.ship = this;
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
