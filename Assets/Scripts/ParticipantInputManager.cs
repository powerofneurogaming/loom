using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using TMPro;
using System;
using System.IO;

public class ParticipantInputManager : Singleton<ParticipantInputManager>
{
    public string PlayerID1;
    public string PlayerID2;
    public TMP_InputField inputField;
    public TMP_Text inputText;
    public static bool loggedIN = false;
    string pathHead;
    string UserListPath;
    public TMP_Dropdown UserDropDown;
    public GameObject PlayerCountScreen;
    public GameObject PlayerSelectScreen;
    public GameObject InputScreen;
    public GameObject StartScreen;
    public GameObject PauseScreen;

    /// <summary>
    /// this is the participant input manager
    /// manages the player interaction with UI and backends
    /// PUT UI MENU SHOW/HIDE INSIDE BUTTON CLICKS
    /// </summary>

    // Start is called before the first frame update
    void Start()
    {
        pathHead = Application.dataPath + "\\Analytics";
        UserListPath = Application.dataPath + "/Userlist.txt";
        PlayerCountScreen.SetActive(false);
        PlayerSelectScreen.SetActive(false);
        InputScreen.SetActive(false);
        StartScreen.SetActive(false);
        PauseScreen.SetActive(false);

        if (!File.Exists(UserListPath))
        {
            UserDropDown.ClearOptions();
            InputScreen.SetActive(true);
        }
        else if (File.Exists(UserListPath))
        {
            string[] readText = File.ReadAllLines(UserListPath);
            UserDropDown.ClearOptions();
            foreach(string s in readText)
            {
                UserDropDown.AddOptions(new List<string> { s });
            }
            
        }
    }

    /// <summary>
    /// BUTTON LIST FOR ALL UI SCREENS
    /// Player Count Screen
    ///     - 1 Player
    ///     - 2 Player
    /// 
    /// Select Character Screen
    ///     -DropDown list of players
    ///     -New Player Button
    ///     
    /// Create User Screen
    ///     -UserName Input box
    ///     -Confirm Button
    ///     
    /// Difficulty Select Screen
    ///     -Easy
    ///     -Medium
    ///     -Hard
    ///    
    /// Game Start Screen
    ///     -START Button
    /// </summary>

    //Player Count Screen
    public void PlayerCountButton(int PlayerCount)
    {
        PlayerPrefs.SetInt("PlayerCount", PlayerCount);
    }

    //Select Character Screen
    public string[] array;
    public void LoginBUtton()
    {
        string selectedUserName = UserDropDown.captionText.text;
        if (selectedUserName == PlayerID1)
        {

        }
        else
        {
            string savingUserPath = pathHead + "/" + PlayerID1 + "/";
            string text = string.Join(Environment.NewLine, GetAllPlayerPrefs());
            File.WriteAllText(savingUserPath + "save.txt", text);

            PlayerID1 = selectedUserName;
            string seekUserPath = pathHead+"/"+selectedUserName+"/save.txt";
            LoadPlayerPrefs(seekUserPath, selectedUserName);
            array = GetAllPlayerPrefs();
            
        }
    }

    public void NewPlayerButton()
    {
        string savingUserPath = pathHead + "/" + PlayerID1 + "/";
        string text = string.Join(Environment.NewLine, GetAllPlayerPrefs());
        File.WriteAllText(savingUserPath + "save.txt", text);

        //input screen stuff here
    }

    //Create User Screen
    void RegisterUser()
    {
        int currentIndex = 1; //set this somwhere else later to change between player1 vs player2 creation
        TakeUserName(currentIndex);
        PlayerIDEnter();
        //AnalyticsManager.instance.WeeklySummary();
    }

    public void PlayerIDEnter()
    {
        //set all the player prefs initial here
        PlayerPrefs.SetString("TimeStamp", System.DateTime.Now.ToString());
        PlayerPrefs.SetString("SummaryGenDate", DateTime.Today.AddDays(1).ToString());
        PlayerPrefs.SetString("PlaceHolderSummaryGenDate", DateTime.Today.ToString("d"));
        PlayerPrefs.SetString("PlayerID1", PlayerID1);
        PlayerPrefs.SetString("PlayerID2", PlayerID2);
        //PlayerPrefs.SetString("TotalGameTime", AnalyticsManager.instance.GetTotalGameTime());
        //PlayerPrefs.SetString("SessionCount", AnalyticsManager.instance.GetSessionCount());
        //PlayerPrefs.SetString("GetSessionGameTime", AnalyticsManager.instance.GetSessionGameTime());
    }

    public void TakeUserName(int PlayerIndex)
    {
        if (PlayerIndex == 1)
        {
            PlayerID1 = inputField.text;
        }
        else
        {
            PlayerID2 = inputField.text;
        }
    }

    void updateUserList(string NewUserID)
    {
        string path = Application.dataPath + "/UserList.txt";
        List<string> saveinfo = new List<string>();
        if (File.Exists(path))
        {
            //saveinfo = File.ReadAllLines(path).ToString();
        }

        saveinfo.Add(NewUserID);
        saveinfo.Sort();
        UserDropDown.ClearOptions();
        UserDropDown.AddOptions(saveinfo);
        File.WriteAllLines(path, saveinfo.ToArray());
        RegisterUser();
    }

    //Difficulty Select Screen
    public void SelectGameDifficulty(string difficulty)
    {
        PlayerPrefs.SetString("Difficulty", difficulty);
    }

    //PlayGame Screen
    public void playGame()
    {
        //call to manager to start game
        //might need settings to be selected first
    }

    /// <summary>
    /// gets all the current players prefs, add to its list if you need
    /// </summary>
    string[] GetAllPlayerPrefs()
    {
        string[] PrefList =
        {
            PlayerPrefs.GetString("PlayerID1",PlayerID1),
            PlayerPrefs.GetString("timestamp"),
            PlayerPrefs.GetString("SummaryGenDate",DateTime.Today.AddDays(1).ToString()),
            PlayerPrefs.GetString("PlaceHolderSummaryGenDate"),

            //game data player prefs go here
        };
        return PrefList;
    }

    void LoadPlayerPrefs(string path,string username)
    {
        if (File.Exists(path))
        {
            string[] saveinfo = File.ReadAllLines(path);
            //load the data into playerpref
            //will be added when list is consolidated
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Home))
        {
            array = GetAllPlayerPrefs();
        }
    }
}
