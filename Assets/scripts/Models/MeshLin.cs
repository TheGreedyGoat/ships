using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct MeshLin
{
    public Vector3[] vertices;

    public Mesh Build(int segments, Vector3 offsetPerSegment, Mesh mesh = null)
    {
        List<Vector3> meshVerts = new();
        List<int> tris = new();
        for (int i = 0; i <= segments; i++)
        {
            var offset = i * offsetPerSegment;
            for (int j = 0; j < vertices.Length; j++)
            {
                meshVerts.Add(vertices[j] + offset);

                if (i < segments && j < vertices.Length - 1)
                {
                    int bottomLeft = vertices.Length * i + j;
                    tris.Add(bottomLeft);
                    tris.Add(bottomLeft + 1);
                    tris.Add(bottomLeft + 1 + vertices.Length);

                    tris.Add(bottomLeft + 1 + vertices.Length);
                    tris.Add(bottomLeft + vertices.Length);
                    tris.Add(bottomLeft);
                }
            }
        }
        if (mesh == null)
        {
            mesh = new Mesh();
        }
        mesh.vertices = meshVerts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
