using System.Collections.Generic;
using Unity.Mathematics;
using UnityCommunity.UnitySingleton;
using UnityEditor;
using UnityEngine;

public class Shipbaker : MonoSingleton<Shipbaker>
{
    [Header("Prefabs")]
    [SerializeField]
    MeshFilter[] topPrefabs;

    [SerializeField]
    MeshFilter[] CannonDeckPrefabs;

    [SerializeField]
    MeshFilter[] bottomPrefabs;

    [SerializeField]
    GameObject CannonPrefab;

    [Header("Values")]
    [SerializeField]
    int cannonDecks = 2;

    [SerializeField]
    int cannonColumns = 4;

    [SerializeField]
    float topCenterExtraScale = 0.1f;

    List<Vector3> cannonSlotPositions;

    [SerializeField]
    GameObject shipPrefab;

    [SerializeField]
    Material[] materials;

    [SerializeField]
    BuoyancySettings buoyancySettings;

    [SerializeField]
    Sail sailPrefab;

    float topDeckHeight;
    float frontOffset;

    [Header("PhysicsPresets")]
    [SerializeField, Range(0, 1)]
    float YawInertiaTensorFactor = 0.5f;

    public Mesh BakeShipMesh(int cannonDecks, int cannonColumns)
    {
        cannonSlotPositions = new();
        var m = CannonDeckPrefabs[1].sharedMesh;

        Vector3 cannonSlotDimensions = GetMeshDimensions(m);

        topDeckHeight = cannonSlotDimensions.y * cannonDecks;
        frontOffset = cannonSlotDimensions.z * cannonColumns;
        var hullParts = new List<CombineInstance>();
        var windowParts = new List<CombineInstance>();
        //
        // ########   #######  ######## ########  #######  ##     ##
        // ##     ## ##     ##    ##       ##    ##     ## ###   ###
        // ##     ## ##     ##    ##       ##    ##     ## #### ####
        // ########  ##     ##    ##       ##    ##     ## ## ### ##
        // ##     ## ##     ##    ##       ##    ##     ## ##     ##
        // ##     ## ##     ##    ##       ##    ##     ## ##     ##
        // ########   #######     ##       ##     #######  ##     ##
        //

        // bottom tail
        hullParts.Add(
            new CombineInstance
            {
                mesh = bottomPrefabs[0].sharedMesh,
                transform = Matrix4x4.identity,
            }
        );

        // bottom front
        hullParts.Add(
            new CombineInstance
            {
                mesh = bottomPrefabs[2].sharedMesh,
                transform = Matrix4x4.TRS(
                    new(0, 0, frontOffset),
                    Quaternion.identity,
                    Vector3.one
                ),
            }
        );

        //
        // ########  #######  ########
        //    ##    ##     ## ##     ##
        //    ##    ##     ## ##     ##
        //    ##    ##     ## ########
        //    ##    ##     ## ##
        //    ##    ##     ## ##
        //    ##     #######  ##
        //

        // top tail
        hullParts.Add(
            new CombineInstance
            {
                mesh = topPrefabs[0].sharedMesh,
                transform = Matrix4x4.TRS(
                    new(0, topDeckHeight, 0),
                    Quaternion.identity,
                    Vector3.one
                ),
            }
        );
        // top front
        hullParts.Add(
            new CombineInstance
            {
                mesh = topPrefabs[2].sharedMesh,
                transform = Matrix4x4.TRS(
                    new(0, topDeckHeight, frontOffset),
                    Quaternion.identity,
                    Vector3.one
                ),
            }
        );

        if (cannonColumns > 0)
        {
            //bottom center
            hullParts.Add(
                new CombineInstance
                {
                    mesh = bottomPrefabs[1].sharedMesh,
                    transform = Matrix4x4.TRS(
                        new(0, 0, frontOffset * 0.5f),
                        quaternion.identity,
                        new(1, 1, cannonColumns)
                    ),
                }
            );

            // top center
            hullParts.Add(
                new CombineInstance
                {
                    mesh = topPrefabs[1].sharedMesh,
                    transform = Matrix4x4.TRS(
                        new(0, topDeckHeight, frontOffset * 0.5f),
                        quaternion.identity,
                        new(1, 1, cannonColumns + topCenterExtraScale)
                    ),
                }
            );
        }

        if (cannonDecks > 0)
        {
            // deck tail
            hullParts.Add(
                new CombineInstance
                {
                    mesh = CannonDeckPrefabs[0].sharedMesh,
                    transform = Matrix4x4.TRS(
                        new(0, topDeckHeight * 0.5f, 0),
                        Quaternion.identity,
                        new(1, cannonDecks, 1)
                    ),
                }
            );
            // top front
            hullParts.Add(
                new CombineInstance
                {
                    mesh = CannonDeckPrefabs[2].sharedMesh,
                    transform = Matrix4x4.TRS(
                        new(0, topDeckHeight * 0.5f, frontOffset),
                        Quaternion.identity,
                        new(1, cannonDecks, 1)
                    ),
                }
            );
        }
        Mesh cannonSlotMesh = CannonDeckPrefabs[1].sharedMesh;

        var slotPositions = GetAllCannonSlotPositions(
            cannonDecks,
            cannonColumns
        );

        foreach (var pos in slotPositions)
        {
            var transform = Matrix4x4.TRS(
                pos,
                Quaternion.identity,
                new(1, 1, 1)
            );
            cannonSlotPositions.Add(pos);
            hullParts.Add(
                new CombineInstance
                {
                    mesh = cannonSlotMesh,
                    transform = transform,
                    subMeshIndex = 0,
                }
            );
            windowParts.Add(
                new CombineInstance
                {
                    mesh = cannonSlotMesh,
                    transform = transform,
                    subMeshIndex = 1,
                }
            );
        }
        // for (int c = 0; c < cannonColumns; c++)
        // {
        //     for (int d = 0; d < cannonDecks; d++)
        //     {
        //         var transform = Matrix4x4.TRS(GetCannonSlotPosition(d, c, cannonSlotDimensions), Quaternion.identity, new(1, 1, 1));
        //         cannonSlotPositions.Add(GetCannonSlotPosition(d, c, cannonSlotDimensions));

        //     }
        // }

        var hullMesh = new Mesh();
        var windowMesh = new Mesh();

        hullMesh.CombineMeshes(hullParts.ToArray(), true);
        windowMesh.CombineMeshes(windowParts.ToArray(), true);

        CombineInstance[] comb =
        {
            new CombineInstance()
            {
                mesh = hullMesh,
                transform = Matrix4x4.identity,
            },
            new CombineInstance()
            {
                mesh = windowMesh,
                transform = Matrix4x4.identity,
            },
        };

        var combined = new Mesh();

        combined.CombineMeshes(comb, false);
        combined.name = GetShipName(cannonDecks, cannonColumns);
        return combined;
    }

