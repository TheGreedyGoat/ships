using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Preview : MonoSingleton<Preview>
{
    [SerializeField]
    int cannonDecks = 1;

    [SerializeField]
    int cannonColumns = 1;

    [SerializeField]
    MeshFilter target;

    public void GetPreviewFromSerialized()
    {
        GetComponent<MeshFilter>().mesh = BakeMeshFromSerialized();
    }

    public Mesh BakeMeshFromSerialized()
    {
        return Shipbaker.Instance.BakeShipMesh(
            cannonDecks,
            cannonColumns,
            out Vector3 _,
            out float __
        );
    }

    public void PreviewOnTarget()
    {
        if (target == null)
        {
            Debug.LogWarning("No target MeshFilter provided");
            return;
        }
        target.mesh = BakeMeshFromSerialized();
    }

    public void ShipBody()
    {
        // Shipbaker.Instance.BakeShip(
        //     cannonDecks,
        //     cannonColumns,
        //     new GameObject().AddComponent<Ship>()
        // );
    }
} //
