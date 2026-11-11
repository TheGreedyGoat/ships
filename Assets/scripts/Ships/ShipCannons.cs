using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using UnityEngine;

public class ShipCannons : MonoBehaviour
{
    [SerializeField]
    Cannon[] cannons;

    Rigidbody cannonBallPrefab => Stats.CannonBall;

    int[] nextDecksToFire = { 0, 0 };

    [SerializeField]
    float reloadTime = 5;

    float currentCannonPitch = 0;
    float currentCannonYaw = 0;

    MeshFilter[] aimDisplays;

    public void SetAimingMeshVisibility(int side, bool visible)
    {
        if (side < 0 || side > 1)
            return;
        aimDisplays[side].GetComponent<MeshRenderer>().enabled = visible;
    }

    Ship Ship
    {
        get
        {
            var s = GetComponentInParent<Ship>();
            return s;
        }
    }
    CannonStats Stats
    {
        get
        {
            var s = Ship.CannonStats;
            return s;
        }
    }

    Vector3 CannonEulers
    {
        get
        {
            if (cannons.Length == 0)
            {
                return Vector3.zero;
            }
            else
            {
                return cannons[0].transform.eulerAngles;
            }
        }
        set
        {
            for (int s = 0; s < 2; s++)
            {
                for (int d = 0; d < cannonDecks; d++)
                {
                    for (int c = 0; c < cannonCols; c++) { }
                }
            }
        }
    }

    /// <summary>
    /// stores the time left to reload for each broadside and deck
    /// </summary>
    float[,] reloadTimes;

    bool IsReloading(int broadSide, int deck)
    {
        if (broadSide < 0 || broadSide > 1 || deck < 0 || deck >= cannonDecks)
        {
            Debug.LogWarning(
                $"invalid cannon deck parameters {broadSide}, {deck}",
                this
            );
            return true;
        }
        return reloadTimes[broadSide, deck] > 0;
    }

    public int cannonDecks;
    public int cannonCols;

    int Index(int side, int deck, int column) =>
        (side * cannonDecks + deck) * cannonCols + column;

    void Update()
    {
        UpdateReload();
    }

    void UpdateReload()
    {
        for (int s = 0; s < 2; s++)
        {
            for (int d = 0; d < cannonDecks; d++)
            {
                if (reloadTimes[s, d] > 0)
                    reloadTimes[s, d] = Mathf.Max(
                        reloadTimes[s, d] - Time.deltaTime,
                        0
                    );
            }
        }
    }

    //
    //  ######  ######## ######## ##     ## ########
    // ##    ## ##          ##    ##     ## ##     ##
    // ##       ##          ##    ##     ## ##     ##
    //  ######  ######      ##    ##     ## ########
    //       ## ##          ##    ##     ## ##
    // ##    ## ##          ##    ##     ## ##
    //  ######  ########    ##     #######  ##
    //

    public void Setup(
        Vector3[,,] cannonPos,
        GameObject prefab,
        Transform parent,
        MeshFilter leftAimFilter,
        MeshFilter rightAimFilter
    )
    {
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        cannonDecks = cannonPos.GetLength(1);
        cannonCols = cannonPos.GetLength(2);
        cannons = new Cannon[2 * cannonDecks * cannonCols];
        reloadTimes = new float[2, cannonDecks];
        for (int d = 0; d < cannonDecks; d++)
        {
            reloadTimes[0, d] = 0;
            reloadTimes[1, d] = 0;
            for (int c = 0; c < cannonCols; c++)
            {
                cannons[Index(0, d, c)] = Instantiate(
                        prefab,
                        transform.TransformPoint(cannonPos[0, d, c]),
                        Quaternion.Euler(new(0, 180, 0)) * transform.rotation,
                        transform
                    )
                    .GetComponent<Cannon>();

                cannons[Index(1, d, c)] = Instantiate(
                        prefab,
                        transform.TransformPoint(cannonPos[1, d, c]),
                        transform.rotation * Quaternion.Euler(new(0, 0, 0)),
                        transform
                    )
                    .GetComponent<Cannon>();
            }
        }
        aimDisplays = new MeshFilter[2] { leftAimFilter, rightAimFilter };
    }

