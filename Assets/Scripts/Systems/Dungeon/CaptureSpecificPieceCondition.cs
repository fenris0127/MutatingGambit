using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Victory condition: Capture a specific enemy piece.
    /// </summary>
    [CreateAssetMenu(fileName = "CaptureSpecificPiece", menuName = "Victory Conditions/Capture Specific Piece")]
    public class CaptureSpecificPieceCondition : VictoryCondition
    {
        #region 변수
        [Header("Target")]
        [SerializeField]
        [Tooltip("The type of piece that must be captured.")]
        private PieceType targetPieceType = PieceType.Queen;

        [SerializeField]
        [Tooltip("The starting position of the target piece (for identification).")]
        private Vector2Int targetStartPosition;

        [SerializeField]
        [Tooltip("If true, any piece of the target type counts. If false, must be at specific position.")]
        private bool anyOfType = true;

        private bool targetCaptured = false;
        #endregion

        #region 속성
        /// <summary>
        /// Gets the target piece type.
        /// </summary>
        public PieceType TargetPieceType => targetPieceType;
        #endregion

        #region 공개 메서드
        public override bool IsVictoryAchieved(Board board, int currentTurn, Team playerTeam)
        {
            if (targetCaptured)
            {
                return true;
            }

            Team enemyTeam = playerTeam == Team.White ? Team.Black : Team.White;
            var enemyPieces = board.GetPiecesByTeam(enemyTeam);

            // Check if target piece still exists
            bool targetExists = false;

            foreach (var piece in enemyPieces)
            {
                if (anyOfType)
                {
                    // Any piece of this type
                    if (piece.Type == targetPieceType)
                    {
                        targetExists = true;
                        break;
                    }
                }
                else
                {
                    // Specific piece at position
                    if (piece.Type == targetPieceType && piece.Position == targetStartPosition)
                    {
                        targetExists = true;
                        break;
                    }
                }
            }

            // Victory if target no longer exists (was captured)
            targetCaptured = !targetExists;
            return targetCaptured;
        }

        public override bool IsDefeatConditionMet(Board board, int currentTurn, Team playerTeam) => false;

        public override string GetProgressString(Board board, int currentTurn, Team playerTeam) => 
            targetCaptured ? $"Target {targetPieceType} captured!" : $"Capture enemy {targetPieceType}";

        public override void Reset() => targetCaptured = false;

        /// <summary>
        /// Sets the target piece parameters.
        /// </summary>
        public void SetTarget(PieceType type, Vector2Int position, bool anyType = true)
        {
            targetPieceType = type;
            targetStartPosition = position;
            anyOfType = anyType;
        }
        #endregion
    }
}
