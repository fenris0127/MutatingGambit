using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using MutatingGambit.Core;

namespace MutatingGambit
{
    /// <summary>
    /// Displays rest screen where pieces can be repaired
    /// </summary>
    public class RestController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI pieceListText;
        public Button healAllButton;
        public Button continueButton;

        private Board _playerBoard;

        public event Action OnRestComplete;

        public void Initialize(Board playerBoard)
        {
            _playerBoard = playerBoard;
            DisplayRestScreen();
        }

        void DisplayRestScreen()
        {
            if (titleText != null)
            {
                titleText.text = "=== REST AREA ===\n❤️ Repair Your Pieces ❤️";
            }

            // Find all pieces and their HP
            List<Piece> allPieces = GetAllPlayerPieces();
            List<Piece> damagedPieces = new List<Piece>();

            string pieceInfo = "\nYour Pieces:\n\n";

            foreach (var piece in allPieces)
            {
                string hpBar = GenerateHPBar(piece);
                pieceInfo += $"{piece.Color} {piece.Type}: {hpBar} ({piece.HP}/{piece.MaxHP} HP)\n";

                if (piece.HP < piece.MaxHP)
                {
                    damagedPieces.Add(piece);
                }
            }

            if (damagedPieces.Count > 0)
            {
                pieceInfo += $"\n💔 {damagedPieces.Count} piece(s) need repair";
            }
            else
            {
                pieceInfo += "\n✨ All pieces are at full health!";
            }

            if (pieceListText != null)
            {
                pieceListText.text = pieceInfo;
            }

            // Setup heal button
            if (healAllButton != null)
            {
                healAllButton.onClick.RemoveAllListeners();
                healAllButton.onClick.AddListener(HealAllPieces);
                healAllButton.interactable = damagedPieces.Count > 0;

                var btnText = healAllButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                {
                    btnText.text = damagedPieces.Count > 0 ? "Heal All Pieces" : "Already Healed";
                }
            }

            // Setup continue button
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        List<Piece> GetAllPlayerPieces()
        {
            List<Piece> pieces = new List<Piece>();

            for (int x = 0; x < _playerBoard.Width; x++)
            {
                for (int y = 0; y < _playerBoard.Height; y++)
                {
                    var pos = new Position(x, y);
                    var piece = _playerBoard.GetPieceAt(pos);

                    if (piece != null && piece.Color == PieceColor.White)
                    {
                        pieces.Add(piece);
                    }
                }
            }

            return pieces;
        }

        string GenerateHPBar(Piece piece)
        {
            int maxBars = 10;
            int filledBars = Mathf.RoundToInt((float)piece.HP / piece.MaxHP * maxBars);

            string bar = "[";
            for (int i = 0; i < maxBars; i++)
            {
                bar += i < filledBars ? "█" : "░";
            }
            bar += "]";

            return bar;
        }

        void HealAllPieces()
        {
            List<Piece> pieces = GetAllPlayerPieces();
            int healedCount = 0;

            foreach (var piece in pieces)
            {
                if (piece.HP < piece.MaxHP)
                {
                    piece.Heal(piece.MaxHP);
                    healedCount++;
                }
            }

            Debug.Log($"Healed {healedCount} pieces!");

            // Refresh display
            DisplayRestScreen();
        }

        void OnContinueClicked()
        {
            Debug.Log("Leaving rest area...");
            OnRestComplete?.Invoke();
        }
    }
}
