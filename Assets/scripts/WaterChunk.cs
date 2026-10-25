
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterChunk : MonoBehaviour
{

    // WaveFunctionManager manager;

    MeshFilter filter;

    static Mesh flat => WaterChunkController.instance.flat;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        filter = GetComponent<MeshFilter>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        WaveFunctionManager manager = WaveFunctionManager.instance;
        Vector3[] vertices = new Vector3[flat.vertices.Length];
        int res = Mathf.FloorToInt(Mathf.Sqrt(vertices.Length));
        for (int i = 0; i < flat.vertices.Length; i++)
        {

            Vector3 worldPoint = transform.TransformPoint(flat.vertices[i]);
            Vector3 displacement = manager.GetWaveOffset(worldPoint);

            Vector3 displacedWP = worldPoint + displacement;
            vertices[i] = transform.InverseTransformPoint(displacedWP);
            // normals[i] = displacedWP.normalized * Mathf.Sign(displacedWP.y);
        }

        Mesh nextMesh = new Mesh
        {
            vertices = vertices,
            triangles = flat.triangles,
            // normals = normals,
            uv = flat.uv
        };
        nextMesh.RecalculateNormals();

        List<int> triangles = nextMesh.triangles.ToList();

        foreach (int index in WaterChunkController.instance.removeTriIndices)
        {
            triangles.RemoveAt(index);
        }

        nextMesh.triangles = triangles.ToArray();

        filter.mesh = nextMesh;

    }
}
