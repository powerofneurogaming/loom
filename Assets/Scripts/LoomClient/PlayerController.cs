using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void FixedUpdate()
    {
        SendPlayerStatsToServer();
    }

    private void SendPlayerStatsToServer()
    {
        PlayerManager myPlayer = GameManager.players[Client.instance.myId];
        Vector3[] _statsVec4 = new Vector3[]
        {
            myPlayer.transform.position,
            myPlayer.lefthand.transform.position,
            myPlayer.righthand.transform.position,
        };
        Quaternion[] _statsQuat = new Quaternion[]
        {
            myPlayer.transform.rotation,
            myPlayer.lefthand.transform.rotation,
            myPlayer.righthand.transform.rotation,
        };

        ClientSend.PlayerStats(_statsVec4, _statsQuat);
    }
}
