using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.Systems.PieceManagement
{
    /// <summary>
    /// 부서진 기물의 수리 시스템을 관리합니다.
    /// 부서진 기물을 추적하고 수리 로직을 처리합니다.
    /// </summary>
    public class RepairSystem : MonoBehaviour
    {
        #region 변수
        [Header("Settings")]
        [SerializeField]
        [Tooltip("휴식 방문 당 수리할 수 있는 최대 기물 수.")]
        private int maxRepairsPerRest = 2;

        [SerializeField]
        [Tooltip("기물 수리에 자원이 필요한지 여부.")]
        private bool usesRepairCost = false;

        [Header("State")]
        [SerializeField]
        private List<PieceHealth> brokenPieces = new List<PieceHealth>();

        [SerializeField]
        private List<PieceHealth> activePieces = new List<PieceHealth>();

        [SerializeField]
        private int repairsUsedThisRest = 0;

        [Header("Events")]
        public UnityEvent<PieceHealth> OnPieceBroken;
        public UnityEvent<PieceHealth> OnPieceRepaired;
        public UnityEvent<int> OnRepairsAvailableChanged;
        #endregion

        #region 속성
        /// <summary>
        /// 부서진 기물 목록을 가져옵니다.
        /// </summary>
        public List<PieceHealth> BrokenPieces => new List<PieceHealth>(brokenPieces);

        /// <summary>
        /// 활성 기물 목록을 가져옵니다.
        /// </summary>
        public List<PieceHealth> ActivePieces => new List<PieceHealth>(activePieces);

        /// <summary>
        /// 부서진 기물의 수를 가져옵니다.
        /// </summary>
        public int BrokenPieceCount => brokenPieces.Count;

        /// <summary>
        /// 이번 휴식에서 남은 수리 횟수를 가져옵니다.
        /// </summary>
        public int RepairsRemaining => Mathf.Max(0, maxRepairsPerRest - repairsUsedThisRest);

        /// <summary>
        /// 수리가 가능한지 여부를 가져옵니다.
        /// </summary>
        public bool CanRepair => RepairsRemaining > 0;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 수리 시스템에 기물을 등록합니다.
        /// </summary>
        public void RegisterPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null)
            {
                return;
            }

            if (pieceHealth.IsBroken)
            {
                if (!brokenPieces.Contains(pieceHealth))
                {
                    brokenPieces.Add(pieceHealth);
                }
            }
            else if (pieceHealth.IsActive)
            {
                if (!activePieces.Contains(pieceHealth))
                {
                    activePieces.Add(pieceHealth);
                }
            }

            // 상태 변경 구독
            pieceHealth.OnPieceBroken.AddListener(() => HandlePieceBroken(pieceHealth));
            pieceHealth.OnPieceRepaired.AddListener(() => HandlePieceRepaired(pieceHealth));
        }

        /// <summary>
        /// 수리 시스템에서 기물 등록을 취소합니다.
        /// </summary>
        public void UnregisterPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null)
            {
                return;
            }

            brokenPieces.Remove(pieceHealth);
            activePieces.Remove(pieceHealth);
            
            // 참고: 익명 델리게이트(람다)는 쉽게 구독 취소할 수 없습니다.
            // 더 견고한 시스템에서는 메서드 그룹이나 래퍼를 사용하지만,
            // 지금은 기물과 함께 PieceHealth 컴포넌트가 파괴될 때 Unity의 이벤트 시스템 정리에 의존합니다.
        }

        /// <summary>
        /// 기물을 부서진 것으로 표시하고 부서진 목록에 추가합니다.
        /// </summary>
        public void BreakPiece(PieceHealth pieceHealth)
        {
            if (pieceHealth == null || pieceHealth.IsBroken)
            {
                return;
            }

            pieceHealth.BreakPiece();
            // 이벤트 핸들러가 부서진 목록으로 이동
        }

        /// <summary>
        /// 기물을 수리하려고 시도합니다.
        /// </summary>
        public bool RepairPiece(PieceHealth pieceHealth, PlayerState playerState = null)
        {
            if (pieceHealth == null || !pieceHealth.CanBeRepaired)
            {
                Debug.LogWarning("이 기물을 수리할 수 없습니다!");
                return false;
            }

            if (RepairsRemaining <= 0)
            {
                Debug.LogWarning("이번 휴식에 남은 수리 횟수가 없습니다!");
                return false;
            }

            // 수리 비용 확인 (화폐 시스템 사용 시)
            if (usesRepairCost && pieceHealth.RepairCost > 0)
            {
                if (playerState != null)
                {
                    if (playerState.Currency >= pieceHealth.RepairCost)
                    {
                        playerState.Currency -= pieceHealth.RepairCost;
                        Debug.Log($"수리 비용 {pieceHealth.RepairCost} 지불. 남은 금액: {playerState.Currency}");
                    }
                    else
                    {
                        Debug.LogWarning("수리하기에 화폐가 부족합니다!");
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning("화폐를 확인할 수 없습니다 - PlayerState가 제공되지 않았습니다.");
                    // 디자인에 따라 여기서 false를 반환하거나 상태가 없으면 무료 수리를 허용할 수 있습니다.
                    // 안전을 위해 비용이 필요하지만 상태가 없으면 실패합니다.
                    return false; 
                }
            }

            bool repaired = pieceHealth.RepairPiece();

            if (repaired)
            {
                repairsUsedThisRest++;
                OnRepairsAvailableChanged?.Invoke(RepairsRemaining);
                // 이벤트 핸들러가 활성 목록으로 다시 이동
            }

            return repaired;
        }

        /// <summary>
        /// 휴식 방에 들어갈 때 호출됩니다 - 수리 횟수를 재설정합니다.
        /// </summary>
        public void EnterRestRoom()
        {
            repairsUsedThisRest = 0;
            OnRepairsAvailableChanged?.Invoke(RepairsRemaining);

            Debug.Log($"휴식 방에 입장했습니다. {maxRepairsPerRest}번의 수리가 가능합니다.");
        }

        /// <summary>
        /// 휴식 방에서 나갈 때 호출됩니다.
        /// </summary>
        public void ExitRestRoom()
        {
            Debug.Log($"휴식 방에서 나왔습니다. {repairsUsedThisRest}/{maxRepairsPerRest}번의 수리를 사용했습니다.");
        }

        /// <summary>
        /// 킹이 부서졌는지 확인합니다 (게임 오버 조건).
        /// </summary>
        public bool IsKingBroken(Team team)
        {
            foreach (var brokenPiece in brokenPieces)
            {
                if (brokenPiece.Piece != null &&
                    brokenPiece.Piece.Type == PieceType.King &&
                    brokenPiece.Piece.Team == team)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 특정 팀의 부서진 기물을 모두 가져옵니다.
        /// </summary>
        public List<PieceHealth> GetBrokenPiecesByTeam(Team team)
        {
            var result = new List<PieceHealth>();

            foreach (var piece in brokenPieces)
            {
                if (piece.Piece != null && piece.Piece.Team == team)
                {
                    result.Add(piece);
                }
            }

            return result;
        }

        /// <summary>
        /// 부서진 모든 기물의 저장 데이터를 가져옵니다.
        /// </summary>
        public List<PieceHealthData> GetSaveData()
        {
            var data = new List<PieceHealthData>();

            foreach (var piece in brokenPieces)
            {
                data.Add(piece.GetSaveData());
            }

            foreach (var piece in activePieces)
            {
                if (!piece.IsActive) continue;
                data.Add(piece.GetSaveData());
            }

            return data;
        }

        /// <summary>
        /// 추적 중인 모든 기물을 제거합니다.
        /// </summary>
        public void Clear()
        {
            brokenPieces.Clear();
            activePieces.Clear();
            repairsUsedThisRest = 0;
        }

        /// <summary>
        /// 기물 체력에 대한 통계를 가져옵니다.
        /// </summary>
        public string GetStats() => 
            $"활성: {activePieces.Count}, 부서짐: {brokenPieces.Count}, 남은 수리: {RepairsRemaining}";
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 기물이 부서졌을 때 처리합니다.
        /// </summary>
        private void HandlePieceBroken(PieceHealth pieceHealth)
        {
            if (activePieces.Contains(pieceHealth))
            {
                activePieces.Remove(pieceHealth);
            }

            if (!brokenPieces.Contains(pieceHealth))
            {
                brokenPieces.Add(pieceHealth);
            }

            OnPieceBroken?.Invoke(pieceHealth);

            Debug.Log($"기물이 부서졌습니다. 부서진 기물 수: {brokenPieces.Count}");
        }

        /// <summary>
        /// 기물이 수리되었을 때 처리합니다.
        /// </summary>
        private void HandlePieceRepaired(PieceHealth pieceHealth)
        {
            if (brokenPieces.Contains(pieceHealth))
            {
                brokenPieces.Remove(pieceHealth);
            }

            if (!activePieces.Contains(pieceHealth))
            {
                activePieces.Add(pieceHealth);
            }

            OnPieceRepaired?.Invoke(pieceHealth);

            Debug.Log($"기물이 수리되었습니다. 부서진 기물 수: {brokenPieces.Count}");
        }
        #endregion
    }
}
