//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using System.IO;

//public class LevelDesigner : Singleton<LevelDesigner>
//{
//    // Start is called before the first frame update
//    public InputField rowsInput;
//    public InputField columnsInput;
//    public GridLayoutGroup BuildScreen;

//    void Start()
//    {
//        LoadDesign();
//        Manager.instance.startLevel();
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
    
//    public GameObject button;
//    public void CreateBuildSCreenTiles()
//    {
//        int defaultColumn = 5;
//        int defaultRow = 5;
//        int column = 0;
//        int row = 0;
//        column = (string.IsNullOrEmpty(columnsInput.text)) ? defaultColumn : int.Parse(columnsInput.text);
//        row = (string.IsNullOrEmpty(rowsInput.text)) ? defaultRow : int.Parse(rowsInput.text);
//        count = row * column;
//        BuildScreen.constraintCount = column;
//        BuildScreen.transform.parent.gameObject.SetActive(true);
//        for (int i = 0; i < row * column; i++)
//        {
//            GameObject cube=Instantiate(button, transform.position, Quaternion.identity);
//            cube.transform.SetParent(BuildScreen.transform);
            
//        }
//    }
//    public Color selected = Color.white;
//    public void UpdateColor(Color color)
//    {
//        selected = color;
//    }
//    int blue;
//    int red;
//    int gold;
//    int white;
//    List<GameObject> allblocks = new List<GameObject>();
//    int x = 10;
//    int count = 0;
//    public void test()
//    {
//        int[,] array = new int[x, x];
//    }
//    public InputField saveName;
//    public void SaveDesign()
//    {
//        int columncount = BuildScreen.GetComponent<GridLayoutGroup>().constraintCount;
//        int currCount = 0;
//        blue = 0;
//        red = 0;
//        gold = 0;
//        white = 0;
//        string[,] array = new string[ count / BuildScreen.constraintCount,BuildScreen.constraintCount];

//        //CubeSpawner.instance.CubeDrops.Clear(); //clear the cube que before each load
//        PlayZoneController.instance.spawningPool = new List<SquareController.BlockTypes>();

//        foreach (Transform child in BuildScreen.transform)
//        {
//            Color color = child.GetComponent<Image>().color;
//            string colorstring;
//            if (color == Color.blue)
//            {
//                blue++;
//                //CubeSpawner.instance.AddCube(Color.blue);
//                PlayZoneController.instance.spawningPool.Add(SquareController.BlockTypes.Blue);
//                colorstring = "blue";
//            }
//            else if (color == Color.red)
//            {
//                red++;
//                //CubeSpawner.instance.AddCube(Color.red);
//                PlayZoneController.instance.spawningPool.Add(SquareController.BlockTypes.Red);
//                colorstring = "red";
//            }
//            else if (color == Color.yellow)
//            {
//                gold++;
//                //CubeSpawner.instance.AddCube(Color.yellow);
//                PlayZoneController.instance.spawningPool.Add(SquareController.BlockTypes.Gold);
//                colorstring = "yellow";
//            }
//            else
//            {
//                white++;
//                //CubeSpawner.instance.AddCube(Color.white);
//                PlayZoneController.instance.spawningPool.Add(SquareController.BlockTypes.Invisible);
//                colorstring = "white";
//            }
//            allblocks.Add(child.gameObject);
//            int rowcalc = Mathf.FloorToInt(currCount / BuildScreen.constraintCount);
//            int colcalc = currCount % BuildScreen.constraintCount;
//            Debug.Log(currCount + "\t" + rowcalc + "\t" + colcalc);
//            array[rowcalc,colcalc] = colorstring;

//            currCount++;
//        }
//        string jsonsave = JsonUtility.ToJson(array);
//        List<string> writestring = new List<string>();
//        foreach(string s in array)
//        {
//            writestring.Add(s);
//            Debug.Log(s);
//        }
//        //Debug.Log(array);
//        Debug.Log(jsonsave);
//        string path = Application.dataPath+"/Saves/save1.csv";
//        Debug.Log(path);
//        //using (FileStream fs = new FileStream(path, FileMode.Create))
//        //{
//        //    using(StreamWriter writer = new StreamWriter(fs))
//        //    {
//        //        writer.Write(jsonsave);
//        //    }
//        //}
//        string delimiter = "\n";
//        //string text = string.Join(delimiter, writes);
//        File.WriteAllLines(path, writestring);
       

//        Debug.Log(blue + "\t" + red + "\t" + gold);
//    }

//    public void LoadDesign()
//    {
        
//        string path = Application.dataPath + "/Saves/save1.csv";
//        string[] colors = File.ReadAllLines(path);
//        //foreach (string s in colors)
//        //{
//        //    Debug.Log(s);
//        //}
//        int size = (int)Mathf.Sqrt(colors.Length);
//        BuildScreen.constraintCount = size;
//        foreach (string s in colors)
//        {
//            GameObject buttonObj = Instantiate(button, transform.position, Quaternion.identity);
//            buttonObj.transform.SetParent(BuildScreen.transform);
//            if (s == "red")
//            {
//                buttonObj.GetComponent<Image>().color = Color.red;
//            }
//            else if (s == "blue")
//            {
//                buttonObj.GetComponent<Image>().color = Color.blue;
//            }
//            else if (s == "yellow")
//            {
//                buttonObj.GetComponent<Image>().color = Color.yellow;
//            }
//        }
//        count = colors.Length;
//    }
//}
