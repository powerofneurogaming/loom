using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;
using Doozy.Engine.UI;
using TMPro;

public class Manager : Singleton<Manager>
{

    public UIView startView;
    public UIView waitingView;
    public UIView gameOverView;
    public TextMeshProUGUI gameOverText;

    public static int NumPlayers = 1;
    public static string gameDifficulty;
    public GameObject[] Player;
    public ControllerVR[] vrControllers;

    public List<SquareController.BlockTypes> MP1Blocks;
    public List<SquareController.BlockTypes> MP2Blocks;

    public GameObject buildWall1;
    public GameObject buildWall2;
    public GameObject ViewWall1;
    public GameObject ViewWall2;

    public GameObject obj1;
    public GameObject obj2;
    public GameObject obj3;
    public GameObject obj4;
    public GameObject obj5;
    public float GameTimer = 0;
    public List<GameObject> objects = new List<GameObject>();

    public bool GamePaused = false;
    public TextMesh TimerText1;

    public TextMesh TimerText2;

    public List<GameObject>[] MasterPool = new List<GameObject>[6];

    public bool isHost = false;

    //Sets the state of the game over condition to 0, it's set to 1 for lose, and 2 for win. - Nathan
    int stateSet = 0;

    private void Awake()
    {
        //PlayerPrefs.SetInt("playerCount", 2);
    }

    void Start()
    {
        //StartCoroutine(ServerClientSetup(PlayerPrefs.GetString("playerChoice")));
        //BuildZoneController.instance.ConstructSection(5);
        SetPlayerHead(); 
        SetHost();
        BuildMasterPool();
        Player = GameObject.FindGameObjectsWithTag("Player");
        P1Laser = GameObject.FindGameObjectWithTag("SelfLine");
        P2Laser = GameObject.FindGameObjectWithTag("OtherLine");
        P2Laser.SetActive(false);
        GamePaused = true;
        Manager.instance.Swap();
    }
    /// <summary>
    /// builds the masterpool with predefined maxsize 15x15 and 40% max proportion
    /// </summary>
    /// 

    public void SetHost()
    {
        string player = PlayerPrefs.GetString("playerChoice");
        if (player == "P1")
        {
            isHost = true;
        }
        if(player == "P2")
        {
            isHost = false;
        }
        isHost = true;

    }
    public int GoldMergedCounte = 0;
    public Transform MasterPoolCubes;
    float cubeLimit = 1f;
    void BuildMasterPool() 
    {
        for (int i = 0; i < MasterPool.Length; i++)
        {
            MasterPool[i] = new List<GameObject>();
        }
        for (int i = 0; i < 15 * 15 * cubeLimit * 2; i++)
        {
            MasterPool[0].Add(Instantiate(PlayZoneController.instance.blueBlock, Vector3.right * 1000, Quaternion.identity));
            MasterPool[1].Add(Instantiate(PlayZoneController.instance.redBlock, Vector3.right * 1000, Quaternion.identity));
            MasterPool[2].Add(Instantiate(PlayZoneController.instance.invisBlock, Vector3.right * 1000, Quaternion.identity));
            MasterPool[3].Add(Instantiate(PlayZoneController.instance.goldBlock, Vector3.right * 1000, Quaternion.identity));
        }
        for(int i = 0; i < (15 * 15 + 2) * 2; i++)
        {
            MasterPool[3].Add(Instantiate(PlayZoneController.instance.goldBlock, Vector3.right * 1000, Quaternion.identity));
            MasterPool[4].Add(Instantiate(PlayZoneController.instance.goldBlockLeft, Vector3.right * 1000, Quaternion.identity));
            MasterPool[5].Add(Instantiate(PlayZoneController.instance.goldBlockRight, Vector3.right * 1000, Quaternion.identity));
        }

        int ind = 0;
        foreach (var o in MasterPool)
        {
            foreach (var x in o)
            {
                LSLServer.instance.objectsToStream.Add(new LSLServer.streamData(x, ind, false));
                x.transform.SetParent(MasterPoolCubes);
                ++ind;
            }
        }
    }