    //
    // ######## ##    ## ########     ########     ###    ##    ## ########    ##     ## ########  ######  ##     ##
    // ##       ###   ## ##     ##    ##     ##   ## ##   ##   ##  ##          ###   ### ##       ##    ## ##     ##
    // ##       ####  ## ##     ##    ##     ##  ##   ##  ##  ##   ##          #### #### ##       ##       ##     ##
    // ######   ## ## ## ##     ##    ########  ##     ## #####    ######      ## ### ## ######    ######  #########
    // ##       ##  #### ##     ##    ##     ## ######### ##  ##   ##          ##     ## ##             ## ##     ##
    // ##       ##   ### ##     ##    ##     ## ##     ## ##   ##  ##          ##     ## ##       ##    ## ##     ##
    // ######## ##    ## ########     ########  ##     ## ##    ## ########    ##     ## ########  ######  ##     ##
    //

    List<Vector3> GetAllCannonSlotPositions(int cannonDecks, int cannonColumns)
    {
        List<Vector3> positions = new();
        Vector3 dimensions = GetMeshDimensions(CannonDeckPrefabs[1].sharedMesh);
        for (int c = 0; c < cannonColumns; c++)
        {
            for (int d = 0; d < cannonDecks; d++)
            {
                positions.Add(GetCannonSlotPosition(d, c, dimensions));
            }
        }

        return positions;
    }

