namespace MutatingGambit.Core.AI
{
    /// <summary>
    /// Represents a move that the AI can make
    /// </summary>
    public class AIMove
    {
        public Position From { get; set; }
        public Position To { get; set; }
        public float Evaluation { get; set; }

        public AIMove(Position from, Position to, float evaluation = 0)
        {
            From = from;
            To = to;
            Evaluation = evaluation;
        }

        public override string ToString()
        {
            return $"{From} -> {To} (Eval: {Evaluation:F2})";
        }
    }
}
