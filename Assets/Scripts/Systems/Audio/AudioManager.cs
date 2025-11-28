using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Audio
{
    /// <summary>
    /// Manages audio playback for the game.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField]
        private AudioConfig config;

        [SerializeField]
        private AudioSource sfxSource;

        [SerializeField]
        private AudioSource musicSource;

        [SerializeField]
        private Board board;

        [SerializeField]
        private GameManager gameManager;

        private void Start()
        {
            // Cache references if not assigned
            if (board == null) board = FindObjectOfType<Board>();
            if (gameManager == null) gameManager = GameManager.Instance;

            if (board != null)
            {
                board.OnPieceMoved += HandlePieceMoved;
            }

            if (gameManager != null)
            {
                gameManager.OnVictory.AddListener(PlayVictorySound);
                gameManager.OnDefeat.AddListener(PlayDefeatSound);
            }

            PlayMusic();
        }

        private void OnDestroy()
        {
            if (board != null)
            {
                board.OnPieceMoved -= HandlePieceMoved;
            }

            if (gameManager != null)
            {
                gameManager.OnVictory.RemoveListener(PlayVictorySound);
                gameManager.OnDefeat.RemoveListener(PlayDefeatSound);
            }
        }

        private void HandlePieceMoved(Piece piece, Vector2Int from, Vector2Int to, Piece capturedPiece)
        {
            if (capturedPiece != null)
            {
                PlaySFX(config.captureSound);
            }
            else
            {
                PlaySFX(config.moveSound);
            }
        }

        private void PlayVictorySound()
        {
            PlaySFX(config.victorySound);
        }

        private void PlayDefeatSound()
        {
            PlaySFX(config.defeatSound);
        }

        public void PlayUIClick()
        {
            PlaySFX(config.uiClickSound);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        private void PlayMusic()
        {
            if (musicSource != null && config.backgroundMusic != null)
            {
                musicSource.clip = config.backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
    }
}
