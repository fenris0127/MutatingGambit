using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations.Chaos
{
    /// <summary>
    /// 각 이동마다 30% 확률로 무작위 유효한 칸으로 이동합니다.
    /// Each move, this piece has a 30% chance to move to a random valid square instead.
    ///
    /// <para>구현 노트 (Implementation Notes):</para>
    /// <para>
    /// 현재 OnMove는 이동이 완료된 후에 호출되므로, 목적지를 직접 변경할 수 없습니다.
    /// 실제로 혼돈의 이동을 구현하려면 다음 중 하나의 방법이 필요합니다:
    /// </para>
    ///
    /// <list type="number">
    /// <item>
    /// <term>OnBeforeMove 훅 추가:</term>
    /// <description>Board/GameManager에서 이동 전에 호출되는 이벤트를 추가하여 목적지를 변경</description>
    /// </item>
    /// <item>
    /// <term>재이동 (Re-move):</term>
    /// <description>OnMove에서 현재 위치(to)에서 무작위 위치로 Board.MovePiece() 호출</description>
    /// </item>
    /// <item>
    /// <term>UI 통합:</term>
    /// <description>사용자가 이동을 선택할 때 확률적으로 다른 목적지로 변경 (가장 UX 친화적)</description>
    /// </item>
    /// </list>
    ///
    /// <para>Currently, OnMove is called after the move is complete, so we cannot change the destination directly.</para>
    /// <para>To implement chaotic movement, one of these approaches is needed:</para>
    /// <para>1. Add OnBeforeMove hook in Board/GameManager</para>
    /// <para>2. Re-move from current position to random position using Board.MovePiece()</para>
    /// <para>3. UI integration: Change destination probabilistically when user selects move (most UX-friendly)</para>
    /// </summary>
    [CreateAssetMenu(fileName = "ChaosStepMutation", menuName = "MutatingGambit/Mutations/Chaos/Chaos Step")]
    public class ChaosStepMutation : Mutation
    {
        #region Fields

        [SerializeField]
        [Tooltip("혼돈 이동이 발생할 확률 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        private float chaosChance = 0.3f;

        /// <summary>
        /// 혼돈 이동 확률을 가져오거나 설정합니다
        /// </summary>
        public float ChaosChance
        {
            get => chaosChance;
            set => chaosChance = Mathf.Clamp01(value);
        }

        #endregion

        #region Mutation Lifecycle

        public override void ApplyToPiece(Piece piece)
        {
            // 현재는 상태 초기화가 필요 없음
            // 향후 통계 추적이 필요하면 MutationState 사용
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // 정리 작업 없음
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 기물 이동 후 호출됩니다. 혼돈 확률에 따라 추가 이동을 시도합니다.
        /// Called after piece moves. Attempts additional move based on chaos chance.
        /// </summary>
        /// <remarks>
        /// 주의: 현재 구현은 데모/로그 용도입니다. 실제 이동 변경은 게임 시스템 통합이 필요합니다.
        /// Note: Current implementation is for demo/logging purposes. Actual move override requires game system integration.
        /// </remarks>
        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            // 혼돈 확률 체크
            if (Random.value < chaosChance)
            {
                // 현재 위치에서 가능한 모든 이동 가져오기
                var validMoves = mutatedPiece.GetValidMoves(board);

                if (validMoves.Count > 0)
                {
                    // 무작위 목적지 선택
                    var randomMove = validMoves[Random.Range(0, validMoves.Count)];

                    Debug.Log($"Chaos Step activated! Piece at {to} would move to {randomMove}");

                    // TODO: 실제 이동 구현
                    // 방법 1: board.MovePiece(to, randomMove);
                    // 방법 2: GameManager에 이동 요청 이벤트 발생
                    // 방법 3: OnBeforeMove 훅에서 처리하도록 변경

                    // 임시 구현: 이동만 로그
                    // Temporary implementation: Only log the movement
                }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 혼돈 이동을 실행합니다 (게임 시스템 통합 후 사용)
        /// Executes chaos movement (use after game system integration)
        /// </summary>
        /// <param name="piece">이동할 기물</param>
        /// <param name="currentPos">현재 위치</param>
        /// <param name="board">체스판</param>
        /// <returns>실제로 이동했는지 여부</returns>
        public bool TryExecuteChaosMove(Piece piece, Vector2Int currentPos, Board board)
        {
            if (piece == null || board == null) return false;

            var validMoves = piece.GetValidMoves(board);
            if (validMoves.Count == 0) return false;

            // 무작위 목적지 선택
            var randomMove = validMoves[Random.Range(0, validMoves.Count)];

            // 실제 이동 (GameManager나 Board가 제공하는 이동 메서드 사용)
            // board.MovePiece(currentPos, randomMove);

            Debug.Log($"Chaos Step: {piece.Type} moved from {currentPos} to {randomMove}");
            return true;
        }

        #endregion
    }
}
