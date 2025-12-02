using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Effects
{
    /// <summary>
    /// 게임의 시각 효과(파티클)를 관리합니다.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        #region 필드 - 프리팹
        [Header("프리팹")]
        [SerializeField]
        private GameObject moveEffectPrefab;

        [SerializeField]
        private GameObject captureEffectPrefab;

        [SerializeField]
        private GameObject victoryEffectPrefab;
        #endregion

        #region 필드 - 참조
        [Header("참조")]
        [SerializeField]
        private Board board;

        [SerializeField]
        private GameManager gameManager;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 이벤트를 구독하고 참조를 초기화합니다.
        /// </summary>
        private void Start()
        {
            InitializeReferences();
            SubscribeToEvents();
        }

        /// <summary>
        /// 모든 이벤트 구독을 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 할당되지 않은 경우 참조를 캐시합니다.
        /// </summary>
        private void InitializeReferences()
        {
            if (board == null)
            {
                board = Board.Instance;
            }

            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }
        }
        #endregion

        #region 비공개 메서드 - 이벤트 구독
        /// <summary>
        /// 보드 및 게임 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeToEvents()
        {
            SubscribeToBoardEvents();
            SubscribeToGameEvents();
        }

        /// <summary>
        /// 보드 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeToBoardEvents()
        {
            if (board != null)
            {
                board.OnPieceMoved += HandlePieceMoved;
            }
        }

        /// <summary>
        /// 게임 이벤트를 구독합니다.
        /// </summary>
        private void SubscribeToGameEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnVictory.AddListener(PlayVictoryEffect);
            }
        }

        /// <summary>
        /// 모든 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            UnsubscribeFromBoardEvents();
            UnsubscribeFromGameEvents();
        }

        /// <summary>
        /// 보드 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromBoardEvents()
        {
            if (board != null)
            {
                board.OnPieceMoved -= HandlePieceMoved;
            }
        }

        /// <summary>
        /// 게임 이벤트 구독을 해제합니다.
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnVictory.RemoveListener(PlayVictoryEffect);
            }
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 기물 이동 이벤트를 처리합니다.
        /// </summary>
        /// <param name="piece">이동한 기물</param>
        /// <param name="from">시작 위치</param>
        /// <param name="to">목표 위치</param>
        /// <param name="capturedPiece">포획된 기물 (있는 경우)</param>
        private void HandlePieceMoved(Piece piece, Vector2Int from, Vector2Int to, Piece capturedPiece)
        {
            Vector3 targetPos = ConvertBoardToWorldPosition(to);

            if (IsCaptureMove(capturedPiece))
            {
                PlayCaptureEffect(targetPos);
            }
            else
            {
                PlayMoveEffect(targetPos);
            }
        }

        /// <summary>
        /// 승리 효과를 재생합니다.
        /// </summary>
        private void PlayVictoryEffect()
        {
            SpawnEffect(victoryEffectPrefab, Vector3.zero);
        }
        #endregion

        #region 비공개 메서드 - 효과 재생
        /// <summary>
        /// 이동 효과를 재생합니다.
        /// </summary>
        private void PlayMoveEffect(Vector3 position)
        {
            SpawnEffect(moveEffectPrefab, position);
        }

        /// <summary>
        /// 포획 효과를 재생합니다.
        /// </summary>
        private void PlayCaptureEffect(Vector3 position)
        {
            SpawnEffect(captureEffectPrefab, position);
        }

        /// <summary>
        /// 지정된 위치에 효과를 생성합니다.
        /// </summary>
        /// <param name="prefab">효과 프리팹</param>
        /// <param name="position">생성 위치</param>
        private void SpawnEffect(GameObject prefab, Vector3 position)
        {
            if (prefab != null)
            {
                Instantiate(prefab, position, Quaternion.identity);
            }
        }
        #endregion

        #region 비공개 메서드 - 유틸리티
        /// <summary>
        /// 보드 좌표를 월드 위치로 변환합니다.
        /// </summary>
        /// <param name="boardPosition">보드 위치</param>
        /// <returns>월드 위치</returns>
        private Vector3 ConvertBoardToWorldPosition(Vector2Int boardPosition)
        {
            // 타일당 1 유닛 가정, 보드가 적절히 중앙 또는 오프셋 처리됨
            // 실제 보드 시각화에 따라 조정 필요
            return new Vector3(boardPosition.x, boardPosition.y, 0);
        }

        /// <summary>
        /// 포획 이동인지 확인합니다.
        /// </summary>
        /// <param name="capturedPiece">포획된 기물</param>
        /// <returns>포획 이동 여부</returns>
        private bool IsCaptureMove(Piece capturedPiece)
        {
            return capturedPiece != null;
        }
        #endregion
    }
}
