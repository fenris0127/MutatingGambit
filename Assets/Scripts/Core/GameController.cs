using UnityEngine;
using TMPro;
using MutatingGambit.Core;
using MutatingGambit.Core.UI;
using MutatingGambit.Core.AI;
using MutatingGambit.Core.Movement;

namespace MutatingGambit
{
    // Main game controller for Unity integration
    public class GameController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI boardText;
        public TextMeshProUGUI gameStateText;
        public TMP_InputField inputField;

        [Header("Game Settings")]
        public int boardWidth = 8;
        public int boardHeight = 8;
        public Difficulty aiDifficulty = Difficulty.Normal;


        private Board _board;
        private BoardDisplay _boardDisplay;
        private GameStateDisplay _gameStateDisplay;
        private ChessAI _ai;
        private PieceColor _currentPlayer = PieceColor.White;
        private Position _selectedPosition;

 

        void Start()
        {
            InitializeGame();
            SetupUI();
            UpdateDisplay();
        }

        void SetupUI()
        {
            // Setup BoardText
            if (boardText != null)
            {
                boardText.fontSize = 24;
                boardText.alignment = TextAlignmentOptions.TopLeft;
                boardText.enableWordWrapping = false;
            }
 
            // Setup GameStateText
            if (gameStateText != null)
            {
                gameStateText.fontSize = 18;
                gameStateText.alignment = TextAlignmentOptions.TopLeft;
            }

            // Setup InputField
            if (inputField != null)
            {
                inputField.onSubmit.AddListener(OnInputSubmit);
                inputField.ActivateInputField();
            }
        }

        void InitializeGame()
        {
            // Create board
            _board = new Board(boardWidth, boardHeight);
            _board.SetupStandardBoard();

            // Create UI systems
            _boardDisplay = new BoardDisplay();
            _gameStateDisplay = new GameStateDisplay{
                CurrentPlayer = _currentPlayer,
                MoveCount = 0
            };

            // Create AI
            _ai = new ChessAI(PieceColor.Black, aiDifficulty);
            Debug.Log("Game initialized!");
        }

        void UpdateDisplay()
        {
            if (boardText != null)
                boardText.text = _boardDisplay.RenderBoard(_board);
 
            if (gameStateText != null)
                gameStateText.text = _gameStateDisplay.RenderFullGameState(_board);
        }

        public void OnInputSubmit(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            ProcessPlayerInput(input);

            // Clear and refocus
            inputField.text = "";
            inputField.ActivateInputField();
        }

        void ProcessPlayerInput(string input)
        {
            input = input.Trim().ToLower();

            if (input.Contains("-"))
            {
                // Move format: "e2-e4"
                string[] parts = input.Split('-');

                if (parts.Length == 2)
                    TryMove(parts[0], parts[1]);
            }
            else
            {
                // Selection format: "e2"
                if (_selectedPosition == null)
                {
                    // Select piece
                    TrySelectPiece(input);
                }
                else
                {
                    // Move to position
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

                if (piece == null || piece.Color != _currentPlayer)
                {
                    Debug.Log("Invalid move!");
                    _selectedPosition = null;
                    _boardDisplay.SelectedPosition = null;
                    UpdateDisplay();
                    return;
                }

                // Check if move is legal
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

                Debug.Log($"Moved {piece.Type} from {fromNotation} to {toNotation}");
 
                // Clear selection
                _selectedPosition = null;
                _boardDisplay.SelectedPosition = null;
                _boardDisplay.HighlightMoves = false;

                // Switch player
                _currentPlayer = _currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
                _gameStateDisplay.CurrentPlayer = _currentPlayer;
                _gameStateDisplay.MoveCount++;

                UpdateDisplay();
 
                // AI turn
                if (_currentPlayer == PieceColor.Black)
                    Invoke(nameof(ExecuteAIMove), 0.5f);
            }
            catch
            {
                Debug.Log("Invalid move format!");
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
                Debug.Log("AI has no legal moves!");
                return;
            }

            var piece = _board.GetPieceAt(aiMove.From);
            _board.PlacePiece(null, aiMove.From);
            _board.PlacePiece(piece, aiMove.To);
            piece.HasMoved = true;
 
            Debug.Log($"AI moved {piece.Type} from {aiMove.From.ToNotation()} to {aiMove.To.ToNotation()}");

            // Switch back to player
            _currentPlayer = PieceColor.White;
            _gameStateDisplay.CurrentPlayer = _currentPlayer;
            _gameStateDisplay.MoveCount++;
 
            UpdateDisplay();
        }
 
        public void ResetGame()
        {
            _currentPlayer = PieceColor.White;
            _selectedPosition = null;
            InitializeGame();
            UpdateDisplay();
        }
    }
}