using UnityEngine;
using System.Collections.Generic;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    #region 열거형
    /// <summary>
    /// 아티팩트를 분류하고 시너지를 탐지하기 위한 태그입니다.
    /// </summary>
    public enum ArtifactTag
    {
        Global,      // 전체 보드에 영향
        Movement,    // 이동 수정
        Combat,      // 전투 효과
        Economic,    // 자원/화폐
        Defensive,   // 방어
        Aggressive,  // 공격력
        Chaos,       // 랜덤/예측 불가
        Temporal     // 시간/턴 효과
    }

    /// <summary>
    /// 아티팩트 효과가 발동되는 시점을 정의합니다.
    /// </summary>
    public enum ArtifactTrigger
    {
        OnTurnStart,      // 플레이어 턴 시작
        OnTurnEnd,        // 플레이어 턴 종료
        OnPieceMove,      // 기물 이동 후
        OnPieceCapture,   // 기물 포획 시
        OnKingMove,       // 킹 이동 시
        Passive           // 상시 활성 (규칙 수정)
    }

    /// <summary>
    /// 아티팩트의 희귀도 티어를 정의합니다.
    /// </summary>
    public enum ArtifactRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    #endregion

    #region 컨텍스트 클래스
    /// <summary>
    /// 아티팩트가 발동될 때 전달되는 컨텍스트 데이터입니다.
    /// </summary>
    public class ArtifactContext
    {
        #region 공개 속성
        /// <summary>이동한 기물</summary>
        public Piece MovedPiece { get; set; }
        
        /// <summary>시작 위치</summary>
        public Vector2Int FromPosition { get; set; }
        
        /// <summary>목표 위치</summary>
        public Vector2Int ToPosition { get; set; }
        
        /// <summary>포획된 기물</summary>
        public Piece CapturedPiece { get; set; }
        
        /// <summary>현재 팀</summary>
        public Team CurrentTeam { get; set; }
        
        /// <summary>턴 번호</summary>
        public int TurnNumber { get; set; }
        
        /// <summary>수리 시스템 참조 (선택적)</summary>
        public object RepairSystem { get; set; }
        #endregion

        #region 생성자
        /// <summary>
        /// 기본 생성자입니다.
        /// </summary>
        public ArtifactContext() { }

        /// <summary>
        /// 기물 이동 정보로 컨텍스트를 생성합니다.
        /// </summary>
        public ArtifactContext(Piece movedPiece, Vector2Int from, Vector2Int to)
        {
            MovedPiece = movedPiece;
            FromPosition = from;
            ToPosition = to;
        }
        #endregion
    }
    #endregion

    #region 아티팩트 베이스 클래스
    /// <summary>
    /// 전역 게임 수정 아티팩트의 추상 기반 클래스입니다.
    /// 아티팩트는 모든 기물에 영향을 미치고 게임 규칙을 전역적으로 수정합니다.
    /// </summary>
    public abstract class Artifact : ScriptableObject
    {
        #region 필드
        [Header("아티팩트 정보")]
        [SerializeField]
        private string artifactName;

        [SerializeField]
        [TextArea(2, 4)]
        private string description;

        [SerializeField]
        private Sprite icon;

        [Header("속성")]
        [SerializeField]
        [Tooltip("이 아티팩트 구매 비용.")]
        private int cost = 100;

        [SerializeField]
        [Tooltip("이 아티팩트의 효과 발동 시점.")]
        private ArtifactTrigger trigger = ArtifactTrigger.Passive;

        [SerializeField]
        [Tooltip("희귀도 티어 (높을수록 강력/비쌈).")]
        private ArtifactRarity rarity = ArtifactRarity.Common;

        [SerializeField]
        [Tooltip("이 아티팩트의 태그.")]
        private ArtifactTag[] tags = new ArtifactTag[0];

        [SerializeField]
        [Tooltip("이 아티팩트를 자신과 스택할 수 있는지 여부.")]
        private bool canStack = false;

        [SerializeField]
        [Tooltip("이 아티팩트와 시너지를 이루는 아티팩트들.")]
        private Artifact[] synergyArtifacts;
        #endregion

        #region 공개 속성
        /// <summary>이 아티팩트의 이름을 가져옵니다.</summary>
        public string ArtifactName => artifactName;

        /// <summary>이 아티팩트의 설명을 가져옵니다.</summary>
        public string Description => description;

        /// <summary>이 아티팩트의 아이콘을 가져옵니다.</summary>
        public Sprite Icon => icon;

        /// <summary>이 아티팩트를 획득하는 비용을 가져옵니다.</summary>
        public int Cost => cost;

        /// <summary>이 아티팩트가 발동되는 시점을 가져옵니다.</summary>
        public ArtifactTrigger Trigger => trigger;

        /// <summary>이 아티팩트의 희귀도 티어를 가져옵니다.</summary>
        public ArtifactRarity Rarity => rarity;

        /// <summary>이 아티팩트의 태그를 가져옵니다.</summary>
        public ArtifactTag[] Tags => tags;

        /// <summary>이 아티팩트를 자신과 스택할 수 있는지 여부.</summary>
        public bool CanStack => canStack;

        /// <summary>이 아티팩트와 시너지를 이루는 아티팩트들을 가져옵니다.</summary>
        public Artifact[] SynergyArtifacts => synergyArtifacts;
        #endregion

        #region 가상 메서드 - 생명주기
        /// <summary>
        /// 아티팩트를 처음 획득했을 때 호출됩니다.
        /// 일회성 설정 효과에 사용하세요.
        /// </summary>
        public virtual void OnAcquired(Board board)
        {
            // 기본: 아무것도 하지 않음
        }

        /// <summary>
        /// 아티팩트가 제거될 때 호출됩니다.
        /// 지속적인 효과를 정리하는 데 사용하세요.
        /// </summary>
        public virtual void OnRemoved(Board board)
        {
            // 기본: 아무것도 하지 않음
        }
        #endregion

        #region 추상 메서드
        /// <summary>
        /// 현재 컨텍스트를 기반으로 아티팩트의 효과를 적용합니다.
        /// 트리거 조건이 충족되면 ArtifactManager가 이를 호출합니다.
        /// </summary>
        /// <param name="board">현재 게임 보드</param>
        /// <param name="context">발동 이벤트에 대한 컨텍스트 정보</param>
        public abstract void ApplyEffect(Board board, ArtifactContext context);
        #endregion

        #region 가상 메서드 - 스택
        /// <summary>
        /// 이 아티팩트를 다른 아티팩트와 스택할 수 있는지 확인합니다.
        /// 기본적으로 같은 타입의 아티팩트는 스택되지 않습니다.
        /// </summary>
        public virtual bool CanStackWith(Artifact other)
        {
            return other.GetType() != this.GetType();
        }
        #endregion

        #region 디버깅
        /// <summary>
        /// 디버깅을 위한 문자열 표현을 반환합니다.
        /// </summary>
        public override string ToString()
        {
            return $"아티팩트: {artifactName} (발동: {trigger})";
        }
        #endregion
    }
    #endregion
}
