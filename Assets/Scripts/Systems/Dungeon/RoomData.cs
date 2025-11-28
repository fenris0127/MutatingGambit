using UnityEngine;
using MutatingGambit.Systems.BoardSystem;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// ScriptableObject that defines a dungeon room's configuration.
    /// Includes board layout, victory condition, enemy setup, and rewards.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomData", menuName = "Mutating Gambit/Room Data")]
    public class RoomData : ScriptableObject
    {
        [Header("Room Info")]
        [SerializeField]
        private string roomName;

        [SerializeField]
        [TextArea(2, 3)]
        private string roomDescription;

        [SerializeField]
        private RoomType roomType = RoomType.NormalCombat;

        [Header("Board Configuration")]
        [SerializeField]
        [Tooltip("The board layout for this room.")]
        private BoardData boardData;

        [Header("Victory Condition")]
        [SerializeField]
        [Tooltip("The victory condition for this room.")]
        private VictoryCondition victoryCondition;

        [Header("Enemy Setup")]
        [SerializeField]
        [Tooltip("Initial positions and types of enemy pieces.")]
        private PieceSetupData[] enemyPieces;

        [SerializeField]
        [Tooltip("Mutations applied to enemy pieces.")]
        private MutationSetupData[] enemyMutations;

        [Header("Player Setup")]
        [SerializeField]
        [Tooltip("Initial positions and types of player pieces (if custom).")]
        private PieceSetupData[] playerPieces;

        [SerializeField]
        [Tooltip("If true, uses standard chess starting position for player.")]
        private bool useStandardPlayerSetup = false;

        [Header("Rewards")]
        [SerializeField]
        [Tooltip("Possible artifact rewards (player chooses 1 of 3).")]
        private Artifact[] possibleArtifactRewards;

        [SerializeField]
        [Tooltip("Currency reward for completing this room.")]
        private int currencyReward = 10;

        /// <summary>
        /// Gets the name of this room.
        /// </summary>
        public string RoomName => roomName;

        /// <summary>
        /// Gets the description of this room.
        /// </summary>
        public string RoomDescription => roomDescription;

        /// <summary>
        /// Gets the type of this room.
        /// </summary>
        public RoomType Type => roomType;

        /// <summary>
        /// Gets the board data for this room.
        /// </summary>
        public BoardData BoardData => boardData;

        /// <summary>
        /// Gets the victory condition for this room.
        /// </summary>
        public VictoryCondition VictoryCondition => victoryCondition;

        /// <summary>
        /// Gets the enemy piece setup data.
        /// </summary>
        public PieceSetupData[] EnemyPieces => enemyPieces;

        /// <summary>
        /// Gets the enemy mutation setup data.
        /// </summary>
        public MutationSetupData[] EnemyMutations => enemyMutations;

        /// <summary>
        /// Gets the player piece setup data.
        /// </summary>
        public PieceSetupData[] PlayerPieces => playerPieces;

        /// <summary>
        /// Gets whether to use standard player setup.
        /// </summary>
        public bool UseStandardPlayerSetup => useStandardPlayerSetup;

        /// <summary>
        /// Gets possible artifact rewards.
        /// </summary>
        public Artifact[] PossibleArtifactRewards => possibleArtifactRewards;

        /// <summary>
        /// Gets the currency reward.
        /// </summary>
        public int CurrencyReward => currencyReward;

        /// <summary>
        /// Validates the room data.
        /// </summary>
        public bool Validate()
        {
            if (boardData == null)
            {
                Debug.LogError($"Room '{roomName}' has no board data assigned.");
                return false;
            }

            if (victoryCondition == null && roomType == RoomType.NormalCombat)
            {
                Debug.LogWarning($"Room '{roomName}' has no victory condition.");
            }

            return true;
        }
    }

    /// <summary>
    /// Data structure for setting up a piece at a specific position.
    /// </summary>
    [System.Serializable]
    public struct PieceSetupData
    {
        public PieceType pieceType;
        public Team team;
        public Vector2Int position;
    }

    /// <summary>
    /// Data structure for applying a mutation to a piece.
    /// </summary>
    [System.Serializable]
    public struct MutationSetupData
    {
        public Vector2Int piecePosition; // Position of the piece to mutate
        public Mutation mutation;        // The mutation to apply
    }
}
