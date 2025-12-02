using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.BoardSystem
{
    /// <summary>
    /// 체스 보드의 단일 타일을 나타냅니다.
    /// 위치, 타입, 시각적 표현을 포함합니다.
    /// </summary>
    public class Tile : MonoBehaviour
    {
        #region 필드
        [SerializeField]
        private Vector2Int position;

        [SerializeField]
        private TileType tileType = TileType.Normal;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Color normalColor = new Color(0.9f, 0.9f, 0.9f);

        [SerializeField]
        private Color alternateColor = new Color(0.6f, 0.6f, 0.6f);

        [SerializeField]
        private Color obstacleColor = new Color(0.3f, 0.2f, 0.2f);

        [SerializeField]
        private Color waterColor = new Color(0.4f, 0.6f, 0.9f);

        [SerializeField]
        private Color highlightColor = new Color(0.9f, 0.9f, 0.3f, 0.5f);

        private bool isHighlighted = false;
        private Piece occupyingPiece;
        #endregion

        #region 공개 속성
        /// <summary>보드에서 이 타일의 위치를 가져옵니다.</summary>
        public Vector2Int Position => position;

        /// <summary>이 타일의 타입을 가져오거나 설정합니다.</summary>
        public TileType Type
        {
            get => tileType;
            set
            {
                tileType = value;
                UpdateVisuals();
            }
        }

        /// <summary>현재 이 타일을 점유하고 있는 기물을 가져오거나 설정합니다.</summary>
        public Piece OccupyingPiece
        {
            get => occupyingPiece;
            set => occupyingPiece = value;
        }

        /// <summary>이 타일이 이동 가능한지 확인합니다 (장애물이나 공허가 아님).</summary>
        public bool IsWalkable => tileType == TileType.Normal || tileType == TileType.Water;

        /// <summary>이 타일이 기물로 점유되어 있는지 확인합니다.</summary>
        public bool IsOccupied => occupyingPiece != null;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// SpriteRenderer를 가져옵니다.
        /// </summary>
        private void Awake()
        {
            EnsureSpriteRenderer();
        }
        #endregion

        #region 공개 메서드 - 초기화
        /// <summary>
        /// 타일을 위치와 타입으로 초기화합니다.
        /// </summary>
        /// <param name="tilePosition">타일 위치</param>
        /// <param name="type">타일 타입</param>
        public void Initialize(Vector2Int tilePosition, TileType type)
        {
            position = tilePosition;
            tileType = type;
            EnsureSpriteRenderer();
            UpdateVisuals();
        }
        #endregion

        #region 공개 메서드 - 시각화
        /// <summary>
        /// 타일의 타입에 따라 시각적 외관을 업데이트합니다.
        /// </summary>
        public void UpdateVisuals()
        {
            if (!ValidateSpriteRenderer())
            {
                return;
            }

            Color baseColor = CalculateBaseColor();
            ApplyColor(baseColor);
        }

        /// <summary>
        /// 이 타일을 하이라이트합니다 (예: 유효한 수 표시).
        /// </summary>
        /// <param name="highlight">하이라이트 여부</param>
        public void Highlight(bool highlight)
        {
            isHighlighted = highlight;
            UpdateVisuals();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// SpriteRenderer가 있는지 확인하고 가져옵니다.
        /// </summary>
        private void EnsureSpriteRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// SpriteRenderer가 유효한지 검증합니다.
        /// </summary>
        private bool ValidateSpriteRenderer()
        {
            return spriteRenderer != null;
        }
        #endregion

        #region 비공개 메서드 - 색상 계산
        /// <summary>
        /// 타일 타입에 기반한 기본 색상을 계산합니다.
        /// </summary>
        private Color CalculateBaseColor()
        {
            switch (tileType)
            {
                case TileType.Obstacle:
                    return obstacleColor;
                case TileType.Water:
                    return waterColor;
                case TileType.Void:
                    return Color.clear;
                case TileType.Normal:
                default:
                    return GetCheckerboardColor();
            }
        }

        /// <summary>
        /// 체커보드 패턴 색상을 가져옵니다.
        /// </summary>
        private Color GetCheckerboardColor()
        {
            bool isAlternate = (position.x + position.y) % 2 == 0;
            return isAlternate ? alternateColor : normalColor;
        }

        /// <summary>
        /// 계산된 색상을 SpriteRenderer에 적용합니다.
        /// </summary>
        private void ApplyColor(Color baseColor)
        {
            if (isHighlighted)
            {
                spriteRenderer.color = Color.Lerp(baseColor, highlightColor, 0.5f);
            }
            else
            {
                spriteRenderer.color = baseColor;
            }
        }
        #endregion

        #region 디버깅
        /// <summary>
        /// 디버깅을 위한 이 타일의 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            string occupiedStatus = IsOccupied ? $" (점유: {occupyingPiece})" : "";
            return $"타일 at {BoardPosition.ToNotation(position)} - {tileType}{occupiedStatus}";
        }
        #endregion
    }
}
