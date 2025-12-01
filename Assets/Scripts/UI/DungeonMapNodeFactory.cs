using UnityEngine;
using UnityEngine.UI;
using MutatingGambit.Systems.Dungeon;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 던전 맵 노드 UI 오브젝트를 생성합니다.
    /// 단일 책임: UI GameObject 생성 및 초기 설정만 담당
    /// </summary>
    public class DungeonMapNodeFactory
    {
        #region 프리팹 참조
        private readonly GameObject nodeButtonPrefab;
        private readonly Transform parentContainer;
        #endregion

        #region 생성자
        /// <summary>
        /// 노드 팩토리를 초기화합니다.
        /// </summary>
        /// <param name="prefab">노드 버튼 프리팹</param>
        /// <param name="container">부모 컨테이너</param>
        public DungeonMapNodeFactory(GameObject prefab, Transform container)
        {
            nodeButtonPrefab = prefab;
            parentContainer = container;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 노드 UI GameObject를 생성합니다.
        /// </summary>
        /// <param name="nodeId">노드 ID (이름 지정용)</param>
        /// <returns>생성된 GameObject</returns>
        public GameObject CreateNodeObject(string nodeId)
        {
            if (parentContainer == null)
            {
                Debug.LogError("부모 컨테이너가 null입니다.");
                return null;
            }

            return UsePrefabOrCreateNew(nodeId);
        }

        /// <summary>
        /// 노드 오브젝트의 위치를 설정합니다.
        /// </summary>
        /// <param name="nodeObject">설정할 GameObject</param>
        /// <param name="position">화면 위치</param>
        public void SetNodePosition(GameObject nodeObject, Vector2 position)
        {
            var rectTransform = GetRectTransform(nodeObject);
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = position;
            }
        }

        /// <summary>
        /// 노드 UI 컴포넌트를 설정합니다.
        /// </summary>
        /// <param name="nodeObject">설정할 GameObject</param>
        /// <param name="node">연결할 방 노드</param>
        /// <param name="color">노드 색상</param>
        /// <returns>설정된 DungeonMapNodeUI 컴포넌트</returns>
        public DungeonMapNodeUI SetupNodeComponent(GameObject nodeObject, RoomNode node, Color color)
        {
            var nodeUI = EnsureNodeUIComponent(nodeObject);
            nodeUI.Initialize(node, color);
            return nodeUI;
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 프리팹을 사용하거나 새로 생성합니다.
        /// </summary>
        private GameObject UsePrefabOrCreateNew(string nodeId)
        {
            if (nodeButtonPrefab != null)
            {
                return Object.Instantiate(nodeButtonPrefab, parentContainer);
            }

            return CreateNewNodeObject(nodeId);
        }

        /// <summary>
        /// 새 노드 GameObject를 생성합니다.
        /// </summary>
        private GameObject CreateNewNodeObject(string nodeId)
        {
            var nodeObject = new GameObject($"Node_{nodeId}");
            nodeObject.transform.SetParent(parentContainer);
            return nodeObject;
        }

        /// <summary>
        /// RectTransform을 가져옵니다.
        /// </summary>
        private RectTransform GetRectTransform(GameObject obj)
        {
            return obj.GetComponent<RectTransform>();
        }

        /// <summary>
        /// NodeUI 컴포넌트를 확보합니다 (없으면 추가).
        /// </summary>
        private DungeonMapNodeUI EnsureNodeUIComponent(GameObject obj)
        {
            var nodeUI = obj.GetComponent<DungeonMapNodeUI>();
            if (nodeUI == null)
            {
                nodeUI = obj.AddComponent<DungeonMapNodeUI>();
            }
            return nodeUI;
        }
        #endregion
    }
}
