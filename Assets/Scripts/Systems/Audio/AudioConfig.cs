using UnityEngine;

namespace MutatingGambit.Systems.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "MutatingGambit/Audio/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Sound Effects")]
        public AudioClip moveSound;
        public AudioClip captureSound;
        public AudioClip checkSound;
        public AudioClip victorySound;
        public AudioClip defeatSound;
        public AudioClip uiClickSound;

        [Header("Music")]
        public AudioClip backgroundMusic;
    }
}
