using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Shadow Pawn Mutation: Pawn leaves a shadow copy at its previous position.
    /// Shadow blocks movement for 1 turn but cannot capture.
    /// </summary>
    [CreateAssetMenu(fileName = "ShadowPawnMutation", menuName = "Mutating Gambit/Mutations/Shadow Pawn")]
    public class ShadowPawnMutation : Mutation
    {
        [SerializeField]
        private int shadowDuration = 1;

        private Dictionary<Vector2Int, int> shadowPositions = new Dictionary<Vector2Int, int>();

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Pawn)
            {
                Debug.LogWarning("Shadow Pawn mutation can only be applied to pawns!");
                return;
            }

            Debug.Log($"Applied Shadow Pawn mutation to {piece.Team} pawn");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            shadowPositions.Clear();
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (mutatedPiece.Type != PieceType.Pawn)
            {
                return;
            }

            // Leave shadow at previous position
            if (board.IsPositionValid(from) && board.GetPiece(from) == null)
            {
                board.SetObstacle(from, true);
                shadowPositions[from] = shadowDuration;

                Debug.Log($"Shadow Pawn left shadow at {from} for {shadowDuration} turns");
            }
        }

        /// <summary>
        /// Call this at the end of each turn to decay shadows.
        /// </summary>
        public void OnTurnEnd(Board board)
        {
            var shadowsToRemove = new List<Vector2Int>();

            foreach (var kvp in shadowPositions)
            {
                shadowPositions[kvp.Key]--;

                if (shadowPositions[kvp.Key] <= 0)
                {
                    // Remove shadow
                    board.SetObstacle(kvp.Key, false);
                    shadowsToRemove.Add(kvp.Key);

                    Debug.Log($"Shadow dissipated at {kvp.Key}");
                }
            }

            // Clean up expired shadows
            foreach (var pos in shadowsToRemove)
            {
                shadowPositions.Remove(pos);
            }
        }

        /// <summary>
        /// Gets all active shadow positions.
        /// </summary>
        public List<Vector2Int> GetActiveShadows()
        {
            return new List<Vector2Int>(shadowPositions.Keys);
        }
    }
}
