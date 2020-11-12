using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;

public class CanvasController : MonoBehaviour
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
    public UIButton avatarsPanelButton;
    public UIButton optionsBackButton;

    //public UIView startView;
    //public UIView playersCountView;
    //public UIView difficultyView;
    //public UIView whichPlayerView;
    //public UIView optionsView;
    //public UIView avatarsView;

    public UIView [] uiViews;

    private int currPanelIndex = 0;
    private int prevPanelIndex = 0;

    public int mainMenuIndex;
    public int optionPanelIndex;
    public int avatarsPanelIndex;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void goNextPanel()
    {
        goPanel(currPanelIndex + 1);
    }

    void goLastPanel()
    {
        goPanel(currPanelIndex - 1);
    }

    public void goBackPrevPanel()
    {
        goPanel(prevPanelIndex);
    }

    void goPanel(int i)
    {
        prevPanelIndex = currPanelIndex;
        uiViews[currPanelIndex].Hide();
        uiViews[i].Show();
        currPanelIndex = i;
    }

    public void OnePlayerButton()
    {
        PlayerPrefs.SetInt("playerCount", 1);
        PlayerPrefs.SetString("playerChoice", "P1");
        Manager.NumPlayers = 1;
        goNextPanel();
        //onePlayerButton.ExecuteClick();
        //LSLServer.instance.SetServer();
    }
    public void TwoPlayerButton()
    {
        PlayerPrefs.SetInt("playerCount", 2);
        Manager.NumPlayers = 2;
        goNextPanel();
        //twoPlayerButton.ExecuteClick();
        //LSLServer.instance.SetServer();
        //LSLServer.instance.StartClient();
    }
    public void EasyButton()
    {
        //LSLServer.instance.SetStreaming(true);
        PlayerPrefs.SetString("gameDifficulty", "easy");
        Manager.gameDifficulty = "easy";
        goNextPanel();
        //easyButton.ExecuteClick();
    }
    public void MedButton()
    {
        PlayerPrefs.SetString("gameDifficulty", "medium");
        Manager.gameDifficulty = "medium";
        goNextPanel();
        //medButton.ExecuteClick();
    }
    public void HardButton()
    {
        PlayerPrefs.SetString("gameDifficulty", "hard");
        Manager.gameDifficulty = "hard";
        goNextPanel();
        //hardButton.ExecuteClick();
    }
    public void TplayButton()
    {
        goNextPanel();
        //tPlayButton.ExecuteClick();
    }
    public void Player1Button()
    {
        PlayerPrefs.SetString("playerChoice", "P1");
        PlayerPrefs.SetString("isServer", "true");
        goNextPanel();
        //player1Button.ExecuteClick();
    }
    public void Player2Button()
    {
        PlayerPrefs.SetString("playerChoice", "P2");
        PlayerPrefs.SetString("isServer", "false");
        goNextPanel();
        //player2Button.ExecuteClick();
    }
    public void OptionsButton()
    {
        goPanel(optionPanelIndex);
        //optionsButton.ExecuteClick();
    }
    public void AvatarsPanelButton()
    {
        goPanel(avatarsPanelIndex);
        //avatarsPanelButton.ExecuteClick();
    }
    public void OptionsBackButton()
    {
        goPanel(mainMenuIndex);
        //optionsBackButton.ExecuteClick();
    }
}
