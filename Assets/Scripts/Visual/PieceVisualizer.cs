using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Visual
{
    /// <summary>
    /// 체스 기물을 위한 간단한 시각적 표현을 생성합니다.
    /// 프로토타입용 - 각 기물 타입을 기본 도형으로 표현합니다.
    /// 리팩토링: Region 그룹화, 함수 분해(10줄), 한국어 문서화, 도형 생성 로직 분리
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PieceVisualizer : MonoBehaviour
    {
        #region 컴포넌트 참조
        [Header("References")]
        [SerializeField]
        private Piece piece;

        [SerializeField]
        private SpriteRenderer spriteRenderer;
        #endregion

        #region 시각 설정
        [Header("Visual Settings")]
        [SerializeField]
        private float pieceSize = 0.8f;

        [SerializeField]
        private Color whiteColor = new Color(0.9f, 0.9f, 0.9f);

        [SerializeField]
        private Color blackColor = new Color(0.2f, 0.2f, 0.2f);
        #endregion

        #region 스프라이트 참조 (선택사항)
        [Header("Sprite References (Optional)")]
        [SerializeField] private Sprite kingSprite;
        [SerializeField] private Sprite queenSprite;
        [SerializeField] private Sprite rookSprite;
        [SerializeField] private Sprite bishopSprite;
        [SerializeField] private Sprite knightSprite;
        [SerializeField] private Sprite pawnSprite;
        #endregion

        #region 헬퍼
        private ProceduralSpriteGenerator spriteGenerator;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 컴포넌트 참조를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            EnsurePieceReference();
            EnsureSpriteRenderer();
            CreateSpriteGenerator();
        }

        /// <summary>
        /// 초기 시각적 표현을 업데이트합니다.
        /// </summary>
        private void Start()
        {
            UpdateVisual();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 기물 타입과 팀에 따라 시각적 표현을 업데이트합니다.
        /// </summary>
        public void UpdateVisual()
        {
            if (!ValidateComponents())
            {
                return;
            }

            UpdateSprite();
            UpdateColor();
            UpdateScale();
            UpdateSortingOrder();
        }

        /// <summary>
        /// 기물 위에 텍스트 라벨을 추가합니다 (디버깅용).
        /// </summary>
        public void AddLabel()
        {
            GameObject labelObject = CreateLabelObject();
            TextMesh textMesh = ConfigureLabelText(labelObject);
            SetLabelContent(textMesh);
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// Piece 컴포넌트 참조를 확보합니다.
        /// </summary>
        private void EnsurePieceReference()
        {
            if (piece == null)
            {
                piece = GetComponent<Piece>();
            }
        }

        /// <summary>
        /// SpriteRenderer 컴포넌트 참조를 확보합니다.
        /// </summary>
        private void EnsureSpriteRenderer()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// 스프라이트 생성기를 생성합니다.
        /// </summary>
        private void CreateSpriteGenerator()
        {
            spriteGenerator = new ProceduralSpriteGenerator();
        }
        #endregion

        #region 비공개 메서드 - 검증
        /// <summary>
        /// 필수 컴포넌트들이 있는지 확인합니다.
        /// </summary>
        private bool ValidateComponents()
        {
            return piece != null && spriteRenderer != null;
        }
        #endregion

        #region 비공개 메서드 - 시각 업데이트
        /// <summary>
        /// 스프라이트를 업데이트합니다.
        /// </summary>
        private void UpdateSprite()
        {
            Sprite selectedSprite = GetSpriteForPieceType(piece.Type);
            
            if (selectedSprite != null)
            {
                UseCustomSprite(selectedSprite);
            }
            else
            {
                GenerateProceduralSprite();
            }
        }

        /// <summary>
        /// 커스텀 스프라이트를 사용합니다.
        /// </summary>
        private void UseCustomSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// 프로시저럴 스프라이트를 생성합니다.
        /// </summary>
        private void GenerateProceduralSprite()
        {
            spriteRenderer.sprite = spriteGenerator.GenerateSprite(piece.Type);
        }

        /// <summary>
        /// 팀에 따라 색상을 업데이트합니다.
        /// </summary>
        private void UpdateColor()
        {
            Color teamColor = GetColorForTeam();
            spriteRenderer.color = teamColor;
        }

        /// <summary>
        /// 팀에 해당하는 색상을 가져옵니다.
        /// </summary>
        private Color GetColorForTeam()
        {
            return piece.Team == Team.White ? whiteColor : blackColor;
        }

        /// <summary>
        /// 크기를 업데이트합니다.
        /// </summary>
        private void UpdateScale()
        {
            transform.localScale = Vector3.one * pieceSize;
        }

        /// <summary>
        /// 정렬 순서를 업데이트합니다 (보드 위에 표시).
        /// </summary>
        private void UpdateSortingOrder()
        {
            spriteRenderer.sortingOrder = 10;
        }
        #endregion

        #region 비공개 메서드 - 스프라이트 선택
        /// <summary>
        /// 기물 타입에 해당하는 스프라이트를 가져옵니다.
        /// </summary>
        /// <param name="type">기물 타입</param>
        /// <returns>해당 스프라이트 (없으면 null)</returns>
        private Sprite GetSpriteForPieceType(PieceType type)
        {
            return type switch
            {
                PieceType.King => kingSprite,
                PieceType.Queen => queenSprite,
                PieceType.Rook => rookSprite,
                PieceType.Bishop => bishopSprite,
                PieceType.Knight => knightSprite,
                PieceType.Pawn => pawnSprite,
                _ => null
            };
        }
        #endregion

        #region 비공개 메서드 - 라벨 생성
        /// <summary>
        /// 라벨 GameObject를 생성합니다.
        /// </summary>
        private GameObject CreateLabelObject()
        {
            GameObject labelObject = new GameObject("Label");
            labelObject.transform.SetParent(transform);
            labelObject.transform.localPosition = new Vector3(0, 0.6f, 0);
            return labelObject;
        }

        /// <summary>
        /// 라벨 텍스트를 설정합니다.
        /// </summary>
        private TextMesh ConfigureLabelText(GameObject labelObject)
        {
            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.fontSize = 36;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            return textMesh;
        }

        /// <summary>
        /// 라벨 내용을 설정합니다.
        /// </summary>
        private void SetLabelContent(TextMesh textMesh)
        {
            textMesh.text = GetPieceSymbol(piece.Type);
            textMesh.color = GetLabelColor();
        }

        /// <summary>
        /// 라벨 색상을 가져옵니다.
        /// </summary>
        private Color GetLabelColor()
        {
            return piece.Team == Team.White ? Color.black : Color.white;
        }
        #endregion

        #region 비공개 메서드 - 유틸리티
        /// <summary>
        /// 기물 타입에 해당하는 체스 심볼을 가져옵니다.
        /// </summary>
        /// <param name="type">기물 타입</param>
        /// <returns>체스 심볼 문자</returns>
        private string GetPieceSymbol(PieceType type)
        {
            return type switch
            {
                PieceType.King => "♔",
                PieceType.Queen => "♕",
                PieceType.Rook => "♖",
                PieceType.Bishop => "♗",
                PieceType.Knight => "♘",
                PieceType.Pawn => "♙",
                _ => "?"
            };
        }
        #endregion
    }
}
