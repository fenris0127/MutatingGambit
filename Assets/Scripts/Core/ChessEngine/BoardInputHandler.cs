using UnityEngine;
using MutatingGambit.Systems.Tutorial;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 플레이어 입력을 처리하고 기물 선택 및 이동을 관리합니다.
    /// </summary>
    public class BoardInputHandler : MonoBehaviour
    {
        #region 변수
        private const int LEFT_MOUSE_BUTTON = 0;

        [Header("References")]
        [SerializeField]
        private Board board;

        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private Camera mainCamera;

        [Header("Visual Feedback")]
        [SerializeField]
        private GameObject highlightPrefab;

        [SerializeField]
        private Color validMoveColor = Color.green;

        [SerializeField]
        private Color selectedColor = Color.yellow;

        private Piece selectedPiece;
        private Vector2Int selectedPosition;
        private GameObject[] highlightObjects = new GameObject[0];
        #endregion

        #region Unity 생명주기
        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (board == null)
            {
                board = Board.Instance;
            }

            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON))
            {
                HandleInput();
            }
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 현재 플레이어 턴인지 확인합니다.
        /// </summary>
        private bool IsPlayerTurn() => gameManager.CurrentTurn == Team.White;

        /// <summary>
        /// 마우스 입력을 처리하고 기물을 선택하거나 이동합니다.
        /// </summary>
        private void HandleInput()
        {
            if (!IsPlayerTurn()) return;

            Vector2Int gridPosition = GetGridPosition();
            if (!board.IsPositionValid(gridPosition)) return;

            if (selectedPiece == null)
            {
                TrySelectPiece(gridPosition);
            }
            else
            {
                TryMovePiece(gridPosition);
            }
        }

        /// <summary>
        /// 마우스 위치를 그리드 좌표로 변환합니다.
        /// </summary>
        private Vector2Int GetGridPosition()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null)
            {
                Vector3 hitPoint = hit.point;
                return new Vector2Int(Mathf.FloorToInt(hitPoint.x), Mathf.FloorToInt(hitPoint.y));
            }
            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// 지정된 위치에서 기물을 선택하려고 시도합니다.
        /// </summary>
        private void TrySelectPiece(Vector2Int position)
        {
            Piece piece = board.GetPiece(position);

            if (piece != null && piece.Team == Team.White)
            {
                selectedPiece = piece;
                selectedPosition = position;

                ShowValidMoves();

                // 튜토리얼 통합
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnPieceSelected(piece);
                }
            }
        }

        /// <summary>
        /// 선택된 기물을 목표 위치로 이동하려고 시도합니다.
        /// </summary>
        private void TryMovePiece(Vector2Int targetPosition)
        {
            var validMoves = MoveValidator.GetValidMoves(board, selectedPosition);

            if (validMoves.Contains(targetPosition))
            {
                gameManager.ExecuteMove(selectedPosition, targetPosition);
                ClearSelection();
            }
            else
            {
                ClearSelection();
            }
        }

        /// <summary>
        /// 선택된 기물의 유효한 이동을 시각적으로 표시합니다.
        /// </summary>
        private void ShowValidMoves()
        {
            ClearHighlights();

            if (selectedPiece == null) return;

            var validMoves = MoveValidator.GetValidMoves(board, selectedPosition);

            highlightObjects = new GameObject[validMoves.Count + 1];

            // 선택된 기물 강조 표시
            highlightObjects[0] = CreateHighlight(selectedPosition, selectedColor);

            // 유효한 이동 강조 표시
            for (int i = 0; i < validMoves.Count; i++)
            {
                highlightObjects[i + 1] = CreateHighlight(validMoves[i], validMoveColor);
            }
        }

        /// <summary>
        /// 지정된 위치에 강조 표시 객체를 생성합니다.
        /// </summary>
        private GameObject CreateHighlight(Vector2Int position, Color color)
        {
            if (highlightPrefab == null) return null;

            GameObject highlight = Instantiate(highlightPrefab);
            highlight.transform.position = new Vector3(position.x + 0.5f, position.y + 0.5f, 0);

            var renderer = highlight.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }

            return highlight;
        }

        /// <summary>
        /// 모든 강조 표시 객체를 제거합니다.
        /// </summary>
        private void ClearHighlights()
        {
            foreach (var highlight in highlightObjects)
            {
                if (highlight != null)
                {
                    Destroy(highlight);
                }
            }
            highlightObjects = new GameObject[0];
        }

        /// <summary>
        /// 기물 선택 및 강조 표시를 지웁니다.
        /// </summary>
        private void ClearSelection()
        {
            selectedPiece = null;
            selectedPosition = new Vector2Int(-1, -1);
            ClearHighlights();
        }
        #endregion
    }
}
