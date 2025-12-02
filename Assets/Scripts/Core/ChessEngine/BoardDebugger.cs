using System.Text;
using UnityEngine;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// Board의 디버깅 및 문자열 표현 생성을 전담합니다.
    /// 단일 책임: 보드 상태의 시각적 표현 생성만 담당
    /// </summary>
    public class BoardDebugger
    {
        #region 의존성
        private readonly IBoard board;
        #endregion

        #region 생성자
        /// <summary>
        /// BoardDebugger를 초기화합니다.
        /// </summary>
        /// <param name="board">디버깅할 보드</param>
        public BoardDebugger(IBoard board)
        {
            this.board = board;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 보드의 문자열 표현을 생성합니다.
        /// </summary>
        /// <param name="pieces">기물 배열</param>
        /// <param name="obstacles">장애물 배열</param>
        /// <returns>보드의 문자열 표현</returns>
        public string GenerateBoardString(Piece[,] pieces, bool[,] obstacles)
        {
            var result = new StringBuilder();
            int width = board.Width;
            int height = board.Height;

            AppendBoardHeader(result, width, height);
            AppendBoardRows(result, pieces, obstacles, width, height);
            AppendColumnLabels(result, width);

            return result.ToString();
        }
        #endregion

        #region 비공개 메서드 - 헤더
        /// <summary>
        /// 보드 헤더를 추가합니다.
        /// </summary>
        private void AppendBoardHeader(StringBuilder result, int width, int height)
        {
            result.AppendLine($"보드 ({width}x{height}):");
        }
        #endregion

        #region 비공개 메서드 - 행 렌더링
        /// <summary>
        /// 모든 행을 렌더링합니다.
        /// </summary>
        private void AppendBoardRows(StringBuilder result, Piece[,] pieces, bool[,] obstacles, int width, int height)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                AppendSingleRow(result, pieces, obstacles, width, y);
            }
        }

        /// <summary>
        /// 단일 행을 렌더링합니다.
        /// </summary>
        private void AppendSingleRow(StringBuilder result, Piece[,] pieces, bool[,] obstacles, int width, int y)
        {
            result.Append($"{y + 1} ");
            AppendRowCells(result, pieces, obstacles, width, y);
            result.AppendLine();
        }

        /// <summary>
        /// 행의 모든 셀을 렌더링합니다.
        /// </summary>
        private void AppendRowCells(StringBuilder result, Piece[,] pieces, bool[,] obstacles, int width, int y)
        {
            for (int x = 0; x < width; x++)
            {
                AppendCellSymbol(result, pieces, obstacles, x, y);
            }
        }
        #endregion

        #region 비공개 메서드 - 셀 렌더링
        /// <summary>
        /// 단일 셀의 심볼을 렌더링합니다.
        /// </summary>
        private void AppendCellSymbol(StringBuilder result, Piece[,] pieces, bool[,] obstacles, int x, int y)
        {
            var piece = pieces[x, y];

            if (piece != null)
            {
                result.Append(GetPieceSymbol(piece));
            }
            else if (obstacles[x, y])
            {
                result.Append("# ");
            }
            else
            {
                result.Append(". ");
            }
        }
        #endregion

        #region 비공개 메서드 - 기물 심볼
        /// <summary>
        /// 기물의 심볼 문자열을 가져옵니다.
        /// </summary>
        private string GetPieceSymbol(Piece piece)
        {
            string symbol = GetBasePieceSymbol(piece.Type);
            return FormatSymbolForTeam(symbol, piece.Team);
        }

        /// <summary>
        /// 기본 기물 심볼을 가져옵니다.
        /// </summary>
        private string GetBasePieceSymbol(PieceType type)
        {
            return type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                PieceType.Pawn => "P",
                _ => "?"
            };
        }

        /// <summary>
        /// 팀에 맞게 심볼을 포맷팅합니다.
        /// </summary>
        private string FormatSymbolForTeam(string symbol, Team team)
        {
            return team == Team.White ? symbol + " " : symbol.ToLower() + " ";
        }
        #endregion

        #region 비공개 메서드 - 열 라벨
        /// <summary>
        /// 열 라벨을 추가합니다.
        /// </summary>
        private void AppendColumnLabels(StringBuilder result, int width)
        {
            result.Append("  ");
            for (int x = 0; x < width; x++)
            {
                result.Append((char)('a' + x) + " ");
            }
        }
        #endregion
    }
}
