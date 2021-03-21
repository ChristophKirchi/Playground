using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshScript : MonoBehaviour
{
    public Mesh mesh;
    [Range(2, 1000)]
    public int vertexHeight = 10;
    
    [Range(2, 1000)]
    public int vertexWidth = 10;

    public Vector3 localUp = Vector3.up;

    public Color color = Color.magenta;

    public HalfEdgeMesh heMesh;

    public bool showHalfEdgeGizmos = false;
    public bool showVertexGizmos = false;
    public bool showFaceGizmos = false;
    
//    public void OnValidate()
//    {
//        if (mesh != null && vertexHeight > 1 && vertexWidth > 1)
//        {
//            initMesh();
//            heMesh = HalfEdgeMesh.LoadMesh(mesh);
//        }
//    }

    public void OnDrawGizmos()
    {
        GizmosExtensions.DrawHalfEdgeGizmos(heMesh, showHalfEdgeGizmos, showVertexGizmos, showFaceGizmos);
    }
    
    public void initMesh()
    {
        mesh = GetNewMesh(localUp, vertexWidth, vertexHeight, color);
    }

    private static Vector3 ToWorldSpace(Vector2 localSpace, int width, int height, Vector3 localUp)
    {
        var a = new Vector3(localUp.z, localUp.x, localUp.y);
        var b = Vector3.Cross(localUp, a);
        return localUp + 
               (localSpace.x / (height - 1f) - .5f) * 2 * b + 
               (localSpace.y / (width  - 1f) - .5f) * 2 * a;
    }

    public static Mesh GetNewMesh(Vector3 localUp, int width, int height, Color color)
    {
        var mesh = new Mesh();
        var index = 0;
        var numVertices = height * width;
        var numFaces = (height - 1) * (width - 1);
        var vertices = new Vector3[numVertices];
        var colors = new Color[numVertices];
        var uvs = new Vector2[numVertices];
        var faces = new int[numFaces * 6];
        
        // Generate vertices
        for (var x = 0; x < height; x++)
        {
            for (var y = 0; y < width; y++)
            {
                vertices[index] = ToWorldSpace(new Vector2(x, y), width, height, localUp).normalized;
                colors[index] = new Color(vertices[index].x / 2, vertices[index].y / 2, vertices[index].z / 2, 1);
                uvs[index] = new Vector2(x / (height - 1f), y / (width  - 1f));
                index++;
            }
        }
        
        // Generate faces
        index = 0;
        for (var x = 0; x < height - 1; x++)
        {
            for (var y = 0; y < width - 1; y++)
            {
                faces[index] =     x * width + y;
                faces[index + 1] = x * width + y + 1;
                faces[index + 2] = x * width + y + width + 1;
                faces[index + 3] = x * width + y;
                faces[index + 4] = x * width + y + width + 1;
                faces[index + 5] = x * width + y + width;
                index += 6;
            }
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = faces;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        return mesh;
    }
}
