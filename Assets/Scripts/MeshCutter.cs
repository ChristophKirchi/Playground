using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeshCutter : MonoBehaviour
{
    private Collider _collider;
    private Vector3 _entry;
    private Vector3 _exit;
    private Vector3 _direction;

    [Range(4, 255)]
    public int remeshResolution;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        GetHitPointInLocalSpace(other, ref _entry);
    }

    private void OnTriggerExit(Collider other)
    {
        GetHitPointInLocalSpace(other, ref _exit);
        _direction = other.transform.forward;
        CutMesh(other.gameObject);
    }

    private void GetHitPointInLocalSpace(Collider other, ref Vector3 hitPoint)
    {
        if (Physics.Raycast(other.transform.position, transform.position - other.transform.position, out var hit))
        {
            if (hit.transform.gameObject == other.gameObject)
            {
                hitPoint = other.transform.InverseTransformPoint(hit.point); 
            }
        }
    }

    void CutMesh(GameObject other)
    {
        // Calculate plane between points
        Vector3 plane = Vector3.Cross(_exit - _entry, _direction);
        // Determine which points are above / beyond the plane

        // 0) Duplicate GameObject and mesh
        var copy = Instantiate(other);
        var upperMesh = copy.GetComponent<MeshFilter>().mesh;
        var lowerMesh = other.GetComponent<MeshFilter>().mesh;
        var upperVertices = new List<Vector3>();
        var lowerVertices = new List<Vector3>();

        for (int i = 0; i < upperMesh.vertexCount; i++)
        {
            if (Vector3.Dot(upperMesh.vertices[i], plane) > 0)
            {
                upperVertices.Add(upperMesh.vertices[i]);
            }
            else
            {
                lowerVertices.Add(upperMesh.vertices[i]);
            }
        }
        
        // For upper points:
        // 1) Get bounding cut area, only use points above threshhold
        // 2) Fill the area with vertices
        // 3) Re-generate the faces
        // Lower points:
        // 4) Reuse 1) to 3), but below threshhold
    }

    void BoundingVertices(Mesh mesh, Vector3 plane, out List<Vector3> vertices)
    {
        vertices = new List<Vector3>();

        for (int x = 0; x < remeshResolution; x++)
        {
            for (int y = 0; y < remeshResolution; y++)
            {
                Vector2 normalized = new Vector2(x, y);
                normalized = (normalized / (remeshResolution - 1f) - new Vector2(.5f, .5f)) * 2;
                vertices.Add(plane + (_exit - _entry) * normalized.x + _direction * normalized.y);
            }
        }
    }
}