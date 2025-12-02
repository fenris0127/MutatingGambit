using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// 연약한 비숍 변이: 비숍이 정확히 3칸만 대각선으로 이동합니다 (그 이상도 이하도 아님).
    /// PRD에서는 "유리 비숍"으로도 알려져 있습니다.
    /// Fragile Bishop Mutation: Bishop moves exactly 3 squares diagonally (no more, no less).
    /// Also known as "Glass Bishop" in the PRD.
    /// </summary>
    [CreateAssetMenu(fileName = "FragileBishopMutation", menuName = "Mutations/Initial/Fragile Bishop")]
    public class FragileBishopMutation : Mutation
    {
        private DiagonalRule originalRule;
        private FixedDiagonalRule fixedRule;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Bishop)
            {
                Debug.LogWarning("FragileBishopMutation can only be applied to Bishops.");
                return;
            }

            // Remove existing diagonal rules
            var rulesToRemove = new List<MovementRule>();
            foreach (var rule in piece.MovementRules)
            {
                if (rule is DiagonalRule)
                {
                    rulesToRemove.Add(rule);
                    originalRule = rule as DiagonalRule;
                }
            }

            foreach (var rule in rulesToRemove)
            {
                piece.RemoveMovementRule(rule);
            }

            // Add fixed distance diagonal rule
            if (fixedRule == null)
            {
                fixedRule = ScriptableObject.CreateInstance<FixedDiagonalRule>();
            }

            piece.AddMovementRule(fixedRule);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // Remove fixed rule
            if (fixedRule != null)
            {
                piece.RemoveMovementRule(fixedRule);
            }

            // Restore original diagonal rule if it existed
            if (originalRule != null)
            {
                piece.AddMovementRule(originalRule);
            }
        }
    }

    /// <summary>
    /// 정확히 3칸 이동을 요구하는 대각선 이동 규칙입니다.
    /// Diagonal movement rule that requires exactly 3 squares of movement.
    /// </summary>
    public class FixedDiagonalRule : MovementRule
    {
        private const int EXACT_DISTANCE = 3;

        public override List<Vector2Int> GetValidMoves(
            IBoard board,
            Vector2Int fromPosition,
            Team pieceTeam)
        {
            var validMoves = new List<Vector2Int>();

            // Four diagonal directions
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 1),    // Up-Right
                new Vector2Int(1, -1),   // Down-Right
                new Vector2Int(-1, 1),   // Up-Left
                new Vector2Int(-1, -1)   // Down-Left
            };

            foreach (var direction in directions)
            {
                Vector2Int targetPos = fromPosition + (direction * EXACT_DISTANCE);

                // Check if position is valid
                if (!board.IsPositionValid(targetPos))
                {
                    continue;
                }

                // Check if path is clear (can't jump over pieces)
                bool pathClear = true;
                for (int i = 1; i < EXACT_DISTANCE; i++)
                {
                    Vector2Int checkPos = fromPosition + (direction * i);

                    if (board.IsObstacle(checkPos) || !IsEmptyPosition(board, checkPos))
                    {
                        pathClear = false;
                        break;
                    }
                }

                if (!pathClear)
                {
                    continue;
                }

                // Check if destination is valid
                if (board.IsObstacle(targetPos))
                {
                    continue;
                }

                // Can move to empty squares or capture enemy pieces
                if (IsEmptyPosition(board, targetPos) || IsEnemyPiece(board, targetPos, pieceTeam))
                {
                    validMoves.Add(targetPos);
                }
            }

            return validMoves;
        }
    }
}
