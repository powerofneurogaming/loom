//Joe Snider
//1/20
//
//Coordinate streaming data across the network.
//1 object stream, and mark server/client (make sure there's only 1 server).
//   The server owns all the objects and tells the clients where they are.
//2 hands and 1 head stream are owned by each instance and broadcast to all listeners.
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using LSL;
using UnityEngine.SceneManagement;
using Doozy.Engine.UI;


public class LSLServer : Singleton<LSLServer>
{
    [System.Serializable]
    public struct streamData
    {
        public GameObject gameObject;
        //index into streamableObjects, no checking
        public int type;
        //only stream dirty objects
        public bool dirty;

        public streamData(GameObject o, int t, bool d)
        {
            gameObject = o;
            type = t;
            dirty = d;
        }
    };
    //put all the objects to stream here. The server owns all the objects, each client owns the head/hands.
    //one head and two hands hacked in
    [Header("server coordinates these objects")]
    public List<streamData> objectsToStream;

    [Header("Only 1 server on the network")]
    public bool isServer = false;

    [Header("head and two hands to coordinate")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Safe after a few seconds.")]
    public bool isStreaming = false;

    [Header("Render clients as these.")]
    public List<GameObject> headRendersObjects;
    public List<GameObject> leftHandRendersObjects;
    public List<GameObject> rightHandRendersObjects;
    private int lastRenderedClient = 0;

    //make this unique to the device (Unity's dev id is ok, or ip address might work)
    //set first thing in start.
    private string id;

    //object streams have the following ID and their source looks like <objectIdentifier><streamName>
    private string objectStreamID = "id_pos_rot_scale";
    private string objectIdentifier = "object_";
    liblsl.StreamInfo lslObjectInfo;
    liblsl.StreamOutlet lslObjectOutlet;
    liblsl.StreamInlet lslObjectInlet;


    private string soundStreamID = "sourceID_SoundID";
    private string soundIdentifier = "sound_";
    liblsl.StreamInfo lslSoundInfo;
    liblsl.StreamOutlet lslSoundOutlet;
    liblsl.StreamInlet lslSoundInlet;

    private string triggerStreamID = "sourceID_TriggerID";
    private string triggerIdentifier = "trigger_";
    liblsl.StreamInfo lslTriggerInfo;
    liblsl.StreamOutlet lslTriggerOutlet;
    liblsl.StreamInlet lslTriggerInlet;

    //head/hand streams have the following ID and their source looks like <objectIdentifier><streamName>
    //hackish if someone with more than two hands or one head shows up
    private string headStreamID = "head_pos_rot_scale";
    private string headIdentifier = "head_";
    liblsl.StreamInfo lslHeadInfo;
    liblsl.StreamOutlet lslHeadOutlet;
    List<liblsl.StreamInlet> lslHeadInlet = new List<liblsl.StreamInlet>();

    private string leftHandStreamID = "left_hand_pos_rot_scale";
    private string leftHandIdentifier = "left_hand_";
    liblsl.StreamInfo lslLeftHandInfo;
    liblsl.StreamOutlet lslLeftHandOutlet;
    List<liblsl.StreamInlet> lslLeftHandInlet = new List<liblsl.StreamInlet>();

    private string rightHandStreamID = "right_hand_pos_rot_scale";
    private string rightHandIdentifier = "right_hand_";
    liblsl.StreamInfo lslRightHandInfo;
    liblsl.StreamOutlet lslRightHandOutlet;
    List<liblsl.StreamInlet> lslRightHandInlet = new List<liblsl.StreamInlet>();

    private string ControllerTriggerID = "cleint_controller_trigger";
    private string ControllerTriggerIdentifier = "client_trigger";
    liblsl.StreamInfo lslClientTriggerInfo;
    liblsl.StreamOutlet lslClientTriggerOutlet;
    liblsl.StreamInlet lslClientTriggerInlet;

    private Dictionary<string, GameObject> heads = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> leftHands = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> rightHands = new Dictionary<string, GameObject>();

    //max number of objects read each frame. may tweak lower.
    private int maxBufLen = 1;

    private int objectDataSize = 10;
    private int headhandDataSize = 6;
    private int soundDataSize = 2;
    private int triggerDataSize = 1;
    private int networkDataSize = 2;

    public UIView startView;
    public UIView waitingView;

    public int headIndex= 0;


    private double streamSearchTime = 1.0;//s

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("playerCount") == 1) // if the game is in single player mode then this will turn off all network syncing
        {
            gameObject.SetActive(false);
            waitingView.Hide();
            startView.Show();
            return;
        }