    float TimeLeft;
    public void StartTheGame(int numPlayers, string difficulty) // use the static vaiables for numpalyers and gamediffculty for these arguments
    {
        int num = PlayerPrefs.GetInt("playerCount");
        string dif = PlayerPrefs.GetString("gameDifficulty");
        Debug.Log(dif);
        Debug.Log(PlayerPrefs.GetString("gameLevel"));
        if(num == 2 && PlayerPrefs.GetString("playerChoice") == "P1")
        {
            LSLServer.instance.SetLevel(PlayerPrefs.GetString("gameDifficulty"));
            LSLServer.instance.SendCommand("Start Game", "");
        }
        PlayZoneController.instance.levelOver = false;
        MakeBuildWallandViewWall(num, dif);
        startView.Hide();
        waitingView.Hide();
        GamePaused = false;
        TimeLeft = SetupTimer(numPlayers, difficulty);
        PlayZoneController.instance.levelOver = false;
        PlayZoneController.instance.StartSpawning();
        AnalyticsManager.instance.FillEventLog("Game Started");
        //needs to send command to client if in two player mode. to start the the game on their side. 
    }

    public void RestartTheGame()
    {
        int num = PlayerPrefs.GetInt("playerCount");
        string dif = PlayerPrefs.GetString("gameDifficulty");
        if (num == 2 && PlayerPrefs.GetString("playerChoice") == "P1")
        {
            LSLServer.instance.SetLevel(PlayerPrefs.GetString("gameDifficulty"));
            LSLServer.instance.SendCommand("Restart Game", "");
        }
        Debug.Log("restarting game");
        PlayZoneController.instance.levelOver = false;
        GamePaused = false;
        TimeLeft = SetupTimer(NumPlayers, dif);
        PlayZoneController.instance.levelOver = false;
        PlayZoneController.instance.StartSpawning();

    }

    /// <summary>
    /// This function will swap the color blocks each player interacts with.
    /// If p1 can only grab red blocks, now it grabs blue, and vice versa for p2.
    /// </summary>
    public void Swap()
    {
        SquareController.BlockTypes buffer;
        var floors = GameObject.FindGameObjectWithTag("Floor");
        if (MP1Blocks[0] == SquareController.BlockTypes.Red)
        {
            floors.GetComponent<Renderer>().material.SetColor("_FloorColor", Color.blue);
        }
        else
        {
            floors.GetComponent<Renderer>().material.SetColor("_FloorColor", Color.red);
        }

        buffer = MP1Blocks[0];
        MP1Blocks[0] = MP2Blocks[0];
        MP2Blocks[0] = buffer;
    }

    float SetupTimer(int numPlayers, string difficulty)
    {
        float minutes = 0;
        GameTimer = 0;
        if (numPlayers == 1)
        {
            if (difficulty == "easy") { minutes = 2; }
            else if(difficulty == "medium") { minutes = 5; }
            else if(difficulty == "hard") { minutes = 10; }
        }
        else if(numPlayers == 2)
        {
            if (difficulty == "easy") { minutes = 2; }
            else if (difficulty == "medium") { minutes = 4; }
            else if (difficulty == "hard") { minutes = 8; }
        }
        return minutes * 60;
    }

    public IEnumerator StartCoRoutine()
    {
        yield return new WaitForSeconds(5);
        StartTheGame(PlayerPrefs.GetInt("playerCount"), PlayerPrefs.GetString("gameDifficulty"));
    }
    public IEnumerator ServerClientSetup(string playerNumber)
    {
        //playerNumber = "P2";
        PlayerPrefs.SetInt("playerCount", 2);
        if (PlayerPrefs.GetInt("playerCount") == 2)
        {
            if (playerNumber == "P1")
            {
                LSLServer.instance.SetServer();
                LSLServer.instance.SetStreaming(true);
                yield return new WaitForSeconds(20); // waiting for the client to start
                LSLServer.instance.ScanForPlayers();

            }
            if (playerNumber == "P2")
            {
                Debug.Log("player number is" + playerNumber);
                yield return new WaitForSeconds(10); // setting the server
                LSLServer.instance.StartClient();
                LSLServer.instance.SetStreaming(true);
                yield return new WaitForSeconds(20); // player 1 scanning for players
                LSLServer.instance.ScanForPlayers();

            }
        }
    }


    public void startLevel()
    {
        //Player[0].transform.position = new Vector3(7, 0, 0);
        //Player[0].transform.eulerAngles = new Vector3(0, -90, 0); //might change to transform.lookat later for 2 player aligning
        GameTimer = 0;
        SetupPlayer(isHost);
        //foreach (Transform child in LevelDesigner.instance.BuildScreen.transform)
        //{
        //    CubeSpawner.instance.AddCube(child.GetComponent<Image>().color);
        //    Debug.Log(child.GetComponent<Image>().color);
        //}
        //CubeSpawner.instance.dropCubes(2, 2);
    }

