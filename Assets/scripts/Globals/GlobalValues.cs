using UnityCommunity.UnitySingleton;
using UnityEngine;

public class GlobalValues : MonoSingleton<GlobalValues>
{
    [SerializeField]
    public float SailStateChangeRate = 0.1f;

    [SerializeField]
    public Rigidbody CannonBallPrefab;

    [SerializeField]
    public CannonStats DefaultCannonStats;

    [Header("Controls", order = 0)]
    [SerializeField, Range(0, 2)]
    public float CameraSensitivity;
    public bool InvertCameraPitch;

    [SerializeField, Range(0, 1)]
    public float AimingSensitivity;
    public bool InvertCannonPitch;

    public Material AimMeshMaterial;
}
