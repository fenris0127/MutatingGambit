using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 현재 보드 상태와 기물 규칙을 기반으로 체스 수를 검증합니다.
    /// </summary>
    public static class MoveValidator
    {
        /// <summary>
        /// 주어진 위치의 기물에 대한 모든 유효한 수를 가져옵니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="position">기물의 위치</param>
        /// <returns>유효한 목적지 위치 목록</returns>
        public static List<Vector2Int> GetValidMoves(Board board, Vector2Int position)
        {
            if (board == null || !board.IsPositionValid(position))
            {
                return new List<Vector2Int>();
            }

            Piece piece = board.GetPiece(position);
            if (piece == null)
            {
                return new List<Vector2Int>();
            }

            return piece.GetValidMoves(board);
        }

        /// <summary>
        /// 한 위치에서 다른 위치로의 수가 유효한지 확인합니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="from">시작 위치</param>
        /// <param name="to">목적지 위치</param>
        /// <returns>수가 유효하면 true</returns>
        public static bool IsValidMove(Board board, Vector2Int from, Vector2Int to)
        {
            if (board == null || !board.IsPositionValid(from) || !board.IsPositionValid(to))
            {
                return false;
            }

            // 같은 위치로는 이동할 수 없음
            if (from == to)
            {
                return false;
            }

            var validMoves = GetValidMoves(board, from);
            return validMoves.Contains(to);
        }

        /// <summary>
        /// 주어진 위치의 기물이 적 기물을 잡을 수 있는지 확인합니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="attackerPos">공격하는 기물의 위치</param>
        /// <param name="targetPos">목표 기물의 위치</param>
        /// <returns>잡을 수 있으면 true</returns>
        public static bool CanCapture(Board board, Vector2Int attackerPos, Vector2Int targetPos)
        {
            if (!IsValidMove(board, attackerPos, targetPos))
            {
                return false;
            }

            Piece attacker = board.GetPiece(attackerPos);
            Piece target = board.GetPiece(targetPos);

            if (attacker == null || target == null)
            {
                return false;
            }

            // 적 기물만 잡을 수 있음
            return attacker.Team != target.Team;
        }

        /// <summary>
        /// 기물이 공격할 수 있는 모든 위치를 가져옵니다 (일부 기물의 경우 빈 칸 포함).
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="position">기물의 위치</param>
        /// <returns>공격받는 위치 목록</returns>
        public static List<Vector2Int> GetAttackedPositions(Board board, Vector2Int position)
        {
            // 현재는 공격받는 위치가 유효한 수와 동일
            // 표준 체스에서는 폰의 경우 다를 수 있음 (대각선으로 공격하지만 앞으로 이동)
            return GetValidMoves(board, position);
        }

        /// <summary>
        /// 위치가 적 기물의 공격을 받고 있는지 확인합니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="position">확인할 위치</param>
        /// <param name="enemyTeam">공격할 수 있는 팀</param>
        /// <returns>위치가 공격받고 있으면 true</returns>
        public static bool IsPositionUnderAttack(Board board, Vector2Int position, Team enemyTeam)
        {
            var enemyPieces = board.GetPiecesByTeam(enemyTeam);

            foreach (var enemyPiece in enemyPieces)
            {
                var attackedPositions = GetAttackedPositions(board, enemyPiece.Position);
                if (attackedPositions.Contains(position))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 위치로 이동할 수 있는 모든 기물을 가져옵니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="targetPosition">목표 위치</param>
        /// <param name="team">확인할 기물의 팀</param>
        /// <returns>목표 위치로 이동할 수 있는 기물 목록</returns>
        public static List<Piece> GetPiecesAttackingPosition(Board board, Vector2Int targetPosition, Team team)
        {
            var attackingPieces = new List<Piece>();
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                if (IsValidMove(board, piece.Position, targetPosition))
                {
                    attackingPieces.Add(piece);
                }
            }

            return attackingPieces;
        }

        /// <summary>
        /// 팀에게 사용 가능한 유효한 수가 있는지 확인합니다.
        /// </summary>
        /// <param name="board">현재 보드 상태</param>
        /// <param name="team">확인할 팀</param>
        /// <returns>팀에게 최소 하나의 유효한 수가 있으면 true</returns>
        public static bool HasValidMoves(Board board, Team team)
        {
            var pieces = board.GetPiecesByTeam(team);

            foreach (var piece in pieces)
            {
                var validMoves = GetValidMoves(board, piece.Position);
                if (validMoves.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// BoardState에서 유효한 수를 가져옵니다 (AI 시뮬레이션용).
        /// </summary>
        public static List<Vector2Int> GetValidMovesForState(BoardState state, Vector2Int position)
        {
            // BoardState는 MovementRule을 직접 지원하지 않으므로,
            // 기본 움직임 패턴으로 대체
            // 전체 구현에서는 기물 타입별로 움직임을 계산
            var moves = new List<Vector2Int>();
            var piece = state.GetPieceAt(position);
            
            if (piece == null) return moves;

            // 간단한 구현: 모든 인접 위치 확인
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                Vector2Int newPos = new Vector2Int(position.x + dx[i], position.y + dy[i]);
                if (state.IsPositionValid(newPos))
                {
                    var targetPiece = state.GetPieceAt(newPos);
                    if (targetPiece == null || targetPiece.Team != piece.Team)
                    {
                        moves.Add(newPos);
                    }
                }
            }

            return moves;
        }
    }
}
