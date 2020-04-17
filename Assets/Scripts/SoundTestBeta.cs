using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SoundTest : MonoBehaviour
{
    public GameObject testCube;
    // Sounds are here for testing
    // The multiple AudioSources are for individual parts of the code.
    public AudioSource drums;
    public AudioSource blockDropSounds;
    public AudioSource blockGrabSounds;
    public AudioSource blockDestroySounds;
    public AudioClip[] sounds;
   // How to add all sounds to an AudioClip array https://answers.unity.com/questions/1589035/make-audio-array-and-play-random.html
    public int soundInc = 0;


    // Start is called before the first frame update
    void Start()
    {
        drums = gameObject.AddComponent<AudioSource>();
        blockDropSounds = gameObject.AddComponent<AudioSource>();
        //The sounds being added to this list are in the Resources/SOunds/Sounds folder.
        sounds = Resources.LoadAll<AudioClip>("Sounds/Sounds");

        Debug.Log("This clip is loaded successfully: " + sounds[0]);
        drums.clip = sounds[10];
        drums.loop = true;
        drums.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // Checks for Input.GetKey(KeyCode.Mouse0) and that no other sounds are playing
        // to test out the sounds in order.
        if (Input.GetKey(KeyCode.Mouse0) && !(blockDropSounds.isPlaying))
        {
            //Check to see if the sound array is reaching it's length.
            if (soundInc < sounds.Length)
            {
                blockDropSounds.clip = sounds[soundInc];
                blockDropSounds.Play();
                Debug.Log("Playing the next sound, " + sounds[soundInc]);
                soundInc = soundInc + 1;
            }

            else
            {            
                soundInc = 0;
                Debug.Log("Starting over at, " + sounds[soundInc]);
            }
        }
    }
}
