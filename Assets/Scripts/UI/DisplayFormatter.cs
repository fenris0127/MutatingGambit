using System.Collections.Generic;
using System.Text;

namespace MutatingGambit.Core.UI
{
    /// <summary>
    /// Utility class for formatting display elements
    /// </summary>
    public class DisplayFormatter
    {
        public string FormatPosition(Position position)
        {
            if (position == null)
            {
                return "none";
            }

            return position.ToNotation();
        }

        public string FormatMoveHistory(List<string> moves)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Move History ===");

            for (int i = 0; i < moves.Count; i++)
            {
                if (i % 2 == 0)
                {
                    sb.Append($"{(i / 2) + 1}. ");
                }

                sb.Append($"{moves[i]} ");

                if (i % 2 == 1)
                {
                    sb.AppendLine();
                }
            }

            if (moves.Count % 2 == 1)
            {
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string FormatHP(Piece piece)
        {
            if (piece == null)
            {
                return "N/A";
            }

            var barLength = 10;
            var filledBars = (int)((float)piece.HP / piece.MaxHP * barLength);
            var emptyBars = barLength - filledBars;

            var sb = new StringBuilder();
            sb.Append("[");
            sb.Append(new string('█', filledBars));
            sb.Append(new string('░', emptyBars));
            sb.Append($"] {piece.HP}/{piece.MaxHP}");

            return sb.ToString();
        }

        public string FormatPieceInfo(Piece piece, Position position)
        {
            if (piece == null)
            {
                return "Empty square";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{piece.Color} {piece.Type} at {FormatPosition(position)}");
            sb.AppendLine($"HP: {FormatHP(piece)}");

            if (piece.Mutations.Count > 0)
            {
                sb.AppendLine("Mutations:");
                foreach (var mutation in piece.Mutations)
                {
                    sb.AppendLine($"  • {mutation.Name}");
                }
            }

            return sb.ToString();
        }
    }
}
