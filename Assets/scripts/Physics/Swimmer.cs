using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;

public class Swimmer : MonoBehaviour
{
    public Rigidbody body;
    public Buoyancy buoyancy;
    public float weight = 1;

    public float GetDepth() =>
        Mathf.Max(
            WaveField.Instance.GetHeight(transform.position)
                - transform.position.y,
            0f
        // 100
        );

    //
    //  ######   #### ######## ##     ##  #######   ######
    // ##    ##   ##       ##  ###   ### ##     ## ##    ##
    // ##         ##      ##   #### #### ##     ## ##
    // ##   ####  ##     ##    ## ### ## ##     ##  ######
    // ##    ##   ##    ##     ##     ## ##     ##       ##
    // ##    ##   ##   ##      ##     ## ##     ## ##    ##
    //  ######   #### ######## ##     ##  #######   ######
    //

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
