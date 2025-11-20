using System;
using System.Collections.Generic;
using System.Linq;
using MutatingGambit.Core.Mutations;
using MutatingGambit.Core.Artifacts;

namespace MutatingGambit.Core.Meta
{
    /// <summary>
    /// Manages meta progression across runs
    /// </summary>
    public class MetaProgression
    {
        private int _currency;
        private HashSet<string> _unlockedMutations;
        private HashSet<string> _unlockedArtifacts;
        private HashSet<string> _unlockedFeatures;
        private int _totalRuns;
        private int _wins;
        private int _bestLayer;

        // Base currency name: "기보 조각" (Gambit Fragments)
        public const string CURRENCY_NAME = "Gambit Fragments";

        public MetaProgression()
        {
            _currency = 0;
            _unlockedMutations = new HashSet<string>();
            _unlockedArtifacts = new HashSet<string>();
            _unlockedFeatures = new HashSet<string>();
            _totalRuns = 0;
            _wins = 0;
            _bestLayer = 0;

            // Start with basic unlocks
            InitializeStartingUnlocks();
        }

        private void InitializeStartingUnlocks()
        {
            // Players start with 3 basic mutations unlocked
            _unlockedMutations.Add(nameof(LeapingRookMutation));
            _unlockedMutations.Add(nameof(SplittingKnightMutation));
            _unlockedMutations.Add(nameof(GlassBishopMutation));

            // And 3 basic artifacts
            _unlockedArtifacts.Add(nameof(KingsShadowArtifact));
            _unlockedArtifacts.Add(nameof(CavalryChargeArtifact));
            _unlockedArtifacts.Add(nameof(PromotionPrivilegeArtifact));
        }

        #region Currency System

        public int GetCurrency()
        {
            return _currency;
        }

        public void AddCurrency(int amount)
        {
            _currency += Math.Max(0, amount);
        }

        public bool SpendCurrency(int amount)
        {
            if (_currency >= amount)
            {
                _currency -= amount;
                return true;
            }
            return false;
        }

        public int CalculateCurrencyReward(int layerReached, bool won)
        {
            int baseReward = layerReached * 20; // 20 per layer

            if (won)
            {
                baseReward += 100; // Victory bonus
            }

            return baseReward;
        }

        #endregion

        #region Unlock System

        public bool UnlockContent(Mutation mutation)
        {
            if (mutation == null)
                return false;

            string key = mutation.GetType().Name;

            if (_unlockedMutations.Contains(key))
                return false; // Already unlocked

            if (SpendCurrency(mutation.Cost))
            {
                _unlockedMutations.Add(key);
                return true;
            }

            return false;
        }

        public bool UnlockContent(Artifact artifact)
        {
            if (artifact == null)
                return false;

            string key = artifact.GetType().Name;

            if (_unlockedArtifacts.Contains(key))
                return false; // Already unlocked

            if (SpendCurrency(artifact.Cost))
            {
                _unlockedArtifacts.Add(key);
                return true;
            }

            return false;
        }

        public bool UnlockFeature(string featureName)
        {
            if (_unlockedFeatures.Contains(featureName))
                return false;

            int cost = GetFeatureCost(featureName);
            if (SpendCurrency(cost))
            {
                _unlockedFeatures.Add(featureName);
                return true;
            }

            return false;
        }

        public bool IsUnlocked(Mutation mutation)
        {
            if (mutation == null)
                return false;
            return _unlockedMutations.Contains(mutation.GetType().Name);
        }

        public bool IsUnlocked(Artifact artifact)
        {
            if (artifact == null)
                return false;
            return _unlockedArtifacts.Contains(artifact.GetType().Name);
        }

        public bool IsFeatureUnlocked(string featureName)
        {
            return _unlockedFeatures.Contains(featureName);
        }

        private int GetFeatureCost(string featureName)
        {
            // QoL features cost
            switch (featureName)
            {
                case "MoveUndo":
                    return 50;
                case "SaveAnywhere":
                    return 75;
                case "AIHints":
                    return 100;
                default:
                    return 50;
            }
        }

        #endregion

        #region Progression Tracking

        public void RecordRunComplete(int layerReached, bool won)
        {
            _totalRuns++;

            if (won)
            {
                _wins++;
            }

            if (layerReached > _bestLayer)
            {
                _bestLayer = layerReached;
            }

            // Award currency
            int reward = CalculateCurrencyReward(layerReached, won);
            AddCurrency(reward);
        }

        public int GetTotalRuns()
        {
            return _totalRuns;
        }

        public int GetWins()
        {
            return _wins;
        }

        public int GetBestLayer()
        {
            return _bestLayer;
        }

        public int GetTotalUnlockables()
        {
            // 3 starting + 6 advanced mutations
            // 3 starting + 6 advanced artifacts
            // 3 QoL features
            return 21;
        }

        public int GetUnlockedCount()
        {
            return _unlockedMutations.Count + _unlockedArtifacts.Count + _unlockedFeatures.Count;
        }

        public float GetCompletionPercentage()
        {
            return (float)GetUnlockedCount() / GetTotalUnlockables() * 100f;
        }

        #endregion

        #region Save/Load System

        public string Save()
        {
            var saveData = new SaveData
            {
                Currency = _currency,
                UnlockedMutations = _unlockedMutations.ToList(),
                UnlockedArtifacts = _unlockedArtifacts.ToList(),
                UnlockedFeatures = _unlockedFeatures.ToList(),
                TotalRuns = _totalRuns,
                Wins = _wins,
                BestLayer = _bestLayer
            };

            return System.Text.Json.JsonSerializer.Serialize(saveData);
        }

        public void Load(string saveDataJson)
        {
            if (string.IsNullOrEmpty(saveDataJson))
                return;

            try
            {
                var saveData = System.Text.Json.JsonSerializer.Deserialize<SaveData>(saveDataJson);

                _currency = saveData.Currency;
                _unlockedMutations = new HashSet<string>(saveData.UnlockedMutations);
                _unlockedArtifacts = new HashSet<string>(saveData.UnlockedArtifacts);
                _unlockedFeatures = new HashSet<string>(saveData.UnlockedFeatures);
                _totalRuns = saveData.TotalRuns;
                _wins = saveData.Wins;
                _bestLayer = saveData.BestLayer;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Failed to load save data: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Serializable save data structure
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int Currency { get; set; }
        public List<string> UnlockedMutations { get; set; }
        public List<string> UnlockedArtifacts { get; set; }
        public List<string> UnlockedFeatures { get; set; }
        public int TotalRuns { get; set; }
        public int Wins { get; set; }
        public int BestLayer { get; set; }
    }
}
