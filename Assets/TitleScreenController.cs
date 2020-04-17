using System.Collections;
using System.Collections.Generic;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine;
using Doozy.Engine.UI;

public class TitleScreenController : MonoBehaviour
{
    public UIButton onePlayerButton;
    public UIButton twoPlayerButton;
    public UIButton easyButton;
    public UIButton medButton;
    public UIButton hardButton;
    public UIButton player1Button;
    public UIButton player2Button;
    public UIButton tPlayButton;
    public UIButton optionsButton;
    public UIButton headsButton;
    public UIButton optionsBackButton;
    public UIView headMenu;
    public UIView optionMenu;
    public GameObject hand;
    public GameObject MyHand;
    LineRenderer line;

    bool grabbing;
    bool hitSomething;
    Vector3 targetVector;
    Vector3 endPos;
    RaycastHit hit;
    
    // Start is called before the first frame update
    void Start()
    {
        line = transform.GetComponent<LineRenderer>();
        MyHand = GameObject.FindGameObjectWithTag("P1RHand");
    }
    bool ispressed;
    bool waspressed;
    // Update is called once per frame
    void Update()
    {
        grabbing = false;
        hitSomething = false;
        line.SetPosition(0, MyHand.transform.position);
        targetVector = MyHand.transform.forward;

        if (Physics.Raycast(hand.transform.position, targetVector, out hit))
        {
            hitSomething = true;
            endPos = hit.point;
        }
        else
        {
            hitSomething = false;
            endPos = MyHand.transform.position + targetVector * 20;
        }
        

        line.SetPosition(1, endPos);

        ispressed = hand.GetComponent<Hand>().grabPinchAction.GetState(hand.GetComponent<Hand>().handType);
        if(ispressed && !waspressed && hitSomething)
        {
            GameObject hitObject = hit.transform.gameObject;
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
                case "tPlay button":
                    TplayButton();
                    break;
                case "player1 button":
                    Player1Button();
                    break;
                case "player2 button":
                    Player2Button();
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
                    OptionsButton();
                    break;
                case "heads button":
                    HeadsButton();
                    break;
                case "options back button":
                    OptionsBackButton();
                    break;


            }
        }




        waspressed = ispressed;
    }
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
    public void TplayButton()
    {
        tPlayButton.ExecuteClick();
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
    public void OptionsButton()
    {
        optionsButton.ExecuteClick();
    }
    public void HeadsButton()
    {
        headsButton.ExecuteClick();
    }
    public void OptionsBackButton()
    {
        optionsBackButton.ExecuteClick();
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
