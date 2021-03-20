using System;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdgeMesh
{
    public List<Face> faces = new List<Face>();
    public List<Vertex> vertices = new List<Vertex>();
    public List<HalfEdge> halfEdges = new List<HalfEdge>();

    public static HalfEdgeMesh loadMesh(Mesh mesh)
    {
        var heMesh = new HalfEdgeMesh();
        var map = new Dictionary<Tuple<int, int>, HalfEdge>();
        
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            heMesh.vertices.Add(new Vertex(i, new VertexData(mesh.vertices[i], Color.black)));
        }

        if (mesh.colors.Length == mesh.vertexCount)
        {
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                heMesh.vertices[i].data.color = mesh.colors[i];
            }
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            heMesh.faces.Add(new Face(i / 3, new FaceData(mesh.triangles[i], 
                                                              mesh.triangles[i + 1], 
                                                              mesh.triangles[i + 2])));
        }

        int index = 0;
        foreach (var face in heMesh.faces)
        {
            HalfEdge last = null;
            HalfEdge first = null;
            for (int i = 0; i < 3; i++)
            {
                HalfEdge he, hePair;

                var key = new Tuple<int, int>(heMesh.vertices[face.data.vertices[i]].id, heMesh.vertices[face.data.vertices[(i + 1) % 3]].id);
                if (map.ContainsKey(key))
                {
                    he = map[key];
                    map.Remove(key);
                    hePair = he.pair;
                }
                else
                {
                    he = new HalfEdge(index++);
                    hePair = new HalfEdge(index++);
                    heMesh.halfEdges.Add(he);
                    heMesh.halfEdges.Add(hePair);

                    he.pair = hePair;
                    hePair.pair = he;
                    
                    map.Add(new Tuple<int, int>(heMesh.vertices[face.data.vertices[(i + 1) % 3]].id, heMesh.vertices[face.data.vertices[i]].id), hePair);
                }
                
                he.face = face;
                he.origin = heMesh.vertices[face.data.vertices[i]];
                he.prev = last;
                if (first == null)
                {
                    first = he;
                }
                if (last != null)
                {
                    last.next = he;
                }
                last = he;

                heMesh.vertices[face.data.vertices[i]].halfEdge = he; 
                face.halfEdge = he;
            }

            first.prev = last;
            last.next = first;
        }

        foreach (var halfEdgePair in map)
        {
            halfEdgePair.Value.pair.pair = null;
            heMesh.halfEdges.Remove(halfEdgePair.Value);
        }
        map.Clear();

        return heMesh;
    }

    public Mesh ToUnityMesh()
    {
        return new Mesh();
    }

    public Vertex GetVertexAtPosition(Vector3 pos)
    {
        foreach (var vertex in vertices)
        {
            if (vertex.data.pos.Equals(pos))
            {
                return vertex;
            }
        }

        return null;
    }
}

public class Face
{
    public int id;
    public FaceData data;
    public HalfEdge halfEdge;
    
    public Face(int id, FaceData data)
    {
        this.id = id;
        this.data = data;
    }

    public List<HalfEdge> GetAdjacentHalfEdges()
    {
        var he = new List<HalfEdge>();
        HalfEdge start = halfEdge;
        HalfEdge next = start.next;
        he.Add(start);
        while (start != next)
        {
            he.Add(next);
            next = next.next;
        }
        
        return he;
    }

    public List<Vertex> GetAdjacentVertices()
    {
        var vertices = new List<Vertex>();
        HalfEdge start = halfEdge;
        HalfEdge next = start.next;
        vertices.Add(start.origin);
        while (start != next)
        {
            vertices.Add(next.origin);
            next = next.next;
        }
        
        return vertices;
    }
}

public class Vertex
{
    public int id;
    public VertexData data;
    public HalfEdge halfEdge;

    public Vertex(int id, VertexData data)
    {
        this.id = id;
        this.data = data;
    }

    public List<HalfEdge> GetAdjacentHalfEdges()
    {
        var he = new List<HalfEdge>();
        HalfEdge start = halfEdge;
        HalfEdge next = start.pair?.next;
        he.Add(start);
        while (next?.pair != null)
        {
            he.Add(next);
            next = next.pair.next;
            if (start == next) return he;
        }

        next = start.prev.pair;
        while (next?.prev.pair != null)
        {
            he.Add(next);
            next = next.prev.pair;
            if (start == next) return he;
        }
        
        return he;
    }

    public List<Face> GetAdjacentFaces()
    {
        var faces = new List<Face>();
        foreach (var he in GetAdjacentHalfEdges())
        {
            faces.Add(he.face);
        }
        
        return faces;
    }

    public bool IsVertexAtPosition(Vector3 pos)
    {
        return data.pos.Equals(pos);
    }
}

public class HalfEdge
{
    public int id;
    public HalfEdge pair, next, prev;
    public Vertex origin;
    public Face face;

    public HalfEdge(int id)
    {
        this.id = id;
    }
}

public class FaceData
{
    public int[] vertices = new int[3];

    public FaceData(int a, int b, int c)
    {
        vertices[0] = a;
        vertices[1] = b;
        vertices[2] = c;
    }
}

public class VertexData
{
    public Vector3 pos;
    public Color color;

    public VertexData(Vector3 pos, Color color)
    {
        this.pos = pos;
        this.color = color;
    }
}