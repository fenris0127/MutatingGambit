using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// King's Shadow Artifact: When the king moves, it leaves an obstacle at its previous position for 1 turn.
    /// </summary>
    [CreateAssetMenu(fileName = "KingsShadowArtifact", menuName = "Artifacts/King's Shadow")]
    public class KingsShadowArtifact : Artifact
    {
        [Header("Shadow Settings")]
        [SerializeField]
        [Tooltip("How many turns the shadow obstacle lasts.")]
        private int shadowDuration = 1;

        // Track shadow positions and their remaining duration
        private System.Collections.Generic.Dictionary<Vector2Int, int> shadowPositions =
            new System.Collections.Generic.Dictionary<Vector2Int, int>();

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // This is triggered when king moves
            if (context.MovedPiece == null || context.MovedPiece.Type != PieceType.King)
            {
                return;
            }

            Vector2Int previousPosition = context.FromPosition;

            // Check if previous position is now empty
            if (board.GetPiece(previousPosition) == null)
            {
                // Create shadow obstacle
                board.SetObstacle(previousPosition, true);
                shadowPositions[previousPosition] = shadowDuration;

                Debug.Log($"King's Shadow: Created obstacle at {previousPosition.ToNotation()}");
            }
        }

        /// <summary>
        /// Updates shadow durations. Should be called at turn end.
        /// </summary>
        public void UpdateShadows(Board board)
        {
            var positionsToRemove = new System.Collections.Generic.List<Vector2Int>();

            foreach (var kvp in shadowPositions)
            {
                Vector2Int pos = kvp.Key;
                int remainingTurns = kvp.Value - 1;

                if (remainingTurns <= 0)
                {
                    // Shadow expires
                    board.SetObstacle(pos, false);
                    positionsToRemove.Add(pos);
                    Debug.Log($"King's Shadow: Removed obstacle at {pos.ToNotation()}");
                }
                else
                {
                    shadowPositions[pos] = remainingTurns;
                }
            }

            foreach (var pos in positionsToRemove)
            {
                shadowPositions.Remove(pos);
            }
        }

        public override void OnRemoved(Board board)
        {
            // Remove all shadow obstacles
            foreach (var pos in shadowPositions.Keys)
            {
                board.SetObstacle(pos, false);
            }
            shadowPositions.Clear();
        }
    }
}
