
using Unity.Burst.Intrinsics;
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
    [SerializeField][Tooltip("How many cannons per side and cannon deck")] int cannonsPerDeck = 1;
    [SerializeField] int numCannonDecks = 2;
    [SerializeField] float cannonDeckSpacing = 0.1f;
    [SerializeField]
    float yOffsetGeneral = 0;

    [SerializeField]
    float xOffsetGeneral = 1;
    [SerializeField]
    float zOffsetGeneral = 0;

    [Header("Fire Settings")]
    [SerializeField]
    float maxFireDisplacementDeg = 1f;
    [SerializeField] float shotElevationDegrees = 90;
    //0 => left
    // 1 => right
    Vector3[,] cannons;
    float[] reloadCooldowns;

    [Header("Gizmos")]
    [SerializeField]
    bool drawGizmos = true;

    public float reloadTime;

    // 
    //  ######   ######## ######## ######## ######## ########   ######  
    // ##    ##  ##          ##       ##    ##       ##     ## ##    ## 
    // ##        ##          ##       ##    ##       ##     ## ##       
    // ##   #### ######      ##       ##    ######   ########   ######  
    // ##    ##  ##          ##       ##    ##       ##   ##         ## 
    // ##    ##  ##          ##       ##    ##       ##    ##  ##    ## 
    //  ######   ########    ##       ##    ######## ##     ##  ######  
    // 

    float totalBroadSideDepth { get => (cannonsPerDeck - 1) * cannonSpacing; }
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
        cannons = BuildCannons2();
        reloadCooldowns = new float[2] { 0, 0 };
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < reloadCooldowns.Length; i++)
        {
            if (reloadCooldowns[i] > 0)
            {
                reloadCooldowns[i] -= Time.deltaTime;
            }
        }
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

    Vector3[,] BuildCannons2()
    {
        Vector3[,] cannons = new Vector3[2, cannonsPerDeck * numCannonDecks];

        float yStart = -(numCannonDecks - 1) * cannonDeckSpacing * 0.5f + yOffsetGeneral;
        float zStart = -totalBroadSideDepth * 0.5f + zOffsetGeneral;
        for (int y = 0; y < numCannonDecks; y++) // cannon decks
        {
            float height = yStart + y * cannonDeckSpacing;
            for (int z = 0; z < cannonsPerDeck; z++)
            {
                cannons[0, y * cannonsPerDeck + z]
                = new Vector3(-xOffsetGeneral, height, z * cannonSpacing + zStart);
                cannons[1, y * cannonsPerDeck + z]
                = new Vector3(xOffsetGeneral, height, z * cannonSpacing + zStart);
            }

        }

        return cannons;
    }

    void BuildCannons()
    {
        cannons = new Vector3[2, cannonsPerDeck];
        Vector3 startOffset = transform.position + new Vector3(0, yOffsetGeneral, -totalBroadSideDepth * 0.5f + zOffsetGeneral);

        for (int i = 0; i < cannonsPerDeck; i++)
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
            float reloadPerc = Mathf.InverseLerp(reloadTime, 0, reloadCooldowns[s]);
            Vector3 axis = transform.right * 5 * (2 * s - 1);

            for (int i = 0; i < cannons.GetLength(1); i++)
            {
                Vector3 start = GetCannonWorldSpace(s, i);
                Vector3 end = start + axis;
                Vector3 progress = Vector3.Lerp(start, end, reloadPerc);
                Debug.DrawLine(start, progress, Color.green);

                Debug.DrawLine(progress, end, Color.red);

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


    public void Fire(int side)
    {
        if (reloadCooldowns[side] > 0) return;
        int sign = 2 * side - 1;
        Vector3 fireAxis = Quaternion.Euler(sign * transform.forward * shotElevationDegrees) * transform.right * cannonforce * sign;
        int counter = 0;
        for (int i = 0; i < cannons.GetLength(1); i++)
        {
            Vector3 cannonPosition = GetCannonWorldSpace(side, i);
            Rigidbody ballBody = Instantiate(cannonBallPrefab, cannonPosition, Quaternion.identity).GetComponent<Rigidbody>();
            ballBody.AddForce(RandomizeFireAxis(fireAxis));
            counter++;
        }

        reloadCooldowns[side] = reloadTime;

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

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        Vector3[,] cannonPositions;
        Gizmos.color = Color.greenYellow;

        if (cannons != null)
        {
            cannonPositions = cannons;
        }
        else
        {
            cannonPositions = BuildCannons2();
        }
        foreach (Vector3 c in cannonPositions)
        {
            Gizmos.DrawSphere(transform.TransformPoint(c), .05f);
        }
    }

}
