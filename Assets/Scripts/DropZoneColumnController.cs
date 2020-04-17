using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class DropZoneColumnController : MonoBehaviour
{
    public GameObject hand;
    public int column;
    public BuildZoneController buildZone;
    public PlayZoneController playZone;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    SteamVR_Action_Boolean grabPinch = hand.GetComponent<Hand>().grabPinchAction;
    //    SteamVR_Input_Sources handType = hand.GetComponent<Hand>().handType;

    //    SquareController square = other.gameObject.GetComponent<SquareController>();
    //    //Tell square what column this drop zone is, namely so it doens't get deleted/recycled if it is dropped
    //    square.dropColumn = column;

    //    //if grab action is let go
    //    if (grabPinch.GetStateUp(handType) && square.zone == "Play")
    //    {
    //        bool canFit = buildZone.AddSquare(column, square);
    //        if (!canFit)
    //        {
    //            playZone.RecycleBlock(square.type);
    //        }
    //        Destroy(square.gameObject);
    //    }
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    SquareController square = other.gameObject.GetComponent<SquareController>();
    //    //Tell square that it is not in a drop column ie: -1 for not existing
    //    square.dropColumn = -1;
    //}
}
