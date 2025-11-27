using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Frozen Bishop Mutation: Bishop leaves temporary ice obstacles along its path.
    /// Creates tactical frozen barriers that last for 2 turns.
    /// </summary>
    [CreateAssetMenu(fileName = "FrozenBishopMutation", menuName = "Mutating Gambit/Mutations/Frozen Bishop")]
    public class FrozenBishopMutation : Mutation
    {
        [SerializeField]
        private int iceDuration = 2;

        private Dictionary<Vector2Int, int> frozenTiles = new Dictionary<Vector2Int, int>();

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Bishop)
            {
                Debug.LogWarning("Frozen Bishop mutation can only be applied to bishops!");
                return;
            }

            Debug.Log($"Applied Frozen Bishop mutation to {piece.Team} bishop");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // Clean up frozen tiles
            frozenTiles.Clear();
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (mutatedPiece.Type != PieceType.Bishop)
            {
                return;
            }

            // Freeze all tiles along the path
            Vector2Int direction = to - from;
            int steps = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

            if (steps > 0)
            {
                Vector2Int step = new Vector2Int(
                    direction.x / steps,
                    direction.y / steps
                );

                for (int i = 1; i < steps; i++)
                {
                    Vector2Int freezePos = from + step * i;

                    if (board.IsPositionValid(freezePos))
                    {
                        board.SetObstacle(freezePos, true);
                        frozenTiles[freezePos] = iceDuration;

                        Debug.Log($"Froze tile at {freezePos} for {iceDuration} turns");
                    }
                }
            }
        }

        /// <summary>
        /// Call this at the end of each turn to decay frozen tiles.
        /// </summary>
        public void OnTurnEnd(Board board)
        {
            var tilesToRemove = new List<Vector2Int>();

            foreach (var kvp in frozenTiles)
            {
                frozenTiles[kvp.Key]--;

                if (frozenTiles[kvp.Key] <= 0)
                {
                    // Unfreeze tile
                    board.SetObstacle(kvp.Key, false);
                    tilesToRemove.Add(kvp.Key);

                    Debug.Log($"Unfroze tile at {kvp.Key}");
                }
            }

            // Remove expired tiles
            foreach (var pos in tilesToRemove)
            {
                frozenTiles.Remove(pos);
            }
        }
    }
}
