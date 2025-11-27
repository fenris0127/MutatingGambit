using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Teleporting Knight Mutation: After moving, knight teleports to a random valid square.
    /// Unpredictable and chaotic movement pattern.
    /// </summary>
    [CreateAssetMenu(fileName = "TeleportingKnightMutation", menuName = "Mutating Gambit/Mutations/Teleporting Knight")]
    public class TeleportingKnightMutation : Mutation
    {
        [SerializeField]
        [Tooltip("Maximum teleport distance from original position.")]
        private int maxTeleportDistance = 3;

        private System.Random random = new System.Random();

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Knight)
            {
                Debug.LogWarning("Teleporting Knight mutation can only be applied to knights!");
                return;
            }

            Debug.Log($"Applied Teleporting Knight mutation to {piece.Team} knight");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // No special cleanup needed
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (mutatedPiece.Type != PieceType.Knight)
            {
                return;
            }

            // Find random teleport location
            var teleportPosition = FindRandomTeleportLocation(board, to, mutatedPiece.Team);

            if (teleportPosition != to && board.IsPositionValid(teleportPosition))
            {
                // Teleport the knight
                var piece = board.GetPiece(to);
                if (piece != null)
                {
                    board.MovePiece(to, teleportPosition);
                    Debug.Log($"Knight teleported from {to} to {teleportPosition}!");
                }
            }
        }

        private Vector2Int FindRandomTeleportLocation(Board board, Vector2Int currentPos, Team team)
        {
            var validPositions = new System.Collections.Generic.List<Vector2Int>();

            // Search for valid positions within teleport range
            for (int dx = -maxTeleportDistance; dx <= maxTeleportDistance; dx++)
            {
                for (int dy = -maxTeleportDistance; dy <= maxTeleportDistance; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Vector2Int pos = currentPos + new Vector2Int(dx, dy);

                    if (board.IsPositionValid(pos) && !board.IsObstacle(pos))
                    {
                        var occupant = board.GetPiece(pos);

                        // Empty square or enemy piece
                        if (occupant == null || occupant.Team != team)
                        {
                            validPositions.Add(pos);
                        }
                    }
                }
            }

            // Return random position or current position if none found
            if (validPositions.Count > 0)
            {
                int index = random.Next(validPositions.Count);
                return validPositions[index];
            }

            return currentPos;
        }
    }
}