    float LevelSeconds;

    /// <summary>
    /// toggles which player prefab to turn on based on host/client selection
    /// </summary>
    /// <param name="host"></param>
    public void SetupPlayer(bool host)
    {
        if (host)
        {
            Player[0].SetActive(true);
            Player[1].SetActive(false);
            //Player[1].GetComponent<Player>().enabled = false;
            Player[0].GetComponentInChildren<ControllerVR>().PlayerValue(1);
            Player[1].GetComponentInChildren<ControllerVR>().PlayerValue(2);
        }
        else
        {
            Player[0].SetActive(true);
            Player[1].SetActive(false);
        }

    }

    string FormatLevelTimer(float currTime, float LevelTime)
    {
        string DisplayString = "";
        int timeInt = Mathf.FloorToInt(LevelTime-currTime);
        if (timeInt < 0)
        {
            if (stateSet == 0)
            {
                //Player Game oVer Music on time running out. - Nathan
                SoundManager gameOverMusic = GameObject.Find("SoundManager").GetComponent<SoundManager>();
                stateSet = 1;
                gameOverMusic.setState(stateSet);
            }

            gameOverView.Show();
            gameOverText.SetText("You have run out of time");
            return null;
        }
        int minutes = timeInt / 60;
        int seconds = timeInt % 60;
        DisplayString = minutes + " : " + seconds;
        return DisplayString;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }

        if (!GamePaused)
        {
            GameTimer += Time.deltaTime;
            TimerText1.text = FormatLevelTimer(GameTimer, TimeLeft);
            TimerText2.text = TimerText1.text;
        }
        

