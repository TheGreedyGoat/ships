using Unity.Burst;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CannonBall : MonoBehaviour
{

    static float cannonBallSpeed = 5;

    float lifeTime = 0;

    void Update()
    {
        lifeTime += Time.deltaTime;

        if (lifeTime >= 10 || transform.position.y < -5)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }


}