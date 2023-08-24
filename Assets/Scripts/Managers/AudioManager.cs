using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource source1; // for background music
    public AudioSource source2; // for other Instant music

    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void PlayInstantMusic(AudioClip clip)
    {
        source2.clip = clip;
        source2.Play();
    }
    
}
