using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Engine.Soundy;
using UnityEngine.Audio;

public class VolMan : MonoBehaviour
{
    //Some slider code taken from https://docs.unity3d.com/2019.1/Documentation/ScriptReference/UI.Slider-onValueChanged.html
    //Some code via assistance from Brandon. Thank you.
    private Slider musicSlider;
    private Slider soundSlider;
    private Slider volSlider;

    public AudioSource test;

    //create title audioclip and musicSource AudioSource
    [SerializeField] private AudioClip title;
    [SerializeField] private AudioSource musicSource;

    // Start is called before the first frame update
    void Start()
    {

        // Set slider to the slider objects for each sound setting: Overall Volume, Music, and Sound.
        volSlider = this.gameObject.GetComponent<Slider>();
        musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        soundSlider = GameObject.Find("SoundSlider").GetComponent<Slider>();

        // Set all sliders to 1 and initialize PlayerPrefs sound settings as a precautionary measure.
        volSlider.value = 1.0f;
        musicSlider.value = 1.0f;
        soundSlider.value = 1.0f;
        PlayerPrefs.SetFloat("musicVolume", 1.0f);
        PlayerPrefs.SetFloat("soundVolume", 1.0f);
        PlayerPrefs.SetFloat("volume", 1.0f);

        //Adds a listener to the main slider and invokes a method when the value changes.
        volSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        musicSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        soundSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        //Set musicSource to loop and play the TITLE clip;
        if (this.gameObject.name == "VolSlider")
        {
            //The 80's song is the title for our game.
            //musicSource = gameObject.AddComponent<AudioSource>();
            title = Resources.Load("Sounds/Music/80s song") as AudioClip;
            //musicSource.loop = true;
            //musicSource.clip = title;
            //musicSource.Play();

            test = GameObject.Find("Audio Source1").GetComponent<AudioSource>();
            test.loop = true;
            test.clip = title;
            test.Play();
        }

        if (this.gameObject.name == "MusicSlider")
        {
            Debug.Log(this.gameObject);
            for (int loops = 0; musicSource == null && loops < 5; loops++)
            {
                musicSource = GameObject.Find("VolSlider").GetComponent<AudioSource>();
                //Debug.Log("AudioSource is: " + musicSource);
            }
        }
        else if (this.gameObject == musicSlider && musicSource == null)
        {
            Debug.Log(" ERROR: Music Slider Audiosource still = NULL!");
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
    public void ValueChangeCheck()
    {
        if (this.gameObject.name == "MusicSlider")
        {
            if (musicSlider.value > PlayerPrefs.GetFloat("volume"))
            {
                test.volume = PlayerPrefs.GetFloat("volume");
                PlayerPrefs.SetFloat("musicVolume", PlayerPrefs.GetFloat("volume"));
            }
            else
            {
                //musicSource.volume = musicSlider.value;
                test.volume = musicSlider.value;
                PlayerPrefs.SetFloat("musicVolume", musicSlider.value);
            }
            Debug.Log("Current music volume value: " + PlayerPrefs.GetFloat("musicVolume.value"));
        }
        if (this.gameObject.name == "SoundSlider")
        {
            if (soundSlider.value > PlayerPrefs.GetFloat("volume"))
            {
                PlayerPrefs.SetFloat("SoundVolume", PlayerPrefs.GetFloat("volume"));
            }
            else
            {
                PlayerPrefs.SetFloat("soundVolume", soundSlider.value);
            }
            Debug.Log("Current sound volume value: " + soundSlider.value);
        }

        if (this.gameObject.name == "VolSlider")
        {
            PlayerPrefs.SetFloat("volume", volSlider.value);
            //musicSource.volume = volSlider.value;
            test.volume = volSlider.value;
            AudioListener.volume = volSlider.value;

            //Mute UI buttons if sound is below a threshold.
            //This is currently a workaround fix.

            if (test.volume < 0.1)
            {
                SoundyManager.MuteAllControllers();
            }
            else
            {
                SoundyManager.UnmuteAllControllers();
            }
            Debug.Log("Current overall volume value: " + PlayerPrefs.GetFloat("volume"));
        }
    }
}
