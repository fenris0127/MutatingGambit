using NUnit.Framework;
using MutatingGambit.Core;
using MutatingGambit.Core.Meta;
using MutatingGambit.Core.Mutations;
using MutatingGambit.Core.Artifacts;

namespace MutatingGambit.Tests
{
    /// <summary>
    /// Phase 10: Meta Progression System Tests
    /// </summary>
    [TestFixture]
    public class Phase10_MetaProgressionTests
    {
        #region Currency System Tests

        [Test]
        public void Currency_CanBeEarnedOnGameOver()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            int initialCurrency = metaProgression.GetCurrency();

            // Act
            metaProgression.AddCurrency(100);

            // Assert
            Assert.AreEqual(initialCurrency + 100, metaProgression.GetCurrency());
        }

        [Test]
        public void Currency_EarnedBasedOnProgress()
        {
            // Arrange
            var metaProgression = new MetaProgression();

            // Act
            int layer1Reward = metaProgression.CalculateCurrencyReward(1, false);
            int layer5Reward = metaProgression.CalculateCurrencyReward(5, false);
            int bossReward = metaProgression.CalculateCurrencyReward(3, true);

            // Assert
            Assert.Greater(layer5Reward, layer1Reward, "Deeper layers should give more currency");
            Assert.Greater(bossReward, layer1Reward, "Boss victory should give bonus currency");
        }

        [Test]
        public void Currency_IsPersistentAcrossRuns()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(500);

            // Act
            int savedCurrency = metaProgression.GetCurrency();

            // Simulate new run (currency should persist)
            // In a real scenario, this would be loaded from save file

            // Assert
            Assert.AreEqual(500, savedCurrency);
        }

        #endregion

        #region Unlock System Tests

        [Test]
        public void Unlock_NewMutationsCanBeUnlocked()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(100);

            var mutation = new LeapingRookMutation();

            // Act
            bool unlocked = metaProgression.UnlockContent(mutation);

            // Assert
            Assert.IsTrue(unlocked);
            Assert.IsTrue(metaProgression.IsUnlocked(mutation));
            Assert.AreEqual(50, metaProgression.GetCurrency()); // Cost was 50
        }

        [Test]
        public void Unlock_NewArtifactsCanBeUnlocked()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(150);

            var artifact = new KingsShadowArtifact();

            // Act
            bool unlocked = metaProgression.UnlockContent(artifact);

            // Assert
            Assert.IsTrue(unlocked);
            Assert.IsTrue(metaProgression.IsUnlocked(artifact));
            Assert.AreEqual(50, metaProgression.GetCurrency()); // Cost was 100
        }

        [Test]
        public void Unlock_RequiresSufficientCurrency()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(30); // Not enough for 50 cost mutation

            var mutation = new LeapingRookMutation();

            // Act
            bool unlocked = metaProgression.UnlockContent(mutation);

            // Assert
            Assert.IsFalse(unlocked);
            Assert.IsFalse(metaProgression.IsUnlocked(mutation));
            Assert.AreEqual(30, metaProgression.GetCurrency()); // Currency unchanged
        }

        [Test]
        public void Unlock_QoLFeaturesCanBeUnlocked()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(50);

            // Act
            bool unlocked = metaProgression.UnlockFeature("MoveUndo");

            // Assert
            Assert.IsTrue(unlocked);
            Assert.IsTrue(metaProgression.IsFeatureUnlocked("MoveUndo"));
        }

        #endregion

        #region Save/Load System Tests

        [Test]
        public void SaveLoad_ProgressCanBeSaved()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(1000);
            metaProgression.UnlockContent(new LeapingRookMutation());

            // Act
            var saveData = metaProgression.Save();

            // Assert
            Assert.IsNotNull(saveData);
            Assert.IsNotEmpty(saveData);
        }

        [Test]
        public void SaveLoad_ProgressCanBeLoaded()
        {
            // Arrange
            var original = new MetaProgression();
            original.AddCurrency(1000);
            original.UnlockContent(new LeapingRookMutation());

            var saveData = original.Save();

            // Act
            var loaded = new MetaProgression();
            loaded.Load(saveData);

            // Assert
            Assert.AreEqual(original.GetCurrency(), loaded.GetCurrency());
            Assert.IsTrue(loaded.IsUnlocked(new LeapingRookMutation()));
        }

        [Test]
        public void SaveLoad_PreservesMetaProgress()
        {
            // Arrange
            var metaProgression = new MetaProgression();
            metaProgression.AddCurrency(500);
            metaProgression.UnlockFeature("MoveUndo");

            var saveData = metaProgression.Save();

            // Act
            var loaded = new MetaProgression();
            loaded.Load(saveData);

            // Assert
            Assert.AreEqual(500, loaded.GetCurrency());
            Assert.IsTrue(loaded.IsFeatureUnlocked("MoveUndo"));
        }

        #endregion

        #region Progression Tracking Tests

        [Test]
        public void Progression_TracksRunStatistics()
        {
            // Arrange
            var metaProgression = new MetaProgression();

            // Act
            metaProgression.RecordRunComplete(5, true); // Won at layer 5
            metaProgression.RecordRunComplete(3, false); // Lost at layer 3

            // Assert
            Assert.AreEqual(2, metaProgression.GetTotalRuns());
            Assert.AreEqual(1, metaProgression.GetWins());
            Assert.AreEqual(5, metaProgression.GetBestLayer());
        }

        [Test]
        public void Progression_TracksUnlockProgress()
        {
            // Arrange
            var metaProgression = new MetaProgression();

            // Act
            int totalUnlockables = metaProgression.GetTotalUnlockables();
            int currentUnlocks = metaProgression.GetUnlockedCount();

            metaProgression.AddCurrency(1000);
            metaProgression.UnlockContent(new LeapingRookMutation());

            int newUnlocks = metaProgression.GetUnlockedCount();

            // Assert
            Assert.Greater(totalUnlockables, 0);
            Assert.AreEqual(currentUnlocks + 1, newUnlocks);
        }

        [Test]
        public void Progression_CalculatesCompletionPercentage()
        {
            // Arrange
            var metaProgression = new MetaProgression();

            // Act
            float initialProgress = metaProgression.GetCompletionPercentage();

            metaProgression.AddCurrency(10000);
            // Unlock several items
            metaProgression.UnlockContent(new LeapingRookMutation());
            metaProgression.UnlockContent(new SplittingKnightMutation());
            metaProgression.UnlockContent(new GlassBishopMutation());

            float newProgress = metaProgression.GetCompletionPercentage();

            // Assert
            Assert.GreaterOrEqual(initialProgress, 0);
            Assert.LessOrEqual(initialProgress, 100);
            Assert.Greater(newProgress, initialProgress);
        }

        #endregion
    }
}
