namespace MutatingGambit.Systems.BoardSystem
{
    /// <summary>
    /// 보드에 존재할 수 있는 타일의 타입을 정의합니다.
    /// </summary>
    public enum TileType
    {
        /// <summary>
        /// 표준 플레이 가능 타일입니다.
        /// </summary>
        Normal,

        /// <summary>
        /// 통과 불가능한 장애물 타일입니다 (기물이 이동하거나 위에 놓일 수 없음).
        /// </summary>
        Obstacle,

        /// <summary>
        /// 물 타일입니다 (향후 특수 규칙이 적용될 수 있음).
        /// </summary>
        Water,

        /// <summary>
        /// 공허 타일입니다 (경계 밖, 비직사각형 보드에 사용됨).
        /// </summary>
        Void
    }
}
