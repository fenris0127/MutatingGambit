using UnityEngine;

namespace MutatingGambit.Systems.BoardSystem
{
    /// <summary>
    /// 보드 구성 데이터를 저장하는 ScriptableObject입니다.
    /// 체스 보드의 크기, 레이아웃, 타일 타입을 정의합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BoardData", menuName = "Mutating Gambit/Board Data")]
    public class BoardData : ScriptableObject
    {
        #region 필드
        [Header("보드 크기")]
        [SerializeField]
        [Tooltip("보드의 너비 (파일 수).")]
        private int width = 8;

        [SerializeField]
        [Tooltip("보드의 높이 (랭크 수).")]
        private int height = 8;

        [Header("타일 구성")]
        [SerializeField]
        [Tooltip("타일 타입의 2D 배열. null이거나 비어있으면 모든 타일이 Normal입니다.")]
        private TileTypeData[] tiles;

        [Header("비주얼 설정")]
        [SerializeField]
        [Tooltip("월드 단위로 표현된 각 타일의 크기.")]
        private float tileSize = 1.0f;

        [SerializeField]
        [Tooltip("타일 사이의 간격.")]
        private float tileSpacing = 0.1f;
        #endregion

        #region 공개 속성
        /// <summary>보드의 너비를 가져옵니다.</summary>
        public int Width => width;

        /// <summary>보드의 높이를 가져옵니다.</summary>
        public int Height => height;

        /// <summary>월드 단위로 표현된 각 타일의 크기를 가져옵니다.</summary>
        public float TileSize => tileSize;

        /// <summary>타일 사이의 간격을 가져옵니다.</summary>
        public float TileSpacing => tileSpacing;
        #endregion

        #region 공개 메서드 - 타일 타입 조회
        /// <summary>
        /// 지정된 위치의 타일 타입을 가져옵니다.
        /// </summary>
        public TileType GetTileType(int x, int y)
        {
            if (!IsPositionValid(x, y))
            {
                return TileType.Void;
            }

            return GetTileTypeInternal(x, y);
        }

        /// <summary>
        /// 지정된 위치의 타일 타입을 가져옵니다.
        /// </summary>
        public TileType GetTileType(Vector2Int position)
        {
            return GetTileType(position.x, position.y);
        }
        #endregion

        #region 공개 메서드 - 타일 타입 설정
        /// <summary>
        /// 지정된 위치의 타일 타입을 설정합니다.
        /// </summary>
        public void SetTileType(int x, int y, TileType type)
        {
            if (!IsPositionValid(x, y))
            {
                return;
            }

            EnsureTilesInitialized();
            SetTileTypeInternal(x, y, type);
        }

        /// <summary>
        /// 지정된 위치의 타일 타입을 설정합니다.
        /// </summary>
        public void SetTileType(Vector2Int position, TileType type)
        {
            SetTileType(position.x, position.y, type);
        }
        #endregion

        #region 공개 메서드 - 초기화 및 크기 조정
        /// <summary>
        /// 타일 배열을 모두 Normal 타일로 초기화합니다.
        /// </summary>
        public void InitializeTiles()
        {
            tiles = new TileTypeData[width * height];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new TileTypeData { type = TileType.Normal };
            }
        }

        /// <summary>
        /// 보드를 지정된 크기로 조정합니다.
        /// 가능한 경우 기존 타일 데이터를 보존합니다.
        /// </summary>
        public void Resize(int newWidth, int newHeight)
        {
            if (!ValidateDimensions(newWidth, newHeight))
            {
                return;
            }

            PreserveAndResizeTiles(newWidth, newHeight);
        }
        #endregion

        #region 공개 메서드 - 검증
        /// <summary>
        /// 보드 데이터의 일관성을 검증합니다.
        /// </summary>
        public bool Validate()
        {
            if (!ValidateDimensions(width, height))
            {
                return false;
            }

            ValidateTilesArray();
            return true;
        }
        #endregion

