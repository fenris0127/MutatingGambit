using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Time Warp Artifact: Player can make 2 moves in a single turn.
    /// Passive effect that modifies turn structure.
    /// </summary>
    [CreateAssetMenu(fileName = "TimeWarpArtifact", menuName = "Mutating Gambit/Artifacts/Time Warp")]
    public class TimeWarpArtifact : Artifact
    {
        private int movesThisTurn = 0;
        private const int maxMovesPerTurn = 2;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // This artifact modifies the game flow, not individual moves
            // Implementation would require GameManager integration
        }

        public override void OnAcquired(Board board)
        {
            Debug.Log("Time Warp acquired! You can now make 2 moves per turn.");
        }

        public bool CanMakeAnotherMove()
        {
            return movesThisTurn < maxMovesPerTurn;
        }

        public void IncrementMoveCount()
        {
            movesThisTurn++;
        }

        public void ResetMoveCount()
        {
            movesThisTurn = 0;
        }
    }
}
