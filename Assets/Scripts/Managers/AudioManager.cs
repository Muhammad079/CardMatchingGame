using UnityEngine;

namespace Scripts.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource Source1; // for background music
        public AudioSource Source2; // for other Instant music

        public static AudioManager Instance;
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void PlayInstantMusic(AudioClip clip)
        {
            Source2.clip = clip;
            Source2.Play();
        }
    }
}