        #region 비공개 메서드 - 위치 검증
        /// <summary>
        /// 위치가 유효한지 확인합니다.
        /// </summary>
        private bool IsPositionValid(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// 크기가 유효한지 확인합니다.
        /// </summary>
        private bool ValidateDimensions(int w, int h)
        {
            if (w <= 0 || h <= 0)
            {
                Debug.LogError($"보드 크기는 양수여야 합니다: {w}x{h}");
                return false;
            }
            return true;
        }
        #endregion

        #region 비공개 메서드 - 타일 배열 관리
        /// <summary>
        /// 내부적으로 타일 타입을 가져옵니다.
        /// </summary>
        private TileType GetTileTypeInternal(int x, int y)
        {
            if (tiles == null || tiles.Length == 0)
            {
                return TileType.Normal;
            }

            int index = CalculateIndex(x, y);
            return IsIndexValid(index) ? tiles[index].type : TileType.Normal;
        }

        /// <summary>
        /// 내부적으로 타일 타입을 설정합니다.
        /// </summary>
        private void SetTileTypeInternal(int x, int y, TileType type)
        {
            int index = CalculateIndex(x, y);
            if (IsIndexValid(index))
            {
                tiles[index].type = type;
            }
        }

        /// <summary>
        /// 타일 배열이 초기화되었는지 확인하고 필요시 초기화합니다.
        /// </summary>
        private void EnsureTilesInitialized()
        {
            if (tiles == null || tiles.Length != width * height)
            {
                InitializeTiles();
            }
        }

        /// <summary>
        /// 인덱스를 계산합니다.
        /// </summary>
        private int CalculateIndex(int x, int y)
        {
            return y * width + x;
        }

        /// <summary>
        /// 인덱스가 유효한지 확인합니다.
        /// </summary>
        private bool IsIndexValid(int index)
        {
            return index >= 0 && index < tiles.Length;
        }
        #endregion

        #region 비공개 메서드 - 크기 조정
        /// <summary>
        /// 기존 타일을 보존하면서 크기를 조정합니다.
        /// </summary>
        private void PreserveAndResizeTiles(int newWidth, int newHeight)
        {
            var oldTiles = tiles;
            int oldWidth = width;
            int oldHeight = height;

            width = newWidth;
            height = newHeight;
            InitializeTiles();

            CopyOldTiles(oldTiles, oldWidth, oldHeight);
        }

        /// <summary>
        /// 이전 타일 데이터를 복사합니다.
        /// </summary>
        private void CopyOldTiles(TileTypeData[] oldTiles, int oldWidth, int oldHeight)
        {
            if (oldTiles == null) return;

            for (int y = 0; y < Mathf.Min(oldHeight, height); y++)
            {
                for (int x = 0; x < Mathf.Min(oldWidth, width); x++)
                {
                    CopySingleTile(oldTiles, oldWidth, x, y);
                }
            }
        }

        /// <summary>
        /// 단일 타일을 복사합니다.
        /// </summary>
        private void CopySingleTile(TileTypeData[] oldTiles, int oldWidth, int x, int y)
        {
            int oldIndex = y * oldWidth + x;
            int newIndex = y * width + x;

            if (oldIndex < oldTiles.Length && newIndex < tiles.Length)
            {
                tiles[newIndex] = oldTiles[oldIndex];
            }
        }
        #endregion

        #region 비공개 메서드 - 배열 검증
        /// <summary>
        /// 타일 배열을 검증합니다.
        /// </summary>
        private void ValidateTilesArray()
        {
            if (tiles != null && tiles.Length != width * height)
            {
                Debug.LogWarning($"타일 배열 크기 ({tiles.Length})가 보드 크기 ({width * height})와 일치하지 않습니다. 재초기화 중...");
                InitializeTiles();
            }
        }
        #endregion

        #region Unity 콜백
        /// <summary>
        /// 에디터에서 에셋이 로드될 때 호출되는 헬퍼 메서드입니다.
        /// </summary>
        private void OnValidate()
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            if (tiles != null && tiles.Length != width * height)
            {
                Debug.LogWarning($"보드 '{name}': 타일 배열 크기 불일치. 예상: {width * height}, 실제: {tiles.Length}.");
            }
        }
        #endregion

        #region 디버깅
        /// <summary>
        /// 디버깅을 위한 보드의 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return $"BoardData '{name}' ({width}x{height})";
        }
        #endregion
    }

    /// <summary>
    /// 타일 타입을 저장하기 위한 직렬화 가능한 데이터 구조입니다.
    /// </summary>
    [System.Serializable]
    public struct TileTypeData
    {
        public TileType type;
    }
}
