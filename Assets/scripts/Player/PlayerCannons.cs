using System.Collections.Generic;
using System.Linq;
using UnityCommunity.UnitySingleton;
using UnityEngine;

public class PlayerCannons : MonoSingleton<PlayerCannons>
{
    Player player => Player.Instance;

    Ship Ship => player.Ship;

    ShipStatBlock ShipStats => Ship.stats;
    ShipData Data => Ship.Data;

    ShipCannons ShipCannons => player.Cannons;

    Transform CannonTransform => ShipCannons.transform;

    CannonStats Stats => Ship.CannonStats;

    CameraMovement Cam => player.Cam;

    // Aiming
    GameObject[] aimParents;

    MaterialPropertyBlock matProperties;

    protected override void Awake()
    {
        matProperties = new();
    }

    void Update()
    {
        ShowAim();
    }

    public void SetupAimDisplays()
    {
        if (aimParents != null)
        {
            foreach (var p in aimParents)
            {
                if (Application.isPlaying)
                {
                    Destroy(p.gameObject);
                }
                else
                {
                    DestroyImmediate(p.gameObject);
                }
            }
        }
        aimParents = new GameObject[2] { new GameObject(), new GameObject() };
        Vector3 shipDim = Data.MeshDimensions;
        Vector3 slotDim = Data.CannonSlotDimensions;
        var cannonDecks = Data.CannonDecks;
        float xPos = shipDim.x;
        for (int i = 0; i < aimParents.Length; i++)
        {
            // set the parent itself
            var aimP = aimParents[i].transform;
            aimP.parent = CannonTransform;
            aimP.localPosition = new(xPos * (i - 0.5f), 0, 0);

            SetupFilter(aimP, slotDim.y * 0.5f);

            if (cannonDecks > 1)
            {
                SetupFilter(aimP, slotDim.y * (cannonDecks - 0.5f));
                if (cannonDecks > 2)
                {
                    SetupFilter(aimP, slotDim.y * cannonDecks * 0.5f);
                }
            }
            aimP.gameObject.SetActive(false);
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

    public void SetAimVisibility(int side, bool visible)
    {
        aimParents[side].SetActive(visible);
    }

    void ShowAim()
    {
        for (int side = 0; side < 2; side++)
        {
            var aimParent = aimParents[side];
            if (!aimParent.activeSelf)
                continue;

            var mesh = PrecalcFlyingCurve(side, Ship.Data.CannonSlotDimensions);
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
                        ShipCannons.ReloadTimes[side]
                    )
                );
                renderer.SetPropertyBlock(matProperties);
                filter.mesh = mesh;
            }
        }
    }

    Mesh PrecalcFlyingCurve(int side, Vector3 slotDim)
    {
        if (Data.CannonDecks * Data.CannonColumns == 0)
            return new Mesh();

        List<Vector3> points = new() { Vector3.zero };
        float TimeStepsSec = 0.05f;
        Vector3 velocity =
            Stats.FireForce
            * CannonTransform.InverseTransformDirection(
                ShipCannons.GetCannon(side, 0, 0).transform.right
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
            Vector3.forward * Data.CannonColumns * slotDim.z
        );
    }
}
