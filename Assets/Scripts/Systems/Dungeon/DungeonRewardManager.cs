using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.UI;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 던전 보상 시스템을 관리합니다.
    /// </summary>
    public class DungeonRewardManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RewardSelectionUI rewardUI;
        [SerializeField] private DungeonMapUI dungeonMapUI;

        private DungeonManager dungeonManager;
        private PlayerState playerState;

        public void Initialize(DungeonManager manager, PlayerState state)
        {
            dungeonManager = manager;
            playerState = state;

            // UI 이벤트 구독
            if (rewardUI != null)
            {
                rewardUI.OnRewardSelected.AddListener(OnRewardSelected);
            }
        }

        /// <summary>
        /// 보상 선택 UI를 표시합니다.
        /// </summary>
        public void ShowRewardSelection(RoomData roomData)
        {
            if (rewardUI == null)
            {
                Debug.LogWarning("RewardSelectionUI를 찾을 수 없습니다");
                ShowDungeonMap();
                return;
            }

            List<Artifact> possibleRewards = new List<Artifact>();

            if (roomData.PossibleArtifactRewards != null && roomData.PossibleArtifactRewards.Length > 0)
            {
                possibleRewards.AddRange(roomData.PossibleArtifactRewards);
            }

            if (possibleRewards.Count > 0)
            {
                rewardUI.ShowRewards(possibleRewards);
            }
            else
            {
                ShowDungeonMap();
            }
        }

        /// <summary>
        /// 보상이 선택되었을 때 호출됩니다.
        /// </summary>
        private void OnRewardSelected(Artifact artifact)
        {
            if (artifact != null && playerState != null)
            {
                playerState.AddArtifact(artifact);
                Debug.Log($"아티팩트 선택: {artifact.name}");
            }

            ShowDungeonMap();
        }

        /// <summary>
        /// 던전 맵 UI를 표시합니다.
        /// </summary>
        private void ShowDungeonMap()
        {
            if (dungeonMapUI != null && dungeonManager != null)
            {
                dungeonMapUI.UpdateNodeStates();
                dungeonMapUI.ShowMap(dungeonManager.CurrentMap);
            }
        }
    }
}
