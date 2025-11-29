using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// GameObject 오버헤드 없이 AI 시뮬레이션을 위한 경량 보드 상태.
    /// MonoBehaviour를 상속하지 않는 순수 데이터 구조입니다.
    /// </summary>
    public class BoardState : IBoard
    {
        private int width;
        private int height;
        private PieceData[,] pieces;
        private bool[,] obstacles;

        /// <summary>
        /// 시뮬레이션을 위한 경량 기물 데이터.
        /// </summary>
        public class PieceData : IPiece
        {
            public PieceType Type { get; set; }
            public Team Team { get; set; }
            public Vector2Int Position { get; set; }
            public List<MovementRule> MovementRules { get; set; }

            // IPiece 구현
            Team IPiece.Team => Team;
            PieceType IPiece.Type => Type;
            Vector2Int IPiece.Position => Position;

            public PieceData(PieceType type, Team team, Vector2Int position)
            {
                Type = type;
                Team = team;
                Position = position;
                MovementRules = new List<MovementRule>();
            }

            public PieceData(Piece piece)
            {
                Type = piece.Type;
                Team = piece.Team;
                Position = piece.Position;
                MovementRules = new List<MovementRule>(piece.MovementRules);
            }

            public PieceData Clone()
            {
                var clone = new PieceData(Type, Team, Position);
                clone.MovementRules = new List<MovementRule>(MovementRules);
                return clone;
            }
        }

        public int Width => width;
        public int Height => height;

        public BoardState(int width, int height)
        {
            this.width = width;
            this.height = height;
            pieces = new PieceData[width, height];
            obstacles = new bool[width, height];
        }

        /// <summary>
        /// 기존 Board에서 BoardState를 생성합니다.
        /// </summary>
        public static BoardState FromBoard(Board board)
        {
            var state = new BoardState(board.Width, board.Height);

            // 장애물 복사
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    state.obstacles[x, y] = board.HasObstacle(new Vector2Int(x, y));
                }
            }

            // 기물 복사
            var allPieces = board.GetAllPieces();
            foreach (var piece in allPieces)
            {
                if (piece != null)
                {
                    var pieceData = new PieceData(piece);
                    state.pieces[piece.Position.x, piece.Position.y] = pieceData;
                }
            }

            return state;
        }

        /// <summary>
        /// 이 보드 상태의 깊은 복사본을 생성합니다.
        /// </summary>
        public BoardState Clone()
        {
            var clone = new BoardState(width, height);

            // 장애물 복사
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    clone.obstacles[x, y] = obstacles[x, y];
                    if (pieces[x, y] != null)
                    {
                        clone.pieces[x, y] = pieces[x, y].Clone();
                    }
                }
            }

            return clone;
        }

        public bool IsPositionValid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
        }

        public bool HasObstacle(Vector2Int pos)
        {
            if (!IsPositionValid(pos)) return true;
            return obstacles[pos.x, pos.y];
        }

        public PieceData GetPieceData(Vector2Int pos)
        {
            if (!IsPositionValid(pos)) return null;
            return pieces[pos.x, pos.y];
        }

        public bool IsOccupied(Vector2Int pos)
        {
            return GetPieceData(pos) != null;
        }

        public bool IsOccupiedByTeam(Vector2Int pos, Team team)
        {
            var piece = GetPieceData(pos);
            return piece != null && piece.Team == team;
        }

        /// <summary>
        /// 이 보드 상태에서 수를 시뮬레이션합니다.
        /// </summary>
        public bool SimulateMove(Vector2Int from, Vector2Int to)
        {
            if (!IsPositionValid(from) || !IsPositionValid(to))
                return false;

            var piece = GetPieceData(from);
            if (piece == null)
                return false;

            // 이전 위치에서 제거
            pieces[from.x, from.y] = null;

            // 새 위치에 배치 (그곳에 있는 기물 제거)
            pieces[to.x, to.y] = piece;
            piece.Position = to;

            return true;
        }

        /// <summary>
        /// 보드의 모든 기물을 가져옵니다.
        /// </summary>
        public List<PieceData> GetAllPieceData()
        {
            var result = new List<PieceData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        result.Add(pieces[x, y]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 특정 팀에 속한 모든 기물을 가져옵니다.
        /// </summary>
        public List<PieceData> GetPiecesByTeam(Team team)
        {
            var result = new List<PieceData>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null && pieces[x, y].Team == team)
                    {
                        result.Add(pieces[x, y]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 움직임 규칙을 사용하여 기물의 모든 유효한 수를 가져옵니다.
        /// </summary>
        public List<Vector2Int> GetValidMoves(PieceData piece)
        {
            var allMoves = new List<Vector2Int>();

            foreach (var rule in piece.MovementRules)
            {
                var moves = rule.GetValidMoves(this, piece.Position, piece.Team);
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

        // IBoard 구현 (움직임 규칙 호환성을 위해)
        IPiece IBoard.GetPieceAt(Vector2Int position)
        {
            var pieceData = GetPieceData(position);
            if (pieceData == null) return null;

            // PieceData를 IPiece로 반환 (PieceData가 IPiece 구현)
            return pieceData;
        }

        public IPiece GetPieceAt(Vector2Int position) => GetPieceData(position);

        bool IBoard.IsObstacle(Vector2Int position)
        {
            return HasObstacle(position);
        }
    }
}
