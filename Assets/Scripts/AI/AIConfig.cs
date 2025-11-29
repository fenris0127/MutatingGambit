using UnityEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// AI 난이도와 행동에 대한 설정.
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
        [Tooltip("미니맥스 탐색의 최대 깊이.")]
        [Range(1, 6)]
        private int searchDepth = 3;

        [SerializeField]
        [Tooltip("수당 최대 시간 (밀리초).")]
        private int maxTimePerMove = 500;

        [SerializeField]
        [Tooltip("true인 경우 반복 심화를 사용합니다.")]
        private bool useIterativeDeepening = true;

        [Header("Evaluation Weights")]
        [SerializeField]
        [Tooltip("물질적 이점 (기물 가치)에 대한 가중치.")]
        [Range(0f, 10f)]
        private float materialWeight = 1.0f;

        [SerializeField]
        [Tooltip("위치적 이점에 대한 가중치.")]
        [Range(0f, 5f)]
        private float positionalWeight = 0.3f;

        [SerializeField]
        [Tooltip("킹 안전에 대한 가중치.")]
        [Range(0f, 5f)]
        private float kingSafetyWeight = 0.5f;

        [SerializeField]
        [Tooltip("기동성 (유효한 수의 수)에 대한 가중치.")]
        [Range(0f, 5f)]
        private float mobilityWeight = 0.2f;

        [Header("Randomness")]
        [SerializeField]
        [Tooltip("결정론적 플레이를 방지하기 위해 평가에 무작위성을 추가합니다.")]
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
        /// 설정 이름.
        /// </summary>
        public string ConfigName => configName;

        /// <summary>
        /// 설명.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 탐색 깊이.
        /// </summary>
        public int SearchDepth => searchDepth;

        /// <summary>
        /// 수당 최대 시간 (밀리초).
        /// </summary>
        public int MaxTimePerMove => maxTimePerMove;

        /// <summary>
        /// 반복 심화를 사용하는지 여부를 가져옵니다.
        /// </summary>
        public bool UseIterativeDeepening => useIterativeDeepening;

        /// <summary>
        /// 물질 가중치.
        /// </summary>
        public float MaterialWeight => materialWeight;

        /// <summary>
        /// 위치 가중치.
        /// </summary>
        public float PositionalWeight => positionalWeight;

        /// <summary>
        /// 킹 안전 가중치.
        /// </summary>
        public float KingSafetyWeight => kingSafetyWeight;

        /// <summary>
        /// 기동성 가중치.
        /// </summary>
        public float MobilityWeight => mobilityWeight;

        /// <summary>
        /// 무작위성 인자.
        /// </summary>
        public float RandomnessFactor => randomnessFactor;

        /// <summary>
        /// 기물 타입의 가치를 가져옵니다.
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
        /// 디버깅을 위한 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return $"AIConfig: {configName} (Depth: {searchDepth})";
        }

#if UNITY_INCLUDE_TESTS
        /// <summary>
        /// 사용자 정의 값으로 테스트용 AIConfig 인스턴스를 생성합니다.
        /// 취약한 리플렉션 기반 초기화를 방지합니다.
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
