using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.PlayerLoop;

public class ShipCannons : MonoBehaviour
{
    [SerializeField]
    Cannon[] cannons;

    private float currentCannonPitch = 0;

    Quaternion[] defaultCannonRotations;

    /// <summary>
    /// stores the time left to reload for each broadside and deck
    /// </summary>
    float[] reloadTimes;

    //
    //  ######   ######## ########       ##  ######  ######## ########
    // ##    ##  ##          ##         ##  ##    ## ##          ##
    // ##        ##          ##        ##   ##       ##          ##
    // ##   #### ######      ##       ##     ######  ######      ##
    // ##    ##  ##          ##      ##           ## ##          ##
    // ##    ##  ##          ##     ##      ##    ## ##          ##
    //  ######   ########    ##    ##        ######  ########    ##
    //
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

    public int CannonDecks => Ship.Data.CannonDecks;
    public int CannonCols => Ship.Data.CannonColumns;
    CameraMovement cam => CameraMovement.Instance;
    public Cannon[] Cannons => cannons;
    Rigidbody cannonBallPrefab => Stats.CannonBall;
    public int CannonsPerBroadSide => CannonCols * CannonDecks;
    public float CurrentCannonPitch => currentCannonPitch;

    public float[] ReloadTimes => reloadTimes;

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
                for (int d = 0; d < CannonDecks; d++)
                {
                    for (int c = 0; c < CannonCols; c++) { }
                }
            }
        }
    }

    bool IsReloading(int broadSide)
    {
        if (broadSide < 0 || broadSide > 1)
        {
            Debug.LogWarning(
                $"invalid cannon deck parameters {broadSide}",
                this
            );
            return true;
        }
        return reloadTimes[broadSide] > 0;
    }

    public Cannon GetCannon(int side, int deck, int column)
    {
        return cannons[Index(side, deck, column)];
    }

    int Index(int side, int deck, int column)
    {
        int index = (side * CannonDecks + deck) * CannonCols + column;
        if (index < 0 || index >= cannons.Length)
        {
            throw new Exception(
                $"Cannon Parameters\nside = {side}, deck = {deck} column = {column}\nAre not in bounds of this ship\nCannonDecks: {CannonDecks}, CannonColumns: {CannonCols}"
            );
        }

        return index;
    }

    List<Cannon> GetBroadSide(int side)
    {
        List<Cannon> bs = new();

        for (int i = 0; i < CannonsPerBroadSide; i++)
        {
            bs.Add(cannons[side * CannonsPerBroadSide + i]);
        }

        return bs;
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
        Transform parent
    // MeshFilter leftAimFilter,
    // MeshFilter rightAimFilter
    )
    {
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        cannons = new Cannon[2 * CannonDecks * CannonCols];
        reloadTimes = new float[2];
        reloadTimes[0] = 0;
        reloadTimes[1] = 0;
        defaultCannonRotations = new Quaternion[2]
        {
            Quaternion.Euler(0, 180, 0),
            Quaternion.identity,
        };
        for (int d = 0; d < CannonDecks; d++)
        {
            for (int c = 0; c < CannonCols; c++)
            {
                cannons[Index(0, d, c)] = Instantiate(
                        prefab,
                        transform.TransformPoint(cannonPos[0, d, c]),
                        defaultCannonRotations[0] * transform.rotation,
                        transform
                    )
                    .GetComponent<Cannon>();

                cannons[Index(1, d, c)] = Instantiate(
                        prefab,
                        transform.TransformPoint(cannonPos[1, d, c]),
                        transform.rotation,
                        transform
                    )
                    .GetComponent<Cannon>();
            }
        }
    }

    //
    // ##     ## ########  ########     ###    ######## ########
    // ##     ## ##     ## ##     ##   ## ##      ##    ##
    // ##     ## ##     ## ##     ##  ##   ##     ##    ##
    // ##     ## ########  ##     ## ##     ##    ##    ######
    // ##     ## ##        ##     ## #########    ##    ##
    // ##     ## ##        ##     ## ##     ##    ##    ##
    //  #######  ##        ########  ##     ##    ##    ########
    //

    void Update()
    {
        UpdateReload();
    }

    void UpdateReload()
    {
        for (int s = 0; s < 2; s++)
        {
            if (reloadTimes[s] > 0)
                reloadTimes[s] = Mathf.Max(reloadTimes[s] - Time.deltaTime, 0);
        }
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

    public void Aim(int side, float deltaP)
    {
        currentCannonPitch = Mathf.Clamp(
            currentCannonPitch + deltaP,
            -Stats.ElevationLimitDeg,
            Stats.ElevationLimitDeg
        );

        float shipYaw = Ship.transform.eulerAngles.y;
        float sideStraightYaw = shipYaw + (side == 0 ? 180f : 0f);
        float desiredYaw = cam.CurrentYaw - 90f;

        float offset = Mathf.DeltaAngle(sideStraightYaw, desiredYaw);
        float clampedOffset = Mathf.Clamp(
            offset,
            -Stats.TraversalLimitDeg,
            Stats.TraversalLimitDeg
        );

        Quaternion rot = Quaternion.Euler(
            0f,
            sideStraightYaw + clampedOffset,
            currentCannonPitch
        );
        // 0

        for (int d = 0; d < CannonDecks; d++)
        {
            for (int c = 0; c < CannonCols; c++)
            {
                var cannon = cannons[Index(side, d, c)];
                // var right = cannons[Index(1, d, c)];
                cannon.transform.rotation = rot;
            }
        }
    }

    public void ResetCannonRotations()
    {
        for (int d = 0; d < CannonDecks; d++)
        {
            for (int c = 0; c < CannonCols; c++)
            {
                cannons[Index(0, d, c)].transform.localRotation =
                    defaultCannonRotations[0];
                cannons[Index(1, d, c)].transform.localRotation =
                    defaultCannonRotations[1];
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

    public void Fire(int side)
    {
        if (side != 0 && side != 1)
        {
            Debug.LogWarning(
                "Side parameter to fire has to be 0 (left) or 1 (right)"
            );
            return;
        }

        if (IsReloading(side))
            return;

        var broadSide = GetBroadSide(side);
        foreach (var c in broadSide)
        {
            c.Fire(cannonBallPrefab, Stats.FireForce);
        }
        reloadTimes[side] = Stats.ReloadTimeSeconds;
    }
}
