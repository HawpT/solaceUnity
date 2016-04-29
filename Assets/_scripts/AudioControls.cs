using UnityEngine;
using System.Collections;

public class AudioControls : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleMute(AudioSource audio)
    {
        audio.mute = !audio.mute;
    }

    public void TogglePause(AudioSource audio)
    {
        if (audio.isPlaying)
            audio.Pause();
        else
            audio.UnPause();
    }
}
