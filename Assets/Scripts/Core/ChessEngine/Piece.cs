using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 동적 움직임 규칙을 가진 체스 기물을 나타냅니다.
    /// 런타임 규칙 수정을 위해 전략 패턴을 구현합니다.
    /// </summary>
    public class Piece : MonoBehaviour, IPiece
    {
        [SerializeField]
        private PieceType pieceType;

        [SerializeField]
        private Team team;

        [SerializeField]
        private Vector2Int position;

        [SerializeField]
        private List<MovementRule> movementRules = new List<MovementRule>();

        /// <summary>
        /// 이 기물의 타입.
        /// </summary>
        public PieceType Type => pieceType;

        /// <summary>
        /// 이 기물이 속한 팀.
        /// </summary>
        public Team Team => team;

        /// <summary>
        /// 보드에서 이 기물의 현재 위치를 가져오거나 설정합니다.
        /// </summary>
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        /// <summary>
        /// 이 기물에 적용된 움직임 규칙 목록.
        /// </summary>
        public List<MovementRule> MovementRules => movementRules;

        /// <summary>
        /// 타입과 팀으로 기물을 초기화합니다.
        /// </summary>
        public void Initialize(PieceType type, Team pieceTeam, Vector2Int startPosition)
        {
            pieceType = type;
            team = pieceTeam;
            position = startPosition;
            movementRules.Clear();
        }

        /// <summary>
        /// 이 기물에 움직임 규칙을 추가합니다.
        /// </summary>
        public void AddMovementRule(MovementRule rule)
        {
            if (rule != null && !movementRules.Contains(rule))
            {
                movementRules.Add(rule);
            }
        }

        /// <summary>
        /// 이 기물에서 움직임 규칙을 제거합니다.
        /// </summary>
        public void RemoveMovementRule(MovementRule rule)
        {
            movementRules.Remove(rule);
        }

        /// <summary>
        /// 이 기물에서 모든 움직임 규칙을 지웁니다.
        /// </summary>
        public void ClearMovementRules()
        {
            movementRules.Clear();
        }

        /// <summary>
        /// 이 기물을 퀸으로 승급시킵니다.
        /// 메모리 누수를 방지하기 위해 MovementRuleFactory를 사용합니다.
        /// </summary>
        public void PromoteToQueen()
        {
            pieceType = PieceType.Queen;
            movementRules.Clear();
            
            // 메모리 누수를 방지하기 위해 팩토리를 사용하여 퀸 규칙 추가
            var factory = MovementRuleFactory.Instance;
            var queenRules = factory.GetQueenRules();
            foreach (var rule in queenRules)
            {
                AddMovementRule(rule);
            }
            
            Debug.Log($"{position}의 {team} 기물이 퀸으로 승급했습니다!");
        }

        /// <summary>
        /// 이 기물이 변이를 가지고 있는지 확인합니다 (표준 규칙보다 많음).
        /// </summary>
        public bool HasMutations()
        {
            // 기물이 비정상적인 규칙 조합을 가지고 있으면 변이된 것으로 간주
            // 지금은 간단한 휴리스틱
            return movementRules.Count > GetStandardRuleCount(pieceType);
        }

        /// <summary>
        /// 기물 타입의 표준 움직임 규칙 수를 가져옵니다.
        /// </summary>
        private int GetStandardRuleCount(PieceType type)
        {
            switch (type)
            {
                case PieceType.Pawn:
                    return 1; // 특수 폰 규칙
                case PieceType.Knight:
                    return 1; // 나이트 점프
                case PieceType.Bishop:
                    return 1; // 대각선
                case PieceType.Rook:
                    return 1; // 직선
                case PieceType.Queen:
                    return 2; // 대각선 + 직선
                case PieceType.King:
                    return 1; // 킹 한 칸
                default:
                    return 1;
            }
        }

        /// <summary>
        /// 주어진 보드에서 이 기물의 모든 유효한 수를 가져옵니다.
        /// </summary>
        public List<Vector2Int> GetValidMoves(IBoard board)
        {
            var allMoves = new List<Vector2Int>();

            foreach (var rule in movementRules)
            {
                var moves = rule.GetValidMoves(board, position, team);
                foreach (var move in moves)
                {
                    if (!allMoves.Contains(move))
                    {
                        allMoves.Add(move);
                    }
                }
            }

            return allMoves;
        }

        /// <summary>
        /// 디버깅을 위한 이 기물의 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return $"{team} {pieceType} at {BoardPosition.ToNotation(position)}";
        }

        /// <summary>
        /// 메모리 누수를 방지하기 위해 기물이 파괴될 때 정리합니다.
        /// </summary>
        private void OnDestroy()
        {
            // 메모리 누수를 방지하기 위해 MutationManager에서 등록 취소
            if (Systems.Mutations.MutationManager.Instance != null)
            {
                Systems.Mutations.MutationManager.Instance.UnregisterPiece(this);
            }
        }
    }
}
