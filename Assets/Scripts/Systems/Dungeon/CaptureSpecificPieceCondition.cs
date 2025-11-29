using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 승리 조건: 특정 적 기물을 잡습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CaptureSpecificPiece", menuName = "Victory Conditions/Capture Specific Piece")]
    public class CaptureSpecificPieceCondition : VictoryCondition
    {
        #region 변수
        [Header("Target")]
        [SerializeField]
        [Tooltip("잡아야 하는 기물의 종류.")]
        private PieceType targetPieceType = PieceType.Queen;

        [SerializeField]
        [Tooltip("목표 기물의 시작 위치 (식별용).")]
        private Vector2Int targetStartPosition;

        [SerializeField]
        [Tooltip("true인 경우 목표 종류의 모든 기물이 해당됩니다. false인 경우 특정 위치에 있어야 합니다.")]
        private bool anyOfType = true;

        private bool targetCaptured = false;
        #endregion

        #region 속성
        /// <summary>
        /// 목표 기물 타입을 가져옵니다.
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

            // 목표 기물이 아직 존재하는지 확인
            bool targetExists = false;

            foreach (var piece in enemyPieces)
            {
                if (anyOfType)
                {
                    // 이 타입의 모든 기물
                    if (piece.Type == targetPieceType)
                    {
                        targetExists = true;
                        break;
                    }
                }
                else
                {
                    // 위치의 특정 기물
                    if (piece.Type == targetPieceType && piece.Position == targetStartPosition)
                    {
                        targetExists = true;
                        break;
                    }
                }
            }

            // 목표가 더 이상 존재하지 않으면 승리 (잡힘)
            targetCaptured = !targetExists;
            return targetCaptured;
        }

        public override bool IsDefeatConditionMet(Board board, int currentTurn, Team playerTeam) => false;

        public override string GetProgressString(Board board, int currentTurn, Team playerTeam) => 
            targetCaptured ? $"목표 {targetPieceType} 잡음!" : $"적 {targetPieceType}를 잡으세요";

        public override void Reset() => targetCaptured = false;

        /// <summary>
        /// 목표 기물 매개변수를 설정합니다.
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
