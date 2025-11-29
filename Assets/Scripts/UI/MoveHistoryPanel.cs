using UnityEngine;
using UnityEngine.UI;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.UI
{
    /// <summary>
    /// UI panel that displays the history of moves in the current game.
    /// </summary>
    public class MoveHistoryPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Board board;

        [SerializeField]
        private Transform contentContainer;

        [SerializeField]
        private MoveHistoryEntry entryPrefab;

        [SerializeField]
        private ScrollRect scrollRect;

        private void Start()
        {
            if (board == null)
            {
                board = Board.Instance;
            }

            if (board != null)
            {
                board.OnPieceMoved += HandlePieceMoved;
            }
            else
            {
                Debug.LogWarning("MoveHistoryPanel: Board reference not found.");
            }
        }

        private void OnDestroy()
        {
            if (board != null)
            {
                board.OnPieceMoved -= HandlePieceMoved;
            }
        }

        private void HandlePieceMoved(Piece piece, Vector2Int from, Vector2Int to, Piece capturedPiece)
        {
            if (entryPrefab == null || contentContainer == null)
            {
                return;
            }

            string moveString = FormatMoveString(piece, from, to, capturedPiece);
            CreateEntry(moveString);
        }

        private string FormatMoveString(Piece piece, Vector2Int from, Vector2Int to, Piece capturedPiece)
        {
            string pieceSymbol = GetPieceSymbol(piece.Type);
            string fromCoord = ToAlgebraic(from);
            string toCoord = ToAlgebraic(to);
            
            string captureIndicator = capturedPiece != null ? "x" : "-";
            
            // Format: "N e4-f6" or "N e4xf6"
            return $"{pieceSymbol} {fromCoord}{captureIndicator}{toCoord}";
        }

        private void CreateEntry(string text)
        {
            MoveHistoryEntry entry = Instantiate(entryPrefab, contentContainer);
            entry.SetText(text);

            // Auto-scroll to bottom
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private string GetPieceSymbol(PieceType type)
        {
            return type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                PieceType.Pawn => "", // Pawns usually don't have a prefix in algebraic notation, but for simplicity we can leave it empty or use 'P'
                _ => "?"
            };
        }

        private string ToAlgebraic(Vector2Int pos)
        {
            return $"{(char)('a' + pos.x)}{pos.y + 1}";
        }
    }
}
