//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class GrabColor : MonoBehaviour
//{
//    public Color selfcolor;
//    public LevelDesigner designer;
//    // Start is called before the first frame update
//    void Start()
//    {
//        selfcolor = GetComponent<Image>().color;
//        designer = GameObject.Find("Level Designer").GetComponent<LevelDesigner>();
//        switch (enu)
//        {
//            case MyEnum.Blue:
//                GetComponent<Image>().color = Color.blue;
//                break;
//            case MyEnum.Gold:
//                GetComponent<Image>().color = Color.yellow;
//                break;
//            case MyEnum.Red:
//                GetComponent<Image>().color = Color.red;
//                break;

//        }
//    }
//    public enum MyEnum
//    {
//        Red,Blue,Gold
//    }
//    public MyEnum enu = new MyEnum();
//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    public void Setcolor()
//    {
//        designer.selected = GetComponent<Image>().color;
//    }
//}
