using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class ShipCannons : MonoBehaviour
{
    [SerializeField]
    Cannon[] cannons;

    MaterialPropertyBlock matProperties;

    Rigidbody cannonBallPrefab => Stats.CannonBall;

    public int CannonsPerBroadSide => cannonCols * cannonDecks;

    private float currentCannonPitch = 0;

    public float CurrentCannonPitch => currentCannonPitch;
    GameObject[] aimParents;

    Quaternion[] defaultCannonRotations;

    private void Awake()
    {
        matProperties ??= new MaterialPropertyBlock();
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

    CameraMovement cam => CameraMovement.Instance;

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
    float[] reloadTimes;

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

    public int cannonDecks;
    public int cannonCols;

    int Index(int side, int deck, int column) =>
        (side * cannonDecks + deck) * cannonCols + column;

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
        ShowAim();
        // AimIndicatorMaterial.SetFloat("_Progress", Mathf.InverseLerp(Stats.ReloadTimeSeconds, 0, ));
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
        cannonDecks = cannonPos.GetLength(1);
        cannonCols = cannonPos.GetLength(2);
        cannons = new Cannon[2 * cannonDecks * cannonCols];
        reloadTimes = new float[2];
        reloadTimes[0] = 0;
        reloadTimes[1] = 0;
        defaultCannonRotations = new Quaternion[2]
        {
            Quaternion.Euler(0, 180, 0),
            Quaternion.identity,
        };
        for (int d = 0; d < cannonDecks; d++)
        {
            for (int c = 0; c < cannonCols; c++)
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

        SetupAimDisplays();
    }

    private void SetupAimDisplays()
    {
        aimParents = new GameObject[2] { new GameObject(), new GameObject() };
        Vector3 shipDim = Ship.Data.MeshDimensions;
        Vector3 slotDim = Ship.Data.CannonSlotDimensions;
        float xPos = shipDim.x;
        for (int i = 0; i < aimParents.Length; i++)
        {
            // set the parent itself
            var parent = aimParents[i].transform;
            parent.parent = transform;
            parent.localPosition = new(xPos * (i - 0.5f), 0, 0);

            SetupFilter(parent, slotDim.y * 0.5f);

            if (cannonDecks > 1)
            {
                SetupFilter(parent, slotDim.y * (cannonDecks - 0.5f));
                if (cannonDecks > 2)
                {
                    SetupFilter(parent, slotDim.y * cannonDecks * 0.5f);
                }
            }
        }
    }

    private void SetupFilter(Transform parent, float yOffset)
    {
        var filter = new GameObject();
        filter.transform.parent = parent;

        filter.AddComponent<MeshFilter>();
        filter.AddComponent<MeshRenderer>().material = GlobalValues
            .Instance
            .AimMeshMaterial;

        filter.transform.localPosition = new(0, yOffset, 0);
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

    public void SetAimVisibility(int side, bool visible)
    {
        print($"Setting side {side} to visible {visible}");
        aimParents[side].SetActive(visible);
    }

    void ShowAim()
    {
        for (int side = 0; side < 2; side++)
        {
            var aimParent = aimParents[side];
            if (aimParent.activeSelf)
            {
                var mesh = PrecalcFlyingCurve(
                    side,
                    Ship.Data.CannonSlotDimensions
                );
                foreach (
                    var filter in aimParent.GetComponentsInChildren<MeshFilter>()
                )
                {
                    var renderer = filter.GetComponent<MeshRenderer>();
                    renderer.GetPropertyBlock(matProperties);
                    matProperties.SetFloat(
                        "_Progress",
                        Mathf.InverseLerp(
                            Stats.ReloadTimeSeconds,
                            0,
                            reloadTimes[side]
                        )
                    );
                    renderer.SetPropertyBlock(matProperties);
                    filter.mesh = mesh;
                }
            }
        }
    }

    Mesh PrecalcFlyingCurve(int side, Vector3 slotDim)
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
            Vector3.forward * cannonCols * slotDim.z
        );
    }

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

    public void ResetCannonRotations()
    {
        for (int d = 0; d < cannonDecks; d++)
        {
            for (int c = 0; c < cannonCols; c++)
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
