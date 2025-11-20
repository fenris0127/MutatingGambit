using UnityEngine;
using TMPro;
using System;
using MutatingGambit.Core;
using MutatingGambit.Core.Map;
using MutatingGambit.Core.Rooms;
using MutatingGambit.Core.Victory;
using MutatingGambit.Core.AI;
using MutatingGambit.Core.Movement;
using MutatingGambit.Core.UI;

namespace MutatingGambit
{
    /// <summary>
    /// Handles chess puzzle combat with victory conditions
    /// </summary>
    public class CombatController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI boardText;
        public TextMeshProUGUI victoryConditionText;
        public TextMeshProUGUI statusText;
        public TMP_InputField inputField;

        private Board _board;
        private MapNode _currentNode;
        private IVictoryCondition _victoryCondition;
        private BoardDisplay _boardDisplay;
        private ChessAI _ai;
        private PieceColor _currentPlayer = PieceColor.White;
        private Position _selectedPosition;
        private int _moveCount = 0;

        public event Action<MapNode> OnCombatWon;
        public event Action OnCombatLost;

        public void Initialize(Board playerBoard, MapNode node)
        {
            _board = playerBoard;
            _currentNode = node;
            _currentPlayer = PieceColor.White;
            _selectedPosition = null;
            _moveCount = 0;

            // Setup victory condition
            var combatRoom = node.Room as CombatRoom;
            if (combatRoom != null)
            {
                _victoryCondition = combatRoom.VictoryCondition;
            }

            // If no victory condition, create default one
            if (_victoryCondition == null)
            {
                // Default: Capture all black pieces except pawns
                _victoryCondition = new CaptureSpecificPieceCondition(PieceType.Queen, PieceColor.Black);
            }

            // Setup AI
            var difficulty = combatRoom != null
                ? (combatRoom.Difficulty == RoomDifficulty.Elite ? Difficulty.Hard :
                   combatRoom.Difficulty == RoomDifficulty.Boss ? Difficulty.Master :
                   Difficulty.Normal)
                : Difficulty.Normal;

            _ai = new ChessAI(PieceColor.Black, difficulty);

            // Setup display
            _boardDisplay = new BoardDisplay();

            // Setup input
            if (inputField != null)
            {
                inputField.onSubmit.RemoveAllListeners();
                inputField.onSubmit.AddListener(OnInputSubmit);
                inputField.ActivateInputField();
            }

            UpdateDisplay();
        }

