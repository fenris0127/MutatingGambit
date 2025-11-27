using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Unit tests for the piece management system (health, repair, upgrade).
    /// </summary>
    public class PieceManagementTests
    {
        private GameObject pieceObject;
        private Piece piece;
        private PieceHealth pieceHealth;
        private GameObject systemObject;
        private RepairSystem repairSystem;

        [SetUp]
        public void Setup()
        {
            // Create piece with PieceHealth
            pieceObject = new GameObject("TestPiece");
            piece = pieceObject.AddComponent<Piece>();
            piece.Initialize(PieceType.Rook, Team.White, new Vector2Int(0, 0));

            pieceHealth = pieceObject.AddComponent<PieceHealth>();

            // Create RepairSystem
            systemObject = new GameObject("RepairSystem");
            repairSystem = systemObject.AddComponent<RepairSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (pieceObject != null) Object.DestroyImmediate(pieceObject);
            if (systemObject != null) Object.DestroyImmediate(systemObject);
        }

        #region PieceHealth Tests

        [Test]
        public void PieceHealth_InitialState_IsActive()
        {
            Assert.AreEqual(PieceHealth.PieceState.Active, pieceHealth.CurrentState);
            Assert.IsTrue(pieceHealth.IsActive);
            Assert.IsFalse(pieceHealth.IsBroken);
        }

        [Test]
        public void PieceHealth_BreakPiece_ChangeStateToBroken()
        {
            pieceHealth.BreakPiece();

            Assert.AreEqual(PieceHealth.PieceState.Broken, pieceHealth.CurrentState);
            Assert.IsFalse(pieceHealth.IsActive);
            Assert.IsTrue(pieceHealth.IsBroken);
        }

        [Test]
        public void PieceHealth_BreakPiece_FiresEvent()
        {
            bool eventFired = false;
            pieceHealth.OnPieceBroken.AddListener(() => eventFired = true);

            pieceHealth.BreakPiece();

            Assert.IsTrue(eventFired, "OnPieceBroken event should fire");
        }

        [Test]
        public void PieceHealth_RepairPiece_RestoresActiveState()
        {
            pieceHealth.BreakPiece();
            Assert.IsTrue(pieceHealth.IsBroken);

            bool repaired = pieceHealth.RepairPiece();

            Assert.IsTrue(repaired, "Repair should succeed");
            Assert.IsTrue(pieceHealth.IsActive);
            Assert.IsFalse(pieceHealth.IsBroken);
        }

        [Test]
        public void PieceHealth_RepairPiece_FiresEvent()
        {
            pieceHealth.BreakPiece();

            bool eventFired = false;
            pieceHealth.OnPieceRepaired.AddListener(() => eventFired = true);

            pieceHealth.RepairPiece();

            Assert.IsTrue(eventFired, "OnPieceRepaired event should fire");
        }

        [Test]
        public void PieceHealth_CanBeRepaired_TrueWhenBroken()
        {
            Assert.IsFalse(pieceHealth.CanBeRepaired, "Active piece should not be repairable");

            pieceHealth.BreakPiece();

            Assert.IsTrue(pieceHealth.CanBeRepaired, "Broken piece should be repairable");
        }

        [Test]
        public void PieceHealth_DestroyPiece_SetsPermanentState()
        {
            pieceHealth.DestroyPiece();

            Assert.AreEqual(PieceHealth.PieceState.Destroyed, pieceHealth.CurrentState);
            Assert.IsTrue(pieceHealth.IsDestroyed);
        }

        [Test]
        public void PieceHealth_SaveAndLoad_PreservesState()
        {
            pieceHealth.BreakPiece();

            var saveData = pieceHealth.GetSaveData();

            // Create new piece
            var newPieceObject = new GameObject("NewPiece");
            var newPiece = newPieceObject.AddComponent<Piece>();
            newPiece.Initialize(saveData.pieceType, saveData.team, saveData.position);

            var newPieceHealth = newPieceObject.AddComponent<PieceHealth>();
            newPieceHealth.LoadSaveData(saveData);

            Assert.AreEqual(PieceHealth.PieceState.Broken, newPieceHealth.CurrentState);
            Assert.IsTrue(newPieceHealth.IsBroken);

            Object.DestroyImmediate(newPieceObject);
        }

        #endregion

        #region RepairSystem Tests

        [Test]
        public void RepairSystem_RegisterPiece_AddsToBrokenListIfBroken()
        {
            pieceHealth.BreakPiece();
            repairSystem.RegisterPiece(pieceHealth);

            Assert.AreEqual(1, repairSystem.BrokenPieceCount);
            Assert.Contains(pieceHealth, repairSystem.BrokenPieces);
        }

        [Test]
        public void RepairSystem_RegisterPiece_AddsToActiveListIfActive()
        {
            repairSystem.RegisterPiece(pieceHealth);

            Assert.AreEqual(0, repairSystem.BrokenPieceCount);
            Assert.Contains(pieceHealth, repairSystem.ActivePieces);
        }

        [Test]
        public void RepairSystem_BreakPiece_MovesToBrokenList()
        {
            repairSystem.RegisterPiece(pieceHealth);
            Assert.AreEqual(0, repairSystem.BrokenPieceCount);

            repairSystem.BreakPiece(pieceHealth);

            Assert.AreEqual(1, repairSystem.BrokenPieceCount);
            Assert.Contains(pieceHealth, repairSystem.BrokenPieces);
        }

        [Test]
        public void RepairSystem_RepairPiece_MovesToActiveList()
        {
            pieceHealth.BreakPiece();
            repairSystem.RegisterPiece(pieceHealth);
            Assert.AreEqual(1, repairSystem.BrokenPieceCount);

            repairSystem.EnterRestRoom();
            bool success = repairSystem.RepairPiece(pieceHealth);

            Assert.IsTrue(success, "Repair should succeed");
            Assert.AreEqual(0, repairSystem.BrokenPieceCount);
            Assert.Contains(pieceHealth, repairSystem.ActivePieces);
        }

        [Test]
        public void RepairSystem_RepairLimit_RespectedPerRest()
        {
            // Create multiple broken pieces
            var piece2 = new GameObject("Piece2").AddComponent<Piece>();
            piece2.Initialize(PieceType.Knight, Team.White, new Vector2Int(1, 0));
            var health2 = piece2.gameObject.AddComponent<PieceHealth>();

            var piece3 = new GameObject("Piece3").AddComponent<Piece>();
            piece3.Initialize(PieceType.Bishop, Team.White, new Vector2Int(2, 0));
            var health3 = piece3.gameObject.AddComponent<PieceHealth>();

            // Break all pieces
            pieceHealth.BreakPiece();
            health2.BreakPiece();
            health3.BreakPiece();

            repairSystem.RegisterPiece(pieceHealth);
            repairSystem.RegisterPiece(health2);
            repairSystem.RegisterPiece(health3);

            Assert.AreEqual(3, repairSystem.BrokenPieceCount);

            // Enter rest room (max 2 repairs)
            repairSystem.EnterRestRoom();

            // Repair first piece
            bool repair1 = repairSystem.RepairPiece(pieceHealth);
            Assert.IsTrue(repair1);
            Assert.AreEqual(1, repairSystem.RepairsRemaining);

            // Repair second piece
            bool repair2 = repairSystem.RepairPiece(health2);
            Assert.IsTrue(repair2);
            Assert.AreEqual(0, repairSystem.RepairsRemaining);

            // Third repair should fail
            bool repair3 = repairSystem.RepairPiece(health3);
            Assert.IsFalse(repair3);
            Assert.IsTrue(health3.IsBroken, "Third piece should still be broken");

            Object.DestroyImmediate(piece2.gameObject);
            Object.DestroyImmediate(piece3.gameObject);
        }

        [Test]
        public void RepairSystem_EnterRestRoom_ResetsRepairCount()
        {
            pieceHealth.BreakPiece();
            repairSystem.RegisterPiece(pieceHealth);

            repairSystem.EnterRestRoom();
            Assert.AreEqual(2, repairSystem.RepairsRemaining); // Assuming max is 2

            repairSystem.RepairPiece(pieceHealth);
            Assert.AreEqual(1, repairSystem.RepairsRemaining);

            // Exit and re-enter
            repairSystem.ExitRestRoom();
            repairSystem.EnterRestRoom();

            Assert.AreEqual(2, repairSystem.RepairsRemaining, "Repairs should reset");
        }

        [Test]
        public void RepairSystem_IsKingBroken_DetectsKing()
        {
            var kingObject = new GameObject("King");
            var kingPiece = kingObject.AddComponent<Piece>();
            kingPiece.Initialize(PieceType.King, Team.White, new Vector2Int(4, 0));
            var kingHealth = kingObject.AddComponent<PieceHealth>();

            repairSystem.RegisterPiece(kingHealth);
            Assert.IsFalse(repairSystem.IsKingBroken(Team.White));

            kingHealth.BreakPiece();
            Assert.IsTrue(repairSystem.IsKingBroken(Team.White));

            Object.DestroyImmediate(kingObject);
        }

        [Test]
        public void RepairSystem_GetBrokenPiecesByTeam_FiltersCorrectly()
        {
            var whitePiece = pieceHealth;
            whitePiece.BreakPiece();

            var blackPieceObject = new GameObject("BlackPiece");
            var blackPiece = blackPieceObject.AddComponent<Piece>();
            blackPiece.Initialize(PieceType.Pawn, Team.Black, new Vector2Int(0, 6));
            var blackHealth = blackPieceObject.AddComponent<PieceHealth>();
            blackHealth.BreakPiece();

            repairSystem.RegisterPiece(whitePiece);
            repairSystem.RegisterPiece(blackHealth);

            var whiteBroken = repairSystem.GetBrokenPiecesByTeam(Team.White);
            var blackBroken = repairSystem.GetBrokenPiecesByTeam(Team.Black);

            Assert.AreEqual(1, whiteBroken.Count);
            Assert.AreEqual(1, blackBroken.Count);
            Assert.Contains(whitePiece, whiteBroken);
            Assert.Contains(blackHealth, blackBroken);

            Object.DestroyImmediate(blackPieceObject);
        }

        #endregion

        #region PieceUpgrade Tests

        [Test]
        public void PieceUpgrade_InitialLevel_IsZero()
        {
            var upgrade = pieceObject.AddComponent<PieceUpgrade>();

            Assert.AreEqual(0, upgrade.UpgradeLevel);
        }

        [Test]
        public void PieceUpgrade_GetUpgradeValue_ReflectsLevel()
        {
            var upgrade = pieceObject.AddComponent<PieceUpgrade>();

            float initialValue = upgrade.GetUpgradeValue();
            Assert.AreEqual(0f, initialValue);

            // Upgrade is increased through mutations in real usage
            // Just testing the value calculation works
        }

        #endregion
    }
}
