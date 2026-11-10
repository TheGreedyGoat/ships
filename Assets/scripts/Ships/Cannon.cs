using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    Vector3 directionOfFire = new Vector3(1, 0, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void Fire(Rigidbody prefab, float force)
    {
        Vector3 fireAxis = transform.TransformDirection(directionOfFire);
        var ball = Instantiate(
            prefab,
            transform.position + fireAxis * 0.2f,
            Quaternion.identity
        );
        ball.linearVelocity = fireAxis * force;
        // AddForce(fireAxis * force, ForceMode.Acceleration);
    }
}