    /// <summary>
    /// returns the positions for each cannon grouped by broadside (
    ///  0 => Left
    ///  1 => Right
    /// )
    ///  the cannon deck.
    ///
    ///   eg [0, 3, 0] would refer to the first cannon on the left side on the 4th cannondeck, counting bottom up, tail to front
    ///
    /// </summary>
    /// <param name="cannonDecks"></param>
    /// <param name="cannonColumns"></param>
    /// <returns></returns>
    Vector3[,,] CannonsByBroadSideAndDeck(int cannonDecks, int cannonColumns)
    {
        Vector3[,,] result = new Vector3[2, cannonDecks, cannonColumns];
        Vector3 dim = GetMeshDimensions(CannonDeckPrefabs[1].sharedMesh);
        Vector3 xOff = new(dim.x * 0.5f, 0, 0);
        for (int c = 0; c < cannonColumns; c++)
        {
            for (int d = 0; d < cannonDecks; d++)
            {
                var slotPos = GetCannonSlotPosition(d, c, dim);
                result[0, d, c] = slotPos - xOff;
                result[1, d, c] = slotPos + xOff;
            }
        }
        return result;
    }

    Vector3 GetCannonSlotPosition(int deck, int column, Vector3 dimensions)
    {
        return new(
            0,
            dimensions.y * (0.5f + deck),
            dimensions.z * (0.5f + column)
        );
    }

    Vector3 GetMeshDimensions(Mesh mesh)
    {
        return GetMeshDimensions(mesh, out float dump);
    }

    Vector3 GetMeshDimensions(Mesh mesh, out float lowestZ)
    {
        lowestZ = mesh.bounds.min.z;
        return mesh.bounds.size;
    }

