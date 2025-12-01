using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 휴식 방에서 부서진 기물을 수리하는 UI입니다.
    /// 리팩토링: SRP 준수, Region 그룹화, 함수 크기 제약(10줄), 한국어 문서화
    /// </summary>
    public class RepairUI : MonoBehaviour
    {
        #region UI 참조
        [Header("UI References")]
        [SerializeField]
        private GameObject repairPanel;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI repairsRemainingText;

        [SerializeField]
        private Transform brokenPieceContainer;

        [SerializeField]
        private GameObject brokenPieceSlotPrefab;

        [SerializeField]
        private Button confirmButton;
        #endregion

        #region 텍스트 설정
        [Header("Settings")]
        [SerializeField]
        private string titleFormat = "휴식 방 - 기물 수리";

        [SerializeField]
        private string repairsRemainingFormat = "남은 수리 횟수: {0}/{1}";

        [SerializeField]
        private string noBrokenPiecesMessage = "모든 기물이 완벽한 상태입니다!";

        [SerializeField]
        private int maxRepairsDisplayed = 2;
        #endregion

        #region 이벤트
        [Header("Events")]
        public UnityEvent OnRepairCompleted;
        public UnityEvent OnRepairCancelled;
        #endregion

        #region 상태 변수
        private RepairSystem repairSystem;
        private List<BrokenPieceSlot> activeSlots = new List<BrokenPieceSlot>();
        private BrokenPieceSlotFactory slotFactory;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 초기화 시 패널을 숨기고 버튼 리스너를 등록합니다.
        /// </summary>
        private void Awake()
        {
            InitializeSlotFactory();
            HideRepairPanel();
            RegisterConfirmButton();
        }
        #endregion

        #region 공개 메서드 - UI 표시/숨김
        /// <summary>
        /// 수리 UI를 표시합니다.
        /// </summary>
        /// <param name="system">수리 시스템</param>
        public void Show(RepairSystem system)
        {
            if (system == null)
            {
                Debug.LogWarning("수리 시스템이 null입니다.");
                return;
            }

            repairSystem = system;
            ShowRepairPanel();
            UpdateUI();
        }

        /// <summary>
        /// 수리 UI를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            HideRepairPanel();
            ClearAllSlots();
        }
        #endregion

        #region 공개 속성
        /// <summary>
        /// UI가 현재 표시 중인지 여부를 확인합니다.
        /// </summary>
        public bool IsVisible => repairPanel != null && repairPanel.activeSelf;
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 슬롯 팩토리를 초기화합니다.
        /// </summary>
        private void InitializeSlotFactory()
        {
            slotFactory = new BrokenPieceSlotFactory(brokenPieceSlotPrefab, brokenPieceContainer);
        }

        /// <summary>
        /// 확인 버튼을 등록합니다.
        /// </summary>
        private void RegisterConfirmButton()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }
        }
        #endregion

        #region 비공개 메서드 - UI 업데이트
        /// <summary>
        /// 전체 UI를 업데이트합니다.
        /// </summary>
        private void UpdateUI()
        {
            UpdateTitle();
            UpdateRepairsRemaining();
            DisplayAllBrokenPieces();
        }

        /// <summary>
        /// 제목을 업데이트합니다.
        /// </summary>
        private void UpdateTitle()
        {
            if (titleText != null)
            {
                titleText.text = titleFormat;
            }
        }

        /// <summary>
        /// 남은 수리 횟수 텍스트를 업데이트합니다.
        /// </summary>
        private void UpdateRepairsRemaining()
        {
            if (repairsRemainingText == null || repairSystem == null)
            {
                return;
            }

            int remaining = repairSystem.RepairsRemaining;
            string formattedText = FormatRepairsText(remaining);
            repairsRemainingText.text = formattedText;
        }

        /// <summary>
        /// 수리 횟수 텍스트를 포맷팅합니다.
        /// </summary>
        private string FormatRepairsText(int remaining)
        {
            return string.Format(repairsRemainingFormat, remaining, maxRepairsDisplayed);
        }

        /// <summary>
        /// 모든 부서진 기물을 표시합니다.
        /// </summary>
        private void DisplayAllBrokenPieces()
        {
            ClearAllSlots();

            if (!ValidateDisplayData())
            {
                return;
            }

            var brokenPieces = repairSystem.BrokenPieces;
            DisplayPiecesOrMessage(brokenPieces);
        }

        /// <summary>
        /// 표시 데이터의 유효성을 검증합니다.
        /// </summary>
        private bool ValidateDisplayData()
        {
            if (repairSystem == null || brokenPieceContainer == null)
            {
                Debug.LogWarning("수리 시스템 또는 컨테이너가 null입니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 기물을 표시하거나 메시지를 표시합니다.
        /// </summary>
        private void DisplayPiecesOrMessage(List<PieceHealth> brokenPieces)
        {
            if (brokenPieces.Count > 0)
            {
                DisplayBrokenPieceSlots(brokenPieces);
            }
            else
            {
                DisplayNoBrokenPiecesMessage();
            }
        }

        /// <summary>
        /// 부서진 기물 슬롯들을 표시합니다.
        /// </summary>
        private void DisplayBrokenPieceSlots(List<PieceHealth> brokenPieces)
        {
            foreach (var pieceHealth in brokenPieces)
            {
                CreateAndRegisterSlot(pieceHealth);
            }
        }

        /// <summary>
        /// 슬롯을 생성하고 등록합니다.
        /// </summary>
        private void CreateAndRegisterSlot(PieceHealth pieceHealth)
        {
            bool canRepair = repairSystem.CanRepair;
            BrokenPieceSlot slot = slotFactory.CreateSlot(pieceHealth, canRepair);
            
            RegisterSlotEvents(slot, pieceHealth);
            activeSlots.Add(slot);
        }

        /// <summary>
        /// 슬롯의 이벤트를 등록합니다.
        /// </summary>
        private void RegisterSlotEvents(BrokenPieceSlot slot, PieceHealth pieceHealth)
        {
            slot.OnRepairRequested += () => HandleRepairRequest(pieceHealth);
        }

        /// <summary>
        /// 부서진 기물이 없을 때 메시지를 표시합니다.
        /// </summary>
        private void DisplayNoBrokenPiecesMessage()
        {
            slotFactory.CreateMessageObject(noBrokenPiecesMessage);
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 수리 요청을 처리합니다.
        /// </summary>
        private void HandleRepairRequest(PieceHealth pieceHealth)
        {
            if (repairSystem == null)
            {
                Debug.LogWarning("수리 시스템이 없습니다.");
                return;
            }

            AttemptRepair(pieceHealth);
        }

        /// <summary>
        /// 수리를 시도합니다.
        /// </summary>
        private void AttemptRepair(PieceHealth pieceHealth)
        {
            bool success = repairSystem.RepairPiece(pieceHealth);

            if (success)
            {
                RefreshAfterRepair();
            }
        }

        /// <summary>
        /// 수리 후 UI를 새로고침합니다.
        /// </summary>
        private void RefreshAfterRepair()
        {
            UpdateUI();
        }

        /// <summary>
        /// 확인 버튼 클릭 시 호출됩니다.
        /// </summary>
        private void OnConfirmClicked()
        {
            NotifyRepairCompleted();
            Hide();
        }

        /// <summary>
        /// 수리 완료를 외부에 알립니다.
        /// </summary>
        private void NotifyRepairCompleted()
        {
            OnRepairCompleted?.Invoke();
        }
        #endregion

        #region 비공개 메서드 - UI 정리
        /// <summary>
        /// 모든 슬롯을 제거합니다.
        /// </summary>
        private void ClearAllSlots()
        {
            DestroyActiveSlots();
            ClearContainer();
        }

        /// <summary>
        /// 활성 슬롯들을 파괴합니다.
        /// </summary>
        private void DestroyActiveSlots()
        {
            foreach (var slot in activeSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            activeSlots.Clear();
        }

        /// <summary>
        /// 컨테이너의 모든 자식을 제거합니다.
        /// </summary>
        private void ClearContainer()
        {
            if (brokenPieceContainer == null)
            {
                return;
            }

            foreach (Transform child in brokenPieceContainer)
            {
                Destroy(child.gameObject);
            }
        }
        #endregion

        #region 비공개 메서드 - 패널 제어
        /// <summary>
        /// 수리 패널을 표시합니다.
        /// </summary>
        private void ShowRepairPanel()
        {
            if (repairPanel != null)
            {
                repairPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 수리 패널을 숨깁니다.
        /// </summary>
        private void HideRepairPanel()
        {
            if (repairPanel != null)
            {
                repairPanel.SetActive(false);
            }
        }
        #endregion
    }

    /// <summary>
    /// 부서진 기물 슬롯 UI 컴포넌트입니다.
    /// </summary>
    public class BrokenPieceSlot : MonoBehaviour
    {
        #region UI 참조
        [Header("UI References")]
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI costText;

        [SerializeField]
        private Button repairButton;

        [SerializeField]
        private GameObject repairedIndicator;
        #endregion

        #region 상태 변수
        private PieceHealth pieceHealth;
        private bool canRepair;

        /// <summary>
        /// 수리 요청 이벤트 콜백
        /// </summary>
        public System.Action OnRepairRequested;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 버튼 컴포넌트를 초기화하고 클릭 리스너를 등록합니다.
        /// </summary>
        private void Awake()
        {
            EnsureButtonComponent();
            RegisterClickListener();
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 슬롯을 기물 데이터로 초기화합니다.
        /// </summary>
        /// <param name="piece">기물 체력 정보</param>
        /// <param name="repairsAvailable">수리 가능 여부</param>
        public void Initialize(PieceHealth piece, bool repairsAvailable)
        {
            pieceHealth = piece;
            canRepair = DetermineCanRepair(repairsAvailable);

            UpdateAllDisplays();
        }
        #endregion

        #region 비공개 메서드 - 초기화
        /// <summary>
        /// 버튼 컴포넌트를 확보합니다.
        /// </summary>
        private void EnsureButtonComponent()
        {
            if (repairButton == null)
            {
                repairButton = GetComponentInChildren<Button>();
            }
        }

        /// <summary>
        /// 클릭 리스너를 등록합니다.
        /// </summary>
        private void RegisterClickListener()
        {
            if (repairButton != null)
            {
                repairButton.onClick.AddListener(HandleRepairClick);
            }
        }

        /// <summary>
        /// 수리 가능 여부를 결정합니다.
        /// </summary>
        private bool DetermineCanRepair(bool repairsAvailable)
        {
            return repairsAvailable && pieceHealth.CanBeRepaired;
        }
        #endregion

        #region 비공개 메서드 - UI 업데이트
        /// <summary>
        /// 모든 표시 요소를 업데이트합니다.
        /// </summary>
        private void UpdateAllDisplays()
        {
            if (!ValidatePieceData())
            {
                return;
            }

            UpdateNameDisplay();
            UpdateCostDisplay();
            UpdateButtonState();
            HideRepairedIndicator();
        }

        /// <summary>
        /// 기물 데이터의 유효성을 검증합니다.
        /// </summary>
        private bool ValidatePieceData()
        {
            return pieceHealth != null && pieceHealth.Piece != null;
        }

        /// <summary>
        /// 이름 표시를 업데이트합니다.
        /// </summary>
        private void UpdateNameDisplay()
        {
            if (nameText != null)
            {
                nameText.text = FormatPieceName();
            }
        }

        /// <summary>
        /// 기물 이름을 포맷팅합니다.
        /// </summary>
        private string FormatPieceName()
        {
            string teamColor = GetTeamColorName();
            return "{teamColor} {pieceHealth.Piece.Type}";
        }

        /// <summary>
        /// 팀 색상 이름을 가져옵니다.
        /// </summary>
        private string GetTeamColorName()
        {
            return pieceHealth.Piece.Team == Team.White ? "백색" : "흑색";
        }

        /// <summary>
        /// 비용 표시를 업데이트합니다.
        /// </summary>
        private void UpdateCostDisplay()
        {
            if (costText != null)
            {
                costText.text = FormatCostText();
            }
        }

        /// <summary>
        /// 비용 텍스트를 포맷팅합니다.
        /// </summary>
        private string FormatCostText()
        {
            return pieceHealth.RepairCost > 0
                ? "비용: {pieceHealth.RepairCost}"
                : "무료";
        }

        /// <summary>
        /// 버튼 상태를 업데이트합니다.
        /// </summary>
        private void UpdateButtonState()
        {
            if (repairButton != null)
            {
                repairButton.interactable = canRepair;
            }
        }

        /// <summary>
        /// 수리 완료 표시를 숨깁니다.
        /// </summary>
        private void HideRepairedIndicator()
        {
            if (repairedIndicator != null)
            {
                repairedIndicator.SetActive(false);
            }
        }
        #endregion

        #region 비공개 메서드 - 이벤트 핸들러
        /// <summary>
        /// 수리 버튼 클릭을 처리합니다.
        /// </summary>
        private void HandleRepairClick()
        {
            NotifyRepairRequested();
            ShowRepairedState();
            DisableButton();
        }

        /// <summary>
        /// 수리 요청을 외부에 알립니다.
        /// </summary>
        private void NotifyRepairRequested()
        {
            OnRepairRequested?.Invoke();
        }

        /// <summary>
        /// 수리 완료 상태를 표시합니다.
        /// </summary>
        private void ShowRepairedState()
        {
            if (repairedIndicator != null)
            {
                repairedIndicator.SetActive(true);
            }
        }

        /// <summary>
        /// 버튼을 비활성화합니다.
        /// </summary>
        private void DisableButton()
        {
            if (repairButton != null)
            {
                repairButton.interactable = false;
            }
        }
        #endregion
    }
}
