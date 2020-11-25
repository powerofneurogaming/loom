using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayer;
    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab);
            //GameObject child = Instantiate(localPlayerPrefab);
            _player.transform.SetParent(localPlayer.GetComponent<PlayerManager>().hmd.transform);
            _player.GetComponent<PlayerManager>().lefthand.transform.SetParent(localPlayer.GetComponent<PlayerManager>().lefthand.transform);
            _player.GetComponent<PlayerManager>().lefthand.transform.localPosition = new Vector3();
            _player.GetComponent<PlayerManager>().righthand.transform.SetParent(localPlayer.GetComponent<PlayerManager>().righthand.transform);
            _player.GetComponent<PlayerManager>().righthand.transform.localPosition = new Vector3();
            _player.AddComponent<PlayerController>();
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }
}
