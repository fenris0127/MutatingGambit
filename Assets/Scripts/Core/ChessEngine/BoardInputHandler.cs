using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Systems.Tutorial;

namespace MutatingGambit.Core.ChessEngine
{
    public class BoardInputHandler : MonoBehaviour
    {
        #region 변수
        [SerializeField]
        private Board board;

        [SerializeField]
        private GameManager gameManager;

        [Header("Visualization")]
        [SerializeField]
        private GameObject highlightPrefab;

        private Piece selectedPiece;
        private Camera mainCamera;
        private List<GameObject> activeHighlights = new List<GameObject>();
        private List<Vector2Int> highlightedMoves = new List<Vector2Int>();

        private const int LeftMouseButton = 0;
        #endregion

        #region Unity 생명주기
        private void Start()
        {
            if (board == null) board = FindObjectOfType<Board>();
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!IsPlayerTurn())
            {
                return;
            }

            if (Input.GetMouseButtonDown(LeftMouseButton))
            {
                HandleInput();
            }
        }
        #endregion

        #region 비공개 메서드
        private bool IsPlayerTurn() => 
            gameManager.CurrentTurn == gameManager.PlayerTeam && gameManager.State == GameManager.GameState.PlayerTurn;

        private void HandleInput()
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                Vector2Int gridPos = GetGridPosition(hit.point);
                
                if (board.IsPositionValid(gridPos))
                {
                    HandleClick(gridPos);
                }
            }
        }

        private Vector2Int GetGridPosition(Vector2 worldPos) => 
            new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));

        private void HandleClick(Vector2Int gridPos)
        {
            Piece clickedPiece = board.GetPiece(gridPos);

            if (selectedPiece == null)
            {
                TrySelectPiece(clickedPiece);
            }
            else
            {
                if (clickedPiece != null && clickedPiece.Team == gameManager.PlayerTeam)
                {
                    ChangeSelection(clickedPiece);
                }
                else
                {
                    TryMovePiece(gridPos);
                }
            }
        }

        private void TrySelectPiece(Piece piece)
        {
            if (piece != null && piece.Team == gameManager.PlayerTeam)
            {
                SelectPiece(piece);
            }
        }

        private void ChangeSelection(Piece newPiece)
        {
            ClearHighlights();
            SelectPiece(newPiece);
        }

        private void SelectPiece(Piece piece)
        {
            selectedPiece = piece;
            highlightedMoves = selectedPiece.GetValidMoves(board);
            HighlightMoves(highlightedMoves);
            Debug.Log($"Selected {selectedPiece}");
        }

        private void TryMovePiece(Vector2Int targetPos)
        {
            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsMoveAllowed(selectedPiece.Position, targetPos))
            {
                Debug.Log("Move restricted by tutorial");
                return;
            }

            bool success = gameManager.ExecuteMove(selectedPiece.Position, targetPos);
            if (success)
            {
                ClearHighlights();
                selectedPiece = null;
            }
            else
            {
                Debug.Log("Invalid move");
            }
        }

        /// <summary>
        /// Highlights the given positions on the board.
        /// </summary>
        private void HighlightMoves(List<Vector2Int> positions)
        {
            ClearHighlights();

            if (highlightPrefab == null)
            {
                foreach (var pos in positions)
                {
                    Debug.Log($"Highlighting position: {pos}");
                }
                return;
            }

            foreach (var pos in positions)
            {
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0); 
                GameObject highlight = Instantiate(highlightPrefab, worldPos, Quaternion.identity);
                activeHighlights.Add(highlight);
            }
        }

        /// <summary>
        /// Clears all move highlights.
        /// </summary>
        private void ClearHighlights()
        {
            foreach (var highlight in activeHighlights)
            {
                if (highlight != null)
                {
                    Destroy(highlight);
                }
            }
            activeHighlights.Clear();
            highlightedMoves.Clear();
        }
        #endregion
    }
}
