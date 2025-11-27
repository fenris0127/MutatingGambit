using UnityEngine;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// Defines the type of reward.
    /// </summary>
    public enum RewardType
    {
        Artifact,      // Player receives an artifact
        Mutation,      // Player receives a mutation to apply to a piece
        Currency,      // Player receives gold/currency
        PieceRepair,   // Repair broken pieces
        PieceUpgrade,  // Upgrade a specific piece
        HealthRestore  // Restore piece health (if health system exists)
    }

    /// <summary>
    /// ScriptableObject that defines a reward package.
    /// Used for room completion rewards, treasure rooms, and boss defeats.
    /// </summary>
    [CreateAssetMenu(fileName = "RewardData", menuName = "Mutating Gambit/Reward Data")]
    public class RewardData : ScriptableObject
    {
        [Header("Reward Info")]
        [SerializeField]
        [Tooltip("Name of this reward package.")]
        private string rewardName = "Basic Reward";

        [SerializeField]
        [TextArea(2, 3)]
        [Tooltip("Description of this reward.")]
        private string description = "A standard reward for clearing a room.";

        [Header("Reward Configuration")]
        [SerializeField]
        [Tooltip("How many rewards can the player choose from this package?")]
        [Range(1, 5)]
        private int numberOfChoices = 1;

        [SerializeField]
        [Tooltip("How many reward options to present to the player?")]
        [Range(1, 10)]
        private int numberOfOptions = 3;

        [Header("Artifact Rewards")]
        [SerializeField]
        [Tooltip("Pool of possible artifact rewards.")]
        private Artifact[] possibleArtifacts;

        [SerializeField]
        [Tooltip("Weight for selecting artifacts (higher = more likely).")]
        [Range(0f, 100f)]
        private float artifactWeight = 50f;

        [Header("Mutation Rewards")]
        [SerializeField]
        [Tooltip("Pool of possible mutation rewards.")]
        private Mutation[] possibleMutations;

        [SerializeField]
        [Tooltip("Weight for selecting mutations (higher = more likely).")]
        [Range(0f, 100f)]
        private float mutationWeight = 30f;

        [Header("Currency Rewards")]
        [SerializeField]
        [Tooltip("Should currency be offered as a reward option?")]
        private bool includeCurrency = true;

        [SerializeField]
        [Tooltip("Minimum currency reward.")]
        private int minCurrency = 10;

        [SerializeField]
        [Tooltip("Maximum currency reward.")]
        private int maxCurrency = 50;

        [SerializeField]
        [Tooltip("Weight for selecting currency (higher = more likely).")]
        [Range(0f, 100f)]
        private float currencyWeight = 20f;

        [Header("Special Rewards")]
        [SerializeField]
        [Tooltip("Should piece repair be offered as a reward option?")]
        private bool includePieceRepair = false;

        [SerializeField]
        [Tooltip("Number of pieces that can be repaired.")]
        [Range(1, 5)]
        private int piecesRepairable = 1;

        [SerializeField]
        [Tooltip("Weight for selecting piece repair (higher = more likely).")]
        [Range(0f, 100f)]
        private float repairWeight = 10f;

        /// <summary>
        /// Gets the name of this reward package.
        /// </summary>
        public string RewardName => rewardName;

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Gets the number of rewards the player can choose.
        /// </summary>
        public int NumberOfChoices => numberOfChoices;

        /// <summary>
        /// Gets the number of reward options to present.
        /// </summary>
        public int NumberOfOptions => numberOfOptions;

        /// <summary>
        /// Gets the possible artifact rewards.
        /// </summary>
        public Artifact[] PossibleArtifacts => possibleArtifacts;

        /// <summary>
        /// Gets the artifact selection weight.
        /// </summary>
        public float ArtifactWeight => artifactWeight;

        /// <summary>
        /// Gets the possible mutation rewards.
        /// </summary>
        public Mutation[] PossibleMutations => possibleMutations;

        /// <summary>
        /// Gets the mutation selection weight.
        /// </summary>
        public float MutationWeight => mutationWeight;

        /// <summary>
        /// Gets whether currency should be included.
        /// </summary>
        public bool IncludeCurrency => includeCurrency;

        /// <summary>
        /// Gets the minimum currency reward.
        /// </summary>
        public int MinCurrency => minCurrency;

        /// <summary>
        /// Gets the maximum currency reward.
        /// </summary>
        public int MaxCurrency => maxCurrency;

        /// <summary>
        /// Gets the currency selection weight.
        /// </summary>
        public float CurrencyWeight => currencyWeight;

        /// <summary>
        /// Gets whether piece repair should be included.
        /// </summary>
        public bool IncludePieceRepair => includePieceRepair;

        /// <summary>
        /// Gets the number of pieces that can be repaired.
        /// </summary>
        public int PiecesRepairable => piecesRepairable;

        /// <summary>
        /// Gets the repair selection weight.
        /// </summary>
        public float RepairWeight => repairWeight;

        /// <summary>
        /// Gets the total weight of all reward types.
        /// Used for weighted random selection.
        /// </summary>
        public float TotalWeight
        {
            get
            {
                float total = 0f;

                if (possibleArtifacts != null && possibleArtifacts.Length > 0)
                    total += artifactWeight;

                if (possibleMutations != null && possibleMutations.Length > 0)
                    total += mutationWeight;

                if (includeCurrency)
                    total += currencyWeight;

                if (includePieceRepair)
                    total += repairWeight;

                return total;
            }
        }

        /// <summary>
        /// Generates a random reward type based on weights.
        /// </summary>
        public RewardType GetRandomRewardType()
        {
            float totalWeight = TotalWeight;
            if (totalWeight <= 0f)
            {
                return RewardType.Currency;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            // Check artifacts
            if (possibleArtifacts != null && possibleArtifacts.Length > 0)
            {
                currentWeight += artifactWeight;
                if (randomValue <= currentWeight)
                    return RewardType.Artifact;
            }

            // Check mutations
            if (possibleMutations != null && possibleMutations.Length > 0)
            {
                currentWeight += mutationWeight;
                if (randomValue <= currentWeight)
                    return RewardType.Mutation;
            }

            // Check currency
            if (includeCurrency)
            {
                currentWeight += currencyWeight;
                if (randomValue <= currentWeight)
                    return RewardType.Currency;
            }

            // Check repair
            if (includePieceRepair)
            {
                currentWeight += repairWeight;
                if (randomValue <= currentWeight)
                    return RewardType.PieceRepair;
            }

            return RewardType.Currency; // Fallback
        }

        /// <summary>
        /// Gets a random artifact from the possible artifacts.
        /// </summary>
        public Artifact GetRandomArtifact()
        {
            if (possibleArtifacts == null || possibleArtifacts.Length == 0)
                return null;

            return possibleArtifacts[Random.Range(0, possibleArtifacts.Length)];
        }

        /// <summary>
        /// Gets a random mutation from the possible mutations.
        /// </summary>
        public Mutation GetRandomMutation()
        {
            if (possibleMutations == null || possibleMutations.Length == 0)
                return null;

            return possibleMutations[Random.Range(0, possibleMutations.Length)];
        }

        /// <summary>
        /// Gets a random currency amount.
        /// </summary>
        public int GetRandomCurrency()
        {
            return Random.Range(minCurrency, maxCurrency + 1);
        }

        /// <summary>
        /// Validates the reward data.
        /// </summary>
        public bool Validate()
        {
            if (numberOfChoices > numberOfOptions)
            {
                Debug.LogWarning($"Reward '{rewardName}': Number of choices ({numberOfChoices}) is greater than number of options ({numberOfOptions}).");
                numberOfChoices = numberOfOptions;
            }

            if (TotalWeight <= 0f)
            {
                Debug.LogError($"Reward '{rewardName}': No valid reward options configured (total weight is 0).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"RewardData: {rewardName} ({numberOfChoices} of {numberOfOptions} choices)";
        }
    }
}
