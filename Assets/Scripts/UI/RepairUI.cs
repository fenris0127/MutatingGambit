using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.UI
{
    /// <summary>
    /// UI for the repair system in rest rooms.
    /// Shows broken pieces and allows player to select which to repair.
    /// </summary>
    public class RepairUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject repairPanel;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI repairsRemainingText;

        [SerializeField]
        private Transform brokenPieceContainer;

        [SerializeField]
        private GameObject brokenPieceSlotPrefab;

        [SerializeField]
        private Button confirmButton;

        [Header("Settings")]
        [SerializeField]
        private string titleFormat = "Rest Room - Repair Your Pieces";

        [SerializeField]
        private string repairsRemainingFormat = "Repairs Remaining: {0}/{1}";

        [Header("Events")]
        public UnityEvent OnRepairCompleted;
        public UnityEvent OnRepairCancelled;

        private RepairSystem repairSystem;
        private List<BrokenPieceSlot> pieceSlots = new List<BrokenPieceSlot>();

        private void Awake()
        {
            if (repairPanel != null)
            {
                repairPanel.SetActive(false);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
        }

        /// <summary>
        /// Shows the repair UI with the given repair system.
        /// </summary>
        public void Show(RepairSystem system)
        {
            repairSystem = system;

            if (repairPanel != null)
            {
                repairPanel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = titleFormat;
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Hides the repair UI.
        /// </summary>
        public void Hide()
        {
            if (repairPanel != null)
            {
                repairPanel.SetActive(false);
            }

            ClearSlots();
        }

        /// <summary>
        /// Refreshes the UI display.
        /// </summary>
        private void RefreshDisplay()
        {
            if (repairSystem == null)
            {
                return;
            }

            UpdateRepairsRemainingText();
            DisplayBrokenPieces();
        }

        /// <summary>
        /// Updates the repairs remaining text.
        /// </summary>
        private void UpdateRepairsRemainingText()
        {
            if (repairsRemainingText != null && repairSystem != null)
            {
                int remaining = repairSystem.RepairsRemaining;
                int max = repairSystem.RepairsRemaining + (repairSystem.BrokenPieceCount > 0 ? 1 : 0);
                // Approximation - would need maxRepairsPerRest exposed

                repairsRemainingText.text = string.Format(repairsRemainingFormat, remaining, "2");
            }
        }

        /// <summary>
        /// Displays all broken pieces in the UI.
        /// </summary>
        private void DisplayBrokenPieces()
        {
            ClearSlots();

            if (repairSystem == null || brokenPieceContainer == null)
            {
                return;
            }

            var brokenPieces = repairSystem.BrokenPieces;

            foreach (var pieceHealth in brokenPieces)
            {
                CreateBrokenPieceSlot(pieceHealth);
            }

            // Show message if no broken pieces
            if (brokenPieces.Count == 0)
            {
                CreateNoBrokenPiecesMessage();
            }
        }

        /// <summary>
        /// Creates a UI slot for a broken piece.
        /// </summary>
        private void CreateBrokenPieceSlot(PieceHealth pieceHealth)
        {
            GameObject slotObject;

            if (brokenPieceSlotPrefab != null)
            {
                slotObject = Instantiate(brokenPieceSlotPrefab, brokenPieceContainer);
            }
            else
            {
                slotObject = new GameObject($"BrokenPiece_{pieceHealth.Piece?.Type}");
                slotObject.transform.SetParent(brokenPieceContainer);
            }

            BrokenPieceSlot slot = slotObject.GetComponent<BrokenPieceSlot>();
            if (slot == null)
            {
                slot = slotObject.AddComponent<BrokenPieceSlot>();
            }

            slot.Initialize(pieceHealth, repairSystem.CanRepair);
            slot.OnRepairRequested += () => HandleRepairRequest(pieceHealth);

            pieceSlots.Add(slot);
        }

        /// <summary>
        /// Creates a message when there are no broken pieces.
        /// </summary>
        private void CreateNoBrokenPiecesMessage()
        {
            GameObject messageObject = new GameObject("NoBrokenPiecesMessage");
            messageObject.transform.SetParent(brokenPieceContainer);

            TextMeshProUGUI text = messageObject.AddComponent<TextMeshProUGUI>();
            text.text = "All your pieces are in perfect condition!";
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.green;
        }

        /// <summary>
        /// Handles a repair request for a piece.
        /// </summary>
        private void HandleRepairRequest(PieceHealth pieceHealth)
        {
            if (repairSystem == null)
            {
                return;
            }

            bool success = repairSystem.RepairPiece(pieceHealth);

            if (success)
            {
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Called when confirm button is clicked.
        /// </summary>
        private void OnConfirmClicked()
        {
            OnRepairCompleted?.Invoke();
            Hide();
        }

        /// <summary>
        /// Clears all piece slots.
        /// </summary>
        private void ClearSlots()
        {
            foreach (var slot in pieceSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }

            pieceSlots.Clear();

            // Clear container
            if (brokenPieceContainer != null)
            {
                foreach (Transform child in brokenPieceContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Checks if the UI is currently visible.
        /// </summary>
        public bool IsVisible => repairPanel != null && repairPanel.activeSelf;
    }

    /// <summary>
    /// UI component for a single broken piece slot.
    /// </summary>
    public class BrokenPieceSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI costText;

        [SerializeField]
        private Button repairButton;

        [SerializeField]
        private GameObject repairedIndicator;

        private PieceHealth pieceHealth;
        private bool canRepair;

        public System.Action OnRepairRequested;

        private void Awake()
        {
            if (repairButton == null)
            {
                repairButton = GetComponentInChildren<Button>();
            }

            if (repairButton != null)
            {
                repairButton.onClick.AddListener(HandleRepairClick);
            }
        }

        /// <summary>
        /// Initializes the slot with piece data.
        /// </summary>
        public void Initialize(PieceHealth piece, bool repairsAvailable)
        {
            pieceHealth = piece;
            canRepair = repairsAvailable && piece.CanBeRepaired;

            UpdateDisplay();
        }

        /// <summary>
        /// Updates the visual display.
        /// </summary>
        private void UpdateDisplay()
        {
            if (pieceHealth == null || pieceHealth.Piece == null)
            {
                return;
            }

            // Set name
            if (nameText != null)
            {
                string teamColor = pieceHealth.Piece.Team == Team.White ? "White" : "Black";
                nameText.text = $"{teamColor} {pieceHealth.Piece.Type}";
            }

            // Set cost
            if (costText != null)
            {
                if (pieceHealth.RepairCost > 0)
                {
                    costText.text = $"Cost: {pieceHealth.RepairCost}";
                }
                else
                {
                    costText.text = "Free";
                }
            }

            // Set button state
            if (repairButton != null)
            {
                repairButton.interactable = canRepair;
            }

            // Hide repaired indicator
            if (repairedIndicator != null)
            {
                repairedIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Handles the repair button click.
        /// </summary>
        private void HandleRepairClick()
        {
            OnRepairRequested?.Invoke();

            // Show repaired indicator
            if (repairedIndicator != null)
            {
                repairedIndicator.SetActive(true);
            }

            // Disable button
            if (repairButton != null)
            {
                repairButton.interactable = false;
            }
        }
    }
}
