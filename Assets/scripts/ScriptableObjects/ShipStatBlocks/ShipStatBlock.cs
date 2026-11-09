using System;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ShipStatBlock",
    menuName = "Scriptable Objects/ShipStatBlock"
)]
[Serializable]
public class ShipStatBlock : ScriptableObject
{
    [Header("Mobility")]
    [SerializeField]
    [Range(0f, 5.0f)]
    public float lateralFrictionFactor = 1;

    [SerializeField]
    [Range(0f, 1.0f)]
    public float longitudalFrictionFactor = 0.2f;

    [SerializeField]
    public float turningAgility = 2f;

    [SerializeField]
    public float acceleration = 1f;

    [Header("Weaponry")]
    [SerializeField]
    public float reloadTimeSeconds = 10;

    [SerializeField]
    [Range(0, 30)]
    public float MinPropulsionAngle = 15;

    public ShipStatBlock CopyWith(
        float? lateralFrictionFactor = null,
        float? longitudalFrictionFactor = null,
        float? turningAgility = null,
        float? acceleration = null,
        float? reloadTimeSeconds = null,
        float? MinPropulsionAngle = null
    ) =>
        new()
        {
            lateralFrictionFactor =
                lateralFrictionFactor ?? this.lateralFrictionFactor,
            longitudalFrictionFactor =
                longitudalFrictionFactor ?? this.longitudalFrictionFactor,
            turningAgility = turningAgility ?? this.turningAgility,
            acceleration = acceleration ?? this.acceleration,
            reloadTimeSeconds = reloadTimeSeconds ?? this.reloadTimeSeconds,
            MinPropulsionAngle = MinPropulsionAngle ?? this.MinPropulsionAngle,
        };
}
