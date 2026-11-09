using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class WaterChunkController : MonoBehaviour
{
    private static WaterChunkController _instance;
    public static WaterChunkController instance
    {
        get
        {
            return _instance;
        }
    }
    [SerializeField][Range(10, 100)] int chunkResolution = 10;
    [SerializeField][Range(10, 20)] float chunkWidth;
    [SerializeField] int numChunks = 1;

    private Mesh flatMesh;

    public Mesh flat { get => flatMesh; }

    [SerializeField] Material chunkMaterial;
    public int[] removeTriIndices;



    void Awake()
    {
        if (_instance != null)
        {
            throw new System.Exception("Only one Chunk Controller allowed");
        }
        _instance = this;
        BuildFlat();
        SpawnChunks();

    }

    void BuildFlat()
    {
        int extendedRes = chunkResolution + 2;
        float vertexSpacing = chunkWidth / (chunkResolution - 1);
        float offset = chunkWidth * 0.5f;

        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<int> noRender = new();

        for (int i = 0; i < extendedRes; i++)
        {
            float x = vertexSpacing * (i - 1) - offset;
            for (int j = 0; j < extendedRes; j++)
            {
                float z = vertexSpacing * (j - 1) - offset;

                vertices.Add(new Vector3(x, 0, z));
                if (i < extendedRes - 1 && j < extendedRes - 1)
                {

                    int quadIndex = i * extendedRes + j;

                    int indexTL = j * extendedRes + i;
                    int indexTR = indexTL + 1;
                    int indexBL = indexTL + extendedRes;
                    int indexBR = indexBL + 1;

                    // tri 0
                    triangles.Add(indexTL);
                    triangles.Add(indexTR);
                    triangles.Add(indexBR);

                    // tri 1
                    triangles.Add(indexBR);
                    triangles.Add(indexBL);
                    triangles.Add(indexTL);


                    ///    TL___TR
                    ///     |\ 0 |
                    ///     | \  |
                    ///     |1 \ |
                    ///   BL|___\|BR

                }
            }
        }

        flatMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        int res = extendedRes;
        int quadsPerRow = res - 1;
        List<int> removeIndices = new();

        for (int i = 0; i < quadsPerRow; i++)
        {
            for (int j = 0; j < quadsPerRow; j++)
            {
                bool isBorder = i == 0 || j == 0 ||
                                i == quadsPerRow - 1 || j == quadsPerRow - 1;

                if (!isBorder) continue;

                int firstIndex = (i * quadsPerRow + j) * 6;
                for (int k = 0; k < 6; k++)
                    removeIndices.Add(firstIndex + k);
            }
        }

        removeTriIndices = removeIndices
            .OrderByDescending(index => index)
            .ToArray();

        // List<int> removedQuads = new();
        // List<int> removeTriIndicesList = new();
        // for (int i = res - 1; i >= 0; i--)
        // {
        //     print(i + "," + 0);
        //     print(0 + "," + i);
        //     print(i + "," + (res - 1));
        //     print(res - 1 + "," + i);

        //     int[] quadIndices = {
        //         i,
        //         i * (res - 1),
        //         res - 1 + i,
        //         (i + 1) * res - 1,
        //     };

        //     for (int j = quadIndices.Length - 1; j >= 0; j--)
        //     {
        //         int currentIndex = quadIndices[j];
        //         if (removedQuads.Contains(currentIndex)) continue;
        //         for (int k = 5; k >= 0; k--)
        //         {
        //             removeTriIndicesList.Add(currentIndex * 6 + k);
        //             // triangles.RemoveAt(currentIndex * 6 + k);
        //         }
        //     }
        //     removeTriIndices = removeTriIndicesList.ToArray();
        // }

    }


    void SpawnChunks()
    {
        int halfchunks = numChunks / 2;
        for (int i = -halfchunks; i <= halfchunks; i++)
        {
            for (int j = -halfchunks; j <= halfchunks; j++)
            {
                GameObject chunkObj = new("Chunk (" + i + ", " + j + ")");
                chunkObj.AddComponent<WaterChunk>();
                chunkObj.transform.position = new Vector3(i, 0, j) * chunkWidth;

                chunkObj.GetComponent<MeshRenderer>().material = chunkMaterial;
            }
        }
    }
}