        id = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("Unique ID: " + id);
        objectBuffer = new float[objectDataSize];
        headhandBuffer = new float[headhandDataSize];
        soundBuffer = new float[soundDataSize];
        triggerBuffer = new float[triggerDataSize];
        networkBuffer = new string[networkDataSize];

        //only needed for testing on single device (same server and client)
        string letters = "qwertyuiopasdfghjklzxcvbnm";
        //for (int i = 0; i < 10; ++i) { id += letters[Random.Range(0, letters.Length)]; }

        lslHeadInfo = new liblsl.StreamInfo("StreamHead", headStreamID, headhandDataSize, 0,
            liblsl.channel_format_t.cf_float32, headIdentifier + id);
        lslHeadOutlet = new liblsl.StreamOutlet(lslHeadInfo, 0, maxBufLen);
        
        lslLeftHandInfo = new liblsl.StreamInfo("StreamLeftHand", leftHandStreamID, headhandDataSize, 0,
            liblsl.channel_format_t.cf_float32, leftHandIdentifier + id);
        lslLeftHandOutlet = new liblsl.StreamOutlet(lslLeftHandInfo, 0, maxBufLen);
        
        lslRightHandInfo = new liblsl.StreamInfo("StreamRightHand", rightHandStreamID, headhandDataSize, 0,
            liblsl.channel_format_t.cf_float32, rightHandIdentifier + id);
        lslRightHandOutlet = new liblsl.StreamOutlet(lslRightHandInfo, 0, maxBufLen);

        lslNetworkInfo = new liblsl.StreamInfo("StreamNetwork", networkStreamID, networkDataSize, 0,
                liblsl.channel_format_t.cf_string, networkIdentifier + id);
        lslNetworkOutlet = new liblsl.StreamOutlet(lslNetworkInfo, 0, maxBufLen);

        lslTriggerInfo = new liblsl.StreamInfo("StreamTriggers", triggerStreamID, triggerDataSize, 0,
                liblsl.channel_format_t.cf_float32, triggerIdentifier + id);
        lslTriggerOutlet = new liblsl.StreamOutlet(lslTriggerInfo, 0, maxBufLen);

        startAudioStream();