    public void FireNextDeck(int side)
    {
        Fire(side, nextDecksToFire[side]);
        nextDecksToFire[side]++;
        nextDecksToFire[side] %= cannonDecks;
    }

    //
    //    ###    #### ##     ##
    //   ## ##    ##  ###   ###
    //  ##   ##   ##  #### ####
    // ##     ##  ##  ## ### ##
    // #########  ##  ##     ##
    // ##     ##  ##  ##     ##
    // ##     ## #### ##     ##
    //

    public void ShowAim(int s)
    {
        int d = nextDecksToFire[s];
        var display = aimDisplays[s];
        display.mesh = PrecalcFlyingCurve(s);
        display.transform.localPosition = new(
            display.transform.localPosition.x,
            d,
            display.transform.localPosition.z
        );
    }

    Mesh PrecalcFlyingCurve(int side)
    {
        if (cannonDecks * cannonCols == 0)
            return new Mesh();

        List<Vector3> points = new() { Vector3.zero };
        float TimeStepsSec = 0.05f;
        Vector3 velocity =
            Stats.FireForce
            * transform.InverseTransformDirection(
                cannons[Index(side, 0, 0)].transform.right
            );

        for (int i = 0; i < 20; i++)
        {
            points.Add(points.Last() + velocity * TimeStepsSec);
            velocity += Physics.gravity * TimeStepsSec;
            if (points.Last().y < -5)
                break;
        }
        return new MeshLin() { vertices = points.ToArray() }.Build(
            1,
            Vector3.forward * cannonCols
        );
    }

    public void Aim(float deltaP, int side)
    {
        var cam = CameraMovement.Instance;
        currentCannonPitch = Mathf.Clamp(
            currentCannonPitch + deltaP,
            -Stats.ElevationLimitDeg,
            Stats.ElevationLimitDeg
        );

        float shipYaw = Ship.transform.eulerAngles.y;
        float sideCenterYaw = shipYaw + (side == 0 ? 180f : 0f);
        float desiredYaw = cam.CurrentYaw - 90f;

        float offset = Mathf.DeltaAngle(sideCenterYaw, desiredYaw);
        float clampedOffset = Mathf.Clamp(
            offset,
            -Stats.TraversalLimitDeg,
            Stats.TraversalLimitDeg
        );

        Quaternion rot = Quaternion.Euler(
            0f,
            sideCenterYaw + clampedOffset,
            -currentCannonPitch
        );
        // 0

        for (int d = 0; d < cannonDecks; d++)
        {
            for (int c = 0; c < cannonCols; c++)
            {
                var cannon = cannons[Index(side, d, c)];
                // var right = cannons[Index(1, d, c)];
                cannon.transform.rotation = rot;
            }
        }
    }

    //
    // ######## #### ########  ########
    // ##        ##  ##     ## ##
    // ##        ##  ##     ## ##
    // ######    ##  ########  ######
    // ##        ##  ##   ##   ##
    // ##        ##  ##    ##  ##
    // ##       #### ##     ## ########
    //

    public void Fire(int side, int deck)
    {
        if (side != 0 && side != 1)
        {
            Debug.LogWarning(
                "Side parameter to fire has to be 0 (left) or 1 (right)"
            );
            return;
        }
        if (deck < 0 || deck >= cannonDecks)
        {
            Debug.LogWarning(
                $"This ship has {cannonDecks} decks. deck index must be less than that and >= 0"
            );
            return;
        }
        if (IsReloading(side, deck))
            return;
        for (int c = 0; c < cannonCols; c++)
        {
            cannons[Index(side, deck, c)]
                .Fire(cannonBallPrefab, Stats.FireForce);
        }
        reloadTimes[side, deck] = Stats.ReloadTimeSeconds;
    }
}
