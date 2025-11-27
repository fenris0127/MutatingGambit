using UnityEngine;
using MutatingGambit.AI;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// ScriptableObject that defines a boss encounter configuration.
    /// Includes boss AI settings, special abilities, and rewards.
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "Mutating Gambit/Boss Data")]
    public class BossData : ScriptableObject
    {
        [Header("Boss Info")]
        [SerializeField]
        [Tooltip("The name of the boss.")]
        private string bossName = "The Chess Master";

        [SerializeField]
        [TextArea(2, 4)]
        [Tooltip("Flavor text describing the boss.")]
        private string bossDescription = "A legendary opponent with unparalleled strategic prowess.";

        [SerializeField]
        [Tooltip("Icon/portrait for the boss.")]
        private Sprite bossIcon;

        [Header("AI Configuration")]
        [SerializeField]
        [Tooltip("AI configuration for this boss.")]
        private AIConfig aiConfig;

        [Header("Special Abilities")]
        [SerializeField]
        [Tooltip("Can this boss use special abilities?")]
        private bool canUseSpecialAbilities = true;

        [SerializeField]
        [Tooltip("Number of times the boss can use special abilities.")]
        [Range(1, 10)]
        private int specialAbilityCharges = 3;

        [SerializeField]
        [Tooltip("Chance to use special ability when conditions are met (0-1).")]
        [Range(0f, 1f)]
        private float abilityUsageChance = 0.7f;

        [Header("Boss Pieces Setup")]
        [SerializeField]
        [Tooltip("Initial piece setup for the boss (if custom).")]
        private PieceSetupData[] bossPieces;

        [SerializeField]
        [Tooltip("If true, uses standard chess starting position for boss.")]
        private bool useStandardBossSetup = true;

        [SerializeField]
        [Tooltip("Mutations applied to boss pieces at the start.")]
        private MutationSetupData[] bossMutations;

        [SerializeField]
        [Tooltip("Artifacts the boss has active during the fight.")]
        private Artifact[] bossArtifacts;

        [Header("Victory Condition")]
        [SerializeField]
        [Tooltip("Victory condition for defeating this boss.")]
        private VictoryCondition victoryCondition;

        [Header("Rewards")]
        [SerializeField]
        [Tooltip("Possible artifact rewards for defeating the boss.")]
        private Artifact[] rewardArtifacts;

        [SerializeField]
        [Tooltip("Possible mutation rewards for defeating the boss.")]
        private Mutation[] rewardMutations;

        [SerializeField]
        [Tooltip("Currency reward for defeating this boss.")]
        private int currencyReward = 100;

        [SerializeField]
        [Tooltip("Number of rewards player can choose.")]
        [Range(1, 5)]
        private int numberOfRewardsToChoose = 2;

        /// <summary>
        /// Gets the boss name.
        /// </summary>
        public string BossName => bossName;

        /// <summary>
        /// Gets the boss description.
        /// </summary>
        public string BossDescription => bossDescription;

        /// <summary>
        /// Gets the boss icon.
        /// </summary>
        public Sprite BossIcon => bossIcon;

        /// <summary>
        /// Gets the AI configuration.
        /// </summary>
        public AIConfig AIConfig => aiConfig;

        /// <summary>
        /// Gets whether the boss can use special abilities.
        /// </summary>
        public bool CanUseSpecialAbilities => canUseSpecialAbilities;

        /// <summary>
        /// Gets the number of special ability charges.
        /// </summary>
        public int SpecialAbilityCharges => specialAbilityCharges;

        /// <summary>
        /// Gets the chance to use abilities.
        /// </summary>
        public float AbilityUsageChance => abilityUsageChance;

        /// <summary>
        /// Gets the boss piece setup data.
        /// </summary>
        public PieceSetupData[] BossPieces => bossPieces;

        /// <summary>
        /// Gets whether to use standard boss setup.
        /// </summary>
        public bool UseStandardBossSetup => useStandardBossSetup;

        /// <summary>
        /// Gets the boss mutation setup data.
        /// </summary>
        public MutationSetupData[] BossMutations => bossMutations;

        /// <summary>
        /// Gets the artifacts the boss has active.
        /// </summary>
        public Artifact[] BossArtifacts => bossArtifacts;

        /// <summary>
        /// Gets the victory condition.
        /// </summary>
        public VictoryCondition VictoryCondition => victoryCondition;

        /// <summary>
        /// Gets the artifact rewards.
        /// </summary>
        public Artifact[] RewardArtifacts => rewardArtifacts;

        /// <summary>
        /// Gets the mutation rewards.
        /// </summary>
        public Mutation[] RewardMutations => rewardMutations;

        /// <summary>
        /// Gets the currency reward.
        /// </summary>
        public int CurrencyReward => currencyReward;

        /// <summary>
        /// Gets the number of rewards player can choose.
        /// </summary>
        public int NumberOfRewardsToChoose => numberOfRewardsToChoose;

        /// <summary>
        /// Validates the boss data.
        /// </summary>
        public bool Validate()
        {
            if (aiConfig == null)
            {
                Debug.LogError($"Boss '{bossName}' has no AI config assigned.");
                return false;
            }

            if (useStandardBossSetup && (bossPieces != null && bossPieces.Length > 0))
            {
                Debug.LogWarning($"Boss '{bossName}' has useStandardBossSetup enabled but also has custom pieces defined.");
            }

            if (victoryCondition == null)
            {
                Debug.LogWarning($"Boss '{bossName}' has no victory condition assigned.");
            }

            return true;
        }

        /// <summary>
        /// Returns a string representation for debugging.
        /// </summary>
        public override string ToString()
        {
            return $"BossData: {bossName} (AI: {aiConfig?.ConfigName ?? "None"})";
        }
    }
}
