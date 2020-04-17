using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Doozy.Engine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ControllerVR : MonoBehaviour
{

    // Menu stuff
    public UIView StartMenu;
    public UIButton onePlayerButton;
    public UIButton twoPlayerButton;
    public UIButton easyButton;
    public UIButton medButton;
    public UIButton hardButton;
    public UIButton startButton;
    public UIButton player1Button;
    public UIButton player2Button;
    public UIButton tPlayButton;
    public TMP_Text playerCheckText;
    public TMP_Text difficultyText;
    public UIButton gameoverButton;
    public UIButton restartButton;

    LineRenderer line;
    public GameObject hand;
    public GameObject endpointDebug;
    //public SteamVR_Action_Boolean grabPinch;
    public Material goldBase;
    public Material redBase;
    public Material blueBase;

    public TextMesh grabdistance;
    public TextMesh endpointDistance;
    public TextMesh objDistance;

    public TextMesh currentGrabDistance;
    //public SteamVR_Input_Sources input

    bool grabbing = false;
    bool wasGrabbing = false;
    public GameObject grabbedObject = null;
    float grabDistance = 0;
    float defaultDist = 20;
    bool hitSomething = false;
    Vector3 endPos;
    Vector3 targetVector;
    RaycastHit hit;
    
    float compareDistance;
    float newGrabDistance;

    float deleteTimer = 0.0f;
    public float deleteWaitTime = 2.0f;
    public float goldHoldTimer = 0.0f;

    public int player = 0;
    public int handNum;

    //Sound Manager - Nathan
    public SoundManager SoundManager;

    void Start()
    {
        //PlayerPrefs.SetInt("playerCount", 1);
        line = transform.GetComponent<LineRenderer>();
        string[] names = UnityEngine.Input.GetJoystickNames();
        foreach (string s in names)
        {
            //Debug.Log(s);
        }
        MyHand = GameObject.FindGameObjectWithTag("P1RHand");
        //Shows which player's hand is in the current version of ControllerVR (largely used for AnalyticsManager)
        handNum = transform.tag.Contains("Self") ? 0 : 1;
    }

    public void StartSendingTriggger()
    {
        //StartCoroutine(TriggerDetectionLSL());
    }

    /// <summary>
    /// attach coroutine for 
    /// </summary>
    /// <returns></returns>
    public IEnumerator TriggerDetectionLSL()
    {
        if(LSLServer.instance.isServer != (transform.tag.Contains("Self")))
        {
            yield return null;
        }
        Debug.Log("started trigger coroutine |"+transform.gameObject.name);
        GameObject CoroHand = GameObject.FindGameObjectWithTag("SelfHandTag");
        while (true)
        {
            if (LSLServer.instance.isStreaming)
            {
                //if (!LSLServer.instance.isServer)
                //{
                //    this.enabled = false;
                //}
                //LSLServer.instance.SendTrigger(isPresed ? 1 : 0);
                Debug.Log("sending " + isPresed.ToString());
            }
            yield return null;
        }
    }
    public GameObject MyHand;
    public void PlayerValue(int i)
    {
        player = i;
        MyHand = GameObject.FindGameObjectWithTag((i == 1) ? "P1RHand" : "P2RHand");
    }

    //public void SetPlayerHand(GameObject hand)
    //{
    //    MyHand = hand;
    //}

    static public float P2GrabValue = 0;
    public bool isPresed=false;
    bool wasPressed=false;
    bool startPressed=false;
    // Update is called once per frame
    void Update()
    {
        if (LSLServer.instance.isStreaming)
        {
            player = LSLServer.instance.isServer?0:1;
        }
            Vector3 objRay = line.GetPosition(1)-line.GetPosition(0);
            Vector3 handRay = MyHand.transform.forward.normalized;
            if (objRay != handRay)
            {
                //Debug.Log("ispressed: " + isPresed);
                //Debug.Log("waspressed: " + wasPressed);
                //Debug.Log("not same");
            }

        grabbing = false;
        hitSomething = false;
        //line.SetPosition(0, hand.transform.position);
        //pointer vector in the direction of the controller
        //targetVector = (hand.transform.forward - hand.transform.up).normalized;
        line.SetPosition(0, MyHand.transform.position);
        targetVector = MyHand.transform.forward;
        //find the closest target or default to 20 range
        //for idle laser tracking
        if (Physics.Raycast(MyHand.transform.position, targetVector, out hit))
        {
            //Debug.Log(gameObject.tag + "      meow    " + hit.transform.gameObject.name);
            hitSomething = true;
            if (Vector3.Distance(hit.point, MyHand.transform.position) < defaultDist)
            {
                endPos = hit.point;
            }
            else
            {
                endPos = MyHand.transform.position + targetVector * defaultDist;
            }
        }
        else
        {
            //If a block is being deleted from the build zone and the player stops pointing at it, reset the timer and ungrab the block
            if (grabbedObject != null && grabbedObject.GetComponent<SquareController>().zone == "Build")
            {
                deleteTimer = 0.0f;
                grabbedObject = null;
            }
            hitSomething = false;
            endPos = MyHand.transform.position + targetVector * defaultDist;
        }
        line.SetPosition(1, endPos);
        //endpointDebug.transform.position = hit.point;
        //Debug.Log(true);
        if (PlayerPrefs.GetInt("playerCount") == 1 || !LSLServer.instance.isStreaming)
        {
            startPressed = hand.GetComponent<Hand>().grabPinchAction.GetStateDown(hand.GetComponent<Hand>().handType);
            isPresed = hand.GetComponent<Hand>().grabPinchAction.GetState(hand.GetComponent<Hand>().handType);
            wasPressed = hand.GetComponent<Hand>().grabPinchAction.GetLastState(hand.GetComponent<Hand>().handType);
        }
        else
        {
            if(transform.tag.Contains("Self"))
            {
                startPressed = hand.GetComponent<Hand>().grabPinchAction.GetStateDown(hand.GetComponent<Hand>().handType);
                isPresed = hand.GetComponent<Hand>().grabPinchAction.GetState(hand.GetComponent<Hand>().handType);
                wasPressed = hand.GetComponent<Hand>().grabPinchAction.GetLastState(hand.GetComponent<Hand>().handType);
            }
        }

        if (PlayerPrefs.GetInt("playerCount") == 1 || LSLServer.instance.isServer)
        {
            //if the hand is starting grab on current frame
            if (!wasPressed && isPresed && hitSomething)
            {
                //Debug.Log(transform.gameObject.name + "  grabbed mofo");
                HandGrabStart();
                P2GrabValue = 1;
            }
            //if holding onto grab action
            if (wasPressed && isPresed)
            {
                HandGrabHold();
                P2GrabValue = 1;
            }
            //if grab action is let go
            if (wasPressed && !isPresed)
            {
                HandGrabRelease();
                P2GrabValue = 0;
            }
        }
        wasPressed = isPresed;

        //line.material = (grabbing) ? greenBase : redBase;
        wasGrabbing = grabbing;
        //grabdistance.text = "Grab Distance: " + grabDistance;
        // endpointDistance.text = "End Distance: " + Vector3.Distance(hand.transform.position, hit.point);
        // objDistance.text = "Obj Distance : " + Vector3.Distance(line.GetPosition(0), line.GetPosition(1));
        //  currentGrabDistance.text = "Current Distance: " + newGrabDistance.ToString();
    }

    /// <summary>
    /// This function checks to see if the ray cast hit either a cube or a button. 
    /// The else statement contains the game buttons ready to be called if they are clicked.
    /// </summary>
    void HandGrabStart()
    {
        GameObject hitObject = hit.transform.gameObject;
        if (Vector3.Distance(hitObject.transform.position, MyHand.transform.position) > 20) //THIS WILL COME BACK AND BITE ME IN THE ASS
        {
            return;
        }
        //Checks to see if object is a cube and if it can be picked up (or is gold)
        if (hitObject.tag.Contains("cube"))
        {
            SquareController pickUpSquare = hitObject.GetComponent<SquareController>();
            //Debug.Log((gameObject.tag == "SelfLine" ? 0 : 1)+"  square stuff");
            //Debug.Log("can pickup: "+pickUpSquare.CanPickUp(gameObject.tag == "SelfLine" ? 0 : 1));
            if (pickUpSquare.CanPickUp(gameObject.tag == "SelfLine" ? 0 : 1) || pickUpSquare.type == 2 || pickUpSquare.zone == "Build")
            {
                //Debug.Log("grabinggggg");
                SetGrabbedObject(hit.transform.gameObject);
                grabbing = true;

                if(pickUpSquare.type == 2 && Manager.NumPlayers == 2)
                {
                    AnalyticsManager.instance.FillEventLog("Gold Block Hold", handNum);
                }
            }
        }
        else
        {
            switch (hitObject.tag)
            {
                case "one player button":
                    OnePlayerButton();
                    break;
                case "two player button":
                    TwoPlayerButton();
                    break;
                case "easy button":
                    EasyButton();
                    break;
                case "med button":
                    MedButton();
                    break;
                case "hard button":
                    HardButton();
                    break;
                case "start button":
                    StartButton();
                    break;
                case "tPlay button":
                    TplayButton();
                    break;
                case "player1 button":
                    Player1Button();
                    break;
                case "player2 button":
                    Player2Button();
                    break;
                case "pause button":
                    PauseButton();
                    break;
                case "submit button":
                    SubmitButton();
                    break;
                case "restart button":
                    RestartButton();
                    break;
            }
        }
    }

    /// <summary>
    /// Handles the repositioning, sounds and interactive logic 
    /// of the cubes while they are being held by the players.
    /// </summary>
    void HandGrabHold()
    {
        line.material = redBase;
        if (grabbedObject != null)
        {
            //Debug.Log("holding" + grabbedObject.name);
            bool forceposition = false;
            SquareController grabbedBlock = grabbedObject.GetComponent<SquareController>();
            
            if (grabbedBlock.zone == "Build")
            {
                //Checks to make sure you're still pointing at the block you're trying to delete from the build wall, if not, reset timer and set grabbedObject to null
                if (!hit.transform.gameObject.Equals(grabbedObject))
                {
                    deleteTimer = 0.0f;
                    grabbedObject = null;
                    return;
                }
                //Don't move objects in the build zone
                //Instead increment timer towards deleting
                deleteTimer += Time.deltaTime;
                if (deleteTimer > deleteWaitTime)
                {
                    AnalyticsManager.instance.FillEventLog("Block Deleted", handNum, grabbedBlock);
                    DissolveObject(grabbedObject);
                    grabbedObject = null;

                    //Tell the sound script to play the corresponding sound
                    //at the blockDestroySounds AudioSource. -Nathan
                    SoundManager.instance.soundPlay(1, 1);
                }
            }
            //Don't move full gold blocks unless only 1 player
            else if (grabbedBlock.type == 2 && Manager.NumPlayers != 1)
            {
                goldHoldTimer += Time.deltaTime;
                if (grabbedBlock.playerHolds[0] && grabbedBlock.playerHolds[1])
                {
                    Debug.Log("Time Player " + (handNum + 1) + "waited for other to pick up Gold Block: " + goldHoldTimer);
                    PlayZoneController.instance.PickupGold(grabbedBlock);
                }
            }
            else if (grabbedBlock.zone == "Interact")
            {
                //Regular move block around on stick logic
                if (!hitSomething)
                {
                    compareDistance = 20f;
                }
                else
                {
                    compareDistance = Vector3.Distance(MyHand.transform.position, hit.point);
                    if (hit.transform.gameObject.tag == "DropZone" && grabbedObject != null)
                    {
                        grabbedObject.transform.position = hit.transform.position;
                        line.SetPosition(1, hit.transform.position);
                        //hit.transform.GetComponent<MeshRenderer>().enabled = false;
                        forceposition = true;
                    }
                }
                
                if (!forceposition && grabbedObject != null)
                {
                    newGrabDistance = Mathf.Min(compareDistance, grabDistance);
                    line.SetPosition(1, MyHand.transform.position + targetVector * newGrabDistance);
                    grabbedObject.transform.position = MyHand.transform.position + targetVector * newGrabDistance;

                    SoundManager.instance.soundPlay(1, 2);
                }
            }
        }
    }

    /// <summary>
    /// Handles the sound and logic for when cubes are release.
    /// </summary>
    void HandGrabRelease()
    {
        //Debug.Log("let go");
        line.SetPosition(1, endPos);
        line.material = blueBase;

        deleteTimer = 0.0f;

        if (grabbedObject != null)
        {
            SquareController grabbedBlock = grabbedObject.GetComponent<SquareController>();
            AnalyticsManager.instance.FillEventLog("Released Block", handNum, grabbedBlock);
            grabbedBlock.isHeld = false;
            grabbedBlock.playerHolds[player] = false;

            if (goldHoldTimer > 0.0f)
            {
                Debug.Log("Player " + (handNum + 1) + "dropped the Gold Block after waiting: " + goldHoldTimer);
                AnalyticsManager.instance.FillEventLog("Gold Block Drop", handNum);
                goldHoldTimer = 0.0f;
            }

            //STOP playing a sound when a block is no longer held, and play the let go sound instead. - Nathan
            SoundManager.instance.soundPlay(1, -1);
            SoundManager.instance.soundPlay(1, 3);

            // If letting go of block already in Build zone, if it hasn't already deleted by timer, just return it back to the main layer and let it resume falling
            if (grabbedBlock.zone == "Build")
            {
                grabbedObject.layer = 1;
            }
            // If dropping a block in a DropZone above the build zone
            else if (hitSomething && hit.transform.gameObject.tag == "DropZone")
            {
                grabbedObject.layer = 1;
                //get the curent dropzone number
                Transform DropParent = hit.transform.parent;
                // will respond true if square can be placed in column of buildZone, false otherwise
                bool canFit = BuildZoneController.instance.AddSquare(hit.transform.GetSiblingIndex(), grabbedBlock, handNum);
                //If it can't fit in the build zone column, it recycles back into the playZone spawning pool
                if (!canFit)
                {
                    PlayZoneController.instance.RecycleBlock(grabbedBlock);
                }
                //hit.transform.GetComponent<MeshRenderer>().enabled = true;
            }
            // Otherwise the block is dissolved to recycle back into the playZone's spawning pool
            else
            {
                DissolveObject(grabbedObject);

                //Play Sound through SoundManager.
                SoundManager.instance.soundPlay(1, 1);
                if (PlayerPrefs.GetInt("playerCount") == 2 && LSLServer.instance.isStreaming)
                {
                    LSLServer.instance.SendSound(1, 1);
                }
            }

            grabbedObject = null;
        }

        grabbing = false;
    }

    /// <summary>
    /// This function is called when a cube needs to be deleted.
    /// Uses cool shaders!
    /// </summary>
    /// <param name="obj"> Game object that will be deleted and dissolved.</param>
    void DissolveObject(GameObject obj)
    {
        //dissolve shader here
        //make sure to turn off collider and other interaction scripts
        obj.GetComponent<SquareController>().grabbed = false;
        obj.GetComponent<SquareController>().DeleteSquare();
       // Destroy(obj);
    }

    /// <summary>
    /// Pre logic used to determine who the object was grabbed by and where it was interacted with.
    /// </summary>
    /// <param name="obj"> Object in question. </param>
    public void SetGrabbedObject(GameObject obj)
    {
        grabbedObject = obj;
        Debug.Log("grabbed object: " + obj.name);
        SquareController grabbedSquare = grabbedObject.GetComponent<SquareController>();
        AnalyticsManager.instance.FillEventLog("Grabbed Block", handNum, grabbedSquare);
        grabbedSquare.grabbed = true;
        grabDistance = Vector3.Distance(MyHand.transform.position, hit.point);
        if (grabbedSquare.zone == "Play" || grabbedSquare.zone == "Pool")
        {
            grabbedSquare.SetZone(false, "Interact", grabbedSquare.transform.position);
        }
        if ((Manager.NumPlayers == 1 || grabbedSquare.type != 2) && grabbedSquare.zone != "Build")
        {
            grabbedObject.layer = 2;
        }
        grabbedSquare.playerHolds[transform.tag.Contains("Self")?0:1] = true;
        goldHoldTimer = 0.0f;
    }

    //Buttons////////////////////

    public void OnePlayerButton()
    {
        PlayerPrefs.SetInt("playerCount", 1);
        PlayerPrefs.SetString("playerChoice", "P1");
        Manager.NumPlayers = 1;
        onePlayerButton.ExecuteClick();
        //LSLServer.instance.SetServer();
    }
    public void TwoPlayerButton()
    {
        PlayerPrefs.SetInt("playerCount", 2);
        Manager.NumPlayers = 2;
        twoPlayerButton.ExecuteClick();
        //LSLServer.instance.SetServer();
        //LSLServer.instance.StartClient();
    }
    public void EasyButton()
    {
        //LSLServer.instance.SetStreaming(true);
        PlayerPrefs.SetString("gameDifficulty", "easy");
        Manager.gameDifficulty = "easy";
        easyButton.ExecuteClick();
    }
    public void MedButton()
    {
        PlayerPrefs.SetString("gameDifficulty", "medium");
        Manager.gameDifficulty = "medium";
        medButton.ExecuteClick();
    }
    public void HardButton()
    {
        PlayerPrefs.SetString("gameDifficulty", "hard");
        Manager.gameDifficulty = "hard";
        hardButton.ExecuteClick();
    }
    public void StartButton()
    {
        //LSLServer.instance.ScanForPlayers();
        Manager.instance.StartCoroutine(Manager.instance.StartCoRoutine());
        startButton.ExecuteClick();
    }
    public void Player1Button()
    {
        PlayerPrefs.SetString("playerChoice", "P1");
        PlayerPrefs.SetString("isServer", "true");
        player1Button.ExecuteClick();
    }
    public void Player2Button()
    {
        PlayerPrefs.SetString("playerChoice", "P2");
        PlayerPrefs.SetString("isServer", "false");
        player2Button.ExecuteClick();
    }
    public void TplayButton()
    {
        tPlayButton.ExecuteClick();
    }
    public void GameoverButton()
    {
        gameoverButton.ExecuteClick();
    }
    public void RestartButton()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        restartButton.ExecuteClick();
        Manager.instance.RestartTheGame();
    }
    public void PauseButton()
    {
        Manager.instance.PauseGame();
        //Log which player paused/unpaused the game
        AnalyticsManager.instance.FillEventLog(Manager.instance.GamePaused ? "Game Paused" : "Game Resumed", handNum);
    }
    public void SubmitButton()
    {
        Manager.instance.CheckCorrectness();
        AnalyticsManager.instance.FillEventLog("Game Submitted", handNum);
    }
}
