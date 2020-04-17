using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : Singleton<TitleScreenManager>
{

    Camera playerCamera;
    GameObject Player;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("player head", "human"); // this is the head that willl be used unless another is chosen
        playerCamera = Camera.main;
        Player = GameObject.FindGameObjectWithTag("Player");
        offset = Player.transform.position - playerCamera.transform.position;
        offset.y = 0;
        Player.transform.position += offset;
        //Vector3 positionOffset = UnityEngine.XR.InputTracking.GetLocalPosition(UnityEngine.XR.XRNode.Head);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene(string scene)
    {
        Destroy(GameObject.FindGameObjectWithTag("Player"));
        SceneManager.LoadScene(scene);
    }



}
