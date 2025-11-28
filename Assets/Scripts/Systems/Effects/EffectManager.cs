using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Effects
{
    /// <summary>
    /// Manages visual effects (particles) for the game.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject moveEffectPrefab;

        [SerializeField]
        private GameObject captureEffectPrefab;

        [SerializeField]
        private GameObject victoryEffectPrefab;

        [Header("References")]
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
                gameManager.OnVictory.AddListener(PlayVictoryEffect);
            }
        }

        private void OnDestroy()
        {
            if (board != null)
            {
                board.OnPieceMoved -= HandlePieceMoved;
            }

            if (gameManager != null)
            {
                gameManager.OnVictory.RemoveListener(PlayVictoryEffect);
            }
        }

        private void HandlePieceMoved(Piece piece, Vector2Int from, Vector2Int to, Piece capturedPiece)
        {
            // Convert board coordinates to world position
            // Assuming 1 unit per tile and board centered or offset appropriately
            // This might need adjustment based on actual board visualization
            Vector3 targetPos = new Vector3(to.x, to.y, 0); 

            if (capturedPiece != null)
            {
                SpawnEffect(captureEffectPrefab, targetPos);
            }
            else
            {
                SpawnEffect(moveEffectPrefab, targetPos);
            }
        }

        private void PlayVictoryEffect()
        {
            SpawnEffect(victoryEffectPrefab, Vector3.zero);
        }

        private void SpawnEffect(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
        }
    }
}
