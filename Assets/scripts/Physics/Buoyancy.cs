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
    public float buoyancy = .3f;
    [SerializeField]
    public float damping = 1f;
    [SerializeField]
    Transform swimmerParent;


    Rigidbody body;


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
        body = GetComponent<Rigidbody>();
        BuildSwimmers();
    }
    void BuildSwimmers()
    {
        // instantiate swimmers
        Swimmer FL = new GameObject("FL Swimmer").AddComponent<Swimmer>();
        Swimmer FR = new GameObject("FR Swimmer").AddComponent<Swimmer>();
        Swimmer BL = new GameObject("BL Swimmer").AddComponent<Swimmer>();
        Swimmer BR = new GameObject("BR Swimmer").AddComponent<Swimmer>();

        Swimmer[] swimmers = { FL, FR, BL, BR };
        foreach (Swimmer swimmer in swimmers)
        {
            swimmer.transform.parent = swimmerParent;
            swimmer.body = body;
            swimmer.buoyancy = this;
            swimmer.influence = .25f;
        }
        // locate swimmers
        FL.transform.localPosition = new Vector3(-swimmerOffsetX, swimmerOffsetY, +swimmerOffsetZ);
        FR.transform.localPosition = new Vector3(+swimmerOffsetX, swimmerOffsetY, +swimmerOffsetZ);
        BL.transform.localPosition = new Vector3(-swimmerOffsetX, swimmerOffsetY, -swimmerOffsetZ);
        BR.transform.localPosition = new Vector3(+swimmerOffsetX, swimmerOffsetY, -swimmerOffsetZ);

    }

}