        StartCoroutine(HandleEyeHandSamples());
        StartCoroutine(HandleTriggerSamples());
        StartCoroutine(HandleNetworkSamples());
        //PlayerPrefs.SetString("playerChoice", "P2");
        isServer = PlayerPrefs.GetString("playerChoice") == "P1";
        if (isServer)
        {
            SetServer();
        }
        else
        {
            StartCoroutine(HandleSoundSamples());
            StartCoroutine(ScanForNetwork());
        }
        GameObject.FindGameObjectWithTag("SelfLine").GetComponent<ControllerVR>().StartSendingTriggger();
    }

    private void startAudioStream()
    {
        lslSoundInfo = new liblsl.StreamInfo("StreamSounds", soundStreamID, soundDataSize, 0,
                liblsl.channel_format_t.cf_float32, soundIdentifier + id);
        lslSoundOutlet = new liblsl.StreamOutlet(lslSoundInfo, 0, maxBufLen);
    }

    private Coroutine clientReader = null;
    public void StartClient()
    {
        if (clientReader == null)
        {
            Debug.Log("gh1");
            ScanForObjectStream();
            if (lslObjectInlet != null)
            {
                clientReader = StartCoroutine(HandleObjectSamples());
            }
            else
            {
                Debug.LogWarning("Warning: could not find an object stream ... check the server and try again.");
            }
        }
        else
        {
            Debug.LogWarning("Warning: stream already started ... ignoring and continuing");
        }
        
    }

    public int lastObjectFramesMissed = 0;
    /// <summary>
    /// client only read
    /// </summary>
    /// <returns>coroutine</returns>
    public IEnumerator HandleObjectSamples()
    {
        yield return new WaitForSeconds(1);
        float[] buffer = new float[objectDataSize];
        Vector3 pos = new Vector3();
        Quaternion q = new Quaternion();
        Vector3 rot = new Vector3();
        Vector3 scale = new Vector3();
        double t = 0;
        while (true)
        {
            if (isServer)
            {
                break;
            }

            //non-blocking, but clear everything each frame
            //set maxBufLen to decrease total allowed to be in pull_sample. 
            lastObjectFramesMissed = -1;
            t = lslObjectInlet.pull_sample(buffer, 0.0);
            while (t > 0)
            {
                pos.Set(buffer[1], buffer[2], buffer[3]);
                rot.Set(buffer[4], buffer[5], buffer[6]);
                q.eulerAngles = rot;
                scale.Set(buffer[7], buffer[8], buffer[9]);
                objectsToStream[(int)(buffer[0] + 0.5f)].gameObject.transform.SetPositionAndRotation(pos, q);
                t = lslObjectInlet.pull_sample(buffer, 0.0);
                ++lastObjectFramesMissed;
            }
            yield return null;
        }
        yield return null;
    }

    public int lastHeadFramesMissed = 0;
    public IEnumerator HandleEyeHandSamples()
    {
        //mostly for luck
        yield return new WaitForSeconds(1);
        float[] buffer = new float[headhandDataSize];
        Vector3 pos = new Vector3();
        Quaternion q = new Quaternion();
        Vector3 rot = new Vector3();
        double t = 0;
        while (true)
        {
            //usual hack here for two hands and one head (venusians can write their own VR:)
            //non-blocking, but clear everything each frame
            //set maxBufLen to decrease total allowed to be in pull_sample. 
            lastHeadFramesMissed = -1;
            foreach (var h in lslHeadInlet)
            {
                t = h.pull_sample(buffer, 0.0);
                while (t > 0)
                {
                    pos.Set(buffer[0], buffer[1], buffer[2]);
                    rot.Set(buffer[3], buffer[4], buffer[5]);
                    q.eulerAngles = rot;
                    heads[h.info().source_id()].transform.SetPositionAndRotation(pos, q);
                    t = h.pull_sample(buffer, 0.0);
                    ++lastHeadFramesMissed;
                }
            }
            foreach (var h in lslLeftHandInlet)
            {
                t = h.pull_sample(buffer, 0.0);
                while (t > 0)
                {
                    pos.Set(buffer[0], buffer[1], buffer[2]);
                    rot.Set(buffer[3], buffer[4], buffer[5]);
                    q.eulerAngles = rot;
                    leftHands[h.info().source_id()].transform.SetPositionAndRotation(pos, q);
                    t = h.pull_sample(buffer, 0.0);
                    ++lastHeadFramesMissed;
                }
            }
            foreach (var h in lslRightHandInlet)
            {
                t = h.pull_sample(buffer, 0.0);
                while (t > 0)
                {
                    pos.Set(buffer[0], buffer[1], buffer[2]);
                    rot.Set(buffer[3], buffer[4], buffer[5]);
                    q.eulerAngles = rot;
                    rightHands[h.info().source_id()].transform.SetPositionAndRotation(pos, q);
                    t = h.pull_sample(buffer, 0.0);
                    ++lastHeadFramesMissed;
                }
            }
            yield return null;
        }
    }

    public IEnumerator HandleSoundSamples()
    {
        yield return new WaitForSeconds(1);
        float[] buffer = new float[soundDataSize];
        int sourceID;
        int soundID;
        int lastSoundFramesMissed = 0;
        double t = 0;
        while (true)
        {
            lastSoundFramesMissed = -1;
            if (lslSoundInlet != null)
            {
                t = lslSoundInlet.pull_sample(buffer, 0.0);
                while (t > 0)
                {
                    sourceID = (int)buffer[0];
                    soundID = (int)buffer[1];
                    SoundManager.instance.soundPlay(sourceID, soundID);
                    t = lslSoundInlet.pull_sample(buffer, 0.0);
                    ++lastSoundFramesMissed;
                }
            }
            yield return null;
        }
    }

    public IEnumerator HandleTriggerSamples()
    {
        yield return new WaitForSeconds(1);
        float[] buffer = new float[triggerDataSize];
        int lastTriggerFramesMissed = 0;
        double t = 0;
        while (true)
        {
            lastTriggerFramesMissed = -1;
            if (lslTriggerInlet != null)
            {
                t = lslTriggerInlet.pull_sample(buffer, 0.0);
                while (t > 0)
                {
                    //Manager.instance.P2Laser.GetComponent<ControllerVR>().isPresed = (t>0);
//                    Debug.Log(buffer[0]);
                    Manager.instance.P2Laser.GetComponent<ControllerVR>().isPresed = (buffer[0]==1)?true:false;
                    ++lastTriggerFramesMissed;
                    t = lslTriggerInlet.pull_sample(buffer, 0.0);
                    yield return null;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// Add streaming objects at run time.
    /// TODO: This is trickier than it looks. 
    ///       Have to coordinate server and client.
    ///       Prefer adding directly to the list in the editor.
    /// </summary>
    /// <param name="g">Gameobject to add</param>
    public void AddStreamingObject(GameObject g)
    {
        streamData s = new streamData();
        s.gameObject = g;
        s.type = objectsToStream.Count;
        objectsToStream.Add(s);
    }

    /// <summary>
    /// Set this as the server. A checkbox at startup or something would work. Or read from a config file, or etc...
    /// Starts up the object outlet stream
    /// </summary>
    public void SetServer()
    {
        isServer = true;
        if (isServer)
        {
            Debug.Log("Starting device as server.");
            //this stream will have all the objects that are enabled to stream.
            lslObjectInfo = new liblsl.StreamInfo("StreamObjects", objectStreamID, objectDataSize, 0,
                liblsl.channel_format_t.cf_float32, objectIdentifier + id);
            lslObjectOutlet = new liblsl.StreamOutlet(lslObjectInfo, 0, maxBufLen);
            Debug.Log("started stream: " + lslObjectOutlet.info().source_id());
            Manager.instance.isHost = true;
            StartCoroutine(ScanForNetwork());
        }
    }

    /// <summary>
    /// For the server this starts the objects and the head/hands.
    /// For clients, just the head/hands
    /// </summary>
    /// <param name="s">streaming state</param>
    public void SetStreaming(bool s)
    {
        isStreaming = s;
    }

    /// <summary>
    /// Scan for players, each of which should be sending data from VR tracking.
    /// Does not include itself or anyone already added.
    /// </summary>
    public void ScanForPlayers()
    {
        SendCommand("Set Player Head", PlayerPrefs.GetString("player head"));
        Debug.Log("Searching for players.");
        liblsl.StreamInfo[] allInlets = liblsl.resolve_streams(streamSearchTime);
        Debug.Log("Done searching for players. Found " + allInlets.Length + " streams.");
        foreach (var s in allInlets)
        {
            string streamType = s.type();
            string streamSourceId = s.source_id();
            Debug.Log("   stream id: " + streamSourceId);
            if (!IsMe(streamSourceId) && !AlreadyFoundPlayer(streamSourceId))
            {
                if (streamSourceId.StartsWith(headIdentifier))
                {
                    lslHeadInlet.Add(new liblsl.StreamInlet(s));
                    var obj = Instantiate(headRendersObjects[headIndex]);

                    //var obj = Instantiate(headRendersObjects[lastRenderedClient % headRendersObjects.Count]);
                    obj.transform.localScale = Vector3.one * 1;
                    obj.tag = (Manager.instance.isHost) ? "P2Head" : "P1Head";
                    head.gameObject.tag = (!Manager.instance.isHost) ? "P2Head" : "P1Head";
                    heads.Add(streamSourceId, obj);
                }
                else if (streamSourceId.StartsWith(leftHandIdentifier))
                {
                    lslLeftHandInlet.Add(new liblsl.StreamInlet(s));
                    var obj = Instantiate(leftHandRendersObjects[lastRenderedClient % leftHandRendersObjects.Count]);
                    obj.transform.localScale = Vector3.one * 1;
                    obj.tag = (Manager.instance.isHost) ? "P2LHand" : "P1LHand";
                    leftHand.gameObject.tag = (!Manager.instance.isHost) ? "P2LHand" : "P1LHand";
                    leftHands.Add(streamSourceId, obj);
                }
                else if (streamSourceId.StartsWith(rightHandIdentifier))
                {
                    lslRightHandInlet.Add(new liblsl.StreamInlet(s));
                    var obj = Instantiate(rightHandRendersObjects[lastRenderedClient % rightHandRendersObjects.Count]);
                    obj.transform.localScale = Vector3.one * 1;
                    obj.tag = (Manager.instance.isHost) ? "P2RHand" : "P1RHand";
                    rightHand.gameObject.tag = (!Manager.instance.isHost) ? "P2RHand" : "P1RHand";
                    rightHands.Add(streamSourceId, obj);
                    if (Manager.instance.isHost)
                    {
                        Manager.instance.AssignSelfPointer("P1RHand");
                        Manager.instance.AddP2Pointer("P2RHand");
                    }
                    else
                    {
                        Manager.instance.AssignSelfPointer("P2RHand");
                        Manager.instance.AddP2Pointer("P1RHand");
                    }
                }
                else if (streamSourceId.StartsWith(triggerIdentifier))
                {
                        lslTriggerInlet = new liblsl.StreamInlet(s);
                        Debug.Log("created triggerInlet");
                }
                //Just want to cycle through them randomly-ish
                ++lastRenderedClient;
            }
        }
        ScanForSoundStream();
    }

    void ScanForSoundStream()
    {
        if (isServer)
        {
            Debug.LogError("Error: only the clients should receive the sound location stream ... continuing.");
            return;
        }
        Debug.Log("Searching for sound streams.");
        liblsl.StreamInfo[] allInlets = liblsl.resolve_streams(streamSearchTime);
        Debug.Log("Done searching for streams. Found " + allInlets.Length + " streams.");
        foreach (var s in allInlets)
        {
            string streamType = s.type();
            string streamSourceId = s.source_id();
            Debug.Log("   stream id: " + streamSourceId);
            if (streamSourceId.StartsWith(soundIdentifier) && !IsMe(streamSourceId))
            {
                lslSoundInlet = new liblsl.StreamInlet(s);
            }
        }
    }


    private bool IsMe(string s)
    {
        return s.EndsWith(id);
    }

    /// <summary>
    /// Check each of heads and hands. Not real fast, or meant to be called often.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private bool AlreadyFoundPlayer(string s)
    {
        foreach (var inlet in lslHeadInlet)
        {
            if (inlet.info().source_id() == s) { return true; }
        }
        foreach (var inlet in lslLeftHandInlet)
        {
            if (inlet.info().source_id() == s) { return true; }
        }
        foreach (var inlet in lslRightHandInlet)
        {
            if (inlet.info().source_id() == s) { return true; }
        }
        return false;
    }

    /// <summary>
    /// On the client: scan for an object stream coming from the server.
    /// Not real safe to call this multiple times.
    /// Server crash is probably bad.
    /// </summary>
    void ScanForObjectStream()
    {
        bool hasObjectStream = false;
        if (isServer)
        {
            Debug.LogError("Error: only the clients should receive the object location stream ... continuing.");
            return;
        }
        Debug.Log("Searching for object streams.");
        liblsl.StreamInfo[] allInlets = liblsl.resolve_streams(streamSearchTime);
        Debug.Log("Done searching for streams. Found " + allInlets.Length + " streams.");
        foreach (var s in allInlets)
        {
            string streamType = s.type();
            string streamSourceId = s.source_id();
            Debug.Log("   stream id: " + streamSourceId);
            if (streamSourceId.StartsWith(objectIdentifier))
            {
                lslObjectInlet = new liblsl.StreamInlet(s);
                hasObjectStream = true;
            }
        }
        Debug.Log("Has ObjectStream: " + hasObjectStream);
    }

    public void SendSound(int sourceID, int soundID)
    {
        if (isStreaming)
        {
            soundBuffer[0] = sourceID;
            soundBuffer[1] = soundID;
            lslSoundOutlet.push_sample(soundBuffer);
        }
    }
    
    public void SendTrigger(int pressed)
    {
        if (isStreaming)
        {
            triggerBuffer[0] = pressed;
            lslTriggerOutlet.push_sample(triggerBuffer);
        }
    }

    private string networkStreamID = "command_value";
    private string networkIdentifier = "network_";
    liblsl.StreamInfo lslNetworkInfo;
    liblsl.StreamOutlet lslNetworkOutlet;
    liblsl.StreamInlet lslNetworkInlet;
    string[] networkBuffer;
    public IEnumerator HandleNetworkSamples()
    {
        yield return new WaitForSeconds(1);
        string[] buffer = new string[networkDataSize];
        string command;
        string value;
        int lastNetworkFramesMissed = 0;
        double t = 0;
        while (true)
        {
            if (lslNetworkInlet != null)
            {
                lastNetworkFramesMissed = -1;
                t = lslNetworkInlet.pull_sample(buffer, 0.0);
                while (t > 0)
                {
                    command = buffer[0];
                    value = buffer[1];
                    Debug.Log("Received command: " + command);
                    //Do something with command
                    switch (command)
                    {
                        case "P1 Set Difficulty":
                            difficulty = value;
                            PlayerPrefs.SetString("gameDifficulty", value);
                            break;
                        case "P1 Set Level":
                            level = value;
                            PlayZoneController.instance.player2Path = value;
                            PlayerPrefs.SetString("gameLevel", value);
                            break;
                        case "P1 Ready":
                            ready[0] = true;
                            if (!isServer)
                            {
                                SendCommand(command, "");
                            }
                            break;
                        case "P2 Ready":
                            ready[1] = true;
                            if (isServer)
                            {
                                SendCommand(command, "");
                            }
                            break;
                        case "Start Game":
                            waitingView.Hide();
                            Manager.instance.StartTheGame(PlayerPrefs.GetInt("playerCount"), PlayerPrefs.GetString("gameDifficulty"));
                            break;
                        case "Restart Game":
                            Manager.instance.RestartTheGame();
                            break;
                        case "Set Player Head":
                            SetClientHead(value);
                            Debug.Log(value + "   : this was the head sent");
                            break;
                        default:
                            Debug.Log("Can't find command: " + command);
                            break;
                    }
                    t = lslNetworkInlet.pull_sample(buffer, 0.0);
                    ++lastNetworkFramesMissed;
                }
            }
            yield return null;
        }
    }
    public void SetClientHead(string value)
    {
        if(value == "human")
        {
            headIndex = 0;
        }
        if (value == "mouse")
        {
            headIndex = 1;
        }
        if (value == "cat")
        {
            headIndex = 2;
        }

    }

    public IEnumerator ScanForNetwork()
    {
        while (lslNetworkInlet == null)
        {
            Debug.Log("Searching for network streams.");
            liblsl.StreamInfo[] allInlets = liblsl.resolve_streams(streamSearchTime);
            Debug.Log("Done searching for streams. Found " + allInlets.Length + " streams.");
            foreach (var s in allInlets)
            {
                string streamType = s.type();
                string streamSourceId = s.source_id();
                Debug.Log("   stream id: " + streamSourceId);
                if (streamSourceId.StartsWith(networkIdentifier) && !IsMe(streamSourceId))
                {
                    lslNetworkInlet = new liblsl.StreamInlet(s);
                }
            }
            yield return new WaitForSeconds(5);
        }

        do
        {
            if (lslNetworkInlet != null)
            {
                Debug.Log("Other player found");
                SendCommand((isServer ? "P1" : "P2") + " Ready", "");
            }
            yield return new WaitForSeconds(1);
        }
        while (!(ready[0] && ready[1]));

        ScanForPlayers();
        if (isServer)
        {
            PlayZoneController.instance.levelOver = false;
        }
        else
        {
            StartClient();
        }
        Debug.Log("Game Ready");
        isStreaming = true;
        if (PlayerPrefs.GetString("playerChoice") == "P1")
        {
            waitingView.Hide();
            startView.Show();
        }
        else
        {
            waitingView.Show();
            startView.Hide();
        }
        Debug.Log("Game Starting");
        yield return null;
    }

    string difficulty;
    string level;
    public void SetLevel(string difficult)
    {
        difficulty = difficult;
        if (isServer)
        {
            SendCommand("P1 Set Difficulty", difficulty);
            string path = "Assets/Difficulty Settings/";
            switch (PlayerPrefs.GetString("gameDifficulty"))
            {
                case "easy":
                    path += "Easy";
                    break;
                case "medium":
                    path += "Medium";
                    break;
                case "hard":
                    path += "Hard";
                    break;
            }
            level = PlayZoneController.instance.GetDifficultyMap(path); ; //Pick level randomly from difficulty set
            Debug.Log(level);
            level = level.Substring(level.IndexOf("Assets"));
            Debug.Log(level.LastIndexOf("Assets"));
            PlayerPrefs.SetString("gameLevel", level);
            SendCommand("P1 Set Level", level);
        }
    }

    public bool[] ready = new bool[2];
    public bool[] ready2 = new bool[2];

    public void SendCommand(string command, string value)
    {
        networkBuffer[0] = command;
        networkBuffer[1] = value;
        lslNetworkOutlet.push_sample(networkBuffer);
    }

    IEnumerator WaitForStream()
    {
        bool searching = true;
        while (searching)
        {
            liblsl.StreamInfo[] allInlets = liblsl.resolve_streams(streamSearchTime);
            if (Manager.instance.isHost)
            {
                if (allInlets.Length == 6)
                {
                    Debug.Log("Found 6 stream total, starting host audio stream");
                    //code to start host audio stream here
                    startAudioStream();
                }
            }
            else
            {
                if (allInlets.Length == 7)
                {
                    Debug.Log("client sees host audio stream, starting client audio stream");
                    //code to start client audio stream here
                    startAudioStream();
                }
            }

            if (allInlets.Length == 8)
            {
                Debug.Log("both side ready to start, starting host object stream and sync ssequence");
                if(Manager.instance.isHost)
                {
                    //host setserver stream code here
                    SetServer();
                }
                else
                {
                    //client start controller trigger stream here
                }
                searching = false;
            }
            yield return new WaitForSeconds(5);
        }
        yield return null;
    }


    private float[] soundBuffer;
    private float[] objectBuffer;
    private float[] headhandBuffer;
    private float[] triggerBuffer;
    void Update()
    {
        if (isStreaming)
        {
            if (isServer)
            {
                int i = 0;
                foreach (var t in objectsToStream)
                {
                    if (t.gameObject.GetComponent<SquareController>().dirty)
                    {
                        objectBuffer[0] = t.type;
                        objectBuffer[1] = t.gameObject.transform.position.x;
                        objectBuffer[2] = t.gameObject.transform.position.y;
                        objectBuffer[3] = t.gameObject.transform.position.z;
                        objectBuffer[4] = t.gameObject.transform.eulerAngles.x;
                        objectBuffer[5] = t.gameObject.transform.eulerAngles.y;
                        objectBuffer[6] = t.gameObject.transform.eulerAngles.z;
                        objectBuffer[7] = t.gameObject.transform.localScale.x;
                        objectBuffer[8] = t.gameObject.transform.localScale.y;
                        objectBuffer[9] = t.gameObject.transform.localScale.z;
                        //trigger data
                        lslObjectOutlet.push_sample(objectBuffer);
                        //Debug.Log("gh1");
                    }
                }
                ++i;
            }


            //clients stream/own their heads and hands (hackish, but 6dof and 1head/2hands is not too bad).
            if (head != null)
            {
                headhandBuffer[0] = head.position.x;
                headhandBuffer[1] = head.position.y;
                headhandBuffer[2] = head.position.z;
                headhandBuffer[3] = head.eulerAngles.x;
                headhandBuffer[4] = head.eulerAngles.y;
                headhandBuffer[5] = head.eulerAngles.z;
                //headhandBuffer[6] = 0;
                lslHeadOutlet.push_sample(headhandBuffer);
            }
            if (leftHand != null)
            {
                headhandBuffer[0] = leftHand.position.x;
                headhandBuffer[1] = leftHand.position.y;
                headhandBuffer[2] = leftHand.position.z;
                headhandBuffer[3] = leftHand.eulerAngles.x;
                headhandBuffer[4] = leftHand.eulerAngles.y;
                headhandBuffer[5] = leftHand.eulerAngles.z;
                //headhandBuffer[6] = 0;
                lslLeftHandOutlet.push_sample(headhandBuffer);
            }
            if (rightHand != null)
            {
                headhandBuffer[0] = rightHand.position.x;
                headhandBuffer[1] = rightHand.position.y;
                headhandBuffer[2] = rightHand.position.z;
                headhandBuffer[3] = rightHand.eulerAngles.x;
                headhandBuffer[4] = rightHand.eulerAngles.y;
                headhandBuffer[5] = rightHand.eulerAngles.z;
                //headhandBuffer[6] = ControllerVR.P2GrabValue;
                lslRightHandOutlet.push_sample(headhandBuffer);
            }

            triggerBuffer[0] = (GameObject.FindGameObjectWithTag("SelfLine").GetComponent<ControllerVR>().isPresed)?1:0;
            lslTriggerOutlet.push_sample(triggerBuffer);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            liblsl.StreamInfo[] allInlets = liblsl.resolve_streams(streamSearchTime);
            Debug.Log("Done searching for streams. Found " + allInlets.Length + " streams.");
            foreach (var s in allInlets)
            {
                string streamType = s.type();
                string streamSourceId = s.source_id();
                Debug.Log("   stream id: " + streamSourceId);
            }
        }
    }
    //this function will be called in the OnApplicationQuit function in Manager -- will destroy all the streams
    public void DestroyAllStreams()
    {
        

        liblsl.dll.lsl_destroy_outlet(lslRightHandOutlet.handle());
        liblsl.dll.lsl_destroy_outlet(lslHeadOutlet.handle());
        liblsl.dll.lsl_destroy_outlet(lslLeftHandOutlet.handle());

        foreach ( var a in lslHeadInlet)
        {
            liblsl.dll.lsl_destroy_inlet(a.handle());
        }
        foreach (var a in lslLeftHandInlet)
        {
            liblsl.dll.lsl_destroy_inlet(a.handle());
        }
        foreach (var a in lslRightHandInlet)
        {
            liblsl.dll.lsl_destroy_inlet(a.handle());
        }

        liblsl.dll.lsl_destroy_streaminfo(lslHeadInfo.handle());
        liblsl.dll.lsl_destroy_streaminfo(lslRightHandInfo.handle());
        liblsl.dll.lsl_destroy_streaminfo(lslLeftHandInfo.handle());


    }

}
