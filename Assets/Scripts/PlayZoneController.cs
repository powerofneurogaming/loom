using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayZoneController : Singleton<PlayZoneController>
{
    [SerializeField] public int cols = 6;
    [SerializeField] public float fallSpeed = 1.0f;
    [SerializeField] public float fallDistance = 10.0f;
    [SerializeField] public float dropRate = 2.0f;
    [SerializeField] public float squareSize = 1.0f;
    [SerializeField] public Vector3 offset;
    [SerializeField] public Vector3[] dropLocs;
    [SerializeField] public Vector3[] endLocs;
    public bool levelOver = false;

    [SerializeField] SquareController[] blockPrefabs; // refrances to the cube prefabs
    public List<SquareController> spawningPool = new List<SquareController>(); // list of pool blocks
    public List<SquareController>[] mirrorPool = new List<SquareController>[6];
    public List<SquareController> goldLeftPool = new List<SquareController>();
    public List<SquareController> goldRightPool = new List<SquareController>();
    public List<SquareController> goldMergePool = new List<SquareController>();
    public List<SquareController> fallingBlocks = new List<SquareController>(); // list of falling blocks
    public int[] cubeInts; // raw cube input for filling view wall and checking at submit

    public GameObject ViewWall1;
    public GameObject ViewWall2;
    public GameObject blueBlock;
    public GameObject redBlock;
    public GameObject goldBlock;
    public GameObject goldBlockLeft;
    public GameObject goldBlockRight;
    public GameObject invisBlock;

    public float RecycleHeight = -1.0f;

    int blueAdded;
    int redAdded;
    int goldAdded;
    int invisAddd;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("sdf");

        endLocs = new Vector3[cols];
        dropLocs = new Vector3[cols];
        for (int i = 0; i < cols; i++)
        {
            dropLocs[i] = transform.position - offset + transform.parent.forward * squareSize * i;
            endLocs[i] = dropLocs[i];
            endLocs[i].y = -1;
        }
        for (int i = 0; i < mirrorPool.Length; i++)
        {
            mirrorPool[i] = new List<SquareController>();
        }

        //Test, eventually call from manager after difficulty and level are picked
        //GeneratePoolListandViewWall("medium");
        //if (PlayerPrefs.GetString("playerChoice") == "P1")
        //{
        //    StartSpawning(/*spawningPool*/);
        //}
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartSpawning(/*List<SquareController> newPool*/)  //commented out argument since it was not getting used
    {
        //spawningPool = newPool;
        StartCoroutine(SpawnBlocks());
    }

    IEnumerator SpawnBlocks()
    {
        while (!levelOver)
        {
            if (spawningPool.Count != 0)
            {
                DropBlock(spawningPool[Random.Range(0, spawningPool.Count)], Random.Range(0, cols));
            }
            yield return new WaitForSeconds(dropRate);
        }
    }

    public void DropBlock(SquareController block, int col)
    {
        fallingBlocks.Add(block);
        block.transform.position = dropLocs[col];
        block.SetZone(true, "Play", endLocs[col], col);
        spawningPool.Remove(block);

        //Tell the sound script to play the corresponding sound
        //at the blockDropSounds AudioSource. -Nathan
        //REMOVED, TRENT SAYS TOO MUCH STIMULI
        //SoundManager.instance.soundPlay(0, col + 2);
    }

    public void RecycleBlock(SquareController block)
    {
        fallingBlocks.Remove(block);
        if (block.isMirror)
        {
            mirrorPool[block.type].Add(block);
        }
        else
        {
            switch (block.type)
            {
                case 2:
                    if (block.zone == "Build" && Manager.NumPlayers == 2)
                    {
                        goldMergePool.Add(block);
                    }
                    else
                    {
                        spawningPool.Add(block);
                    }
                    break;
                case 3:
                    goldLeftPool.Add(block);
                    break;
                case 4:
                    goldRightPool.Add(block);
                    break;
                default:
                    spawningPool.Add(block);
                    break;
            }
        }

        block.playerHolds[0] = false;
        block.playerHolds[1] = false;
        block.gameObject.layer = 1;
        block.transform.position = new Vector3(1000, 0, 0);
        block.SetZone(false, "Pool", new Vector3(1000, 0, 0));
        block.dirty = false;
    }

    public SquareController GetMirror(SquareController square)
    {
        SquareController mirror = mirrorPool[square.type][0];
        mirrorPool[square.type].Remove(mirror);
        mirror.isMirror = true;
        mirror.mirror = square;
        return mirror;
    }

    public SquareController GetGoldMerge(SquareController square)
    {
        SquareController goldMerge = goldMergePool[0];
        goldMergePool.Remove(goldMerge);
        goldMerge.row = square.row;
        goldMerge.column = square.column;
        return goldMerge;
    }

    // Handles logic for gold block pickup
    public void PickupGold(SquareController gold)
    {
        Debug.Log("gold split called");
        // Splits gold block in half and gives a half to each player
        SquareController goldL = goldLeftPool[0];
        SquareController goldR = goldRightPool[0];
        goldLeftPool.Remove(goldL);
        goldRightPool.Remove(goldR);
        goldL.transform.position = gold.transform.position;
        goldR.transform.position = gold.transform.position;
        //Manager.instance.vrControllers[0].SetGrabbedObject(goldR.gameObject);
        //Manager.instance.vrControllers[1].SetGrabbedObject(goldL.gameObject);
        Manager.instance.P1Laser.GetComponent<ControllerVR>().SetGrabbedObject(goldR.gameObject);
        Manager.instance.P2Laser.GetComponent<ControllerVR>().SetGrabbedObject(goldL.gameObject);
        RecycleBlock(gold);
        AnalyticsManager.instance.FillEventLog("Gold Block Split");

        //Tell the SoundManager script to play the corresponding sound
        //at the blockGrabSounds AudioSource. -Nathan
        SoundManager.instance.soundPlay(0, 6);
    }

    public string GetDifficultyMap(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.txt");
        for(int i = 0; i< info.Length; i++)
        {
            //Debug.Log(info[i]);
        }
        return info[Random.Range(0, info.Length)].ToString();
    }

    int difficultyRow;
    int difficultyCol;
    int difficultyCount;
    public int[,] ints;
    public string player2Path;
    public void GeneratePoolListandViewWall(string difficulty)
    {
        int counter = 0;
        int blueCounter = 0;
        int redCounter = 0;
        int goldCounter = 0;
        int invisCounter = 0;
        int blueAdded = 0;
        int redAdded = 0;
        int goldAdded = 0;
        int invisAddd = 0;
        string path = "";
        string[] mapData;

        if (difficulty == "easy")
        {
            difficultyCount = 25;
            difficultyRow = 5;
            difficultyCol = 5;
            path = "Assets/Difficulty Settings/Easy";
        }
        else if (difficulty == "medium")
        {
            difficultyCount = 100;
            difficultyRow = 10;
            difficultyCol = 10;
            path = "Assets/Difficulty Settings/Medium";
        }
        else if (difficulty == "hard")
        {
            difficultyCount = 225;
            difficultyRow = 15;
            difficultyCol = 15;
            path = "Assets/Difficulty Settings/Hard";
        }
        if (PlayerPrefs.GetInt("playerCount") == 1)
        {
            path = GetDifficultyMap(path);
        }
        else
        {
            path = PlayerPrefs.GetString("gameLevel");
        }
        mapData = File.ReadAllLines(path);

        //clear the viewalls if they are not empty
        if (ViewWall1.transform.childCount > 0)
        {
            foreach(Transform child in ViewWall1.transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (ViewWall2.transform.childCount > 0)
        {
            foreach (Transform child in ViewWall2.transform)
            {
                Destroy(child.gameObject);
            }
        }

        ints = new int[difficultyRow, difficultyCol];
        for (int i = 0; i < difficultyRow; i++)
        {
            for (int g = 0; g < difficultyCol; g++)
            {
                char c = mapData[i][g];
                string temp = c + "";
                if (temp == "1")
                {
                    ints[i, g] = 0;
                    blueCounter++;
                    GameObject obj=Instantiate(blueBlock, ViewWall1.transform.position + new Vector3(-g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall1.transform);
                    obj = Instantiate(blueBlock, ViewWall2.transform.position + new Vector3(g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall2.transform);
                }
                else if (temp == "2")
                {
                    ints[i, g] = 1;
                    redCounter++;
                    GameObject obj = Instantiate(redBlock, ViewWall1.transform.position + new Vector3(-g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall1.transform);
                    obj = Instantiate(redBlock, ViewWall2.transform.position + new Vector3(g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall2.transform);
                }
                else if (temp == "3")
                {
                    ints[i, g] = 2;
                    goldCounter++;
                    GameObject obj = Instantiate(goldBlock, ViewWall1.transform.position + new Vector3(-g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall1.transform);
                    obj = Instantiate(goldBlock, ViewWall2.transform.position + new Vector3(g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall2.transform);
                }
                else if (temp == "4")
                {
                    ints[i, g] = 5;
                    invisCounter++;
                    GameObject obj = Instantiate(invisBlock, ViewWall1.transform.position + new Vector3(-g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall1.transform);
                    obj = Instantiate(invisBlock, ViewWall2.transform.position + new Vector3(g, -i, 0), Quaternion.identity);
                    obj.layer = 2; obj.tag = "Untagged";
                    obj.transform.SetParent(ViewWall2.transform);
                }

              //  Debug.Log(temp);
                counter++;
            }
        }

        blueAdded = Mathf.RoundToInt(blueCounter * 1.3f);
        redAdded = Mathf.RoundToInt(redCounter * 1.3f);
        goldAdded = Mathf.RoundToInt(goldCounter * 1.3f);
        invisAddd = Mathf.RoundToInt(invisCounter * 1.3f);

        // spawingPool = new int[25 + blueCounter + redCounter + goldCounter + invisCounter];

        //clear the pools incase they are not empty
        spawningPool.Clear();
        goldLeftPool.Clear();
        goldRightPool.Clear();
        goldMergePool.Clear();
        foreach(List<SquareController> mirrorList in mirrorPool)
        {
            mirrorList.Clear();
        }

        //Fills in spawningPool etc with blocks from the Master Pool
        for (int i = 1; i <= blueAdded; i++)
        {
            spawningPool.Add(Manager.instance.MasterPool[0][i].GetComponent<SquareController>());
            mirrorPool[0].Add(Manager.instance.MasterPool[0][blueAdded + i].GetComponent<SquareController>());
        }
        for (int i = 1; i <= redAdded; i++)
        {
            spawningPool.Add(Manager.instance.MasterPool[1][i].GetComponent<SquareController>());
            mirrorPool[1].Add(Manager.instance.MasterPool[1][redAdded + i].GetComponent<SquareController>());
        }
        for (int i = 1; i <= goldAdded; i++)
        {
            spawningPool.Add(Manager.instance.MasterPool[3][i].GetComponent<SquareController>());
        }
        int maxGoldHalves = 226; // Maximum number of gold halves in the game, should always be the max number of squares + 1 so that gold half blocks don't stop working.
        for (int i = 1; i <= maxGoldHalves; i++)
        {
            mirrorPool[2].Add(Manager.instance.MasterPool[3][goldAdded + i].GetComponent<SquareController>());
            goldMergePool.Add(Manager.instance.MasterPool[3][goldAdded + maxGoldHalves + i].GetComponent<SquareController>());
            goldLeftPool.Add(Manager.instance.MasterPool[4][i].GetComponent<SquareController>());
            mirrorPool[3].Add(Manager.instance.MasterPool[4][maxGoldHalves + i].GetComponent<SquareController>());
            goldRightPool.Add(Manager.instance.MasterPool[5][i].GetComponent<SquareController>());
            mirrorPool[4].Add(Manager.instance.MasterPool[5][maxGoldHalves + i].GetComponent<SquareController>());
        }
        for (int i = 1; i <= invisAddd; i++)
        {
            spawningPool.Add(Manager.instance.MasterPool[2][i].GetComponent<SquareController>());
            mirrorPool[5].Add(Manager.instance.MasterPool[2][invisAddd + i].GetComponent<SquareController>());
        }
    }
}
