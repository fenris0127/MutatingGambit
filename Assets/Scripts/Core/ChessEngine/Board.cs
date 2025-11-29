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
    /// </summary>
    public class Board : MonoBehaviour, IBoard
    {
        #region 변수
        [SerializeField]
        private int width = 8;

        [SerializeField]
        private int height = 8;

        [SerializeField]
        private bool[,] obstacles;

        [Header("Artifact System")]
        [SerializeField]
        private ArtifactManager artifactManager;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject piecePrefab;

        private Piece[,] pieces;
        private List<Piece> allPieces = new List<Piece>();

        /// <summary>
        /// 기물이 이동할 때 발생하는 이벤트.
        /// 인자: 움직이는 기물, 시작 위치, 목표 위치, 잡힌 기물 (null 가능)
        /// </summary>
        public event System.Action<Piece, Vector2Int, Vector2Int, Piece> OnPieceMoved;
        #endregion

        #region 속성
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
        private void Awake()
        {
            if (pieces == null)
            {
                Initialize(width, height);
            }
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 지정된 크기로 보드를 초기화합니다.
        /// </summary>
        public void Initialize(int boardWidth, int boardHeight)
        {
            width = boardWidth;
            height = boardHeight;
            pieces = new Piece[width, height];
            obstacles = new bool[width, height];
            allPieces.Clear();
        }

        /// <summary>
        /// 지정된 위치의 기물을 가져옵니다.
        /// </summary>
        public IPiece GetPieceAt(Vector2Int position) => GetPiece(position);

        /// <summary>
        /// 지정된 위치의 기물을 가져옵니다 (Piece 타입 반환).
        /// </summary>
        public Piece GetPiece(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return null;
            }

            return pieces[position.x, position.y];
        }

        /// <summary>
        /// 위치가 보드 범위 내에 있는지 확인합니다.
        /// </summary>
        public bool IsPositionValid(Vector2Int position) => position.IsWithinBounds(width, height);

        /// <summary>
        /// 위치에 장애물이 있는지 확인합니다.
        /// </summary>
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
        public bool HasObstacle(Vector2Int position) => IsObstacle(position);

        /// <summary>
        /// 보드에 새로운 기물을 생성합니다.
        /// </summary>
        public Piece SpawnPiece(PieceType type, Team team, Vector2Int position)
        {
            GameObject pieceObject;

            if (piecePrefab != null)
            {
                pieceObject = Instantiate(piecePrefab);
                pieceObject.name = $"{team}_{type}";
            }
            else
            {
                pieceObject = new GameObject($"{team}_{type}");
            }

            Piece piece = pieceObject.GetComponent<Piece>();
            if (piece == null)
            {
                piece = pieceObject.AddComponent<Piece>();
            }

            piece.Initialize(type, team, position);
            PlacePiece(piece, position);

            return piece;
        }

        /// <summary>
        /// 보드의 지정된 위치에 기물을 배치합니다.
        /// </summary>
        public void PlacePiece(Piece piece, Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                Debug.LogError($"Cannot place piece at invalid position {position}");
                return;
            }

            // 이전 위치에서 기물 제거 (존재하고 보드에 있는 경우)
            if (piece.Position.IsWithinBounds(width, height) && pieces[piece.Position.x, piece.Position.y] == piece)
            {
                pieces[piece.Position.x, piece.Position.y] = null;
            }

            // 새 위치에 기물 배치
            pieces[position.x, position.y] = piece;
            piece.Position = position;

            // 아직 목록에 없으면 추가
            if (!allPieces.Contains(piece))
            {
                allPieces.Add(piece);
            }
        }

        /// <summary>
        /// 기물을 한 위치에서 다른 위치로 이동합니다.
        /// </summary>
        public bool MovePiece(Vector2Int from, Vector2Int to)
        {
            if (!IsPositionValid(from) || !IsPositionValid(to))
            {
                return false;
            }

            Piece piece = pieces[from.x, from.y];
            if (piece == null)
            {
                return false;
            }

            // 목적지에 있는 기물 잡기
            Piece capturedPiece = pieces[to.x, to.y];
            if (capturedPiece != null)
            {
                RemovePiece(to);
            }

            // 기물 이동
            pieces[from.x, from.y] = null;
            pieces[to.x, to.y] = piece;
            piece.Position = to;

            OnPieceMoved?.Invoke(piece, from, to, capturedPiece);

            return true;
        }

        /// <summary>
        /// 보드에서 기물을 제거합니다.
        /// </summary>
        public void RemovePiece(Vector2Int position)
        {
            if (!IsPositionValid(position))
            {
                return;
            }

            Piece piece = pieces[position.x, position.y];
            if (piece != null)
            {
                pieces[position.x, position.y] = null;
                allPieces.Remove(piece);
                Destroy(piece.gameObject);
            }
        }

        /// <summary>
        /// 특정 팀의 모든 기물을 가져옵니다.
        /// </summary>
        public List<Piece> GetPiecesByTeam(Team team)
        {
            var result = new List<Piece>();
            foreach (var piece in allPieces)
            {
                if (piece != null && piece.Team == team)
                {
                    result.Add(piece);
                }
            }
            return result;
        }

        /// <summary>
        /// 보드의 모든 기물을 가져옵니다.
        /// </summary>
        public List<Piece> GetAllPieces() => new List<Piece>(allPieces);

        /// <summary>
        /// 보드에서 모든 기물을 제거합니다.
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (pieces[x, y] != null)
                    {
                        Destroy(pieces[x, y].gameObject);
                        pieces[x, y] = null;
                    }
                    obstacles[x, y] = false;
                }
            }
            allPieces.Clear();
        }

        /// <summary>
        /// GameObject 오버헤드 없이 AI 시뮬레이션을 위한 경량 보드 상태를 생성합니다.
        /// AI 수 평가를 위해 Clone()보다 훨씬 빠릅니다.
        /// </summary>
        public BoardState CloneAsState() => BoardState.FromBoard(this);

        /// <summary>
        /// 디버깅을 위한 보드의 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine($"Board ({width}x{height}):");

            for (int y = height - 1; y >= 0; y--)
            {
                result.Append($"{y + 1} ");
                for (int x = 0; x < width; x++)
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
                result.AppendLine();
            }

            result.Append("  ");
            for (int x = 0; x < width; x++)
            {
                result.Append((char)('a' + x) + " ");
            }

            return result.ToString();
        }
        #endregion

        #region 비공개 메서드
        private string GetPieceSymbol(Piece piece)
        {
            string symbol = piece.Type switch
            {
                PieceType.King => "K",
                PieceType.Queen => "Q",
                PieceType.Rook => "R",
                PieceType.Bishop => "B",
                PieceType.Knight => "N",
                PieceType.Pawn => "P",
                _ => "?"
            };

            return piece.Team == Team.White ? symbol + " " : symbol.ToLower() + " ";
        }
        #endregion
    }
}
