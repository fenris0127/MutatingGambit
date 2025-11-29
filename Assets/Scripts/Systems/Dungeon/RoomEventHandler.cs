using UnityEngine;
using MutatingGambit.UI;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 미스터리 방 및 특수 이벤트를 처리합니다.
    /// </summary>
    public class RoomEventHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private NotificationUI notificationUI;
        [SerializeField] private RepairUI repairUI;

        private PlayerState playerState;
        private DungeonRewardManager rewardManager;
        private DungeonManager dungeonManager;

        public void Initialize(DungeonManager manager, PlayerState state, DungeonRewardManager rewards)
        {
            dungeonManager = manager;
            playerState = state;
            rewardManager = rewards;
        }

        /// <summary>
        /// 미스터리 방에 진입합니다.
        /// </summary>
        public void EnterMysteryRoom(RoomNode roomNode)
        {
            Debug.Log("미스터리 방 진입");

            int roll = Random.Range(0, 100);

            if (roll < 40)
            {
                // 보물
                Debug.Log("미스터리 방 결과... 보물!");
                EnterTreasureRoom(roomNode);
            }
            else if (roll < 70)
            {
                // 저주
                Debug.Log("미스터리 방 결과... 저주!");
                HandleCurseEvent();
            }
            else
            {
                // 축복
                Debug.Log("미스터리 방 결과... 축복!");
                HandleBlessingEvent();
            }
        }

        /// <summary>
        /// 보물 방에 진입합니다.
        /// </summary>
        public void EnterTreasureRoom(RoomNode roomNode)
        {
            Debug.Log("보물 방 진입");
            
            if (rewardManager != null && roomNode.Room != null)
            {
                rewardManager.ShowRewardSelection(roomNode.Room);
            }
        }

        /// <summary>
        /// 휴식 방에 진입합니다.
        /// </summary>
        public void EnterRestRoom(MutatingGambit.Systems.PieceManagement.RepairSystem repairSystem)
        {
            Debug.Log("휴식 방 진입");

            if (repairUI != null && repairSystem != null)
            {
                repairUI.Show(repairSystem);
            }
            else
            {
                Debug.LogWarning("RepairUI 또는 RepairSystem을 찾을 수 없습니다");
                ContinueAfterRest();
            }
        }

        /// <summary>
        /// 저주 이벤트를 처리합니다.
        /// </summary>
        private void HandleCurseEvent()
        {
            int currencyLoss = Random.Range(10, 31);
            playerState.Currency = Mathf.Max(0, playerState.Currency - currencyLoss);
            
            Debug.Log($"저주! {currencyLoss} 화폐 손실. 남은 화폐: {playerState.Currency}");
            ShowNotification($"저주! {currencyLoss} 화폐를 잃었습니다.");
            
            ContinueAfterRest();
        }

        /// <summary>
        /// 축복 이벤트를 처리합니다.
        /// </summary>
        private void HandleBlessingEvent()
        {
            int currencyGain = Random.Range(20, 51);
            playerState.Currency += currencyGain;
            
            Debug.Log($"축복! {currencyGain} 화폐 획득. 총 화폐: {playerState.Currency}");
            ShowNotification($"축복! {currencyGain} 화폐를 얻었습니다.");
            
            ContinueAfterRest();
        }

        /// <summary>
        /// 알림을 표시합니다.
        /// </summary>
        private void ShowNotification(string message)
        {
            Debug.Log($"[알림] {message}");
            
            if (notificationUI != null)
            {
                notificationUI.Show(message);
            }
        }

        /// <summary>
        /// 휴식 후 계속 진행합니다.
        /// </summary>
        private void ContinueAfterRest()
        {
            if (dungeonManager != null)
            {
dungeonManager.ContinueAfterRest();
            }
        }
    }
}
