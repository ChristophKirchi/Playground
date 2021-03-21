using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HalfEdgeMesh
{
    public List<Face> faces = new List<Face>();
    public List<Vertex> vertices = new List<Vertex>();
    public List<HalfEdge> halfEdges = new List<HalfEdge>();

    public static HalfEdgeMesh LoadMesh(Mesh mesh)
    {
        var heMesh = new HalfEdgeMesh();
        var map = new Dictionary<Tuple<int, int>, HalfEdge>();
        
        for (var i = 0; i < mesh.vertexCount; i++)
        {
            heMesh.vertices.Add(new Vertex(i, new VertexData(mesh.vertices[i], Color.black)));
        }

        if (mesh.colors.Length == mesh.vertexCount)
        {
            for (var i = 0; i < mesh.vertexCount; i++)
            {
                heMesh.vertices[i].data.color = mesh.colors[i];
            }
        }        
        if (mesh.uv.Length == mesh.vertexCount)
        {
            for (var i = 0; i < mesh.vertexCount; i++)
            {
                heMesh.vertices[i].data.uv = mesh.uv[i];
            }
        }

        for (var i = 0; i < mesh.triangles.Length; i += 3)
        {
            heMesh.faces.Add(new Face(i / 3, new FaceData(mesh.triangles[i], 
                                                              mesh.triangles[i + 1], 
                                                              mesh.triangles[i + 2])));
        }

        var index = 0;
        foreach (var face in heMesh.faces)
        {
            HalfEdge last = null;
            HalfEdge first = null;
            for (var i = 0; i < 3; i++)
            {
                HalfEdge he;

                var key = new Tuple<int, int>(heMesh.vertices[face.data.vertices[i]].id, heMesh.vertices[face.data.vertices[(i + 1) % 3]].id);
                if (map.ContainsKey(key))
                {
                    he = map[key];
                    map.Remove(key);
                }
                else
                {
                    he = new HalfEdge(index++);
                    var hePair = new HalfEdge(index++);
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
    public static HalfEdgeMesh CombineMeshes(ref Mesh meshA, Mesh meshB)
    {
        if (meshA == null || meshB == null)
        { 
            return null;
        }
        
        var newVertices = new List<Vector3>();
        var newFaces = new List<int>();
        var newColors = new List<Color>();
        var newUVs = new List<Vector2>();
        var meshAHalfEdge = LoadMesh(meshA);
        var meshBHalfEdge = LoadMesh(meshB);

        newVertices.AddRange(meshA.vertices);
        newFaces.AddRange(meshA.triangles);
        newColors.AddRange(meshA.colors);
        newUVs.AddRange(meshA.uv);

        var vertexCount = newVertices.Count;
        var replace = new Dictionary<int, int>();
        
        foreach (var v in meshBHalfEdge.vertices)
        {
            var vertex = meshAHalfEdge.GetVertexAtPosition(v.data.pos);
            if (vertex != null)
            {
                replace.Add(v.id, vertex.id);
                vertex.data.color = (vertex.data.color + v.data.color) / 2;
            }
            else
            {
                replace.Add(v.id, newVertices.Count);
                newVertices.Add(v.data.pos);
                newColors.Add(v.data.color);
                newUVs.Add(v.data.uv);
            }
        }

        foreach (var face in meshBHalfEdge.faces)
        {
            newFaces.AddRange(face.data.vertices.Select(v => replace.ContainsKey(v) ? replace[v] : (v + vertexCount)));
        }

        var mesh = new Mesh
        {
            vertices = newVertices.ToArray(), triangles = newFaces.ToArray(), 
            colors = newColors.ToArray(), uv = newUVs.ToArray()
        };
        mesh.RecalculateNormals();
        
        meshA = mesh;
        return LoadMesh(mesh);
    }
    
    public Mesh ToUnityMesh()
    {
        var mesh = new Mesh();
        var listVertices = new List<Vector3>();
        var listColors = new List<Color>();
        var listUVs = new List<Vector2>();
        var listFaces = new List<int>();

        foreach (var vertex in vertices)
        {
            listVertices.Add(vertex.data.pos);
            listColors.Add(vertex.data.color);
            listUVs.Add(vertex.data.uv);
        }

        mesh.vertices = listVertices.ToArray();
        mesh.colors = listColors.ToArray();
        mesh.uv = listUVs.ToArray();

        foreach (var face in faces)
        {
            listFaces.AddRange(face.data.vertices);
        }

        mesh.triangles = listFaces.ToArray();
        return mesh;
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
    public Vector2 uv;

    public VertexData(Vector3 pos, Color color)
    {
        this.pos = pos;
        this.color = color;
        this.uv = new Vector2();
    }
    
    public VertexData(Vector3 pos, Color color, Vector2 uv)
    {
        this.pos = pos;
        this.color = color;
        this.uv = uv;
    }
}