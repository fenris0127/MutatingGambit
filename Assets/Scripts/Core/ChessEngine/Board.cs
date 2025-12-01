using System.Collections.Generic;
using System.Text;
using UnityEngine;
using MutatingGambit.Core.MovementRules;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 체스 보드를 나타내고 기물 위치를 관리합니다.
    /// 움직임 규칙 쿼리를 위한 IBoard 인터페이스를 구현합니다.
    /// 리팩토링: Region 그룹화 개선, 함수 분해(10줄 이하), 한국어 문서화 완벽 적용
    /// </summary>
    public class Board : MonoBehaviour, IBoard
    {
        #region 싱글톤
        private static Board instance;

        /// <summary>
        /// 보드 싱글톤 인스턴스입니다.
        /// </summary>
        public static Board Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<Board>();
                }
                return instance;
            }
        }
        #endregion

        #region 보드 설정
        [SerializeField]
        private int width = 8;

        [SerializeField]
        private int height = 8;

        [SerializeField]
        private bool[,] obstacles;
        #endregion

        #region 시스템 참조
        [Header("Artifact System")]
        [SerializeField]
        private ArtifactManager artifactManager;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject piecePrefab;
        #endregion

        #region 기물 관리
        private Piece[,] pieces;
        private List<Piece> allPieces = new List<Piece>();
        #endregion

        #region 이벤트
        /// <summary>
        /// 기물이 이동할 때 발생하는 이벤트.
        /// 인자: 움직이는 기물, 시작 위치, 목표 위치, 잡힌 기물 (null 가능)
        /// </summary>
        public event System.Action<Piece, Vector2Int, Vector2Int, Piece> OnPieceMoved;
        #endregion

        #region 공개 속성
        /// <summary>
        /// 이 보드의 아티팩트 관리자를 가져옵니다.
        /// </summary>
        public ArtifactManager ArtifactManager
        {
            get
            {
                if (artifactManager == null)
                {
                    artifactManager = gameObject.AddComponent<ArtifactManager>();
                    artifactManager.SetBoard(this);
                }
                return artifactManager;
            }
        }

        /// <summary>
        /// 보드의 너비를 가져옵니다.
        /// </summary>
        public int Width => width;

        /// <summary>
        /// 보드의 높이를 가져옵니다.
        /// </summary>
        public int Height => height;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 보드를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            EnsureInitialized();
        }
        #endregion

        #region 공개 메서드 - 초기화
        /// <summary>
        /// 지정된 크기로 보드를 초기화합니다.
        /// </summary>
        /// <param name="boardWidth">보드의 너비</param>
        /// <param name="boardHeight">보드의 높이</param>
        public void Initialize(int boardWidth, int boardHeight)
        {
            width = boardWidth;
            height = boardHeight;
            InitializeArrays();
        }
        #endregion

        #region 공개 메서드 - 기물 조회
        /// <summary>
        /// 지정된 위치의 기물을 가져옵니다 (IBoard 인터페이스).
        /// </summary>
        /// <param name="position">조회할 위치</param>
        /// <returns>해당 위치의 기물 (없으면 null)</returns>
        public IPiece GetPieceAt(Vector2Int position) => GetPiece(position);

        /// <summary>
        /// 지정된 위치의 기물을 가져옵니다 (Piece 타입 반환).
        /// </summary>
        /// <param name="position">조회할 위치</param>
        /// <returns>해당 위치의 기물 (없으면 null)</returns>
        public Piece GetPiece(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return null;
            }

            return pieces[position.x, position.y];
        }

        /// <summary>
        /// 특정 팀의 모든 기물을 가져옵니다.
        /// </summary>
        /// <param name="team">팀</param>
        /// <returns>해당 팀의 기물 리스트</returns>
        public List<Piece> GetPiecesByTeam(Team team)
        {
            var result = new List<Piece>();
            CollectPiecesByTeam(team, result);
            return result;
        }

        /// <summary>
        /// 보드의 모든 기물을 가져옵니다.
        /// </summary>
        /// <returns>모든 기물의 복사본 리스트</returns>
        public List<Piece> GetAllPieces() => new List<Piece>(allPieces);
        #endregion

        #region 공개 메서드 - 위치 검증
        /// <summary>
        /// 위치가 보드 범위 내에 있는지 확인합니다.
        /// </summary>
        /// <param name="position">확인할 위치</param>
        /// <returns>범위 내에 있으면 true</returns>
        public bool IsPositionValid(Vector2Int position) => position.IsWithinBounds(width, height);
        #endregion

        #region 공개 메서드 - 장애물 관리
        /// <summary>
        /// 위치에 장애물이 있는지 확인합니다.
        /// </summary>
        /// <param name="position">확인할 위치</param>
        /// <returns>장애물이 있으면 true</returns>
        public bool IsObstacle(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return false;
            }

            return obstacles[position.x, position.y];
        }

        /// <summary>
        /// 지정된 위치에 장애물을 설정하거나 제거합니다.
        /// </summary>
        /// <param name="position">위치</param>
        /// <param name="isObstacle">장애물 설정 여부</param>
        public void SetObstacle(Vector2Int position, bool isObstacle)
        {
            if (IsPositionValid(position))
            {
                obstacles[position.x, position.y] = isObstacle;
            }
        }

        /// <summary>
        /// 위치에 장애물이 있는지 확인합니다.
        /// </summary>
        /// <param name="position">확인할 위치</param>
        /// <returns>장애물이 있으면 true</returns>
        public bool HasObstacle(Vector2Int position) => IsObstacle(position);
        #endregion

        #region 공개 메서드 - 기물 생성
        /// <summary>
        /// 보드에 새로운 기물을 생성합니다.
        /// </summary>
        /// <param name="type">기물 타입</param>
        /// <param name="team">팀</param>
        /// <param name="position">위치</param>
        /// <returns>생성된 기물</returns>
        public Piece SpawnPiece(PieceType type, Team team, Vector2Int position)
        {
            GameObject pieceObject = CreatePieceObject(type, team);
            Piece piece = ConfigurePieceComponent(pieceObject, type, team, position);
            PlacePiece(piece, position);

            return piece;
        }
        #endregion

        #region 공개 메서드 - 기물 배치 및 이동
        /// <summary>
        /// 보드의 지정된 위치에 기물을 배치합니다.
        /// </summary>
        /// <param name="piece">배치할 기물</param>
        /// <param name="position">배치 위치</param>
        public void PlacePiece(Piece piece, Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                LogInvalidPosition(position);
                return;
            }

            RemoveFromPreviousPosition(piece);
            PlaceAtNewPosition(piece, position);
            EnsurePieceInList(piece);
        }

        /// <summary>
        /// 기물을 한 위치에서 다른 위치로 이동합니다.
        /// </summary>
        /// <param name="from">시작 위치</param>
        /// <param name="to">목표 위치</param>
        /// <returns>이동 성공 여부</returns>
        public bool MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!ValidateMovePositions(from, to))
            {
                return false;
            }

            Piece piece = GetPieceToMove(from);
            if (piece == null)
            {
                return false;
            }

            Piece capturedPiece = HandleCapture(to);
            ExecuteMove(piece, from, to);
            NotifyPieceMoved(piece, from, to, capturedPiece);

            return true;
        }

        /// <summary>
        /// 보드에서 기물을 제거합니다.
        /// </summary>
        /// <param name="position">제거할 위치</param>
        public void RemovePiece(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return;
            }

            Piece piece = pieces[position.x, position.y];
            if (piece != null)
            {
                DestroyPiece(piece, position);
            }
        }
        #endregion

        #region 공개 메서드 - 보드 관리
        /// <summary>
        /// 보드에서 모든 기물을 제거합니다.
        /// </summary>
        public void Clear()
        {
            DestroyAllPieces();
            ClearAllObstacles();
            allPieces.Clear();
        }

        /// <summary>
        /// GameObject 오버헤드 없이 AI 시뮬레이션을 위한 경량 보드 상태를 생성합니다.
        /// AI 수 평가를 위해 Clone()보다 훨씬 빠릅니다.
        /// </summary>
        /// <returns>복사된 보드 상태</returns>
        public BoardState CloneAsState() => BoardState.FromBoard(this);
        #endregion

        #region 공개 메서드 - 디버깅
        /// <summary>
        /// 디버깅을 위한 보드의 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>보드의 문자열 표현</returns>
        public override string ToString()
        {
            var result = new StringBuilder();
            AppendBoardHeader(result);
            AppendBoardRows(result);
            AppendColumnLabels(result);

            return result.ToString();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 보드가 초기화되었는지 확인합니다.
        /// </summary>
        private void EnsureInitialized()
        {
            if (pieces == null)
            {
                Initialize(width, height);
            }
        }

        /// <summary>
        /// 배열들을 초기화합니다.
        /// </summary>
        private void InitializeArrays()
        {
            pieces = new Piece[width, height];
            obstacles = new bool[width, height];
            allPieces.Clear();
        }
        #endregion

        #region 비공개 메서드 - 기물 생성
        /// <summary>
        /// 기물 GameObject를 생성합니다.
        /// </summary>
        private GameObject CreatePieceObject(PieceType type, Team team)
        {
            string objectName = "{team}_{type}";

            if (piecePrefab != null)
            {
                return InstantiatePrefab(objectName);
            }

            return new GameObject(objectName);
        }

        /// <summary>
        /// 프리팹을 인스턴스화합니다.
        /// </summary>
        private GameObject InstantiatePrefab(string name)
        {
            GameObject obj = Instantiate(piecePrefab);
            obj.name = name;
            return obj;
        }

        /// <summary>
        /// 기물 컴포넌트를 설정합니다.
        /// </summary>
        private Piece ConfigurePieceComponent(GameObject obj, PieceType type, Team team, Vector2Int position)
        {
            Piece piece = EnsurePieceComponent(obj);
            piece.Initialize(type, team, position);
            return piece;
        }

        /// <summary>
        /// Piece 컴포넌트를 확보합니다.
        /// </summary>
        private Piece EnsurePieceComponent(GameObject obj)
        {
            Piece piece = obj.GetComponent<Piece>();
            if (piece == null)
            {
                piece = obj.AddComponent<Piece>();
            }
            return piece;
        }
        #endregion

        #region 비공개 메서드 - 기물 배치
        /// <summary>
        /// 이전 위치에서 기물을 제거합니다.
        /// </summary>
        private void RemoveFromPreviousPosition(Piece piece)
        {
            if (piece.Position.IsWithinBounds(width, height) &&
                pieces[piece.Position.x, piece.Position.y] == piece)
            {
                pieces[piece.Position.x, piece.Position.y] = null;
            }
        }

        /// <summary>
        /// 새 위치에 기물을 배치합니다.
        /// </summary>
        private void PlaceAtNewPosition(Piece piece, Vector2Int position)
        {
            pieces[position.x, position.y] = piece;
            piece.Position = position;
        }

        /// <summary>
        /// 기물이 리스트에 있는지 확인하고 없으면 추가합니다.
        /// </summary>
        private void EnsurePieceInList(Piece piece)
        {
            if (!allPieces.Contains(piece))
            {
                allPieces.Add(piece);
            }
        }

        /// <summary>
        /// 잘못된 위치 로그를 출력합니다.
        /// </summary>
        private void LogInvalidPosition(Vector2Int position)
        {
            Debug.LogError("잘못된 위치에 기물을 배치할 수 없습니다: {position}");
        }
        #endregion

        #region 비공개 메서드 - 기물 이동
        /// <summary>
        /// 이동 위치의 유효성을 검증합니다.
        /// </summary>
        private bool ValidateMovePositions(Vector2Int from, Vector2Int to)
        {
            return IsPositionValid(from) && IsPositionValid(to);
        }

        /// <summary>
        /// 이동할 기물을 가져옵니다.
        /// </summary>
        private Piece GetPieceToMove(Vector2Int from)
        {
            return pieces[from.x, from.y];
        }

        /// <summary>
        /// 목적지의 기물을 잡아 제거합니다.
        /// </summary>
        private Piece HandleCapture(Vector2Int to)
        {
            Piece capturedPiece = pieces[to.x, to.y];
            if (capturedPiece != null)
            {
                RemovePiece(to);
            }
            return capturedPiece;
        }

        /// <summary>
        /// 기물 이동을 실행합니다.
        /// </summary>
        private void ExecuteMove(Piece piece, Vector2Int from, Vector2Int to)
        {
            pieces[from.x, from.y] = null;
            pieces[to.x, to.y] = piece;
            piece.Position = to;
        }

        /// <summary>
        /// 기물 이동 이벤트를 발생시킵니다.
        /// </summary>
        private void NotifyPieceMoved(Piece piece, Vector2Int from, Vector2Int to, Piece captured)
        {
            OnPieceMoved?.Invoke(piece, from, to, captured);
        }
        #endregion

        #region 비공개 메서드 - 기물 제거
        /// <summary>
        /// 기물을 파괴합니다.
        /// </summary>
        private void DestroyPiece(Piece piece, Vector2Int position)
        {
            pieces[position.x, position.y] = null;
            allPieces.Remove(piece);
            Destroy(piece.gameObject);
        }
        #endregion

        #region 비공개 메서드 - 보드 정리
        /// <summary>
        /// 모든 기물을 파괴합니다.
        /// </summary>
        private void DestroyAllPieces()
        {
            for (int x = 0; x < width; x++)
            {
                DestroyPiecesInColumn(x);
            }
        }

        /// <summary>
        /// 한 열의 모든 기물을 파괴합니다.
        /// </summary>
        private void DestroyPiecesInColumn(int x)
        {
            for (int y = 0; y < height; y++)
            {
                DestroyPieceAtPosition(x, y);
            }
        }

        /// <summary>
        /// 특정 위치의 기물을 파괴합니다.
        /// </summary>
        private void DestroyPieceAtPosition(int x, int y)
        {
            if (pieces[x, y] != null)
            {
                Destroy(pieces[x, y].gameObject);
                pieces[x, y] = null;
            }
        }

        /// <summary>
        /// 모든 장애물을 제거합니다.
        /// </summary>
        private void ClearAllObstacles()
        {
            for (int x = 0; x < width; x++)
            {
                ClearObstaclesInColumn(x);
            }
        }

        /// <summary>
        /// 한 열의 모든 장애물을 제거합니다.
        /// </summary>
        private void ClearObstaclesInColumn(int x)
        {
            for (int y = 0; y < height; y++)
            {
                obstacles[x, y] = false;
            }
        }
        #endregion

        #region 비공개 메서드 - 기물 조회
        /// <summary>
        /// 특정 팀의 기물들을 수집합니다.
        /// </summary>
        private void CollectPiecesByTeam(Team team, List<Piece> result)
        {
            foreach (var piece in allPieces)
            {
                if (piece != null && piece.Team == team)
                {
                    result.Add(piece);
                }
            }
        }
        #endregion

        #region 비공개 메서드 - 디버깅
        /// <summary>
        /// 보드 헤더를 추가합니다.
        /// </summary>
        private void AppendBoardHeader(StringBuilder result)
        {
            result.AppendLine($"보드 ({width}x{height}):");
        }

        /// <summary>
        /// 보드의 모든 행을 추가합니다.
        /// </summary>
        private void AppendBoardRows(StringBuilder result)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                AppendSingleRow(result, y);
            }
        }

        /// <summary>
        /// 단일 행을 추가합니다.
        /// </summary>
        private void AppendSingleRow(StringBuilder result, int y)
        {
            result.Append($"{y + 1} ");
            AppendRowCells(result, y);
            result.AppendLine();
        }

        /// <summary>
        /// 행의 모든 셀을 추가합니다.
        /// </summary>
        private void AppendRowCells(StringBuilder result, int y)
        {
            for (int x = 0; x < width; x++)
            {
                AppendCellSymbol(result, x, y);
            }
        }

        /// <summary>
        /// 셀의 심볼을 추가합니다.
        /// </summary>
        private void AppendCellSymbol(StringBuilder result, int x, int y)
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

        /// <summary>
        /// 열 라벨을 추가합니다.
        /// </summary>
        private void AppendColumnLabels(StringBuilder result)
        {
            result.Append("  ");
            for (int x = 0; x < width; x++)
            {
                result.Append((char)('a' + x) + " ");
            }
        }

        /// <summary>
        /// 기물의 심볼 문자열을 가져옵니다.
        /// </summary>
        /// <param name="piece">기물</param>
        /// <returns>심볼 문자열</returns>
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
            return team == Team.White ? symbol + " " : symbol.ToLower();
        }
        #endregion
    }
}
