using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticeCount : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GetComponent<MeshFilter>().mesh.vertexCount);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
