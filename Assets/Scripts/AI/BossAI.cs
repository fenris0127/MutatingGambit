using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// 향상된 능력과 특수 행동을 가진 보스 AI.
    /// 더 공격적이고, 더 깊은 탐색, 특수 능력을 가집니다.
    /// </summary>
    public class BossAI : ChessAI
    {
        [Header("Boss Settings")]
        [SerializeField]
        private string bossName = "The Chess Master";

        [SerializeField]
        [TextArea]
        private string bossDescription = "A legendary opponent with unparalleled strategic prowess.";

        [SerializeField]
        private bool canUseSpecialAbilities = true;

        [SerializeField]
        private int specialAbilityCharges = 3;

        private int remainingCharges;

        /// <summary>
        /// 보스 이름을 가져옵니다.
        /// </summary>
        public string BossName => bossName;

        /// <summary>
        /// 보스 설명을 가져옵니다.
        /// </summary>
        public string BossDescription => bossDescription;

        private void Awake()
        {   
            remainingCharges = specialAbilityCharges;
        }

        /// <summary>
        /// 보스가 특수 능력을 사용할 가능성이 있는 수를 둡니다.
        /// </summary>
        public new Move MakeMove(Board board)
        {
            // 특수 능력 사용 고려
            if (canUseSpecialAbilities && remainingCharges > 0 && ShouldUseSpecialAbility(board))
            {
                UseSpecialAbility(board);
                remainingCharges--;
            }

            // 향상된 평가로 기본 AI 로직 사용
            return base.MakeMove(board);
        }

        /// <summary>
        /// 보스가 특수 능력을 사용해야 하는지 결정합니다.
        /// </summary>
        private bool ShouldUseSpecialAbility(Board board)
        {
            // 위험할 때 또는 이점을 확보하기 위해 능력 사용
            float evaluation = new StateEvaluator(Config, AITeam, 0).EvaluateBoard(board);

            // 지고 있거나 킬을 확보할 때 사용
            return evaluation < -5f || (evaluation > 5f && Random.value > 0.7f);
        }

        /// <summary>
        /// 특수 보스 능력을 실행합니다.
        /// </summary>
        private void UseSpecialAbility(Board board)
        {
            int abilityChoice = Random.Range(0, 3);

            switch (abilityChoice)
            {
                case 0:
                    // 증원 소환
                    Debug.Log($"{bossName}가 증원을 소환합니다!");
                    SummonReinforcement(board);
                    break;

                case 1:
                    // 킹을 안전한 곳으로 텔레포트
                    Debug.Log($"{bossName}가 킹을 안전한 곳으로 텔레포트합니다!");
                    TeleportKingToSafety(board);
                    break;

                case 2:
                    // 임시 무적
                    Debug.Log($"{bossName}가 기물에 무적을 부여합니다!");
                    GrantInvulnerability(board);
                    break;
            }
        }

        /// <summary>
        /// 보드에 새로운 기물을 소환합니다.
        /// </summary>
        private void SummonReinforcement(Board board)
        {
            // 뒷줄에서 빈 칸 찾기
            int backRank = AITeam == Team.White ? 0 : board.Height - 1;

            for (int x = 0; x < board.Width; x++)
            {
                Vector2Int pos = new Vector2Int(x, backRank);

                if (board.GetPiece(pos) == null && !board.IsObstacle(pos))
                {
                    // 나이트 소환
                    GameObject pieceObject = new GameObject($"Boss_{AITeam}_Knight");
                    Piece piece = pieceObject.AddComponent<Piece>();
                    piece.Initialize(PieceType.Knight, AITeam, pos);

                    // 움직임 규칙 추가
                    var knightRule = ScriptableObject.CreateInstance<Core.MovementRules.KnightJumpRule>();
                    piece.AddMovementRule(knightRule);

                    board.PlacePiece(piece, pos);

                    Debug.Log($"보스가 {pos}에 나이트를 소환했습니다");
                    return;
                }
            }
        }

        /// <summary>
        /// 킹을 더 안전한 위치로 텔레포트합니다.
        /// </summary>
        private void TeleportKingToSafety(Board board)
        {
            var pieces = board.GetPiecesByTeam(AITeam);
            Piece king = null;

            foreach (var piece in pieces)
            {
                if (piece.Type == PieceType.King)
                {
                    king = piece;
                    break;
                }
            }

            if (king != null)
            {
                // 가장 안전한 칸 찾기
                Vector2Int safePos = FindSafestPosition(board, AITeam);

                if (safePos != king.Position)
                {
                    board.MovePiece(king.Position, safePos);
                    Debug.Log($"킹이 {safePos}로 텔레포트했습니다");
                }
            }
        }

        /// <summary>
        /// 가치 있는 기물에 임시 무적을 부여합니다.
        /// </summary>
        private void GrantInvulnerability(Board board)
        {
            // 전체 구현에서는 1턴 동안 기물을 무적으로 표시합니다
            Debug.Log("보스가 무적을 부여합니다!");
        }

        /// <summary>
        /// 팀을 위한 보드에서 가장 안전한 위치를 찾습니다.
        /// </summary>
        private Vector2Int FindSafestPosition(Board board, Team team)
        {
            Vector2Int safest = Vector2Int.zero;
            int minThreats = int.MaxValue;

            Team enemyTeam = team == Team.White ? Team.Black : Team.White;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (board.GetPiece(pos) == null && !board.IsObstacle(pos))
                    {
                        bool underAttack = MoveValidator.IsPositionUnderAttack(board, pos, enemyTeam);
                        int threats = underAttack ? 1 : 0;

                        if (threats < minThreats)
                        {
                            minThreats = threats;
                            safest = pos;
                        }
                    }
                }
            }

            return safest;
        }

        /// <summary>
        /// 남은 특수 능력 충전 횟수를 가져옵니다.
        /// </summary>
        public int RemainingCharges => remainingCharges;
    }
}