    public void BakeShip(Ship ship)
    {
        var stats = ship.stats;
        var cannonDecks = ship.CannonDecks;
        var cannonColumns = ship.CannonsPerDeck;
        var mesh = BakeShipMesh(cannonDecks, cannonColumns);

        Vector3 shipDimensions = GetMeshDimensions(mesh, out float lowestZ);
        var shipBodyObject = ship.gameObject;
        var shipTransform = ship.transform;

        var filter = shipBodyObject.AddComponent<MeshFilter>();
        var collider = shipBodyObject.AddComponent<MeshCollider>();
        int numCannons = cannonColumns * cannonDecks;
        Material[] matList = materials;
        if (numCannons == 0)
        {
            matList = new Material[1] { materials[0] };
        }
        shipBodyObject.AddComponent<MeshRenderer>().sharedMaterials = matList;
        collider.convex = true;

        collider.convex = true;
        collider.sharedMesh = mesh;

        filter.mesh = mesh;
        mesh.SetPreBakeCollisionMesh(true, true);

        List<Vector3> swimmerPositions = new();
        float xOffSetAbs = shipDimensions.x * 0.5f;
        int numSwimmersPerSide = 2 + cannonColumns / 4;
        float swimmerSpacing = shipDimensions.z / numSwimmersPerSide;
        for (int i = 0; i <= numSwimmersPerSide; i++)
        {
            swimmerPositions.Add(
                new(
                    -xOffSetAbs,
                    buoyancySettings.YOffset,
                    lowestZ + swimmerSpacing * i
                )
            );
            swimmerPositions.Add(
                new(
                    xOffSetAbs,
                    buoyancySettings.YOffset,
                    lowestZ + swimmerSpacing * i
                )
            );
        }

        Buoyancy buoyancy = shipBodyObject.AddComponent<Buoyancy>();
        buoyancy.Build(swimmerPositions, buoyancySettings);

        //
        //  ######     ###    ##    ## ##    ##  #######  ##    ##  ######
        // ##    ##   ## ##   ###   ## ###   ## ##     ## ###   ## ##    ##
        // ##        ##   ##  ####  ## ####  ## ##     ## ####  ## ##
        // ##       ##     ## ## ## ## ## ## ## ##     ## ## ## ##  ######
        // ##       ######### ##  #### ##  #### ##     ## ##  ####       ##
        // ##    ## ##     ## ##   ### ##   ### ##     ## ##   ### ##    ##
        //  ######  ##     ## ##    ## ##    ##  #######  ##    ##  ######
        //
        var cannonPositions = CannonsByBroadSideAndDeck(
            cannonDecks,
            cannonColumns
        );
        var cannons = new GameObject() { name = "Cannons" };
        cannons.transform.parent = ship.transform;
        cannons.transform.localPosition = Vector3.zero;
        cannons.transform.localRotation = Quaternion.identity;

        var cMono = cannons.AddComponent<ShipCannons>();
        var leftAim = new GameObject("Left Aim");
        var rightAim = new GameObject("Right Aim");
        leftAim.transform.parent = cMono.transform;
        rightAim.transform.parent = cMono.transform;

        leftAim.transform.localPosition =
            Vector3.zero - Vector3.right * shipDimensions.x * 0.5f;
        rightAim.transform.localPosition =
            Vector3.zero + Vector3.right * shipDimensions.x * 0.5f;

        leftAim.transform.localRotation = Quaternion.identity;
        rightAim.transform.localRotation = Quaternion.identity;

        var leftFilter = leftAim.AddComponent<MeshFilter>();
        var rightFilter = rightAim.AddComponent<MeshFilter>();
        var leftRend = leftAim.AddComponent<MeshRenderer>();
        leftRend.enabled = false;
        var rightRend = rightAim.AddComponent<MeshRenderer>();
        rightRend.enabled = false;

        leftRend.material = GlobalValues.Instance.AimMeshMaterial;
        rightRend.material = GlobalValues.Instance.AimMeshMaterial;

        cMono.Setup(
            cannonPositions,
            CannonPrefab,
            shipBodyObject.transform,
            leftFilter,
            rightFilter
        );
        //
        //  ######     ###    #### ##
        // ##    ##   ## ##    ##  ##
        // ##        ##   ##   ##  ##
        //  ######  ##     ##  ##  ##
        //       ## #########  ##  ##
        // ##    ## ##     ##  ##  ##
        //  ######  ##     ## #### ########
        //

        var sail = Instantiate(
            sailPrefab,
            shipTransform.position + new Vector3(0, topDeckHeight, 0.5f),
            shipTransform.rotation,
            shipTransform
        );

        var body = ship.GetComponent<Rigidbody>();
        body.automaticCenterOfMass = true;
        var autoCOM = body.centerOfMass;
        body.automaticCenterOfMass = false;
        body.centerOfMass = new(0, 0, autoCOM.z);

        body.automaticInertiaTensor = true;
        var autoTensor = body.inertiaTensor;
        body.automaticInertiaTensor = false;
        body.inertiaTensor =
            new Vector3(
                autoTensor.x * YawInertiaTensorFactor,
                autoTensor.y * YawInertiaTensorFactor,
                autoTensor.z
            ) / body.mass;
    }

    string GetShipName(int cannonDecks, int cannonColumns) =>
        $"Ship ({cannonColumns} x {cannonDecks})";

    void OnDrawGizmos()
    {
        if (cannonSlotPositions == null)
            return;
        Gizmos.color = Color.burlywood;
        foreach (var pos in cannonSlotPositions)
        {
            Gizmos.DrawSphere(pos, .3f);
        }
    }
}
