using System.Text;

namespace MutatingGambit.Core.UI
{
    /// <summary>
    /// Handles board visualization
    /// </summary>
    public class BoardDisplay
    {
        public bool HighlightMoves { get; set; }
        public Position SelectedPosition { get; set; }

        private readonly char[] _pieceSymbols = new char[]
        {
            '♔', '♕', '♖', '♗', '♘', '♙',  // White
            '♚', '♛', '♜', '♝', '♞', '♟'   // Black
        };

        public string RenderBoard(Board board)
        {
            var sb = new StringBuilder();

            sb.AppendLine("   a b c d e f g h");
            sb.AppendLine("  ┌─────────────────┐");

            for (int rank = 7; rank >= 0; rank--)
            {
                sb.Append($"{rank + 1} │ ");

                for (int file = 0; file < 8; file++)
                {
                    var pos = PositionCache.Get(file, rank);
                    var piece = board.GetPieceAt(pos);
                    var tile = board.GetTileAt(pos);

                    if (piece != null)
                    {
                        char symbol = GetPieceSymbol(piece);

                        // Mark mutated pieces
                        if (piece.Mutations.Count > 0)
                        {
                            sb.Append($"{symbol}*");
                        }
                        else
                        {
                            sb.Append($"{symbol} ");
                        }
                    }
                    else if (tile.IsObstacle)
                    {
                        sb.Append("█ ");
                    }
                    else
                    {
                        // Highlight legal moves if enabled
                        if (HighlightMoves && SelectedPosition != null)
                        {
                            var moves = Movement.MoveValidator.GetLegalMoves(board, SelectedPosition);
                            if (moves.Contains(pos))
                            {
                                sb.Append("· ");
                            }
                            else
                            {
                                sb.Append("  ");
                            }
                        }
                        else
                        {
                            sb.Append("  ");
                        }
                    }
                }

                sb.AppendLine($"│ {rank + 1}");
            }

            sb.AppendLine("  └─────────────────┘");
            sb.AppendLine("   a b c d e f g h");

            return sb.ToString();
        }

        public string RenderArtifacts(Board board)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Active Artifacts ===");

            var artifacts = board.ArtifactManager.Artifacts;
            if (artifacts.Count == 0)
            {
                sb.AppendLine("  (none)");
            }
            else
            {
                foreach (var artifact in artifacts)
                {
                    sb.AppendLine($"  • {artifact.Name}");
                    sb.AppendLine($"    {artifact.Description}");
                }
            }

            return sb.ToString();
        }

        private char GetPieceSymbol(Piece piece)
        {
            int index = (int)piece.Type;
            if (piece.Color == PieceColor.Black)
            {
                index += 6;
            }
            return _pieceSymbols[index];
        }
    }
}
