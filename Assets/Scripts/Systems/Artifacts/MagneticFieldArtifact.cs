using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Magnetic Field Artifact: All pieces are gradually pulled toward the center of the board.
    /// Creates dynamic positional changes each turn.
    /// </summary>
    [CreateAssetMenu(fileName = "MagneticFieldArtifact", menuName = "Mutating Gambit/Artifacts/Magnetic Field")]
    public class MagneticFieldArtifact : Artifact
    {
        [SerializeField]
        private float pullStrength = 1f;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context == null || board == null)
            {
                return;
            }

            // Pull all pieces toward center
            Vector2Int center = new Vector2Int(board.Width / 2, board.Height / 2);
            List<Piece> allPieces = board.GetAllPieces();

            foreach (var piece in allPieces)
            {
                if (piece == null || piece.Type == PieceType.King)
                {
                    continue; // Don't pull kings
                }

                Vector2Int currentPos = piece.Position;
                Vector2Int direction = center - currentPos;

                // Normalize to single step
                if (direction.x != 0) direction.x = direction.x > 0 ? 1 : -1;
                if (direction.y != 0) direction.y = direction.y > 0 ? 1 : -1;

                Vector2Int targetPos = currentPos + direction;

                // Try to move piece one step toward center
                if (board.IsPositionValid(targetPos) &&
                    !board.IsObstacle(targetPos) &&
                    board.GetPiece(targetPos) == null)
                {
                    board.MovePiece(currentPos, targetPos);
                    Debug.Log($"Magnetic field pulled {piece.Type} toward center");
                }
            }
        }
    }
}
