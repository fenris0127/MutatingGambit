using UnityEngine;
using System.Collections.Generic;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Tags for categorizing artifacts and detecting synergies.
    /// </summary>
    public enum ArtifactTag
    {
        Global,      // Affects entire board
        Movement,    // Modifies movement
        Combat,      // Battle effects
        Economic,    // Resource/currency
        Defensive,   // Protection
        Aggressive,  // Offensive power
        Chaos,       // Random/unpredictable
        Temporal     // Time/turn effects
    }

    /// <summary>
    /// Defines when an artifact's effect should trigger.
    /// </summary>
    public enum ArtifactTrigger
    {
        OnTurnStart,      // Beginning of player's turn
        OnTurnEnd,        // End of player's turn
        OnPieceMove,      // After any piece moves
        OnPieceCapture,   // When a piece is captured
        OnKingMove,       // Specifically when king moves
        Passive           // Always active (modifies rules)
    }

    /// <summary>
    /// Defines the rarity tier of an artifact.
    /// </summary>
    public enum ArtifactRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Context data passed to artifacts when they trigger.
    /// </summary>
    public class ArtifactContext
    {
        public Piece MovedPiece { get; set; }
        public Vector2Int FromPosition { get; set; }
        public Vector2Int ToPosition { get; set; }
        public Piece CapturedPiece { get; set; }
        public Team CurrentTeam { get; set; }
        public int TurnNumber { get; set; }
        public object RepairSystem { get; set; } // Optional reference to RepairSystem

        public ArtifactContext() { }

        public ArtifactContext(Piece movedPiece, Vector2Int from, Vector2Int to)
        {
            MovedPiece = movedPiece;
            FromPosition = from;
            ToPosition = to;
        }
    }

    /// <summary>
    /// Abstract base class for global game-modifying artifacts.
    /// Artifacts affect all pieces and modify game rules globally.
    /// </summary>
    public abstract class Artifact : ScriptableObject
    {
        [Header("Artifact Info")]
        [SerializeField]
        private string artifactName;

        [SerializeField]
        [TextArea(2, 4)]
        private string description;

        [SerializeField]
        private Sprite icon;

        [Header("Properties")]
        [SerializeField]
        [Tooltip("Cost to purchase this artifact.")]
        private int cost = 100;

        [SerializeField]
        [Tooltip("When this artifact's effect triggers.")]
        private ArtifactTrigger trigger = ArtifactTrigger.Passive;

        [SerializeField]
        [Tooltip("Rarity tier (higher = more powerful/expensive).")]
        private ArtifactRarity rarity = ArtifactRarity.Common;

        [SerializeField]
        [Tooltip("Tags for this artifact.")]
        private ArtifactTag[] tags = new ArtifactTag[0];

        [SerializeField]
        [Tooltip("Can this artifact be stacked with itself?")]
        private bool canStack = false;

        [SerializeField]
        [Tooltip("Artifacts that synergize with this one.")]
        private Artifact[] synergyArtifacts;

        /// <summary>
        /// Gets the name of this artifact.
        /// </summary>
        public string ArtifactName => artifactName;

        /// <summary>
        /// Gets the description of this artifact.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the icon for this artifact.
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// Gets the cost to acquire this artifact.
        /// </summary>
        public int Cost => cost;

        /// <summary>
        /// Gets when this artifact triggers.
        /// </summary>
        public ArtifactTrigger Trigger => trigger;

        /// <summary>
        /// Gets the rarity tier of this artifact.
        /// </summary>
        public ArtifactRarity Rarity => rarity;

        /// <summary>
        /// Gets the tags of this artifact.
        /// </summary>
        public ArtifactTag[] Tags => tags;

        /// <summary>
        /// Can this artifact stack with itself?
        /// </summary>
        public bool CanStack => canStack;

        /// <summary>
        /// Gets artifacts that synergize with this one.
        /// </summary>
        public Artifact[] SynergyArtifacts => synergyArtifacts;

        /// <summary>
        /// Called when the artifact is first acquired.
        /// Use this for one-time setup effects.
        /// </summary>
        public virtual void OnAcquired(Board board)
        {
            // Default: do nothing
        }

        /// <summary>
        /// Called when the artifact is removed.
        /// Use this to clean up any persistent effects.
        /// </summary>
        public virtual void OnRemoved(Board board)
        {
            // Default: do nothing
        }

        /// <summary>
        /// Applies the artifact's effect based on the current context.
        /// This is called by the ArtifactManager when the trigger condition is met.
        /// </summary>
        /// <param name="board">The current game board</param>
        /// <param name="context">Context information about the triggering event</param>
        public abstract void ApplyEffect(Board board, ArtifactContext context);

        /// <summary>
        /// Checks if this artifact can stack with another artifact.
        /// By default, artifacts of the same type don't stack.
        /// </summary>
        public virtual bool CanStackWith(Artifact other)
        {
            return other.GetType() != this.GetType();
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"Artifact: {artifactName} (Trigger: {trigger})";
        }
    }
}
