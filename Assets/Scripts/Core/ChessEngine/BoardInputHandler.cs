using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Systems.Tutorial;

namespace MutatingGambit.Core.ChessEngine
{
    public class BoardInputHandler : MonoBehaviour
    {
        [SerializeField]
        private Board board;

        [SerializeField]
        private GameManager gameManager;

        [Header("Visualization")]
        [SerializeField]
        private GameObject highlightPrefab;

        private Piece selectedPiece;
        private Camera mainCamera;
        private List<Vector2Int> highlightedMoves = new List<Vector2Int>();

        private void Start()
        {
            if (board == null) board = FindObjectOfType<Board>();
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (gameManager.CurrentTurn != gameManager.PlayerTeam || gameManager.State != GameManager.GameState.PlayerTurn)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                // Check if we clicked a piece or a tile
                Piece clickedPiece = hit.collider.GetComponent<Piece>();
                // If piece has collider, great. If not, maybe tile has it.
                // Assuming Tile has collider.
                
                // Let's assume we get the grid position from the hit point or component
                Vector2Int gridPos = GetGridPosition(hit.point);
                
                if (board.IsPositionValid(gridPos))
                {
                    HandleClick(gridPos);
                }
            }
        }

        private Vector2Int GetGridPosition(Vector2 worldPos)
        {
            // Assuming 1 unit per tile and centered at 0.5, 0.5 or something.
            // We need to know the board's origin and tile size.
            // For now, let's assume standard integer coordinates.
            return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        }

        private void HandleClick(Vector2Int gridPos)
        {
            Piece clickedPiece = board.GetPiece(gridPos);

            if (selectedPiece == null)
            {
                // Select piece
                if (clickedPiece != null && clickedPiece.Team == gameManager.PlayerTeam)
                {
                    selectedPiece = clickedPiece;
                    
                    // Show highlights for valid moves
                    highlightedMoves = selectedPiece.GetValidMoves(board);
                    HighlightMoves(highlightedMoves);
                    
                    Debug.Log($"Selected {selectedPiece}");
                }
            }
            else
            {
                // Move or Deselect
                if (clickedPiece != null && clickedPiece.Team == gameManager.PlayerTeam)
                {
                    // Change selection
                    ClearHighlights();
                    
                    selectedPiece = clickedPiece;
                    highlightedMoves = selectedPiece.GetValidMoves(board);
                    HighlightMoves(highlightedMoves);
                    
                    Debug.Log($"Selected {selectedPiece}");
                }
                else
                {
                    // Attempt move
                    if (TutorialManager.Instance != null && !TutorialManager.Instance.IsMoveAllowed(selectedPiece.Position, gridPos))
                    {
                        Debug.Log("Move restricted by tutorial");
                        return;
                    }

                    // Validate move
                    // We need a MoveValidator or check piece rules
                    // For now, let's assume GameManager.ExecuteMove handles validation or we check Piece.GetValidMoves
                    
                    bool success = gameManager.ExecuteMove(selectedPiece.Position, gridPos);
                    if (success)
                    {
                        // Clear highlights and deselect
                        ClearHighlights();
                        selectedPiece = null;
                    }
                    else
                    {
                        Debug.Log("Invalid move");
                    }
                }
            }
        }

        private List<GameObject> activeHighlights = new List<GameObject>();

        /// <summary>
        /// Highlights the given positions on the board.
        /// </summary>
        private void HighlightMoves(List<Vector2Int> positions)
        {
            ClearHighlights();

            if (highlightPrefab == null)
            {
                // Fallback to debug log if no prefab assigned
                foreach (var pos in positions)
                {
                    Debug.Log($"Highlighting position: {pos}");
                }
                return;
            }

            foreach (var pos in positions)
            {
                // Instantiate highlight prefab at position
                // Assuming board tiles are at (x, y, 0) or similar. 
                // We might need to adjust Z or Y depending on board orientation.
                // Assuming 2D for now as per other scripts.
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
    }
}
