using UnityEngine;

namespace MutatingGambit.Core.MovementRules
{
    /// <summary>
    /// 직선 움직임 규칙 (가로 및 세로).
    /// Rook과 Queen 기물에 사용됩니다.
    /// </summary>
    [CreateAssetMenu(fileName = "StraightLineRule", menuName = "Movement Rules/Straight Line Rule")]
    public class StraightLineRule : DirectionalMovementRule
    {
        #region 보호된 메서드
        /// <summary>
        /// 직선 방향들을 가져옵니다 (상, 하, 좌, 우).
        /// </summary>
        protected override Vector2Int[] GetDirections()
        {
            return new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 위
                new Vector2Int(0, -1),  // 아래
                new Vector2Int(-1, 0),  // 왼쪽
                new Vector2Int(1, 0)    // 오른쪽
            };
        }
        #endregion
    }
}
