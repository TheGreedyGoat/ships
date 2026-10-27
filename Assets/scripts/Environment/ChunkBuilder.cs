using System.Collections.Generic;
using UnityEngine;

public class ChunkBuilder : MonoBehaviour
{
    [SerializeField] float triangleBorder = 1;
    [SerializeField] int chunkSize = 32;
    [SerializeField] int viewRangeChunks = 5;
    [SerializeField] Material waterMaterial;
    [SerializeField][Tooltip("The object to follow")] Transform playerShip;

    Mesh chunkMesh;


    void Awake()
    {

        List<Vector3> vertices = new();
        List<int> triangles = new();
        float lsqr = Mathf.Pow(triangleBorder, 2);
        float triHeigt = Mathf.Sqrt(lsqr - lsqr * 0.25f);
        Vector3[] triVertices = {
        //         1 __________0,5
        //           \        /\      
        //            \      /  \
        //             \    /    \
        //              \  /      \
        //            2,3\/________\ 4
            new(triangleBorder, 0, 0),
            Vector3.zero,
            new(triangleBorder * 0.5f, 0, triHeigt),

            new(triangleBorder * 0.5f, 0, triHeigt),
            new(triangleBorder * 1.5f, 0 , triHeigt),
            new(triangleBorder, 0, 0),
        };
        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float xOffset = (x + z * 0.5f) * triangleBorder;
                float zOffset = z * triHeigt;
                foreach (Vector3 v in triVertices)
                {
                    vertices.Add(v + new Vector3(xOffset, 0, zOffset));
                }

                int triIndexOffset = vertices.Count - 6;
                for (int i = 0; i < 6; i++)
                {
                    triangles.Add(triIndexOffset + i);
                }

            }
        }

        chunkMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        chunkMesh.RecalculateNormals();
    }

    void Start()
    {
        float chunkBorderLength = triangleBorder * chunkSize;
        float chunkBorderHeight = Mathf.Sqrt((1 - 0.25f) * Mathf.Pow(chunkBorderLength, 2));
        for (int x = -viewRangeChunks; x <= viewRangeChunks; x++)
        {
            for (int z = -viewRangeChunks; z <= viewRangeChunks; z++)
            {

                float offX = (x + z * 0.5f) * chunkBorderLength;
                float offZ = z * chunkBorderHeight;
                GameObject chunk = new GameObject($"Chunk ({x}|{z}))");
                var filter = chunk.AddComponent<MeshFilter>();
                var renderer = chunk.AddComponent<MeshRenderer>();
                filter.mesh = chunkMesh;
                renderer.material = waterMaterial;

                chunk.transform.parent = transform;
                chunk.transform.localPosition = new(offX, 0, offZ);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new(playerShip.position.x, 0, playerShip.position.z);
    }
}
