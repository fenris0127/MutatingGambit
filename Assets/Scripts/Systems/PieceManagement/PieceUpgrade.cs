using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Mutations;

namespace MutatingGambit.Systems.PieceManagement
{
    /// <summary>
    /// Handles piece upgrades and enhancements.
    /// Can apply mutations or stat boosts to pieces.
    /// </summary>
    public class PieceUpgrade : MonoBehaviour
    {
        [Header("Upgrade State")]
        [SerializeField]
        private int upgradeLevel = 0;

        [SerializeField]
        private Piece piece;

        [SerializeField]
        private PieceHealth pieceHealth;

        /// <summary>
        /// Gets the current upgrade level.
        /// </summary>
        public int UpgradeLevel => upgradeLevel;

        /// <summary>
        /// Gets the piece component.
        /// </summary>
        public Piece Piece
        {
            get
            {
                if (piece == null)
                {
                    piece = GetComponent<Piece>();
                }
                return piece;
            }
        }

        private void Awake()
        {
            if (piece == null)
            {
                piece = GetComponent<Piece>();
            }

            if (pieceHealth == null)
            {
                pieceHealth = GetComponent<PieceHealth>();
            }
        }

        /// <summary>
        /// Applies a mutation to this piece as an upgrade.
        /// </summary>
        public bool ApplyMutationUpgrade(Mutation mutation)
        {
            if (mutation == null || Piece == null)
            {
                return false;
            }

            // Check if piece is broken
            if (pieceHealth != null && pieceHealth.IsBroken)
            {
                Debug.LogWarning("Cannot upgrade a broken piece!");
                return false;
            }

            // Apply mutation through MutationManager
            MutationManager.Instance.ApplyMutation(Piece, mutation);

            upgradeLevel++;

            Debug.Log($"{Piece.Team} {Piece.Type} upgraded with {mutation.MutationName} (Level {upgradeLevel})");

            return true;
        }

        /// <summary>
        /// Gets the upgrade value/power of this piece.
        /// </summary>
        public float GetUpgradeValue()
        {
            // Base value from upgrade level
            float value = upgradeLevel * 0.5f;

            // Add value from mutations
            if (Piece != null && MutationManager.Instance.HasMutations(Piece))
            {
                var mutations = MutationManager.Instance.GetMutations(Piece);
                value += mutations.Count * 0.3f;
            }

            return value;
        }

        /// <summary>
        /// Resets all upgrades.
        /// </summary>
        public void ResetUpgrades()
        {
            upgradeLevel = 0;

            if (Piece != null)
            {
                MutationManager.Instance.ClearMutations(Piece);
            }

            Debug.Log($"Reset upgrades for {Piece?.Team} {Piece?.Type}");
        }
    }
}
