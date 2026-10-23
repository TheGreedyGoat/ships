using Unity.Hierarchy.Editor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy : MonoBehaviour
{
    [Header("Swimmer Setup")]
    [SerializeField]
    float swimmerOffsetX = 0.5f;
    [SerializeField]
    float swimmerOffsetY = 0;
    [SerializeField]
    float swimmerOffsetZ = 1;

    [Header("Settings")]
    [SerializeField]
    float buoyancy = .3f;
    [SerializeField]
    float damping = 1f;
    [SerializeField]
    Transform swimmerParent;

    private Transform[] swimmers = new Transform[4];

    Rigidbody body;
    Transform FL { get => swimmers[0]; }
    Transform FR { get => swimmers[1]; }
    Transform BL { get => swimmers[2]; }
    Transform BR { get => swimmers[3]; }


    // 
    //  ######  ########    ###    ########  ######## 
    // ##    ##    ##      ## ##   ##     ##    ##    
    // ##          ##     ##   ##  ##     ##    ##    
    //  ######     ##    ##     ## ########     ##    
    //       ##    ##    ######### ##   ##      ##    
    // ##    ##    ##    ##     ## ##    ##     ##    
    //  ######     ##    ##     ## ##     ##    ##    
    // 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildSwimmers();
        body = GetComponent<Rigidbody>();
    }
    void BuildSwimmers()
    {
        // instantiate swimmers
        swimmers[0] = new GameObject("FL Swimmer").transform;
        swimmers[1] = new GameObject("FR Swimmer").transform;
        swimmers[2] = new GameObject("BL Swimmer").transform;
        swimmers[3] = new GameObject("BR Swimmer").transform;
        swimmers[0].parent = swimmerParent;
        swimmers[1].parent = swimmerParent;
        swimmers[2].parent = swimmerParent;
        swimmers[3].parent = swimmerParent;

        // locate swimmers
        swimmers[0].localPosition = new Vector3(-swimmerOffsetX, swimmerOffsetY, +swimmerOffsetZ);
        swimmers[1].localPosition = new Vector3(+swimmerOffsetX, swimmerOffsetY, +swimmerOffsetZ);
        swimmers[2].localPosition = new Vector3(-swimmerOffsetX, swimmerOffsetY, -swimmerOffsetZ);
        swimmers[3].localPosition = new Vector3(+swimmerOffsetX, swimmerOffsetY, -swimmerOffsetZ);
    }

    // 
    // ######## #### ##     ## ######## ########     ##     ## ########  ########     ###    ######## ######## 
    // ##        ##   ##   ##  ##       ##     ##    ##     ## ##     ## ##     ##   ## ##      ##    ##       
    // ##        ##    ## ##   ##       ##     ##    ##     ## ##     ## ##     ##  ##   ##     ##    ##       
    // ######    ##     ###    ######   ##     ##    ##     ## ########  ##     ## ##     ##    ##    ######   
    // ##        ##    ## ##   ##       ##     ##    ##     ## ##        ##     ## #########    ##    ##       
    // ##        ##   ##   ##  ##       ##     ##    ##     ## ##        ##     ## ##     ##    ##    ##       
    // ##       #### ##     ## ######## ########      #######  ##        ########  ##     ##    ##    ######## 
    // 

    void FixedUpdate()
    {
        for (int i = 0; i < swimmers.Length; i++)
        {
            Transform swimmer = swimmers[i];
            var force = CalculateLocalBuoyancy(swimmer);
            Debug.DrawLine(swimmer.position, swimmer.position + force, Color.cadetBlue);
            body.AddForceAtPosition(force * Time.fixedDeltaTime, swimmer.position);
        }
    }

    Vector3 CalculateLocalBuoyancy(Transform swimmer)
    {
        float y = swimmer.position.y;
        float depth = calculateDepth(swimmer.position);

        Vector3 force = Vector3.up * (depth * buoyancy);

        Vector3 pointV = body.GetPointVelocity(swimmer.position);
        force -= Vector3.up * (pointV.y * damping);

        return force;

    }

    float calculateDepth(Vector3 position) => Mathf.Max(0f, EnvironmentManager.instance.GetWaterLevel(position) - position.y);


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
        Gizmos.color = Color.navyBlue;
        foreach (var t in swimmers)
        {
            if (t != null)
            {
                Gizmos.DrawSphere(t.position, 0.03f);
            }
        }
    }
}
