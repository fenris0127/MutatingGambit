using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.AI
{
    /// <summary>
    /// Boss AI with enhanced capabilities and special behaviors.
    /// More aggressive, deeper search, and special abilities.
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
        /// Gets the boss name.
        /// </summary>
        public string BossName => bossName;

        /// <summary>
        /// Gets the boss description.
        /// </summary>
        public string BossDescription => bossDescription;

        private 
        void Awake()
        {   
            remainingCharges = specialAbilityCharges;
        }

        /// <summary>
        /// Boss makes a move with potential special abilities.
        /// </summary>
        public new Move MakeMove(Board board)
        {
            // Consider using special ability
            if (canUseSpecialAbilities && remainingCharges > 0 && ShouldUseSpecialAbility(board))
            {
                UseSpecialAbility(board);
                remainingCharges--;
            }

            // Use base AI logic with enhanced evaluation
            return base.MakeMove(board);
        }

        /// <summary>
        /// Determines if boss should use a special ability.
        /// </summary>
        private bool ShouldUseSpecialAbility(Board board)
        {
            // Use ability when in danger or to secure advantage
            float evaluation = new StateEvaluator(Config, AITeam, 0).EvaluateBoard(board);

            // Use if losing or to secure kill
            return evaluation < -5f || (evaluation > 5f && Random.value > 0.7f);
        }

        /// <summary>
        /// Executes a special boss ability.
        /// </summary>
        private void UseSpecialAbility(Board board)
        {
            int abilityChoice = Random.Range(0, 3);

            switch (abilityChoice)
            {
                case 0:
                    // Summon reinforcement
                    Debug.Log($"{bossName} summons a reinforcement!");
                    SummonReinforcement(board);
                    break;

                case 1:
                    // Teleport king to safety
                    Debug.Log($"{bossName} teleports the king to safety!");
                    TeleportKingToSafety(board);
                    break;

                case 2:
                    // Temporary invulnerability
                    Debug.Log($"{bossName} grants invulnerability to a piece!");
                    GrantInvulnerability(board);
                    break;
            }
        }

        /// <summary>
        /// Summons a new piece on the board.
        /// </summary>
        private void SummonReinforcement(Board board)
        {
            // Find empty square in back rank
            int backRank = AITeam == Team.White ? 0 : board.Height - 1;

            for (int x = 0; x < board.Width; x++)
            {
                Vector2Int pos = new Vector2Int(x, backRank);

                if (board.GetPiece(pos) == null && !board.IsObstacle(pos))
                {
                    // Spawn a knight
                    GameObject pieceObject = new GameObject($"Boss_{AITeam}_Knight");
                    Piece piece = pieceObject.AddComponent<Piece>();
                    piece.Initialize(PieceType.Knight, AITeam, pos);

                    // Add movement rule
                    var knightRule = ScriptableObject.CreateInstance<Core.MovementRules.KnightJumpRule>();
                    piece.AddMovementRule(knightRule);

                    board.PlacePiece(piece, pos);

                    Debug.Log($"Boss summoned knight at {pos}");
                    return;
                }
            }
        }

        /// <summary>
        /// Teleports king to a safer position.
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
                // Find safest square
                Vector2Int safePos = FindSafestPosition(board, AITeam);

                if (safePos != king.Position)
                {
                    board.MovePiece(king.Position, safePos);
                    Debug.Log($"King teleported to {safePos}");
                }
            }
        }

        /// <summary>
        /// Grants temporary invulnerability to a valuable piece.
        /// </summary>
        private void GrantInvulnerability(Board board)
        {
            // In full implementation, would mark piece as invulnerable for 1 turn
            Debug.Log("Boss grants invulnerability!");
        }

        /// <summary>
        /// Finds the safest position on the board for a team.
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
        /// Gets remaining special ability charges.
        /// </summary>
        public int RemainingCharges => remainingCharges;
    }
}