        if (Input.GetKeyDown(KeyCode.Home))
        {
            RestartLevel();
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Swap();
        }
    }
    public UIView PauseMenu1;
    public UIView PauseMenu2;
    public void PauseGame()
    {
       
        foreach (GameObject player in Player)
        {
            //player.transform.position += Vector3.left * 25 * (GamePaused ? -1 : 1);
            if (player.transform.position.x > 0)
            {
                player.transform.position += new Vector3(25, 0, 0) * (GamePaused ? -1 : 1);
            }
            else
            {
                player.transform.position += new Vector3(-25, 0, 0) * (GamePaused ? -1 : 1);
            }
        }
        GamePaused = !GamePaused;
        Time.timeScale = GamePaused ? 0 : 1;
        PauseMenu1.Show();
        PauseMenu2.Show();
    }

    public void MakeBuildWallandViewWall(int numPlayers, string diff)
    {
        int buildWallSize = 0;
        switch (diff)
        {
            case "easy":
                buildWallSize = 5;
                break;
            case "medium":
                buildWallSize = 10;
                break;
            case "hard":
                buildWallSize = 15;
                break;
        }
        PlayZoneController.instance.GeneratePoolListandViewWall(diff);
        BuildZoneController.instance.ConstructSection(buildWallSize, buildWall1, 0);

        if (numPlayers == 2)
        {
            BuildZoneController.instance.ConstructSection(buildWallSize, buildWall2, 1);
        }
        //if(numPlayers == 1)
        //{
        //    if (diff == "easy")
        //    {
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall1);
        //        BuildZoneController.instance.ConstructSection(5, buildWall1);
        //    }
        //    if (diff == "medium")
        //    {
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall1);
        //        BuildZoneController.instance.ConstructSection(10, buildWall1);
        //    }
        //    if (diff == "hard")
        //    {
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall1);
        //        BuildZoneController.instance.ConstructSection(15, buildWall1);
        //    }
        //}
        //else
        //{
        //    if (diff == "easy")
        //    {
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall1);
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall2);
        //        BuildZoneController.instance.ConstructSection(5, buildWall1);
        //        BuildZoneController.instance.ConstructSection(5, buildWall2);

        //    }
        //    if (diff == "medium")
        //    {
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall1);
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall2);
        //        BuildZoneController.instance.ConstructSection(10, buildWall2);
        //        BuildZoneController.instance.ConstructSection(10, buildWall2);

        //    }
        //    if (diff == "hard")
        //    {
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall1);
        //        PlayZoneController.instance.GeneratePoolListandViewWall(diff, ViewWall2);
        //        BuildZoneController.instance.ConstructSection(15, buildWall1);
        //        BuildZoneController.instance.ConstructSection(15, buildWall2);

        //    }
        //}

    }
    /// <summary>
    /// This function compares the play wall with the build wall to see if it's correct.
    /// </summary>
    public void CheckCorrectness()
    {
        GamePaused = true;
        int size = BuildZoneController.instance.buildGrid.Length;
        string str = "";
        string str2 = "";
        gameOverView.Show();

        for (int i = 0; i < size; i++)
        {
            //str += PlayZoneController.instance.cubeInts[i];
            if (BuildZoneController.instance.buildGrid[i].Count < size)// If there aren't enough blocks for a given row
            {
                Debug.Log("Row " + i + " is missing blocks...");
                Debug.Log("Level Incorrect");
                //show level incorrect here
                gameOverText.SetText("Oops... You need to fill in the whole grid");


                //Play the lose game over fanfare - Nathan
                SoundManager gameOverMusic = GameObject.Find("SoundManager").GetComponent<SoundManager>();
                stateSet = 1;
                gameOverMusic.setState(stateSet);

                return;
            }
            for (int j = 0; j < size; j++)
            {
                str += PlayZoneController.instance.ints[i, j].ToString();
                str2 += BuildZoneController.instance.buildGrid[j][size - 1 - i].type;
            }
        }
        Debug.Log(str);
        Debug.Log(str2);
        if (str == str2)
        {
            Debug.Log("Level Correct");
            //show the level complete screen here
            gameOverText.SetText("Congratulations! You win!");

            AnalyticsManager.instance.FillEventLog("Level Complete");

            //Play the win game over fanfare - Nathan
            SoundManager gameOverMusic = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            stateSet = 2;
            gameOverMusic.setState(stateSet);
        }
        else
        {
            Debug.Log("Level Incorrect");
            //show level incorrect here
            gameOverText.SetText("Oops... One of these things is not like the other");

            //Play the lose game over fanfare - Nathan
            SoundManager gameOverMusic = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            stateSet = 1;
            gameOverMusic.setState(stateSet);
        }
    }

    public GameObject P1Laser;
    public GameObject P2Laser;

    public void AssignSelfPointer(string handTag)
    {
        P1Laser.SetActive(true);
        ControllerVR P1Hand = P1Laser.GetComponent<ControllerVR>();
        P1Hand.MyHand = GameObject.FindGameObjectWithTag(handTag);
    }

    public void AddP2Pointer(string handTag)
    {
        P2Laser.SetActive(true);
        ControllerVR P2Hand = P2Laser.GetComponent<ControllerVR>();
        P2Hand.MyHand = GameObject.FindGameObjectWithTag(handTag);
    }

    /// <summary>
    /// reset the blocks in the masterpool
    /// clear the current build grid
    /// </summary>
    public void RestartLevel()
    {
        foreach(List<GameObject> pool in MasterPool)
        {
            foreach(GameObject obj in pool)
            {
                obj.transform.position = Vector3.right * 1000;
                obj.GetComponent<SquareController>().ResetInfo();
            }
        }
        foreach( var l in BuildZoneController.instance.buildGrid)
        {
            l.Clear();
        }
        GameTimer = 0;
        PlayZoneController.instance.levelOver = true;
        gameOverView.Show();

    }
    public GameObject mouseHead;
    public GameObject catHead;
    public GameObject humanHead;
    public void SetPlayerHead()
    {
        if (PlayerPrefs.GetString("player head") == "mouse")
        {
            mouseHead.SetActive(true);
        }
        if (PlayerPrefs.GetString("player head") == "human")
        {
            humanHead.SetActive(true);
        }
        if (PlayerPrefs.GetString("player head") == "cat")
        {
            catHead.SetActive(true);
        }
    }

    public void ToggleManagers(bool state)
    {
        List<Object> managers = new List<Object>();
        Object.FindObjectOfType<BuildZoneController>().gameObject.SetActive(state);
        Object.FindObjectOfType<PlayZoneController>().gameObject.SetActive(state);
        GameObject.FindGameObjectWithTag("SelfLine").GetComponent<LineRenderer>().enabled = state;
        GameObject.FindGameObjectWithTag("OtherLine").GetComponent<LineRenderer>().enabled = state;


    }
    private void OnApplicationQuit()
    {
        Debug.Log("I QUIT");
        //LSLServer.instance.DestroyAllStreams();
    }
}
