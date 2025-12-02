using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// 모든 활성 아티팩트를 관리하고 전역 효과를 적용합니다.
    /// </summary>
    public class ArtifactManager : MonoBehaviour
    {
        #region 싱글톤
        private static ArtifactManager instance;

        /// <summary>
        /// ArtifactManager 싱글톤 인스턴스를 가져옵니다.
        /// </summary>
        public static ArtifactManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<ArtifactManager>();
                }
                return instance;
            }
        }
        #endregion

        #region 필드
        private List<Artifact> activeArtifacts = new List<Artifact>();
        private Board currentBoard;
        #endregion

        #region 공개 속성
        /// <summary>현재 활성화된 모든 아티팩트를 가져옵니다.</summary>
        public List<Artifact> ActiveArtifacts => new List<Artifact>(activeArtifacts);

        /// <summary>활성 아티팩트 수를 가져옵니다.</summary>
        public int ArtifactCount => activeArtifacts.Count;
        #endregion

        #region Unity 생명주기
        /// <summary>
        /// 싱글톤을 설정합니다.
        /// </summary>
        private void Awake()
        {
            if (!EnsureSingleInstance())
            {
                return;
            }

            instance = this;
        }
        #endregion

        #region 공개 메서드 - 보드 설정
        /// <summary>
        /// 이 관리자가 아티팩트를 관리할 보드를 설정합니다.
        /// </summary>
        public void SetBoard(Board board)
        {
            currentBoard = board;
        }
        #endregion

        #region 공개 메서드 - 아티팩트 관리
        /// <summary>
        /// 활성 컬렉션에 아티팩트를 추가합니다.
        /// </summary>
        public void AddArtifact(Artifact artifact)
        {
            if (!ValidateArtifact(artifact))
            {
                return;
            }

            if (!CanStackArtifact(artifact))
            {
                return;
            }

            AddAndActivateArtifact(artifact);
        }

        /// <summary>
        /// 활성 컬렉션에서 아티팩트를 제거합니다.
        /// </summary>
        public void RemoveArtifact(Artifact artifact)
        {
            if (artifact == null || !activeArtifacts.Contains(artifact))
            {
                return;
            }

            DeactivateAndRemoveArtifact(artifact);
        }

        /// <summary>
        /// 모든 아티팩트를 제거합니다.
        /// </summary>
        public void ClearArtifacts()
        {
            var artifactsCopy = new List<Artifact>(activeArtifacts);
            foreach (var artifact in artifactsCopy)
            {
                artifact.OnRemoved(currentBoard);
            }

            activeArtifacts.Clear();
        }
        #endregion

        #region 공개 메서드 - 쿼리
        /// <summary>
        /// 모든 아티팩트를 가져옵니다 (ActiveArtifacts의 별칭).
        /// </summary>
        public List<Artifact> GetAllArtifacts() => new List<Artifact>(activeArtifacts);

        /// <summary>
        /// 특정 아티팩트가 활성화되어 있는지 확인합니다.
        /// </summary>
        public bool HasArtifact(Artifact artifact)
        {
            return activeArtifacts.Contains(artifact);
        }

        /// <summary>
        /// 특정 타입의 아티팩트가 활성화되어 있는지 확인합니다.
        /// </summary>
        public bool HasArtifactOfType<T>() where T : Artifact
        {
            foreach (var artifact in activeArtifacts)
            {
                if (artifact is T)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 특정 트리거에 해당하는 모든 아티팩트를 가져옵니다.
        /// </summary>
        public List<Artifact> GetArtifactsByTrigger(ArtifactTrigger trigger)
        {
            var result = new List<Artifact>();
            foreach (var artifact in activeArtifacts)
            {
                if (artifact.Trigger == trigger)
                {
                    result.Add(artifact);
                }
            }
            return result;
        }
        #endregion

        #region 공개 메서드 - 트리거
        /// <summary>
        /// 지정된 트리거 타입에 해당하는 모든 아티팩트를 발동합니다.
        /// </summary>
        public void TriggerArtifacts(ArtifactTrigger trigger, ArtifactContext context)
        {
            if (!ValidateBoard())
            {
                return;
            }

            var triggeredArtifacts = GetArtifactsByTrigger(trigger);
            ApplyArtifactEffects(triggeredArtifacts, context);
        }
        #endregion

        #region 공개 메서드 - 알림
        /// <summary>
        /// 턴이 시작되었음을 모든 아티팩트에 알립니다.
        /// </summary>
        public void NotifyTurnStart(Team team, int turnNumber)
        {
            var context = CreateTurnContext(team, turnNumber);
            TriggerArtifacts(ArtifactTrigger.OnTurnStart, context);
        }

        /// <summary>
        /// 턴이 종료되었음을 모든 아티팩트에 알립니다.
        /// </summary>
        public void NotifyTurnEnd(Team team, int turnNumber)
        {
            var context = CreateTurnContext(team, turnNumber);
            TriggerArtifacts(ArtifactTrigger.OnTurnEnd, context);
        }

        /// <summary>
        /// 기물이 이동했음을 모든 아티팩트에 알립니다.
        /// </summary>
        public void NotifyPieceMove(Piece piece, Vector2Int from, Vector2Int to)
        {
            var context = new ArtifactContext(piece, from, to);
            TriggerArtifacts(ArtifactTrigger.OnPieceMove, context);

            if (piece.Type == PieceType.King)
            {
                TriggerArtifacts(ArtifactTrigger.OnKingMove, context);
            }
        }

        /// <summary>
        /// 기물이 포획되었음을 모든 아티팩트에 알립니다.
        /// </summary>
        public void NotifyPieceCapture(Piece attacker, Piece captured, Vector2Int capturePosition)
        {
            var context = CreateCaptureContext(attacker, captured, capturePosition);
            TriggerArtifacts(ArtifactTrigger.OnPieceCapture, context);
        }
        #endregion

        #region 비공개 메서드 - 검증
        /// <summary>
        /// 싱글톤 인스턴스가 유일한지 확인합니다.
        /// </summary>
        private bool EnsureSingleInstance()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 아티팩트가 유효한지 검증합니다.
        /// </summary>
        private bool ValidateArtifact(Artifact artifact)
        {
            if (artifact == null)
            {
                Debug.LogError("null 아티팩트를 추가할 수 없습니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 보드가 설정되어 있는지 검증합니다.
        /// </summary>
        private bool ValidateBoard()
        {
            if (currentBoard == null)
            {
                Debug.LogWarning("ArtifactManager에 보드가 설정되지 않았습니다. 아티팩트를 발동할 수 없습니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 아티팩트를 스택할 수 있는지 확인합니다.
        /// </summary>
        private bool CanStackArtifact(Artifact artifact)
        {
            foreach (var existing in activeArtifacts)
            {
                if (!artifact.CanStackWith(existing))
                {
                    Debug.LogWarning($"아티팩트 '{artifact.ArtifactName}'을(를) 기존 '{existing.ArtifactName}'과(와) 스택할 수 없습니다.");
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region 비공개 메서드 - 아티팩트 추가/제거
        /// <summary>
        /// 아티팩트를 추가하고 활성화합니다.
        /// </summary>
        private void AddAndActivateArtifact(Artifact artifact)
        {
            activeArtifacts.Add(artifact);
            artifact.OnAcquired(currentBoard);
            Debug.Log($"아티팩트 획득: {artifact.ArtifactName}");
        }

        /// <summary>
        /// 아티팩트를 비활성화하고 제거합니다.
        /// </summary>
        private void DeactivateAndRemoveArtifact(Artifact artifact)
        {
            artifact.OnRemoved(currentBoard);
            activeArtifacts.Remove(artifact);
            Debug.Log($"아티팩트 제거: {artifact.ArtifactName}");
        }
        #endregion

        #region 비공개 메서드 - 효과 적용
        /// <summary>
        /// 아티팩트 효과를 적용합니다.
        /// </summary>
        private void ApplyArtifactEffects(List<Artifact> artifacts, ArtifactContext context)
        {
            foreach (var artifact in artifacts)
            {
                artifact.ApplyEffect(currentBoard, context);
            }
        }
        #endregion

        #region 비공개 메서드 - 컨텍스트 생성
        /// <summary>
        /// 턴 컨텍스트를 생성합니다.
        /// </summary>
        private ArtifactContext CreateTurnContext(Team team, int turnNumber)
        {
            return new ArtifactContext
            {
                CurrentTeam = team,
                TurnNumber = turnNumber
            };
        }

        /// <summary>
        /// 포획 컨텍스트를 생성합니다.
        /// </summary>
        private ArtifactContext CreateCaptureContext(Piece attacker, Piece captured, Vector2Int position)
        {
            return new ArtifactContext
            {
                MovedPiece = attacker,
                CapturedPiece = captured,
                ToPosition = position
            };
        }
        #endregion

        #region 디버깅
        /// <summary>
        /// 모든 활성 아티팩트 목록을 문자열로 반환합니다.
        /// </summary>
        public override string ToString()
        {
            if (activeArtifacts.Count == 0)
            {
                return "ArtifactManager (활성 아티팩트 없음)";
            }

            var result = $"ArtifactManager ({activeArtifacts.Count}개 아티팩트):\n";
            foreach (var artifact in activeArtifacts)
            {
                result += $"  - {artifact.ArtifactName}\n";
            }
            return result;
        }
        #endregion
    }
}
