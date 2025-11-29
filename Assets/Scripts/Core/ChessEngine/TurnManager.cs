using UnityEngine;
using UnityEngine.Events;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.AI;

namespace MutatingGambit.Core.ChessEngine
{
    /// <summary>
    /// 턴 관리 및 AI 실행을 담당합니다.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board board;
        [SerializeField] private ChessAI aiPlayer;

        [Header("Settings")]
        [SerializeField] private bool simulationMode = false;

        private Team currentTurn = Team.White;
        private Team playerTeam = Team.White;
        private int turnNumber = 0;

        public UnityEvent<Team> OnTurnStart;
        public UnityEvent<Team> OnTurnEnd;

        public Team CurrentTurn => currentTurn;
        public int TurnNumber => turnNumber;
        public bool IsPlayerTurn => currentTurn == playerTeam;

        public void Initialize(Team player, ChessAI ai)
        {
            playerTeam = player;
            aiPlayer = ai;
            currentTurn = playerTeam;
            turnNumber = 0;
        }

        /// <summary>
        /// 새 턴을 시작합니다.
        /// </summary>
        public void StartTurn()
        {
            turnNumber++;
            OnTurnStart?.Invoke(currentTurn);

            Debug.Log($"턴 {turnNumber}: {currentTurn}의 차례");

            // AI 턴이면 AI가 수를 둡니다
            if (!IsPlayerTurn && aiPlayer != null)
            {
                if (simulationMode)
                {
                    ExecuteAITurn();
                }
                else
                {
                    Invoke(nameof(ExecuteAITurn), 0.5f);
                }
            }
        }

        /// <summary>
        /// 현재 턴을 종료하고 다음 플레이어로 전환합니다.
        /// </summary>
        public void EndTurn()
        {
            OnTurnEnd?.Invoke(currentTurn);
            currentTurn = currentTurn == Team.White ? Team.Black : Team.White;
            StartTurn();
        }

        /// <summary>
        /// AI의 턴을 실행합니다.
        /// </summary>
        private void ExecuteAITurn()
        {
            if (board == null || aiPlayer == null)
            {
                Debug.LogError("AI 턴을 실행할 수 없습니다 - 참조 누락!");
                return;
            }

            var move = aiPlayer.MakeMove(board);

            if (move.MovingPiece != null)
            {
                // GameManager에서 실행
                GameManager.Instance?.ExecuteMove(move.From, move.To);
            }
            else
            {
                Debug.LogWarning("AI가 유효한 수를 찾을 수 없습니다!");
                EndTurn();
            }
        }

        public void SetSimulationMode(bool value)
        {
            simulationMode = value;
        }

        public void Reset()
        {
            currentTurn = playerTeam;
            turnNumber = 0;
        }
    }
}
