using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal.Internal;

public class ShipRoll : MonoBehaviour
{
    Rigidbody ship;
    Sail sail;

    [SerializeField]
    float rollFactor = 2000;

    void Start()
    {
        ship = GetComponentInParent<Rigidbody>();
        sail = GetComponentInChildren<Sail>();
    }

    void Update()
    {
        float av = ship.angularVelocity.y;
        float roll = av * rollFactor;
        transform.localEulerAngles = new Vector3(0, 0, roll);
    }

}