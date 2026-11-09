using UnityEngine;

[CreateAssetMenu(
    fileName = "BuoyancySettings",
    menuName = "Ship Physics/BuoyancySettings"
)]
public class BuoyancySettings : ScriptableObject
{
    [SerializeField]
    public float Force;

    [SerializeField]
    public float Damping;

    [SerializeField]
    public float YOffset;

    [SerializeField, Range(0, 1)]
    public float BuoyancyTorqueFactor;
}
