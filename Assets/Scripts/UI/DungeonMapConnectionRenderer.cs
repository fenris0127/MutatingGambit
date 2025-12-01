using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 던전 맵의 연결선을 생성하고 관리합니다.
    /// 단일 책임: 노드 간 연결선 시각화만 담당
    /// </summary>
    public class DungeonMapConnectionRenderer
    {
        #region 프리팹 및 컨테이너
        private readonly LineRenderer connectionLinePrefab;
        private readonly Transform parentContainer;
        private readonly List<LineRenderer> activeLines = new List<LineRenderer>();
        #endregion

        #region 생성자
        /// <summary>
        /// 연결선 렌더러를 초기화합니다.
        /// </summary>
        /// <param name="prefab">연결선 프리팹</param>
        /// <param name="container">부모 컨테이너</param>
        public DungeonMapConnectionRenderer(LineRenderer prefab, Transform container)
        {
            connectionLinePrefab = prefab;
            parentContainer = container;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 두 노드 사이의 연결선을 생성합니다.
        /// </summary>
        /// <param name="fromNodeUI">시작 노드 UI</param>
        /// <param name="toNodeUI">종료 노드 UI</param>
        public void CreateConnection(DungeonMapNodeUI fromNodeUI, DungeonMapNodeUI toNodeUI)
        {
            if (!ValidateNodes(fromNodeUI, toNodeUI))
            {
                return;
            }

            Vector2 startPos = GetNodePosition(fromNodeUI);
            Vector2 endPos = GetNodePosition(toNodeUI);

            CreateLine(startPos, endPos);
        }

        /// <summary>
        /// 모든 연결선을 제거합니다.
        /// </summary>
        public void ClearAllConnections()
        {
            foreach (var line in activeLines)
            {
                if (line != null)
                {
                    Object.Destroy(line.gameObject);
                }
            }
            activeLines.Clear();
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 노드 유효성을 검증합니다.
        /// </summary>
        private bool ValidateNodes(DungeonMapNodeUI from, DungeonMapNodeUI to)
        {
            return from != null && to != null;
        }

        /// <summary>
        /// 노드의 화면 위치를 가져옵니다.
        /// </summary>
        private Vector2 GetNodePosition(DungeonMapNodeUI nodeUI)
        {
            var rectTransform = nodeUI.GetComponent<RectTransform>();
            return rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        }

        /// <summary>
        /// 연결선을 생성합니다.
        /// </summary>
        private void CreateLine(Vector2 start, Vector2 end)
        {
            if (connectionLinePrefab == null)
            {
                return;
            }

            LineRenderer line = Object.Instantiate(connectionLinePrefab, parentContainer);
            SetLinePositions(line, start, end);
            activeLines.Add(line);
        }

        /// <summary>
        /// 라인의 시작/종료 위치를 설정합니다.
        /// </summary>
        private void SetLinePositions(LineRenderer line, Vector2 start, Vector2 end)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
        #endregion
    }
}
