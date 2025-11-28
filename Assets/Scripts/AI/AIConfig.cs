using UnityEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// Configuration for AI difficulty and behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "AIConfig", menuName = "Mutating Gambit/AI Config")]
    public class AIConfig : ScriptableObject
    {
        [Header("AI Info")]
        [SerializeField]
        private string configName = "Normal AI";

        [SerializeField]
        [TextArea(2, 3)]
        private string description = "Balanced AI opponent";

        [Header("Search Settings")]
        [SerializeField]
        [Tooltip("Maximum depth for minimax search.")]
        [Range(1, 6)]
        private int searchDepth = 3;

        [SerializeField]
        [Tooltip("Maximum time per move in milliseconds.")]
        private int maxTimePerMove = 500;

        [SerializeField]
        [Tooltip("If true, uses iterative deepening.")]
        private bool useIterativeDeepening = true;

        [Header("Evaluation Weights")]
        [SerializeField]
        [Tooltip("Weight for material advantage (piece values).")]
        [Range(0f, 10f)]
        private float materialWeight = 1.0f;

        [SerializeField]
        [Tooltip("Weight for positional advantage.")]
        [Range(0f, 5f)]
        private float positionalWeight = 0.3f;

        [SerializeField]
        [Tooltip("Weight for king safety.")]
        [Range(0f, 5f)]
        private float kingSafetyWeight = 0.5f;

        [SerializeField]
        [Tooltip("Weight for mobility (number of valid moves).")]
        [Range(0f, 5f)]
        private float mobilityWeight = 0.2f;

        [Header("Randomness")]
        [SerializeField]
        [Tooltip("Adds randomness to evaluation to prevent deterministic play.")]
        [Range(0f, 1f)]
        private float randomnessFactor = 0.1f;

        [Header("Piece Values")]
        [SerializeField]
        private float pawnValue = 1.0f;

        [SerializeField]
        private float knightValue = 3.0f;

        [SerializeField]
        private float bishopValue = 3.0f;

        [SerializeField]
        private float rookValue = 5.0f;

        [SerializeField]
        private float queenValue = 9.0f;

        [SerializeField]
        private float kingValue = 100.0f;

        /// <summary>
        /// Gets the configuration name.
        /// </summary>
        public string ConfigName => configName;

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the search depth.
        /// </summary>
        public int SearchDepth => searchDepth;

        /// <summary>
        /// Gets the maximum time per move in milliseconds.
        /// </summary>
        public int MaxTimePerMove => maxTimePerMove;

        /// <summary>
        /// Gets whether to use iterative deepening.
        /// </summary>
        public bool UseIterativeDeepening => useIterativeDeepening;

        /// <summary>
        /// Gets the material weight.
        /// </summary>
        public float MaterialWeight => materialWeight;

        /// <summary>
        /// Gets the positional weight.
        /// </summary>
        public float PositionalWeight => positionalWeight;

        /// <summary>
        /// Gets the king safety weight.
        /// </summary>
        public float KingSafetyWeight => kingSafetyWeight;

        /// <summary>
        /// Gets the mobility weight.
        /// </summary>
        public float MobilityWeight => mobilityWeight;

        /// <summary>
        /// Gets the randomness factor.
        /// </summary>
        public float RandomnessFactor => randomnessFactor;

        /// <summary>
        /// Gets the value of a piece type.
        /// </summary>
        public float GetPieceValue(Core.ChessEngine.PieceType type)
        {
            switch (type)
            {
                case Core.ChessEngine.PieceType.Pawn:
                    return pawnValue;
                case Core.ChessEngine.PieceType.Knight:
                    return knightValue;
                case Core.ChessEngine.PieceType.Bishop:
                    return bishopValue;
                case Core.ChessEngine.PieceType.Rook:
                    return rookValue;
                case Core.ChessEngine.PieceType.Queen:
                    return queenValue;
                case Core.ChessEngine.PieceType.King:
                    return kingValue;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"AIConfig: {configName} (Depth: {searchDepth})";
        }

#if UNITY_INCLUDE_TESTS
        /// <summary>
        /// Creates an AIConfig instance for testing with custom values.
        /// Avoids fragile reflection-based initialization.
        /// </summary>
        public static AIConfig CreateForTesting(
            int searchDepth = 3,
            int maxTimePerMove = 1000,
            bool useIterativeDeepening = false,
            float materialWeight = 1.0f,
            float positionalWeight = 0.3f,
            float kingSafetyWeight = 0.5f,
            float mobilityWeight = 0.2f,
            float randomnessFactor = 0.1f,
            float pawnValue = 1.0f,
            float knightValue = 3.0f,
            float bishopValue = 3.0f,
            float rookValue = 5.0f,
            float queenValue = 9.0f,
            float kingValue = 100.0f)
        {
            var config = CreateInstance<AIConfig>();
            config.searchDepth = searchDepth;
            config.maxTimePerMove = maxTimePerMove;
            config.useIterativeDeepening = useIterativeDeepening;
            config.materialWeight = materialWeight;
            config.positionalWeight = positionalWeight;
            config.kingSafetyWeight = kingSafetyWeight;
            config.mobilityWeight = mobilityWeight;
            config.randomnessFactor = randomnessFactor;
            config.pawnValue = pawnValue;
            config.knightValue = knightValue;
            config.bishopValue = bishopValue;
            config.rookValue = rookValue;
            config.queenValue = queenValue;
            config.kingValue = kingValue;
            return config;
        }
#endif
    }
}
