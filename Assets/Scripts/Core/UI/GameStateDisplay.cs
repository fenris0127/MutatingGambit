using System.Text;
using MutatingGambit.Core.Victory;

namespace MutatingGambit.Core.UI
{
    /// <summary>
    /// Displays current game state information
    /// </summary>
    public class GameStateDisplay
    {
        public PieceColor CurrentPlayer { get; set; }
        public IVictoryCondition VictoryCondition { get; set; }
        public int MoveCount { get; set; }

        public string RenderCurrentTurn()
        {
            return $"Current Turn: {CurrentPlayer}";
        }

        public string RenderVictoryCondition()
        {
            if (VictoryCondition == null)
            {
                return "Victory Condition: None";
            }

            return $"Victory Condition: {VictoryCondition.GetDescription()}";
        }

        public string RenderMoveCount()
        {
            return $"Move Count: {MoveCount}";
        }

        public string RenderPieceStatus(Board board)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Piece Status ===");

            int brokenCount = 0;
            int damagedCount = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var piece = board.GetPieceAt(PositionCache.Get(x, y));
                    if (piece != null)
                    {
                        if (piece.IsBroken)
                        {
                            brokenCount++;
                            sb.AppendLine($"  {piece.Color} {piece.Type} - Broken (HP: 0/{piece.MaxHP})");
                        }
                        else if (piece.HP < piece.MaxHP)
                        {
                            damagedCount++;
                            sb.AppendLine($"  {piece.Color} {piece.Type} - Damaged (HP: {piece.HP}/{piece.MaxHP})");
                        }
                    }
                }
            }

            if (brokenCount == 0 && damagedCount == 0)
            {
                sb.AppendLine("  All pieces healthy");
            }

            return sb.ToString();
        }

        public string RenderFullGameState(Board board)
        {
            var sb = new StringBuilder();

            sb.AppendLine("╔═══════════════════════════╗");
            sb.AppendLine("║   THE MUTATING GAMBIT     ║");
            sb.AppendLine("╚═══════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine(RenderCurrentTurn());
            sb.AppendLine(RenderVictoryCondition());
            sb.AppendLine(RenderMoveCount());
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
