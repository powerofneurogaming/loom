using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Doozy.Engine.UI;
using TMPro;

public class ControllerVR2 : MonoBehaviour
{

    // Menu stuff
    [Header("Menu")]
    public UIView StartMenu;
    public UIButton onePlayerButton;
    public UIButton twoPlayerButton;
    public UIButton easyButton;
    public UIButton medButton;
    public UIButton hardButton;
    public UIButton startButton;
    public UIButton optionsButton;
    public TMP_Text playerCheckText;
    public TMP_Text difficultyText;
    public UIView headMenu;
    public UIView optionMenu;

    //line render
    [Header("Line Renderer")]
    LineRenderer line;
    public GameObject hand;
    public GameObject endpointDebug;

    //material
    [Header("Material")]
    public Material goldBase;
    public Material redBase;
    public Material blueBase;

    [Header("Debug Texts")]
    public TextMesh grabdistance;
    public TextMesh endpointDistance;
    public TextMesh objDistance;
    public TextMesh currentGrabDistance;
    //public SteamVR_Input_Sources input

    bool grabbing = false;
    bool wasGrabbing = false;
    GameObject grabbedObject = null;
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

    public int player = 0;
    public string color = "red";
    //Sound Manager - Nathan
    public SoundManager SoundManager;

    void Start()
    {
        line = transform.GetComponent<LineRenderer>();
        string[] names = UnityEngine.Input.GetJoystickNames();
        foreach (string s in names)
        {
            Debug.Log(s);
        }
    }

    // Update is called once per frame
    void Update()
    {
        grabbing = false;
        hitSomething = false;
        line.SetPosition(0, hand.transform.position);
        //pointer vector in the direction of the controller
        targetVector = (hand.transform.forward - hand.transform.up).normalized;

        //find the closest target or default to 20 range
        //for idle laser tracking
        if (Physics.Raycast(hand.transform.position, targetVector, out hit))
        {
            hitSomething = true;
            endPos = hit.point;
        }
        else
        {
            hitSomething = false;
            endPos = hand.transform.forward + targetVector * defaultDist;
        }
        line.SetPosition(1, endPos);
        //endpointDebug.transform.position = hit.point;
        //Debug.Log(true);

        //if the hand is starting grab on current frame
        if (hand.GetComponent<Hand>().grabPinchAction.GetStateDown(hand.GetComponent<Hand>().handType) && hitSomething)
        {
            HandGrabStart();
        }
        //if holding onto grab action
        if (hand.GetComponent<Hand>().grabPinchAction.GetLastState(hand.GetComponent<Hand>().handType) && hand.GetComponent<Hand>().grabPinchAction.GetState(hand.GetComponent<Hand>().handType))
        {
            HandGrabHold();
        }
        //if grab action is let go
        if (hand.GetComponent<Hand>().grabPinchAction.GetStateUp(hand.GetComponent<Hand>().handType))
        {
            HandGrabRelease();
        }

        //line.material = (grabbing) ? greenBase : redBase;
        wasGrabbing = grabbing;
        grabdistance.text = "Grab Distance: " + grabDistance;
        endpointDistance.text = "End Distance: " + Vector3.Distance(hand.transform.position, hit.point);
        objDistance.text = "Obj Distance : " + Vector3.Distance(line.GetPosition(0), line.GetPosition(1));
        currentGrabDistance.text = "Current Distance: " + newGrabDistance.ToString();
    }

    /// <summary>
    /// this is checking to see if the ray cast hit either a cube or a button. else statments are non cube objects
    /// </summary>
    void HandGrabStart()
    {
        GameObject hitObject = hit.transform.gameObject;
        //Checks to see if object is a cube and if it can be picked up (or is gold)
        if (hitObject.tag.Contains("cube"))
        {
            SquareController pickUpSquare = hitObject.GetComponent<SquareController>();
            if (pickUpSquare.CanPickUp(player) || pickUpSquare.type == 2)
            {
                SetGrabbedObject(hit.transform.gameObject);
                grabbing = true;
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
                case "human":
                    ChangeHeadHuman();
                    break;
                case "cat":
                    ChangeHeadCat();
                    break;
                case "mouse":
                    ChangeHeadMouse();
                    break;
                case "options button":
                    ChangeHeadMouse();
                    break;
            }
        }
    }

