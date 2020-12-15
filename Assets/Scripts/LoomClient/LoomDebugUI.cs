//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Debug UI shown for the player
//
//=============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections;


//-------------------------------------------------------------------------
public class LoomDebugUI : MonoBehaviour
{
	private LoomVRPlayer player;

	//-------------------------------------------------
	static private LoomVRPlayer _instance;
	static public LoomVRPlayer instance
	{
		get
		{
			if ( _instance == null )
			{
				_instance = GameObject.FindObjectOfType<LoomVRPlayer>();
			}
			return _instance;
		}
	}


	//-------------------------------------------------
	void Start()
	{
		player = LoomVRPlayer.instance;
	}


#if !HIDE_DEBUG_UI
    //-------------------------------------------------
    private void OnGUI()
	{
        if (Debug.isDebugBuild)
        {
            player.Draw2DDebug();
        }
    }
#endif
}
