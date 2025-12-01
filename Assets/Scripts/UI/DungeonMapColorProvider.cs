using UnityEngine;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 던전 맵 노드의 색상을 제공합니다.
    /// 단일 책임: 노드 타입별 색상 결정만 담당
    /// </summary>
    public class DungeonMapColorProvider
    {
        #region 색상 설정
        private readonly Color normalNodeColor;
        private readonly Color eliteNodeColor;
        private readonly Color bossNodeColor;
        private readonly Color treasureNodeColor;
        private readonly Color restNodeColor;
        #endregion

        #region 생성자
        /// <summary>
        /// 색상 제공자를 초기화합니다.
        /// </summary>
        /// <param name="normalColor">일반 전투 방 색상</param>
        /// <param name="eliteColor">정예 전투 방 색상</param>
        /// <param name="bossColor">보스 방 색상</param>
        /// <param name="treasureColor">보물 방 색상</param>
        /// <param name="restColor">휴식 방 색상</param>
        public DungeonMapColorProvider(
            Color normalColor,
            Color eliteColor,
            Color bossColor,
            Color treasureColor,
            Color restColor)
        {
            normalNodeColor = normalColor;
            eliteNodeColor = eliteColor;
            bossNodeColor = bossColor;
            treasureNodeColor = treasureColor;
            restNodeColor = restColor;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 방 타입에 해당하는 색상을 가져옵니다.
        /// </summary>
        /// <param name="roomType">방 타입</param>
        /// <returns>해당 타입의 색상</returns>
        public Color GetNodeColor(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.NormalCombat => normalNodeColor,
                RoomType.EliteCombat => eliteNodeColor,
                RoomType.Boss => bossNodeColor,
                RoomType.Treasure => treasureNodeColor,
                RoomType.Rest => restNodeColor,
                RoomType.Start => Color.white,
                _ => normalNodeColor
            };
        }
        #endregion
    }
}
