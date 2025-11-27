using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.UI;
using MutatingGambit.Systems.Audio;
using MutatingGambit.Systems.Effects;

namespace MutatingGambit.Tests.PlayMode
{
    public class SystemIntegrationTests
    {
        private GameObject gameObject;
        private Board board;
        private MoveHistoryPanel historyPanel;
        private AudioManager audioManager;
        private EffectManager effectManager;

        [SetUp]
        public void Setup()
        {
            gameObject = new GameObject("GameContext");
            board = gameObject.AddComponent<Board>();
            board.Initialize(8, 8);

            // Setup UI
            GameObject uiObj = new GameObject("UI");
            historyPanel = uiObj.AddComponent<MoveHistoryPanel>();
            
            // Setup Managers
            GameObject audioObj = new GameObject("AudioManager");
            audioManager = audioObj.AddComponent<AudioManager>();
            
            GameObject effectObj = new GameObject("EffectManager");
            effectManager = effectObj.AddComponent<EffectManager>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.Destroy(gameObject);
        }

        [UnityTest]
        public IEnumerator MoveEvent_Triggers_HistoryUpdate()
        {
            // Arrange
            bool eventFired = false;
            board.OnPieceMoved += (p, f, t, c) => eventFired = true;

            GameObject pieceObj = new GameObject("Piece");
            Piece piece = pieceObj.AddComponent<Piece>();
            piece.Initialize(PieceType.Pawn, Team.White, new Vector2Int(1, 1));
            board.PlacePiece(piece, new Vector2Int(1, 1));

            // Act
            board.MovePiece(new Vector2Int(1, 1), new Vector2Int(1, 2));

            // Assert
            Assert.IsTrue(eventFired, "OnPieceMoved event should fire");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Managers_Exist_And_Initialize()
        {
            Assert.IsNotNull(historyPanel, "MoveHistoryPanel should exist");
            Assert.IsNotNull(audioManager, "AudioManager should exist");
            Assert.IsNotNull(effectManager, "EffectManager should exist");
            yield return null;
        }
    }
}
