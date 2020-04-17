using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleRotation : MonoBehaviour
{
    public float z;
    public float y;
    public float x;
    void Update()
    {
        transform.Rotate(x, y, z);
    }
}
