// using System;
using UnityEngine;


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

    [Header("Fire Settings")]
    [SerializeField]
    float maxFireDisplacementDeg = 1f;
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
            ballBody.AddForce(RandomizeFireAxis(fireAxis));
        }



    }

    Vector3 RandomizeFireAxis(Vector3 axis)
    {
        float displaceMentAngleDeg = Random.Range(0f, maxFireDisplacementDeg);

        float displacementAxisAngleDeg = Random.Range(0f, 360f);
        Vector3 disPlacementAxis = transform.forward * Mathf.Cos(displacementAxisAngleDeg) + transform.up * Mathf.Sin(displacementAxisAngleDeg);
        // new Vector2(Mathf.Cos(displacementAxisAngleDeg), Mathf.Sin(displacementAxisAngleDeg));

        Quaternion rotation = Quaternion.AngleAxis(displaceMentAngleDeg, disPlacementAxis);
        return rotation * axis;
    }

}
