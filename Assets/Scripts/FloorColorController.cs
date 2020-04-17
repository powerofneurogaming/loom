using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorColorController : MonoBehaviour
{
    [SerializeField] private Color[] colorList;
    [SerializeField] private float timePerColor = 5.0f;
    private float time = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Rotates through colorList between one color and the next over a period timePerColor seconds
        time += Time.deltaTime / timePerColor;
        Color oldColor = colorList[Mathf.FloorToInt(time) % colorList.Length];
        Color newColor = colorList[Mathf.FloorToInt(time + 1f) % colorList.Length];
        float newT = time - Mathf.Floor(time);
        GetComponent<MeshRenderer>().materials[1].color = Color.Lerp(oldColor, newColor, newT);
    }
}
