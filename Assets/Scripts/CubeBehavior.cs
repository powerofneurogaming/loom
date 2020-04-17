using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float speed=0;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.down *speed* Time.deltaTime);
        if (transform.position.y < -2f)
        {
            CubeSpawner.instance.AddCube(gameObject.GetComponent<MeshRenderer>().material.color);
            Destroy(this.gameObject);
        }
    }
}
