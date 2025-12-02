using UnityEngine;
using System.Collections.Generic;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Mutation rarity levels affecting drop rates and power.
    /// </summary>
    public enum MutationRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Tags for categorizing mutations and detecting synergies.
    /// </summary>
    public enum MutationTag
    {
        Movement,
        Attack,
        Defense,
        Utility,
        Teleport,
        Area,
        Sacrifice,
        Summon,
        Transform,
        Chaos
    }

    /// <summary>
    /// Abstract base class for piece mutations.
    /// Mutations modify how pieces move and behave by adding/removing/modifying movement rules.
    /// </summary>
    public abstract class Mutation : ScriptableObject
    {
        [Header("Mutation Info")]
        [SerializeField]
        private string mutationName;

        [SerializeField]
        [TextArea(2, 4)]
        private string description;

        [SerializeField]
        private Sprite icon;

        [Header("Properties")]
        [SerializeField]
        private MutationRarity rarity = MutationRarity.Common;

        [SerializeField]
        private MutationTag[] tags = new MutationTag[0];

        [SerializeField]
        [Tooltip("Cost to purchase this mutation (for shop system).")]
        private int cost = 50;

        [SerializeField]
        [Tooltip("Which piece types can receive this mutation.")]
        private PieceType[] compatiblePieceTypes;

        [SerializeField]
        [Tooltip("Can this mutation stack multiple times on the same piece?")]
        private bool canStack = false;

        [SerializeField]
        [Tooltip("Maximum number of stacks allowed.")]
        private int maxStacks = 1;

        [SerializeField]
        [Tooltip("Mutations that synergize with this one.")]
        private Mutation[] synergyMutations;

        /// <summary>
        /// Gets the name of this mutation.
        /// </summary>
        public string MutationName => mutationName;

        /// <summary>
        /// Gets the description of this mutation.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the icon for this mutation.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Gets the cost to acquire this mutation.
        /// </summary>
        public int Cost => cost;

        /// <summary>
        /// Gets the rarity of this mutation.
        /// </summary>
        public MutationRarity Rarity => rarity;

        /// <summary>
        /// Gets the tags of this mutation.
        /// </summary>
        public MutationTag[] Tags => tags;

        /// <summary>
        /// Can this mutation stack?
        /// </summary>
        public bool CanStack => canStack;

        /// <summary>
        /// Maximum stack count.
        /// </summary>
        public int MaxStacks => maxStacks;

        /// <summary>
        /// Gets mutations that synergize with this one.
        /// </summary>
        public Mutation[] SynergyMutations => synergyMutations;

        /// <summary>
        /// Checks if this mutation is compatible with the given piece type.
        /// </summary>
        public bool IsCompatibleWith(PieceType pieceType)
        {
            if (compatiblePieceTypes == null || compatiblePieceTypes.Length == 0)
            {
                return true; // No restrictions
            }

            foreach (var type in compatiblePieceTypes)
            {
                if (type == pieceType)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Applies this mutation to a piece.
        /// This method should modify the piece's movement rules.
        /// </summary>
        /// <param name="piece">The piece to mutate</param>
        public abstract void ApplyToPiece(Piece piece);

        /// <summary>
        /// Removes this mutation from a piece.
        /// Should undo the changes made by ApplyToPiece.
        /// </summary>
        /// <param name="piece">The piece to remove mutation from</param>
        public abstract void RemoveFromPiece(Piece piece);

        /// <summary>
        /// Called when the mutated piece captures an enemy piece.
        /// Override this for mutations that trigger on capture (e.g., SplittingKnight).
        /// </summary>
        public virtual void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Core.ChessEngine.Board board)
        {
            // Default: do nothing
        }

        /// <summary>
        /// Called when the mutated piece moves.
        /// Override this for mutations that trigger on movement.
        /// </summary>
        public virtual void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Core.ChessEngine.Board board)
        {
            // Default: do nothing
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"Mutation: {mutationName}";
        }

        #region MovementRule Tracking Helpers

        /// <summary>
        /// MovementRule을 추가하고 MutationState에서 추적합니다
        /// 제거 시 자동으로 정리하기 위해 반드시 이 메서드를 사용하세요
        /// </summary>
        /// <param name="piece">Rule을 추가할 Piece</param>
        /// <param name="rule">추가할 MovementRule</param>
        protected void AddAndTrackRule(Piece piece, MovementRule rule)
        {
            if (piece == null || rule == null)
            {
                Debug.LogWarning($"[{mutationName}] Cannot add null rule or to null piece");
                return;
            }

            var state = MutationManager.Instance.GetMutationState(piece, this);
            if (state == null)
            {
                Debug.LogError($"[{mutationName}] MutationState not found for piece {piece.name}");
                return;
            }

            piece.AddMovementRule(rule);
            state.TrackRule(rule);
        }

        /// <summary>
        /// MutationState에 추적된 모든 MovementRule을 Piece에서 제거합니다
        /// RemoveFromPiece에서 호출하세요
        /// </summary>
        /// <param name="piece">Rule을 제거할 Piece</param>
        protected void RemoveTrackedRules(Piece piece)
        {
            if (piece == null) return;

            var state = MutationManager.Instance.GetMutationState(piece, this);
            if (state == null) return;

            foreach (var rule in state.AddedRules)
            {
                piece.RemoveMovementRule(rule);
            }

            state.ClearTrackedRules();
        }

        #endregion
    }
}
