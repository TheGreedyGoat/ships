using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;


public class Cannons : MonoBehaviour
{
    [SerializeField]
    CannonBall cannonBallPrefab;

    [SerializeField]
    [Range(100, 1000)]
    float cannonforce = 100f;

    // bool isAimingLeft { get => _aimLeft.IsPressed(); }
    // bool isAimingRight { get => _aimRight.IsPressed(); }
    [Header("Cannon Positions")]
    [SerializeField]
    float cannonSpacing = .3f;
    [SerializeField]
    int numBroadSides = 1;
    [SerializeField]
    float yOffsetGeneral = 0;

    [SerializeField]
    float xOffsetGeneral = 1;
    [SerializeField]
    float zOffsetGeneral = 0;


    //0 => left
    // 1 => right
    Vector3[,] cannons;

    // 
    //  ######   ######## ######## ######## ######## ########   ######  
    // ##    ##  ##          ##       ##    ##       ##     ## ##    ## 
    // ##        ##          ##       ##    ##       ##     ## ##       
    // ##   #### ######      ##       ##    ######   ########   ######  
    // ##    ##  ##          ##       ##    ##       ##   ##         ## 
    // ##    ##  ##          ##       ##    ##       ##    ##  ##    ## 
    //  ######   ########    ##       ##    ######## ##     ##  ######  
    // 

    float totalBroadSideWidth { get => (numBroadSides - 1) * cannonSpacing; }
    Vector3 GetCannonWorldSpace(int side, int index)
    {
        return transform.TransformPoint(cannons[side, index]);
    }


    // 
    // ##     ## ##    ## #### ######## ##    ## 
    // ##     ## ###   ##  ##     ##     ##  ##  
    // ##     ## ####  ##  ##     ##      ####   
    // ##     ## ## ## ##  ##     ##       ##    
    // ##     ## ##  ####  ##     ##       ##    
    // ##     ## ##   ###  ##     ##       ##    
    //  #######  ##    ## ####    ##       ##    
    // 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BuildCannons();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {
        ShowCannons();
    }
    // 
    //  ######  ########    ###    ########  ########       
    // ##    ##    ##      ## ##   ##     ##    ##          
    // ##          ##     ##   ##  ##     ##    ##          
    //  ######     ##    ##     ## ########     ##          
    //       ##    ##    ######### ##   ##      ##          
    // ##    ##    ##    ##     ## ##    ##     ##          
    //  ######     ##    ##     ## ##     ##    ##          
    // 


    void BuildCannons()
    {
        cannons = new Vector3[2, numBroadSides];
        Vector3 startOffset = transform.position + new Vector3(0, yOffsetGeneral, -totalBroadSideWidth * 0.5f + zOffsetGeneral);

        for (int i = 0; i < numBroadSides; i++)
        {
            float z = i * cannonSpacing;
            //left
            cannons[0, i] = new Vector3(-xOffsetGeneral, 0, z) + startOffset;
            //right
            cannons[1, i] = new Vector3(xOffsetGeneral, 0, z) + startOffset;
        }
    }

    // 
    //  ######   #### ######## ##     ##  #######   ######  
    // ##    ##   ##       ##  ###   ### ##     ## ##    ## 
    // ##         ##      ##   #### #### ##     ## ##       
    // ##   ####  ##     ##    ## ### ## ##     ##  ######  
    // ##    ##   ##    ##     ##     ## ##     ##       ## 
    // ##    ##   ##   ##      ##     ## ##     ## ##    ## 
    //  ######   #### ######## ##     ##  #######   ######  
    // 


    void ShowCannons()
    {
        Gizmos.color = Color.indianRed;
        Gizmos.DrawSphere(new Vector3(0, yOffsetGeneral, zOffsetGeneral), 0.05f);
        Vector3 startOffset = transform.position + new Vector3(0, yOffsetGeneral, -totalBroadSideWidth * 0.5f + zOffsetGeneral);
        Gizmos.color = Color.blanchedAlmond;
        for (int i = 0; i < numBroadSides; i++)
        {
            float z = i * cannonSpacing;

            Vector3 right = new Vector3(xOffsetGeneral, 0, z) + startOffset;
            Vector3 left = new Vector3(-xOffsetGeneral, 0, z) + startOffset;
            Gizmos.DrawSphere(right, 0.1f);
            Gizmos.DrawSphere(left, 0.1f);
        }
    }

    // 
    // ##     ## ########  ########     ###    ######## ######## 
    // ##     ## ##     ## ##     ##   ## ##      ##    ##       
    // ##     ## ##     ## ##     ##  ##   ##     ##    ##       
    // ##     ## ########  ##     ## ##     ##    ##    ######   
    // ##     ## ##        ##     ## #########    ##    ##       
    // ##     ## ##        ##     ## ##     ##    ##    ##       
    //  #######  ##        ########  ##     ##    ##    ######## 
    // 
    public void ShowAiming()
    {
        for (int s = 0; s < 2; s++)
        {
            Vector3 axis = transform.right * 5 * (2 * s - 1);

            Color c = s == 0 ? Color.aliceBlue : Color.darkGreen;
            for (int i = 0; i < cannons.GetLength(1); i++)
            {
                Vector3 start = GetCannonWorldSpace(s, i);
                Vector3 end = start + axis;
                Debug.DrawLine(start, end, c);
            }

        }
    }

    // 
    // ######## ##     ## ######## ##    ## ########  ######  
    // ##       ##     ## ##       ###   ##    ##    ##    ## 
    // ##       ##     ## ##       ####  ##    ##    ##       
    // ######   ##     ## ######   ## ## ##    ##     ######  
    // ##        ##   ##  ##       ##  ####    ##          ## 
    // ##         ## ##   ##       ##   ###    ##    ##    ## 
    // ########    ###    ######## ##    ##    ##     ######  
    // 


    public void Fire(int s)
    {
        Vector3 fireAxis = transform.right * cannonforce * (2 * s - 1);

        for (int i = 0; i < cannons.GetLength(1); i++)
        {
            Vector3 cannonPosition = GetCannonWorldSpace(s, i);
            Rigidbody ballBody = Instantiate(cannonBallPrefab, cannonPosition, Quaternion.identity).GetComponent<Rigidbody>();
            ballBody.AddForce(fireAxis);
        }



    }

    // Vector3 RandomizeForce()
    // {

    // }


}
