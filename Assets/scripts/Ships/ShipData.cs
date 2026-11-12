using System;
using UnityEngine;

[Serializable]
public class ShipData
{
    public int CannonDecks = 1;

    public int CannonColumns = 1;

    [HideInInspector]
    public Vector3 MeshDimensions;

    [HideInInspector]
    public Vector3 CannonSlotDimensions;

    [HideInInspector]
    public float DeckHeight;

    [HideInInspector]
    public float FrontPartOffset => CannonSlotDimensions.z * CannonColumns;

    public Vector3 LocalCameraPos =>
        new(0, DeckHeight + 1, FrontPartOffset * 0.5f);

    public float MaxFireRange;
}
