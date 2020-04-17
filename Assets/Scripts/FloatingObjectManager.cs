using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObjectManager : MonoBehaviour
{
    public int numPoints = 1000;
    public float radius = 10;
    public float size = 1;

    public int xspin = 0;
    public int yspin = 0;
    public int zspin = 0;

    public GameObject[] floaties;

    bool enabled = true;
    public List<Vector3> points;
    //Adapted from: https://stackoverflow.com/a/26127012
    /// <summary>
    /// Draws spinning shapes in the background.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (enabled)
        {
            
            float rnd = 1f;
            float offset = 2f / numPoints;
            float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

            for (int i = 0; i < numPoints; i++)
            {
                float y = ((i * offset) - 1f) + (offset / 2f);
                float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2f));
                float phi = ((i + rnd) % numPoints) * increment;

                Vector3 point = new Vector3(Mathf.Cos(phi) * r * transform.localScale.x,
                                            y * transform.localScale.y,
                                            Mathf.Sin(phi) * r * transform.localScale.z) * 0.5f;
                Gizmos.DrawSphere(point * radius, size);
            }
        }
    }

    private void Start()
    {
        float rnd = 1f;
        float offset = 2f / numPoints;
        float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < numPoints; i++)
        {
            float y = ((i * offset) - 1f) + (offset / 2f);
            float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2f));
            float phi = ((i + rnd) % numPoints) * increment;

            Vector3 point = new Vector3(Mathf.Cos(phi) * r * transform.localScale.x,
                                        y * transform.localScale.y,
                                        Mathf.Sin(phi) * r * transform.localScale.z) * 0.5f;
            points.Add(point*radius);
            var obj=Instantiate(floaties[Random.Range(0, floaties.Length)], point*radius, Quaternion.identity);
            obj.transform.localScale *= 20;
            obj.transform.SetParent(transform);
        }
        enabled = false;
    }

    private void Update()
    {
        transform.eulerAngles += new Vector3(xspin, yspin, zspin) * Time.deltaTime;
    }



}
