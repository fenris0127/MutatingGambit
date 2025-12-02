using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 대각선 움직임 규칙입니다.
    /// Bishop과 Queen 기물에 사용됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "DiagonalRule", menuName = "Movement Rules/Diagonal Rule")]
    public class DiagonalRule : DirectionalMovementRule
    {
        #region 필드
        [SerializeField]
        [Tooltip("true이면 기물이 정확히 최대 거리만큼 이동해야 합니다 (예: Glass Bishop 변이).")]
        private bool exactDistance = false;
        #endregion

        #region 보호된 메서드 - 방향
        /// <summary>
        /// 대각선 방향들을 가져옵니다 (4개 대각선).
        /// </summary>
        protected override Vector2Int[] GetDirections()
        {
            return new Vector2Int[]
            {
                new Vector2Int(1, 1),    // 우상
                new Vector2Int(1, -1),   // 우하
                new Vector2Int(-1, 1),   // 좌상
                new Vector2Int(-1, -1)   // 좌하
            };
        }
        #endregion

        #region 보호된 메서드 - 거리 검증 (오버라이드)
        /// <summary>
        /// 빈 칸으로 이동할 수 있는지 확인합니다.
        /// exactDistance가 true면 정확한 거리만 허용합니다.
        /// </summary>
        protected override bool CanMoveToEmptySquare(int distance)
        {
            if (!exactDistance)
            {
                return true; // 정확한 거리 불필요 - 모든 빈 칸 허용
            }

            return maxDistance <= 0 || distance == maxDistance;
        }

        /// <summary>
        /// 해당 거리에서 캡처할 수 있는지 확인합니다.
        /// exactDistance가 true면 정확한 거리만 허용합니다.
        /// </summary>
        protected override bool CanCaptureAtDistance(int distance)
        {
            if (!exactDistance)
            {
                return true; // 정확한 거리 불필요 - 모든 거리에서 캡처 허용
            }

            return maxDistance <= 0 || distance == maxDistance;
        }
        #endregion
    }
}
