using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Unity.Editor;

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized = false;

    // Get a reference to the storage service, using the default Firebase App
    FirebaseStorage storage;

    string csvPath;

    // Start is called before the first frame update
    void Start()
    {
        // When the app starts, check to make sure that we have
        // the required dependencies to use Firebase, and if not,
        // add them if possible.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        //LogEvent("testEvent",Time.time, 0);

        csvPath = Application.persistentDataPath + "/Analytics/" + PlayerPrefs.GetString("PlayerID", "unknown") + "/Loom.csv";
        CreateNewHeader();
        PlayerPrefs.SetInt("LoomSC", PlayerPrefs.GetInt("LoomSC") + 1);

        storage = FirebaseStorage.GetInstance("gs://loomunity-c1269.appspot.com");
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPrefs.SetFloat("LoomTTP", PlayerPrefs.GetFloat("LoomTTP") + Time.deltaTime);
        PlayerPrefs.SetFloat("LoomSTP", PlayerPrefs.GetFloat("LoomSTP") + Time.deltaTime);
    }

    private void OnApplicationQuit()
    {
        FillEventLog("Exited Game");
        PlayerPrefs.SetFloat("LoomSTP", 0.0f);
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        // NOTE: You'll need to replace this url with your Firebase App's database
        // path in order for the database connection to work correctly in editor.

        app.SetEditorDatabaseUrl("https://loomunity-c1269.firebaseio.com/");
        if (app.Options.DatabaseUrl != null)
            app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        isFirebaseInitialized = true;
    }

    public void CreateNewHeader()
    {
        //GetSceneStrings(SceneManager.GetActiveScene().name);
        //Debug.Log("csv path: " + csvpath);

        Debug.Log("test csvpath: " + csvPath);
        if (!File.Exists(csvPath))
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Analytics/" + PlayerPrefs.GetString("PlayerID", "unknown")))
            {
                if (!Directory.Exists(Application.persistentDataPath + "/Analytics/"))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + "/Analytics/");
                }
                Directory.CreateDirectory(Application.persistentDataPath + "/Analytics/" + PlayerPrefs.GetString("PlayerID", "unknown"));
            }
            string[] HeaderText = new string[14];
            HeaderText[0] = "Timestamp";
            HeaderText[1] = "Participant";
            HeaderText[2] = "Game";
            HeaderText[3] = "Total Time Played";
            HeaderText[4] = "Session Number";
            HeaderText[5] = "Session Time Played";
            HeaderText[6] = "Level";
            HeaderText[7] = "Event";
            HeaderText[8] = "Player#";
            HeaderText[9] = "BlockType";
            HeaderText[10] = "Column";
            HeaderText[11] = "Row";
            HeaderText[12] = "Height";
            HeaderText[13] = "Zone";
            //recording = true;
            WritetoCSV(HeaderText);
        }
    }

    public void LogEventFirebase(string eventName, float time, int player, int type = 0, int col = 0, int row = 0, float height = 0.0f)
    {
        string difficulty = "";
        string levelName = "";
        int numPlayers = 0;

        //Point reference inside of game
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Players").Child(PlayerPrefs.GetString("PlayerID", "unknown"));

        // Create a new dictionary from the data array to store in reference
        Dictionary<string, object>  newLogMap = new Dictionary<string, object>();

        newLogMap["Event"] = eventName;
        newLogMap["Time"] = time;
        newLogMap["Player"] = player;
        newLogMap["Level"] = levelName;
        newLogMap["Difficulty"] = difficulty;
        newLogMap["NumPlayers"] = numPlayers;

        reference.Push().SetValueAsync(newLogMap);
    }

    //data format
    //0-time stamp
    //1-particpant ID
    //2-current game name
    //3-total time played
    //4-session number
    //5-session time played
    void FillBasicInfo(string[] Info) //this will fill the repetitive 
    {
        Info[0] = DateTime.Now.ToString();
        Info[1] = PlayerPrefs.GetString("PlayerID", "missing");
        Info[2] = "Loom";
        Info[3] = PlayerPrefs.GetFloat("LoomTTP").ToString();
        Info[4] = PlayerPrefs.GetInt("LoomSC").ToString();
        Info[5] = PlayerPrefs.GetFloat("LoomSTP").ToString();
        Info[6] = PlayerPrefs.GetString("gameLevel");
    }

    //function tobe called in other managers to log events
    public void FillEventLog(string eventName, int player = -1, SquareController square = null)
    {
        string[] LogText = new string[14];
        FillBasicInfo(LogText);
        LogText[7] = eventName;
        LogText[8] = player == -1 ? "" : player.ToString();

        //Fill in relevant data if square is provided, otherwise fill entries with empty strings
        if (square != null)
        {
            LogText[9] = square.type.ToString();
            LogText[10] = square.column == -1 ? "" : square.column.ToString();
            LogText[11] = square.row == -1 ? "" : square.row.ToString();
            //If the square is in/from the "Play" zone, give the y coord as height
            LogText[12] = square.zone == "Play" ? square.transform.position.y.ToString() : "";
            LogText[13] = square.zone;
        }
        else
        {
            LogText[9] = "";
            LogText[10] = "";
            LogText[11] = "";
            LogText[12] = "";
            LogText[13] = "";
        }
        WritetoCSV(LogText);

        //Turn on if storing csv files in Firebase
        //if (eventName.Contains("Exited"))
        //{
        //    StoreCSVToFirebase(csvPath);
        //}
    }

    string delimiter = ",";
    void WritetoCSV(string[] info)
    {
        info[7] = info[7].Replace(",", ";");
        string text = string.Join(delimiter, info);
        File.AppendAllText(csvPath, text + Environment.NewLine);
    }

    private void StoreCSVToFirebase(string path)
    {
        // Create a root reference
        StorageReference storage_ref = storage.RootReference;

        // Create a reference to the file you want to upload
        string childString = path.Substring(path.IndexOf("/Analytics/"));
        StorageReference csv_ref = storage_ref.Child(childString);

        // Upload the file to the path
        csv_ref.PutFileAsync(path);
    }
}
