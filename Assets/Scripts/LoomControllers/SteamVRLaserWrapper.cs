//=============================================================================
// SteamVRLaserWrapper
// Purpose: Used for Wrapping Laser Hit event from SteamVRLaserPointer_loom.cs
// Author: Robin Xu
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR.Extras;

[RequireComponent(typeof(SteamVRLaserPointer_loom))]
public class SteamVRLaserWrapper : MonoBehaviour
{
    private SteamVRLaserPointer_loom steamVrLaserPointer;

    private void Awake()
    {
        steamVrLaserPointer = gameObject.GetComponent<SteamVRLaserPointer_loom>();
        steamVrLaserPointer.PointerIn += OnPointerIn;
        steamVrLaserPointer.PointerOut += OnPointerOut;
        steamVrLaserPointer.PointerClick += OnPointerClick;
    }

    private void OnPointerClick(object sender, PointerEventArgs e)
    {
        IPointerClickHandler clickHandler = e.target.GetComponent<IPointerClickHandler>();
        if (clickHandler == null)
        {
            return;
        }

        ExecuteEvents.Execute(e.target.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
        //clickHandler.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    private void OnPointerOut(object sender, PointerEventArgs e)
    {
        IPointerExitHandler pointerExitHandler = e.target.GetComponent<IPointerExitHandler>();
        if (pointerExitHandler == null)
        {
            return;
        }
        ExecuteEvents.Execute(e.target.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
        //pointerExitHandler.OnPointerExit(new PointerEventData(EventSystem.current));
    }

    private void OnPointerIn(object sender, PointerEventArgs e)
    {
        IPointerEnterHandler pointerEnterHandler = e.target.GetComponent<IPointerEnterHandler>();
        if (pointerEnterHandler == null)
        {
            return;
        }

        ExecuteEvents.Execute(e.target.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
        //pointerEnterHandler.OnPointerEnter(new PointerEventData(EventSystem.current));
    }
}
