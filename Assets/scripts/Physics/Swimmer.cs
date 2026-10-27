using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;

public class Swimmer : MonoBehaviour
{
    public Rigidbody body;
    public Buoyancy buoyancy;
    public float influence = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void FixedUpdate()
    {
        var force = CalculateLocalBuoyancy();
        Debug.DrawLine(transform.position, transform.position + force, Color.cadetBlue);
        Vector3 offsetCorrection = transform.up * transform.localPosition.y;
        body.AddForceAtPosition(force * Time.fixedDeltaTime, transform.position - offsetCorrection);

    }


    Vector3 CalculateLocalBuoyancy()
    {
        float depth = calculateDepth();

        Vector3 force = (depth * buoyancy.buoyancy) * -Physics.gravity;

        Vector3 pointV = body.GetPointVelocity(transform.position);
        force -= Vector3.up * (pointV.y * buoyancy.damping);

        return force;

    }


    float calculateDepth() => Mathf.Max(0f, WaveValues.Instance.GetHeight(transform.position) - transform.position.y);


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
        Gizmos.DrawSphere(transform.position, 0.03f);
    }

}