    void HandGrabHold()
    {
        line.material = redBase;
        if (grabbedObject != null)
        {
            bool forceposition = false;
            SquareController grabbedBlock = grabbedObject.GetComponent<SquareController>();

            if (grabbedBlock.zone == "Build")
            {
                //Don't move objects in the build zone
                //Instead increment timer towards deleting
                deleteTimer += Time.deltaTime;
                if (deleteTimer > deleteWaitTime)
                {
                    DissolveObject(grabbedObject);

                    //Tell the sound script to play the corresponding sound
                    //at the blockDestroySounds AudioSource. -Nathan
                    SoundManager.instance.soundPlay(1, 1);
                }
            }
            //Don't move full gold blocks unless only 1 player
            else if (grabbedBlock.type == 2 && Manager.NumPlayers != 1)
            {
                if (grabbedBlock.playerHolds[0] && grabbedBlock.playerHolds[1])
                {
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
                    compareDistance = Vector3.Distance(hand.transform.position, hit.point);
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
                    line.SetPosition(1, hand.transform.position + targetVector * newGrabDistance);
                    grabbedObject.transform.position = hand.transform.position + targetVector * newGrabDistance;

                    SoundManager.instance.soundPlay(1, 2);
                }
            }
        }
    }

    void HandGrabRelease()
    {
        Debug.Log("let go");
        line.SetPosition(1, endPos);
        line.material = blueBase;

        deleteTimer = 0.0f;

        if (grabbedObject != null)
        {
            SquareController grabbedBlock = grabbedObject.GetComponent<SquareController>();
            grabbedBlock.isHeld = false;
            grabbedBlock.playerHolds[player] = false;

            //STOP playing a sound when a block is no longer held. - Nathan
            SoundManager.instance.soundPlay(1, -1);

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
                bool canFit = BuildZoneController.instance.AddSquare(hit.transform.GetSiblingIndex(), grabbedBlock, 0);
                //If it can't fit in the build zone column, it recycles back into the playZone spawning pool
                if (!canFit)
                {
                    PlayZoneController.instance.RecycleBlock(grabbedBlock);
                    Destroy(grabbedObject);
                }
                //hit.transform.GetComponent<MeshRenderer>().enabled = true;
            }
            // Otherwise the block is dissolved to recycle back into the playZone's spawning pool
            else
            {
                DissolveObject(grabbedObject);

                //Play Sound through SoundManager.
                SoundManager.instance.soundPlay(1, 1);
            }

            grabbedObject = null;
        }

        grabbing = false;
    }

    void DissolveObject(GameObject obj)
    {
        //dissolve shader here
        //make sure to turn off collider and other interaction scripts
        obj.GetComponent<SquareController>().DeleteSquare();
        // Destroy(obj);
    }

    public void SetGrabbedObject(GameObject obj)
    {
        grabbedObject = obj;
        SquareController grabbedSquare = grabbedObject.GetComponent<SquareController>();
        grabDistance = Vector3.Distance(hand.transform.position, hit.point);
        if (grabbedSquare.zone == "Play")
        {
            grabbedSquare.SetZone(false, "Interact", grabbedSquare.transform.position);
        }
        if (Manager.NumPlayers == 1 || grabbedSquare.type != 2)
        {
            grabbedObject.layer = 2;
        }
        grabbedSquare.playerHolds[player] = true;
    }

    //Buttons////////////////////

    public void OnePlayerButton()
    {
        Manager.NumPlayers = 1;
        onePlayerButton.ExecuteClick();
        LSLServer.instance.SetServer();

    }
    public void TwoPlayerButton()
    {
        Manager.NumPlayers = 2;
        twoPlayerButton.ExecuteClick();
        //LSLServer.instance.SetServer();
        LSLServer.instance.StartClient();

    }
    public void EasyButton()
    {
        LSLServer.instance.SetStreaming(true);
        Manager.gameDifficulty = "easy";
        easyButton.ExecuteClick();

    }
    public void MedButton()
    {
        Manager.gameDifficulty = "medium";
        medButton.ExecuteClick();

    }
    public void HardButton()
    {
        Manager.gameDifficulty = "hard";

        hardButton.ExecuteClick();

    }
    public void StartButton()
    {
        LSLServer.instance.ScanForPlayers();
        Manager.instance.StartTheGame(Manager.NumPlayers, Manager.gameDifficulty);
        startButton.ExecuteClick();

    }
    public void OptionsButton()
    {
        optionsButton.ExecuteClick();
    }
    public void ChangeHeadHuman()
    {
        PlayerPrefs.SetString("player head", "human");
        headMenu.Hide();
        optionMenu.Show();
    }
    public void ChangeHeadCat()
    {
        PlayerPrefs.SetString("player head", "cat");
        headMenu.Hide();
        optionMenu.Show();
    }
    public void ChangeHeadMouse()
    {
        PlayerPrefs.SetString("player head", "mouse");
        headMenu.Hide();
        optionMenu.Show();
    }
}