        void OnInputSubmit(string input)
        {
            if (string.IsNullOrEmpty(input))
                return;

            ProcessPlayerInput(input);

            if (inputField != null)
            {
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }

        void ProcessPlayerInput(string input)
        {
            input = input.Trim().ToLower();

            if (input.Contains("-"))
            {
                string[] parts = input.Split('-');
                if (parts.Length == 2)
                {
                    TryMove(parts[0], parts[1]);
                }
            }
            else
            {
                if (_selectedPosition == null)
                {
                    TrySelectPiece(input);
                }
                else
                {
                    TryMove(_selectedPosition.ToNotation(), input);
                }
            }
        }

        void TrySelectPiece(string notation)
        {
            try
            {
                var pos = Position.FromNotation(notation);
                var piece = _board.GetPieceAt(pos);

                if (piece == null)
                {
                    Debug.Log("No piece at " + notation);
                    return;
                }

                if (piece.Color != _currentPlayer)
                {
                    Debug.Log("Not your piece!");
                    return;
                }

                if (piece.IsBroken)
                {
                    Debug.Log("Piece is broken!");
                    return;
                }

                _selectedPosition = pos;
                _boardDisplay.SelectedPosition = pos;
                _boardDisplay.HighlightMoves = true;

                Debug.Log($"Selected {piece.Color} {piece.Type} at {notation}");
                UpdateDisplay();
            }
            catch
            {
                Debug.Log("Invalid notation: " + notation);
            }
        }

        void TryMove(string fromNotation, string toNotation)
        {
            try
            {
                var from = Position.FromNotation(fromNotation);
                var to = Position.FromNotation(toNotation);

                var piece = _board.GetPieceAt(from);
                if (piece == null || piece.Color != _currentPlayer || piece.IsBroken)
                {
                    Debug.Log("Invalid move!");
                    _selectedPosition = null;
                    _boardDisplay.SelectedPosition = null;
                    UpdateDisplay();
                    return;
                }

                var legalMoves = MoveValidator.GetLegalMoves(_board, from);
                if (!legalMoves.Contains(to))
                {
                    Debug.Log("Illegal move!");
                    _selectedPosition = null;
                    _boardDisplay.SelectedPosition = null;
                    UpdateDisplay();
                    return;
                }

                // Execute move
                var capturedPiece = _board.GetPieceAt(to);
                _board.PlacePiece(null, from);
                _board.PlacePiece(piece, to);
                piece.HasMoved = true;

                // If captured, deal damage
                if (capturedPiece != null)
                {
                    capturedPiece.TakeDamage(999); // Instant break
                    Debug.Log($"Captured {capturedPiece.Color} {capturedPiece.Type}!");
                }

                _moveCount++;
                Debug.Log($"Moved {piece.Type} from {fromNotation} to {toNotation}");

                // Clear selection
                _selectedPosition = null;
                _boardDisplay.SelectedPosition = null;
                _boardDisplay.HighlightMoves = false;

                // Check victory
                if (CheckVictory())
                {
                    return;
                }

                // Switch to AI
                _currentPlayer = PieceColor.Black;
                UpdateDisplay();

                Invoke(nameof(ExecuteAIMove), 0.5f);
            }
            catch (Exception e)
            {
                Debug.Log("Invalid move: " + e.Message);
                _selectedPosition = null;
                _boardDisplay.SelectedPosition = null;
                UpdateDisplay();
            }
        }

        void ExecuteAIMove()
        {
            var aiMove = _ai.SelectBestMove(_board);

            if (aiMove == null)
            {
                Debug.Log("AI has no legal moves - Player wins!");
                OnCombatWon?.Invoke(_currentNode);
                return;
            }

            var piece = _board.GetPieceAt(aiMove.From);
            var capturedPiece = _board.GetPieceAt(aiMove.To);

            _board.PlacePiece(null, aiMove.From);
            _board.PlacePiece(piece, aiMove.To);
            piece.HasMoved = true;

            if (capturedPiece != null)
            {
                capturedPiece.TakeDamage(999);
                Debug.Log($"AI captured {capturedPiece.Color} {capturedPiece.Type}!");

                // Check if king was captured
                if (capturedPiece.Type == PieceType.King && capturedPiece.Color == PieceColor.White)
                {
                    Debug.Log("Your king was captured - DEFEAT!");
                    OnCombatLost?.Invoke();
                    return;
                }
            }

            Debug.Log($"AI moved {piece.Type} from {aiMove.From.ToNotation()} to {aiMove.To.ToNotation()}");

            _currentPlayer = PieceColor.White;
            UpdateDisplay();

            // Check if player lost
            if (IsPlayerDefeated())
            {
                OnCombatLost?.Invoke();
            }
        }

        bool CheckVictory()
        {
            if (_victoryCondition != null && _victoryCondition.IsMet(_board))
            {
                Debug.Log("VICTORY! Objective complete!");
                OnCombatWon?.Invoke(_currentNode);
                return true;
            }
            return false;
        }

        bool IsPlayerDefeated()
        {
            // Check if white king is broken
            var whiteKing = FindKing(PieceColor.White);
            if (whiteKing == null || whiteKing.IsBroken)
            {
                Debug.Log("Your king is defeated!");
                return true;
            }
            return false;
        }

        Piece FindKing(PieceColor color)
        {
            for (int x = 0; x < _board.Width; x++)
            {
                for (int y = 0; y < _board.Height; y++)
                {
                    var pos = new Position(x, y);
                    var piece = _board.GetPieceAt(pos);
                    if (piece != null && piece.Type == PieceType.King && piece.Color == color)
                    {
                        return piece;
                    }
                }
            }
            return null;
        }

        void UpdateDisplay()
        {
            if (boardText != null)
            {
                boardText.text = _boardDisplay.RenderBoard(_board);
            }

            if (victoryConditionText != null)
            {
                string conditionText = "=== OBJECTIVE ===\n";
                if (_victoryCondition != null)
                {
                    conditionText += _victoryCondition.GetDescription();
                }
                else
                {
                    conditionText += "Defeat all enemy pieces";
                }
                victoryConditionText.text = conditionText;
            }

            if (statusText != null)
            {
                statusText.text = $"Turn: {_currentPlayer}\nMove: {_moveCount}\n\nEnter move (e.g., e2-e4):";
            }
        }
    }
}
