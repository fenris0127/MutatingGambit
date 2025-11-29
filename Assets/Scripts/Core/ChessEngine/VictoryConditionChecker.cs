using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.UI;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 승패 조건을 확인하고 게임 오버를 처리합니다.
    /// </summary>
    public class VictoryConditionChecker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private RepairSystem repairSystem;
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private GameOverScreen gameOverScreen;

        private Team playerTeam = Team.White;

        public void Initialize(Team player)
        {
            playerTeam = player;
        }

        /// <summary>
        /// 게임 승패 조건을 확인합니다.
        /// </summary>
        public bool CheckGameConditions()
        {
            // 플레이어의 킹이 부서졌는지 확인
            if (repairSystem != null && repairSystem.IsKingBroken(playerTeam))
            {
                HandleDefeat();
                return true;
            }

            // AI의 킹이 부서졌는지 확인
            Team aiTeam = playerTeam == Team.White ? Team.Black : Team.White;
            if (repairSystem != null && repairSystem.IsKingBroken(aiTeam))
            {
                HandleVictory();
                return true;
            }

            // 방 특정 승리 조건 확인
            if (roomManager != null)
            {
                roomManager.CheckConditions();

                if (roomManager.IsRoomCompleted)
                {
                    HandleRoomVictory();
                    return true;
                }
                else if (roomManager.IsRoomFailed)
                {
                    HandleDefeat();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 기물이 잡혔을 때 처리합니다.
        /// </summary>
        public void HandlePieceCapture(Piece capturedPiece)
        {
            if (capturedPiece == null) return;

            var pieceHealth = capturedPiece.GetComponent<PieceHealth>();
            if (pieceHealth != null && repairSystem != null)
            {
                repairSystem.BreakPiece(pieceHealth);

                if (capturedPiece.Type == PieceType.King)
                {
                    HandleKingBroken(capturedPiece.Team);
                }
            }
        }

        private void HandleKingBroken(Team team)
        {
            if (team == playerTeam)
            {
                HandleDefeat();
            }
            else
            {
                HandleVictory();
            }
        }

        private void HandleVictory()
        {
            GameManager.Instance?.TriggerVictory();
            
            if (gameOverScreen != null)
            {
                gameOverScreen.ShowVictory();
            }
        }

        private void HandleDefeat()
        {
            GameManager.Instance?.TriggerDefeat();
            
            if (gameOverScreen != null)
            {
                gameOverScreen.ShowDefeat();
            }
        }

        private void HandleRoomVictory()
        {
            Debug.Log("방 완료!");
            GameManager.Instance?.TriggerVictory();

            if (gameOverScreen != null)
            {
                gameOverScreen.ShowVictory("방 클리어!");
            }
        }
    }
}
