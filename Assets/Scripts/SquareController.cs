using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareController : MonoBehaviour
{
    [SerializeField] Vector3 buildZoneOffset;
    [SerializeField] float squareSize = 1.0f;
    //[SerializeField] public BlockTypes type;
    public int type;

    public SquareController mirror;
    public bool isMirror = false;

    [SerializeField] float fallSpeed = 1.0f;
    [SerializeField] public int maxBuildHeight = 10;
    [SerializeField] public int maxPlayHeight = 10;
    bool falling = false;
    public string zone;
    public int column = -1;
    public int row = -1;
    public Vector3 Destination;

    public SquareController pair = null;
    public bool isHeld = false;
    public bool[] playerHolds = { false, false };

    //Sound Manager - Nathan
    public SoundManager SoundManager;

    public void ResetInfo()
    {
        SetZone(false, "Pool", Vector3.right * 1000);
        pair = null;
        isHeld = false;
        grabbed = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        AssignType();
        lastPos = transform.position;
    }

    // Updates zone and height based on zone
    public void SetZone(bool fall, string Zone, Vector3 destination, int c = -1, int r = -1)
    {
        int rotation = (Zone == "Build") ? 0 : 90;// turns to the side if on the build wall
        rotation += isMirror ? 180 : 0;// flips over if on the p2 (mirror) build wall
        zone = Zone;
        falling = fall;
        transform.rotation = Quaternion.Euler(0, rotation, 0);
        Destination = destination;
        column = c;
        row = r;
    }

    Vector3 lastPos=Vector3.zero;
    public bool dirty = false;
    private void Update()
    {
        if (falling)
        {
            transform.position = Vector3.MoveTowards(transform.position, Destination, fallSpeed * Time.deltaTime);
            if (transform.position == Destination)
            {
                falling = false;
                //To be replaced by OnTriggerEnter event
                if (zone == "Play")
                {
                    //SetZone(false, "Pool", poolWaitLoc, -1);
                    PlayZoneController.instance.RecycleBlock(this);
                }
                else if (pair != null && pair.falling == false)
                {
                    BuildZoneController.instance.GoldMerge(row, column);

                    //Tell the SoundManager script to play the corresponding sound
                    //at the blockGrabSounds AudioSource. -Nathan
                    SoundManager.instance.soundPlay(0, 5);
                }
            }
        }
        dirty = ((lastPos != transform.position) || grabbed);
        lastPos = transform.position;
    }
    public bool grabbed = false;
    //Method to fall an additional block when one is deleted below it in its column
    public void UnderGone()
    {
        row -= 1;
        Destination = BuildZoneController.instance.GridLocation[isMirror ? 1 : 0][column, row];
        falling = true;

        if (!isMirror && mirror != null)
        {
            mirror.UnderGone();
        }
    }

    // Checks to see if block can be picked up by player
    public bool CanPickUp(int player)
    {
        //First checks if the game is in singleplayer, then if the player has access to the block type
        if (Manager.NumPlayers == 1)
        {
            return true;
        }
        else
        {
            //Debug.Log("checking: " + (player == 1 ? "Mp1" : "mp2" + (BlockTypes)type));
            return (player == 1 ? Manager.instance.MP1Blocks : Manager.instance.MP2Blocks).Contains((BlockTypes)type);
            //return true;
        }
    }

    public void DeleteSquare()
    {
        if (isMirror && mirror != null)
        {
            mirror.DeleteSquare();
            return;
        }
        if (zone == "Build")
        {
            BuildZoneController.instance.RemoveSquare(column, row, type);
        }
        PlayZoneController.instance.RecycleBlock(this);
        if (mirror != null)
        {
            PlayZoneController.instance.RecycleBlock(mirror);
            mirror = null;
        }
    }

    // this should be called on Start and be used to identify the cube
    public void AssignType()
    {
        switch (gameObject.tag)
        {
            case "blue cube":
                type = 0;
                break;
            case "red cube":
                type = 1;
                break;
            case "gold cube":
                type = 2;
                break;
            case "left gold cube":
                type = 3;
                break;
            case "right gold cube":
                type = 4;
                break;
            case "invis cube":
                type = 5;
                break;
        }
    }
    // Integer values assigned should line up with Prefab arrays and etc.
    public enum BlockTypes : int
    {
        Red = 0,
        Blue = 1,
        Gold = 2,
        GoldL = 3,
        GoldR = 4,
        Invisible = 5
    }
}
