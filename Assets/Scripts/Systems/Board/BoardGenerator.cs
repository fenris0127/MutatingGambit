using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.BoardSystem
{
    /// <summary>
    /// BoardData로부터 시각적 보드를 생성하고 관리합니다.
    /// 타일 GameObject를 생성하고 체스 엔진 Board와 통합합니다.
    /// </summary>
    public class BoardGenerator : MonoBehaviour
    {
        #region 필드 - 참조
        [Header("참조")]
        [SerializeField]
        private BoardData boardData;

        [SerializeField]
        private GameObject tilePrefab;

        [SerializeField]
        private Transform boardContainer;

        [SerializeField]
        private Core.ChessEngine.Board chessBoard;
        #endregion

        #region 필드 - 생성된 타일
        [Header("생성된 타일")]
        private Tile[,] tiles;
        private bool isGenerated = false;
        #endregion

        #region 공개 속성
        /// <summary>이 보드를 생성하는 데 사용된 BoardData를 가져옵니다.</summary>
        public BoardData Data => boardData;

        /// <summary>보드가 생성되었는지 여부를 가져옵니다.</summary>
        public bool IsGenerated => isGenerated;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 데이터가 할당되어 있으면 보드를 자동 생성합니다.
        /// </summary>
        private void Start()
        {
            if (ShouldAutoGenerate())
            {
                GenerateFromData(boardData);
            }
        }
        #endregion

        #region 공개 메서드 - 생성
        /// <summary>
        /// BoardData로부터 보드를 생성합니다.
        /// </summary>
        /// <param name="data">보드 데이터</param>
        public void GenerateFromData(BoardData data)
        {
            if (!ValidateBoardData(data))
            {
                return;
            }

            boardData = data;
            PrepareBoard();
            CreateAllTiles();
            FinalizeBoard();
        }
        #endregion

        #region 공개 메서드 - 타일 조회
        /// <summary>
        /// 지정된 위치의 타일을 가져옵니다.
        /// </summary>
        /// <param name="position">타일 위치</param>
        /// <returns>타일 또는 null</returns>
        public Tile GetTile(Vector2Int position)
        {
            if (!CanGetTile(position))
            {
                return null;
            }

            return tiles[position.x, position.y];
        }
        #endregion

        #region 공개 메서드 - 정리
        /// <summary>
        /// 보드에서 생성된 모든 타일을 제거합니다.
        /// </summary>
        public void ClearBoard()
        {
            DestroyAllTiles();
            ResetState();
        }
        #endregion

        #region 공개 메서드 - 하이라이트
        /// <summary>
        /// 지정된 위치의 타일들을 하이라이트합니다.
        /// </summary>
        /// <param name="positions">타일 위치 배열</param>
        /// <param name="highlight">하이라이트 여부</param>
        public void HighlightTiles(Vector2Int[] positions, bool highlight)
        {
            if (!CanHighlight(positions))
            {
                return;
            }

            foreach (var position in positions)
            {
                HighlightSingleTile(position, highlight);
            }
        }

        /// <summary>
        /// 모든 타일 하이라이트를 제거합니다.
        /// </summary>
        public void ClearHighlights()
        {
            if (!isGenerated || tiles == null)
            {
                return;
            }

            ClearAllTileHighlights();
        }
        #endregion

        #region 공개 메서드 - 위치 변환
        /// <summary>
        /// 지정된 보드 위치의 월드 위치를 가져옵니다.
        /// </summary>
        /// <param name="position">보드 위치</param>
        /// <returns>월드 위치</returns>
        public Vector3 GetTileWorldPosition(Vector2Int position)
        {
            Tile tile = GetTile(position);
            return tile != null ? tile.transform.position : Vector3.zero;
        }

        /// <summary>
        /// 월드 위치로부터 보드 위치를 가져옵니다.
        /// </summary>
        /// <param name="worldPosition">월드 위치</param>
        /// <returns>보드 위치 (유효하지 않으면 (-1, -1))</returns>
        public Vector2Int GetBoardPosition(Vector3 worldPosition)
        {
            if (!CanConvertPosition())
            {
                return new Vector2Int(-1, -1);
            }

            return CalculateBoardPosition(worldPosition);
        }
        #endregion

        #region 비공개 메서드 - 검증
        /// <summary>
        /// 보드 데이터가 유효한지 검증합니다.
        /// </summary>
        private bool ValidateBoardData(BoardData data)
        {
            if (data == null)
            {
                Debug.LogError("BoardData가 null입니다. 보드를 생성할 수 없습니다.");
                return false;
            }

            if (!data.Validate())
            {
                Debug.LogError("BoardData 검증에 실패했습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 자동 생성 조건을 확인합니다.
        /// </summary>
        private bool ShouldAutoGenerate()
        {
            return boardData != null && !isGenerated;
        }

        /// <summary>
        /// 타일을 가져올 수 있는지 확인합니다.
        /// </summary>
        private bool CanGetTile(Vector2Int position)
        {
            if (!isGenerated || tiles == null)
            {
                return false;
            }

            return IsPositionValid(position);
        }

        /// <summary>
        /// 위치가 유효한지 확인합니다.
        /// </summary>
        private bool IsPositionValid(Vector2Int position)
        {
            return position.x >= 0 && position.x < boardData.Width &&
                   position.y >= 0 && position.y < boardData.Height;
        }

        /// <summary>
        /// 하이라이트할 수 있는지 확인합니다.
        /// </summary>
        private bool CanHighlight(Vector2Int[] positions)
        {
            return isGenerated && positions != null;
        }

        /// <summary>
        /// 위치 변환이 가능한지 확인합니다.
        /// </summary>
        private bool CanConvertPosition()
        {
            return isGenerated && boardContainer != null;
        }
        #endregion

        #region 비공개 메서드 - 보드 준비
        /// <summary>
        /// 보드 생성을 준비합니다.
        /// </summary>
        private void PrepareBoard()
        {
            ClearBoard();
            InitializeChessBoard();
            EnsureBoardContainer();
            InitializeTileArray();
        }

        /// <summary>
        /// 체스 보드를 초기화합니다.
        /// </summary>
        private void InitializeChessBoard()
        {
            if (chessBoard != null)
            {
                chessBoard.Initialize(boardData.Width, boardData.Height);
            }
        }

        /// <summary>
        /// 보드 컨테이너가 존재하는지 확인합니다.
        /// </summary>
        private void EnsureBoardContainer()
        {
            if (boardContainer == null)
            {
                CreateBoardContainer();
            }
        }

        /// <summary>
        /// 보드 컨테이너를 생성합니다.
        /// </summary>
        private void CreateBoardContainer()
        {
            boardContainer = new GameObject("BoardContainer").transform;
            boardContainer.SetParent(transform);
            boardContainer.localPosition = Vector3.zero;
        }

        /// <summary>
        /// 타일 배열을 초기화합니다.
        /// </summary>
        private void InitializeTileArray()
        {
            tiles = new Tile[boardData.Width, boardData.Height];
        }
        #endregion

        #region 비공개 메서드 - 타일 생성
        /// <summary>
        /// 모든 타일을 생성합니다.
        /// </summary>
        private void CreateAllTiles()
        {
            for (int y = 0; y < boardData.Height; y++)
            {
                for (int x = 0; x < boardData.Width; x++)
                {
                    CreateSingleTile(x, y);
                }
            }
        }

        /// <summary>
        /// 단일 타일을 생성합니다.
        /// </summary>
        private void CreateSingleTile(int x, int y)
        {
            Vector2Int position = new Vector2Int(x, y);
            TileType tileType = boardData.GetTileType(x, y);

            GameObject tileObject = CreateTileGameObject(x, y);
            PositionTile(tileObject, x, y);
            ConfigureTileComponent(tileObject, position, tileType);
        }

        /// <summary>
        /// 타일 GameObject를 생성합니다.
        /// </summary>
        private GameObject CreateTileGameObject(int x, int y)
        {
            if (tilePrefab != null)
            {
                return Instantiate(tilePrefab, boardContainer);
            }

            return CreateBasicTile(x, y);
        }

        /// <summary>
        /// 기본 타일을 생성합니다.
        /// </summary>
        private GameObject CreateBasicTile(int x, int y)
        {
            GameObject tileObject = new GameObject($"Tile_{x}_{y}");
            tileObject.transform.SetParent(boardContainer);

            AddSpriteRenderer(tileObject);
            AddCollider(tileObject);

            return tileObject;
        }

        /// <summary>
        /// SpriteRenderer를 추가합니다.
        /// </summary>
        private void AddSpriteRenderer(GameObject tileObject)
        {
            var spriteRenderer = tileObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite();
            spriteRenderer.sortingOrder = -1;
        }

        /// <summary>
        /// Collider를 추가합니다.
        /// </summary>
        private void AddCollider(GameObject tileObject)
        {
            var collider = tileObject.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one * boardData.TileSize;
        }

        /// <summary>
        /// 정사각형 스프라이트를 생성합니다.
        /// </summary>
        private Sprite CreateSquareSprite()
        {
            Texture2D texture = CreateWhiteTexture();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        /// <summary>
        /// 흰색 텍스처를 생성합니다.
        /// </summary>
        private Texture2D CreateWhiteTexture()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 타일을 배치합니다.
        /// </summary>
        private void PositionTile(GameObject tileObject, int x, int y)
        {
            float totalTileSize = boardData.TileSize + boardData.TileSpacing;
            Vector3 worldPosition = new Vector3(x * totalTileSize, y * totalTileSize, 0);
            tileObject.transform.localPosition = worldPosition;
            tileObject.transform.localScale = Vector3.one * boardData.TileSize;
        }

        /// <summary>
        /// 타일 컴포넌트를 구성합니다.
        /// </summary>
        private void ConfigureTileComponent(GameObject tileObject, Vector2Int position, TileType tileType)
        {
            Tile tile = EnsureTileComponent(tileObject);
            tile.Initialize(position, tileType);
            tiles[position.x, position.y] = tile;

            UpdateChessBoardObstacle(position, tileType);
        }

        /// <summary>
        /// Tile 컴포넌트가 있는지 확인합니다.
        /// </summary>
        private Tile EnsureTileComponent(GameObject tileObject)
        {
            Tile tile = tileObject.GetComponent<Tile>();
            if (tile == null)
            {
                tile = tileObject.AddComponent<Tile>();
            }
            return tile;
        }

        /// <summary>
        /// 체스 보드의 장애물을 업데이트합니다.
        /// </summary>
        private void UpdateChessBoardObstacle(Vector2Int position, TileType tileType)
        {
            if (chessBoard != null && tileType == TileType.Obstacle)
            {
                chessBoard.SetObstacle(position, true);
            }
        }
        #endregion

        #region 비공개 메서드 - 보드 마무리
        /// <summary>
        /// 보드 생성을 마무리합니다.
        /// </summary>
        private void FinalizeBoard()
        {
            CenterBoard();
            isGenerated = true;
        }

        /// <summary>
        /// 보드를 중앙에 배치합니다.
        /// </summary>
        private void CenterBoard()
        {
            if (boardContainer == null)
            {
                return;
            }

            Vector3 offset = CalculateCenterOffset();
            boardContainer.localPosition = offset;
        }

        /// <summary>
        /// 중앙 오프셋을 계산합니다.
        /// </summary>
        private Vector3 CalculateCenterOffset()
        {
            float totalTileSize = boardData.TileSize + boardData.TileSpacing;
            float boardWidth = boardData.Width * totalTileSize - boardData.TileSpacing;
            float boardHeight = boardData.Height * totalTileSize - boardData.TileSpacing;

            return new Vector3(-boardWidth / 2f, -boardHeight / 2f, 0);
        }
        #endregion

        #region 비공개 메서드 - 정리
        /// <summary>
        /// 모든 타일을 파괴합니다.
        /// </summary>
        private void DestroyAllTiles()
        {
            if (boardContainer == null)
            {
                return;
            }

            for (int i = boardContainer.childCount - 1; i >= 0; i--)
            {
                DestroyChildTile(i);
            }
        }

        /// <summary>
        /// 자식 타일을 파괴합니다.
        /// </summary>
        private void DestroyChildTile(int index)
        {
            GameObject child = boardContainer.GetChild(index).gameObject;
            
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }

        /// <summary>
        /// 상태를 리셋합니다.
        /// </summary>
        private void ResetState()
        {
            tiles = null;
            isGenerated = false;
        }
        #endregion

        #region 비공개 메서드 - 하이라이트
        /// <summary>
        /// 단일 타일을 하이라이트합니다.
        /// </summary>
        private void HighlightSingleTile(Vector2Int position, bool highlight)
        {
            Tile tile = GetTile(position);
            if (tile != null)
            {
                tile.Highlight(highlight);
            }
        }

        /// <summary>
        /// 모든 타일의 하이라이트를 제거합니다.
        /// </summary>
        private void ClearAllTileHighlights()
        {
            for (int y = 0; y < boardData.Height; y++)
            {
                for (int x = 0; x < boardData.Width; x++)
                {
                    ClearTileHighlight(x, y);
                }
            }
        }

        /// <summary>
        /// 타일 하이라이트를 제거합니다.
        /// </summary>
        private void ClearTileHighlight(int x, int y)
        {
            if (tiles[x, y] != null)
            {
                tiles[x, y].Highlight(false);
            }
        }
        #endregion

        #region 비공개 메서드 - 위치 계산
        /// <summary>
        /// 보드 위치를 계산합니다.
        /// </summary>
        private Vector2Int CalculateBoardPosition(Vector3 worldPosition)
        {
            Vector3 localPosition = boardContainer.InverseTransformPoint(worldPosition);
            float totalTileSize = boardData.TileSize + boardData.TileSpacing;

            int x = Mathf.RoundToInt(localPosition.x / totalTileSize);
            int y = Mathf.RoundToInt(localPosition.y / totalTileSize);

            return IsCalculatedPositionValid(x, y) ? new Vector2Int(x, y) : new Vector2Int(-1, -1);
        }

        /// <summary>
        /// 계산된 위치가 유효한지 확인합니다.
        /// </summary>
        private bool IsCalculatedPositionValid(int x, int y)
        {
            return x >= 0 && x < boardData.Width && y >= 0 && y < boardData.Height;
        }
        #endregion

        #region 디버깅
        /// <summary>
        /// 디버깅을 위한 보드의 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            if (boardData == null)
            {
                return "BoardGenerator (데이터 없음)";
            }

            return $"BoardGenerator - {boardData} - 생성됨: {isGenerated}";
        }
        #endregion
    }
}
