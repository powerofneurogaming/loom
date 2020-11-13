using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDataController : Singleton<PlayerDataController>
{
    public GameObject Player;

    public string[] sceneList;
    public int currSceneIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);

        PlayerPrefs.SetString("player head", "human"); // this is the head that willl be used unless another is chosen
        Player = GameObject.FindGameObjectWithTag("Player");
        Vector3 offset = Player.transform.position - Camera.main.transform.position;
        offset.y = 0;
        Player.transform.position += offset;
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

    public void goScene(int i)
    {
        ChangeScene(sceneList[i]);
    }

    public void nextScene()
    {
        if (++currSceneIndex > sceneList.Length-1)
        {
            currSceneIndex = 0;
        }
        goScene(currSceneIndex);
    }

    public void lastScene()
    {
        if (--currSceneIndex < 0)
        {
            currSceneIndex = sceneList.Length-1;
        }
        goScene(currSceneIndex);
    }

    public void ConnectedToServer()
    {
        Client.instance.ConnectToServer();
    }
}
