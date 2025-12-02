namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 던전에서 사용 가능한 방의 타입을 정의합니다.
    /// </summary>
    public enum RoomType
    {
        /// <summary>
        /// 기본 적과의 일반 전투 조우입니다.
        /// </summary>
        NormalCombat,

        /// <summary>
        /// 변이된 적과의 엘리트 전투 조우입니다 (더 나은 보상).
        /// </summary>
        EliteCombat,

        /// <summary>
        /// 층 끝의 보스 조우입니다.
        /// </summary>
        Boss,

        /// <summary>
        /// 보물 방 - 즉시 아티팩트를 부여합니다.
        /// </summary>
        Treasure,

        /// <summary>
        /// 휴식 방 - 파손된 기물을 수리하거나 기물을 업그레이드합니다.
        /// </summary>
        Rest,

        /// <summary>
        /// 무작위 조우가 있는 미스터리 이벤트 방입니다.
        /// </summary>
        Mystery,

        /// <summary>
        /// 던전의 시작 방입니다.
        /// </summary>
        Start,

        /// <summary>
        /// 게임 메커니즘을 가르치는 튜토리얼 방입니다.
        /// </summary>
        Tutorial
    }
}
