using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadTracking : MonoBehaviour
{
    List<GameObject> Heads = new List<GameObject>();
    LineRenderer PlayerAim;
    Camera PlayerCamera;
    public bool PointerTracking = true;
    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform child in transform)
        {
            Heads.Add(child.gameObject);
        }
        PlayerAim = transform.parent.GetComponentInChildren<LineRenderer>();
        PlayerCamera = transform.parent.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = PlayerCamera.transform.position;
        var cam = GameObject.FindGameObjectWithTag("1PCamera");
        transform.position = cam.transform.position;
        if (PointerTracking)
        {
            transform.LookAt(PlayerAim.GetPosition(1));
        }
        else
        {
            transform.rotation = PlayerCamera.transform.rotation;
        }
    }

    public void SelectHead(int Index)
    {
        foreach(GameObject obj in Heads)
        {
            if (obj.transform.GetSiblingIndex() != Index)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }
}
