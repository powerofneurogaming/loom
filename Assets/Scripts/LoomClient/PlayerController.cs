using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerManager myPlayer;

    private void Awake()
    {
        if (!myPlayer)
            myPlayer = this.GetComponent<PlayerManager>();
        if (!myPlayer)
        {
            Debug.LogError("ERR: init PlayerController. cannot find the player manager of local player");
            return;
        }
    }
    private void FixedUpdate()
    {
        SendPlayerStatsToServer();
    }

    private void SendPlayerStatsToServer()
    {
        if (!myPlayer)
        {
            Debug.LogError("ERR: SendPlayerStatsToServer. No player manager for local player");
            return;
        }

        //PlayerManager myPlayer = GameManager.players[Client.instance.myId];
        Vector3[] _statsVec4 = new Vector3[]
        {
            myPlayer.transform.position,
            (myPlayer.lefthand.transform.position != null ? myPlayer.lefthand.transform.position : new Vector3()),
            (myPlayer.righthand.transform.position != null ? myPlayer.righthand.transform.position : new Vector3()),
        };
        Quaternion[] _statsQuat = new Quaternion[]
        {
            myPlayer.transform.rotation,
            (myPlayer.lefthand.transform.rotation != null ? myPlayer.lefthand.transform.rotation : new Quaternion()),
            (myPlayer.righthand.transform.rotation != null ? myPlayer.righthand.transform.rotation : new Quaternion()),
        };

        ClientSend.PlayerStats(_statsVec4, _statsQuat);
    }
}
