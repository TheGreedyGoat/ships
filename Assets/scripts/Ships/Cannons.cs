using Unity.Burst.Intrinsics;
using Unity.Properties;
using UnityEngine;

public class CannonGroup : MonoBehaviour
{
    [SerializeField]
    Rigidbody cannonBallPrefab;

    [SerializeField]
    [Range(100, 1000)]
    float cannonforce = 100f;

    // bool isAimingLeft { get => _aimLeft.IsPressed(); }
    // bool isAimingRight { get => _aimRight.IsPressed(); }
    [Header("Cannon Positions")]
    [SerializeField]
    float cannonSpacing = .3f;

    [SerializeField]
    [Tooltip("How many cannons per side and cannon deck")]
    int cannonsPerDeck = 1;

    [SerializeField]
    int numCannonDecks = 2;

    [SerializeField]
    float cannonDeckSpacing = 0.1f;

    [SerializeField]
    float yOffsetGeneral = 0;

    [SerializeField]
    float xOffsetGeneral = 1;

    [SerializeField]
    float zOffsetGeneral = 0;

    [Header("Fire Settings")]
    [SerializeField]
    float maxFireDisplacementDeg = 1f;

    [SerializeField]
    float shotElevationDegrees = 90;

    //0 => left
    // 1 => right
    Cannon[] cannons;
    float[] reloadCooldowns;

    [Header("Gizmos")]
    [SerializeField]
    bool drawGizmos = true;

    public float reloadTime;

    int cannonDecks;
    int cannonCols;

    float[,] reloadTimes;

    int Index(int side, int deck, int column) =>
        (side * cannonDecks + deck) * cannonCols + column;

    //
    //  ######   ######## ######## ######## ######## ########   ######
    // ##    ##  ##          ##       ##    ##       ##     ## ##    ##
    // ##        ##          ##       ##    ##       ##     ## ##
    // ##   #### ######      ##       ##    ######   ########   ######
    // ##    ##  ##          ##       ##    ##       ##   ##         ##
    // ##    ##  ##          ##       ##    ##       ##    ##  ##    ##
    //  ######   ########    ##       ##    ######## ##     ##  ######
    //

    float totalBroadSideDepth
    {
        get => (cannonsPerDeck - 1) * cannonSpacing;
    }

    Vector3 GetFirePosition(int side, int deck, int column)
    {
        return cannons[Index(side, deck, column)].transform.position;
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
    // void Start()
    // {
    //     cannons = BuildCannons2();
    //     reloadCooldowns = new float[2] { 0, 0 };
    // }

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
    public void Setup(
        Vector3[,,] cannonPos,
        GameObject prefab,
        Transform parent
    )
    {
        transform.parent = parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        cannonDecks = cannonPos.GetLength(1);
        cannonCols = cannonPos.GetLength(2);
        cannons = new Cannon[2 * cannonDecks * cannonCols];
        reloadTimes = new float[2, cannonDecks];
        for (int d = 0; d < cannonDecks; d++)
        {
            reloadTimes[0, d] = 0;
            reloadTimes[1, d] = 0;
            for (int c = 0; c < cannonCols; c++)
            {
                cannons[Index(0, d, c)] = Instantiate(
                        prefab,
                        transform.TransformPoint(cannonPos[0, d, c]),
                        Quaternion.Euler(new(0, 90, 0)) * transform.rotation,
                        transform
                    )
                    .GetComponent<Cannon>();

                cannons[Index(1, d, c)] = Instantiate(
                        prefab,
                        transform.TransformPoint(cannonPos[1, d, c]),
                        transform.rotation * Quaternion.Euler(new(0, -90, 0)),
                        transform
                    )
                    .GetComponent<Cannon>();
            }
        }
        cannonBallPrefab = GlobalValues.Instance.CannonBallPrefab;
    }

    Vector3[,] BuildCannons2()
    {
        Vector3[,] cannons = new Vector3[2, cannonsPerDeck * numCannonDecks];

        float yStart =
            -(numCannonDecks - 1) * cannonDeckSpacing * 0.5f + yOffsetGeneral;
        float zStart = -totalBroadSideDepth * 0.5f + zOffsetGeneral;
        for (int y = 0; y < numCannonDecks; y++) // cannon decks
        {
            float height = yStart + y * cannonDeckSpacing;
            for (int z = 0; z < cannonsPerDeck; z++)
            {
                cannons[0, y * cannonsPerDeck + z] = new Vector3(
                    -xOffsetGeneral,
                    height,
                    z * cannonSpacing + zStart
                );
                cannons[1, y * cannonsPerDeck + z] = new Vector3(
                    xOffsetGeneral,
                    height,
                    z * cannonSpacing + zStart
                );
            }
        }

        return cannons;
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
        // for (int s = 0; s < 2; s++)
        // {
        //     float reloadPerc = Mathf.InverseLerp(
        //         reloadTime,
        //         0,
        //         reloadCooldowns[s]
        //     );
        //     Vector3 axis = transform.right * 5 * (2 * s - 1);

        //     for (int i = 0; i < cannons.GetLength(1); i++)
        //     {
        //         Vector3 start = GetCannonWorldSpace(s, i);
        //         Vector3 end = start + axis;
        //         Vector3 progress = Vector3.Lerp(start, end, reloadPerc);
        //         Debug.DrawLine(start, progress, Color.green);

        //         Debug.DrawLine(progress, end, Color.red);
        //     }
        // }
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
        if (reloadCooldowns[side] > 0)
            return;
        int sign = 2 * side - 1;
        Vector3 fireAxis =
            Quaternion.Euler(sign * transform.forward * shotElevationDegrees)
            * transform.right
            * cannonforce
            * sign;
        int counter = 0;
        for (int i = 0; i < cannons.GetLength(1); i++)
        {
            // Vector3 cannonPosition = GetFirePosition(side, i);
            // Rigidbody ballBody = Instantiate(
            //         cannonBallPrefab,
            //         cannonPosition,
            //         Quaternion.identity
            //     )
            //     .GetComponent<Rigidbody>();
            // ballBody.AddForce(RandomizeFireAxis(fireAxis));
            // counter++;
        }

        reloadCooldowns[side] = reloadTime;
    }

    Vector3 RandomizeFireAxis(Vector3 axis)
    {
        float displaceMentAngleDeg = Random.Range(0f, maxFireDisplacementDeg);

        float displacementAxisAngleDeg = Random.Range(0f, 360f);
        Vector3 disPlacementAxis =
            transform.forward * Mathf.Cos(displacementAxisAngleDeg)
            + transform.up * Mathf.Sin(displacementAxisAngleDeg);
        // new Vector2(Mathf.Cos(displacementAxisAngleDeg), Mathf.Sin(displacementAxisAngleDeg));

        Quaternion rotation = Quaternion.AngleAxis(
            displaceMentAngleDeg,
            disPlacementAxis
        );
        return rotation * axis;
    }
}
