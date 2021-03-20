using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewScript : MonoBehaviour
{
    private Vector3 _start;

    void Start()
    {
        _start = transform.position;
    }
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var camera = FindObjectOfType<Camera>();
            var normalized = Input.mousePosition;
            normalized.x = (normalized.x / (1920f) - .5f) * 2;
            normalized.y = (normalized.y / (1080f) - .5f) * 2;
            transform.position = _start + camera.transform.up * normalized.y + camera.transform.right * normalized.x;
        }
    }
}
