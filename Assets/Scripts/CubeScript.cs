using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Material meshMaterial;
    
    [SerializeField, HideInInspector]
    private MeshScript[] _meshScripts;
    private void OnValidate()
    {
        if (_meshScripts == null)
        {
            _meshScripts = new MeshScript[6];
        }

        for (int i = 0; i < 6; i++)
        {
            if (_meshScripts[i] == null)
            {
                GameObject obj = new GameObject("Face: " + (MeshScript.Direction) i);
                obj.transform.parent = transform;
                obj.AddComponent<MeshRenderer>().sharedMaterial = meshMaterial;
                var mesh = new Mesh();
                obj.AddComponent<MeshFilter>().sharedMesh = mesh;
                _meshScripts[i] = obj.AddComponent<MeshScript>();
                _meshScripts[i].mesh = mesh;
                _meshScripts[i].localUp = MeshScript.getDir((MeshScript.Direction) i);
                _meshScripts[i].initMesh();
            }
            else
            {
                _meshScripts[i].GetComponent<MeshRenderer>().sharedMaterial = meshMaterial;
            }
        }
    }
}
