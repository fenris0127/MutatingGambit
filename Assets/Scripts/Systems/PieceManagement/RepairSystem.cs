using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.PieceManagement
{
    /// <summary>
    /// Manages the repair system for broken pieces.
    /// Tracks broken pieces and handles repair logic.
    /// </summary>
    public class RepairSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        [Tooltip("Maximum number of pieces that can be repaired per rest room visit.")]
        private int maxRepairsPerRest = 2;

        [SerializeField]
        [Tooltip("Whether repairing pieces costs resources.")]
        private bool usesRepairCost = false;

        [Header("State")]
        [SerializeField]
        private List<PieceHealth> brokenPieces = new List<PieceHealth>();

        [SerializeField]
        private List<PieceHealth> activePieces = new List<PieceHealth>();

        [SerializeField]
        private int repairsUsedThisRest = 0;

        [Header("Events")]
        public UnityEvent<PieceHealth> OnPieceBroken;
        public UnityEvent<PieceHealth> OnPieceRepaired;
        public UnityEvent<int> OnRepairsAvailableChanged;

        /// <summary>
        /// Gets the list of broken pieces.
        /// </summary>
        public List<PieceHealth> BrokenPieces => new List<PieceHealth>(brokenPieces);

        /// <summary>
        /// Gets the list of active pieces.
        /// </summary>
        public List<PieceHealth> ActivePieces => new List<PieceHealth>(activePieces);

        /// <summary>
        /// Gets the number of broken pieces.
        /// </summary>
        public int BrokenPieceCount => brokenPieces.Count;

        /// <summary>
        /// Gets the remaining repairs available this rest.
        /// </summary>
        public int RepairsRemaining => Mathf.Max(0, maxRepairsPerRest - repairsUsedThisRest);

        /// <summary>
        /// Gets whether any repairs are available.
        /// </summary>
        public bool CanRepair => RepairsRemaining > 0;

        /// <summary>
        /// Registers a piece with the repair system.
        /// </summary>
        public void RegisterPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null)
            {
                return;
            }

            if (pieceHealth.IsBroken)
            {
                if (!brokenPieces.Contains(pieceHealth))
                {
                    brokenPieces.Add(pieceHealth);
                }
            }
            else if (pieceHealth.IsActive)
            {
                if (!activePieces.Contains(pieceHealth))
                {
                    activePieces.Add(pieceHealth);
                }
            }

            // Subscribe to state changes
            pieceHealth.OnPieceBroken.AddListener(() => HandlePieceBroken(pieceHealth));
            pieceHealth.OnPieceRepaired.AddListener(() => HandlePieceRepaired(pieceHealth));
        }

        /// <summary>
        /// Unregisters a piece from the repair system.
        /// </summary>
        public void UnregisterPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null)
            {
                return;
            }

            brokenPieces.Remove(pieceHealth);
            activePieces.Remove(pieceHealth);
        }

        /// <summary>
        /// Marks a piece as broken and adds it to the broken list.
        /// </summary>
        public void BreakPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null || pieceHealth.IsBroken)
            {
                return;
            }

            pieceHealth.BreakPiece();
            // Event handler will move it to broken list
        }

        /// <summary>
        /// Attempts to repair a piece.
        /// </summary>
        public bool RepairPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null || !pieceHealth.CanBeRepaired)
            {
                Debug.LogWarning("Cannot repair this piece!");
                return false;
            }

            if (RepairsRemaining <= 0)
            {
                Debug.LogWarning("No repairs remaining for this rest!");
                return false;
            }

            // Check repair cost (if using currency system)
            if (usesRepairCost && pieceHealth.RepairCost > 0)
            {
                var dungeonManager = MutatingGambit.Systems.Dungeon.DungeonManager.Instance;
                if (dungeonManager != null && dungeonManager.PlayerState != null)
                {
                    if (dungeonManager.PlayerState.Currency >= pieceHealth.RepairCost)
                    {
                        dungeonManager.PlayerState.Currency -= pieceHealth.RepairCost;
                        Debug.Log($"Paid {pieceHealth.RepairCost} for repair. Remaining: {dungeonManager.PlayerState.Currency}");
                    }
                    else
                    {
                        Debug.LogWarning("Not enough currency to repair!");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning("Cannot check currency - DungeonManager or PlayerState not found.");
                }
            }

            bool repaired = pieceHealth.RepairPiece();

            if (repaired)
            {
                repairsUsedThisRest++;
                OnRepairsAvailableChanged?.Invoke(RepairsRemaining);
                // Event handler will move it back to active list
            }

            return repaired;
        }

        /// <summary>
        /// Called when entering a rest room - resets repair count.
        /// </summary>
        public void EnterRestRoom()
        {
            repairsUsedThisRest = 0;
            OnRepairsAvailableChanged?.Invoke(RepairsRemaining);

            Debug.Log($"Entered rest room. {maxRepairsPerRest} repairs available.");
        }

        /// <summary>
        /// Called when exiting a rest room.
        /// </summary>
        public void ExitRestRoom()
        {
            Debug.Log($"Exited rest room. Used {repairsUsedThisRest}/{maxRepairsPerRest} repairs.");
        }

        /// <summary>
        /// Handles a piece breaking.
        /// </summary>
        private void HandlePieceBroken(PieceHealth pieceHealth)
        {
            if (activePieces.Contains(pieceHealth))
            {
                activePieces.Remove(pieceHealth);
            }

            if (!brokenPieces.Contains(pieceHealth))
            {
                brokenPieces.Add(pieceHealth);
            }

            OnPieceBroken?.Invoke(pieceHealth);

            Debug.Log($"Piece broken. Broken count: {brokenPieces.Count}");
        }

        /// <summary>
        /// Handles a piece being repaired.
        /// </summary>
        private void HandlePieceRepaired(PieceHealth pieceHealth)
        {
            if (brokenPieces.Contains(pieceHealth))
            {
                brokenPieces.Remove(pieceHealth);
            }

            if (!activePieces.Contains(pieceHealth))
            {
                activePieces.Add(pieceHealth);
            }

            OnPieceRepaired?.Invoke(pieceHealth);

            Debug.Log($"Piece repaired. Broken count: {brokenPieces.Count}");
        }

        /// <summary>
        /// Checks if the king is broken (game over condition).
        /// </summary>
        public bool IsKingBroken(Team team)
        {
            foreach (var brokenPiece in brokenPieces)
            {
                if (brokenPiece.Piece != null &&
                    brokenPiece.Piece.Type == PieceType.King &&
                    brokenPiece.Piece.Team == team)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all broken pieces for a specific team.
        /// </summary>
        public List<PieceHealth> GetBrokenPiecesByTeam(Team team)
        {
            var result = new List<PieceHealth>();

            foreach (var piece in brokenPieces)
            {
                if (piece.Piece != null && piece.Piece.Team == team)
                {
                    result.Add(piece);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets save data for all broken pieces.
        /// </summary>
        public List<PieceHealthData> GetSaveData()
        {
            var data = new List<PieceHealthData>();

            foreach (var piece in brokenPieces)
            {
                data.Add(piece.GetSaveData());
            }

            foreach (var piece in activePieces)
            {
                if (!piece.IsActive) continue;
                data.Add(piece.GetSaveData());
            }

            return data;
        }

        /// <summary>
        /// Clears all tracked pieces.
        /// </summary>
        public void Clear()
        {
            brokenPieces.Clear();
            activePieces.Clear();
            repairsUsedThisRest = 0;
        }

        /// <summary>
        /// Gets statistics about piece health.
        /// </summary>
        public string GetStats()
        {
            return $"Active: {activePieces.Count}, Broken: {brokenPieces.Count}, Repairs Left: {RepairsRemaining}";
        }
    }
}
