using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Material meshMaterial;
    
    [Range(2, 25)]
    public int vertexHeight = 10;
    
    [Range(2, 25)]
    public int vertexWidth = 10;

    private void OnValidate()
    {
        foreach (Transform obj in transform)
        {
            StartCoroutine(Destroy(obj.gameObject));
        }
        
        GameObject newObj = new GameObject("Cube");
        var mesh = MeshScript.GetNewMesh(Vector3.up, vertexWidth, vertexHeight, Color.black);
        HalfEdgeMesh heMesh = null;
        Vector3[] directions = {Vector3.down, Vector3.back, Vector3.forward, Vector3.right, Vector3.left};
        foreach (var dir in directions)
        {
            var meshIter = MeshScript.GetNewMesh(dir, vertexWidth, vertexHeight, Color.black);
            heMesh = HalfEdgeMesh.CombineMeshes(ref mesh, meshIter);
        }
        
        var meshScript = newObj.AddComponent<MeshScript>();
        newObj.transform.parent = transform;
        newObj.AddComponent<MeshRenderer>().sharedMaterial = meshMaterial;
        newObj.AddComponent<MeshFilter>().sharedMesh = mesh;
        meshScript.mesh = mesh;
        meshScript.localUp = Vector3.up;
        meshScript.vertexHeight = vertexHeight;
        meshScript.vertexWidth = vertexWidth;
        meshScript.initMesh();
        meshScript.heMesh = heMesh;
    }
    
    IEnumerator Destroy(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(go);
    }
}
