using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MutatingGambit.Systems.PieceManagement;

namespace MutatingGambit.UI
{
    /// <summary>
    /// 부서진 기물 슬롯 UI 오브젝트를 생성합니다.
    /// 단일 책임: 슬롯 GameObject 생성 및 초기 설정만 담당
    /// </summary>
    public class BrokenPieceSlotFactory
    {
        #region 프리팹 및 컨테이너
        private readonly GameObject slotPrefab;
        private readonly Transform parentContainer;
        #endregion

        #region 생성자
        /// <summary>
        /// 슬롯 팩토리를 초기화합니다.
        /// </summary>
        /// <param name="prefab">슬롯 프리팹</param>
        /// <param name="container">부모 컨테이너</param>
        public BrokenPieceSlotFactory(GameObject prefab, Transform container)
        {
            slotPrefab = prefab;
            parentContainer = container;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 부서진 기물 슬롯을 생성합니다.
        /// </summary>
        /// <param name="pieceHealth">기물 체력 정보</param>
        /// <param name="canRepair">수리 가능 여부</param>
        /// <returns>생성된 슬롯 컴포넌트</returns>
        public BrokenPieceSlot CreateSlot(PieceHealth pieceHealth, bool canRepair)
        {
            GameObject slotObject = CreateSlotObject(pieceHealth);
            BrokenPieceSlot slot = EnsureSlotComponent(slotObject);
            
            slot.Initialize(pieceHealth, canRepair);
            return slot;
        }

        /// <summary>
        /// 메시지 오브젝트를 생성합니다.
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        public void CreateMessageObject(string message)
        {
            GameObject messageObject = CreateBasicObject("Message");
            AddMessageText(messageObject, message);
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 슬롯 GameObject를 생성합니다.
        /// </summary>
        private GameObject CreateSlotObject(PieceHealth pieceHealth)
        {
            if (slotPrefab != null)
            {
                return Instantiate(slotPrefab, parentContainer);
            }

            return CreateFallbackSlot(pieceHealth);
        }

        /// <summary>
        /// 프리팹이 없을 때 대체 슬롯을 생성합니다.
        /// </summary>
        private GameObject CreateFallbackSlot(PieceHealth pieceHealth)
        {
            string objectName = $"BrokenPiece_{pieceHealth.Piece?.Type}";
            return CreateBasicObject(objectName);
        }

        /// <summary>
        /// 기본 GameObject를 생성합니다.
        /// </summary>
        private GameObject CreateBasicObject(string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parentContainer);
            return obj;
        }

        /// <summary>
        /// 슬롯 컴포넌트를 확보합니다.
        /// </summary>
        private BrokenPieceSlot EnsureSlotComponent(GameObject obj)
        {
            var slot = obj.GetComponent<BrokenPieceSlot>();
            if (slot == null)
            {
                slot = obj.AddComponent<BrokenPieceSlot>();
            }
            return slot;
        }

        /// <summary>
        /// 메시지 텍스트를 추가합니다.
        /// </summary>
        private void AddMessageText(GameObject obj, string message)
        {
            var text = obj.AddComponent<TextMeshProUGUI>();
            ConfigureMessageText(text, message);
        }

        /// <summary>
        /// 메시지 텍스트를 설정합니다.
        /// </summary>
        private void ConfigureMessageText(TextMeshProUGUI text, string message)
        {
            text.text = message;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 24;
            text.color = Color.green;
        }

        /// <summary>
        /// 프리팹을 인스턴스화합니다.
        /// </summary>
        private GameObject Instantiate(GameObject prefab, Transform parent)
        {
            return Object.Instantiate(prefab, parent);
        }
        #endregion
    }
}
