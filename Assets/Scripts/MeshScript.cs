using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshScript : MonoBehaviour
{
    public Mesh mesh;
    public enum Direction { UP, DOWN, LEFT, RIGHT, FRONT, BACK }
    
    [Range(2, 1000)]
    public int vertexHeight = 10;
    
    [Range(2, 1000)]
    public int vertexWidth = 10;

    public Vector3 localUp = Vector3.up;

    public Color color = Color.magenta;

    private HalfEdgeMesh heMesh;
    
    public void OnValidate()
    {
        if (mesh != null && vertexHeight > 1 && vertexWidth > 1)
        {
            initMesh();
            heMesh = HalfEdgeMesh.loadMesh(mesh);
            foreach (var vertex in heMesh.vertices)
            {
                vertex.GetAdjacentHalfEdges();
            }
        }
    }

    public void OnDrawGizmos()
    {
        GizmosExtensions.DrawHalfEdgeGizmos(heMesh);
    }

    public static Vector3 getDir(Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
                return Vector3.up;
            case Direction.DOWN:
                return Vector3.down;
            case Direction.LEFT:
                return Vector3.left;
            case Direction.RIGHT:
                return Vector3.right;
            case Direction.FRONT:
                return Vector3.forward;
            case Direction.BACK:
                return Vector3.back;
        }
        return Vector3.up;
    }
    
    public void initMesh()
    {
        int index = 0;
        var numVertices = vertexHeight * vertexWidth;
        var numFaces = (vertexHeight - 1) * (vertexWidth - 1);
        Vector3[] vertices = new Vector3[numVertices];
        Color[] colors = new Color[numVertices];
        int[] faces = new int[numFaces * 6];
        
        // Generate vertices
        for (int x = 0; x < vertexHeight; x++)
        {
            for (int y = 0; y < vertexWidth; y++)
            {
                vertices[index] = ToWorldSpace(new Vector2(x, y));
                colors[index] = new Color(vertices[index].x, vertices[index].y, vertices[index].z, 1);
                index++;
            }
        }
        
        // Generate faces
        index = 0;
        for (int x = 0; x < vertexHeight - 1; x++)
        {
            for (int y = 0; y < vertexWidth - 1; y++)
            {
                faces[index] =     x * vertexWidth + y;
                faces[index + 1] = x * vertexWidth + y + 1;
                faces[index + 2] = x * vertexWidth + y + vertexWidth + 1;
                faces[index + 3] = x * vertexWidth + y;
                faces[index + 4] = x * vertexWidth + y + vertexWidth + 1;
                faces[index + 5] = x * vertexWidth + y + vertexWidth;
                index += 6;
            }
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = faces;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    private Vector3 ToWorldSpace(Vector2 localSpace)
    {
        var a = new Vector3(localUp.z, localUp.x, localUp.y);
        var b = Vector3.Cross(localUp, a);
        return localUp + 
               (localSpace.x / (vertexHeight - 1f) - .5f) * 2 * b + 
               (localSpace.y / (vertexWidth  - 1f) - .5f) * 2 * a;
    }
}
