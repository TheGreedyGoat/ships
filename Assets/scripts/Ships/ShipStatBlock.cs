using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipStatBlock", menuName = "Scriptable Objects/ShipStatBlock")]
[Serializable]
public class ShipStatBlock : ScriptableObject
{

    [SerializeField]
    [Range(0.5f, 2.0f)]
    public float lateralFrictionFactor = 1;
    [SerializeField]
    [Range(0.5f, 1.0f)]
    public float longitudalFrictionFactor = 0.2f;
    [SerializeField]
    public float turningAgility = 2f;
    [SerializeField]
    public float acceleration = 1f;

    [SerializeField]
    public float reloadTimeSeconds = 10;

}
