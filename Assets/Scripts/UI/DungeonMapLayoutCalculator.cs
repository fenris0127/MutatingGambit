using UnityEngine;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 던전 맵의 노드 위치를 계산합니다.
    /// 단일 책임: 레이아웃 계산 로직만 담당
    /// </summary>
    public class DungeonMapLayoutCalculator
    {
        #region 레이아웃 설정
        private readonly float layerSpacing;
        private readonly float nodeSpacing;
        private readonly Vector2 mapOffset;
        #endregion

        #region 생성자
        /// <summary>
        /// 레이아웃 계산기를 초기화합니다.
        /// </summary>
        /// <param name="layerSpacing">레이어 간 간격</param>
        /// <param name="nodeSpacing">노드 간 간격</param>
        /// <param name="mapOffset">맵 오프셋</param>
        public DungeonMapLayoutCalculator(float layerSpacing, float nodeSpacing, Vector2 mapOffset)
        {
            this.layerSpacing = layerSpacing;
            this.nodeSpacing = nodeSpacing;
            this.mapOffset = mapOffset;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 노드의 화면 위치를 계산합니다.
        /// </summary>
        /// <param name="node">위치를 계산할 노드</param>
        /// <param name="map">노드가 속한 던전 맵</param>
        /// <returns>계산된 화면 위치</returns>
        public Vector2 CalculateNodePosition(RoomNode node, DungeonMap map)
        {
            if (node == null || map == null)
            {
                Debug.LogWarning("노드 또는 맵이 null입니다.");
                return Vector2.zero;
            }

            float x = CalculateXPosition(node.Layer);
            float y = CalculateYPosition(node, map);

            return new Vector2(x, y);
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// X축 위치를 계산합니다.
        /// </summary>
        private float CalculateXPosition(int layer)
        {
            return layer * layerSpacing + mapOffset.x;
        }

        /// <summary>
        /// Y축 위치를 계산합니다 (레이어 내 중앙 정렬).
        /// </summary>
        private float CalculateYPosition(RoomNode node, DungeonMap map)
        {
            var layerNodes = map.GetNodesInLayer(node.Layer);
            int totalInLayer = layerNodes.Count;
            float centerOffset = (totalInLayer - 1) / 2f;

            return (node.PositionInLayer - centerOffset) * nodeSpacing + mapOffset.y;
        }
        #endregion
    }
}
