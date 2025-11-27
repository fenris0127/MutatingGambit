using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Central library for all mutations in the game.
    /// Provides querying, filtering, and synergy detection.
    /// </summary>
    [CreateAssetMenu(fileName = "MutationLibrary", menuName = "MutatingGambit/Mutation Library")]
    public class MutationLibrary : ScriptableObject
    {
        [SerializeField]
        private List<Mutation> allMutations = new List<Mutation>();

        /// <summary>
        /// Gets all mutations in the library.
        /// </summary>
        public List<Mutation> AllMutations => allMutations;

        /// <summary>
        /// Gets mutations by rarity.
        /// </summary>
        public List<Mutation> GetMutationsByRarity(MutationRarity rarity)
        {
            return allMutations.Where(m => m.Rarity == rarity).ToList();
        }

        /// <summary>
        /// Gets mutations by tag.
        /// </summary>
        public List<Mutation> GetMutationsByTag(MutationTag tag)
        {
            return allMutations.Where(m => m.Tags != null && m.Tags.Contains(tag)).ToList();
        }

        /// <summary>
        /// Gets mutations compatible with a piece type.
        /// </summary>
        public List<Mutation> GetCompatibleMutations(Core.ChessEngine.PieceType pieceType)
        {
            return allMutations.Where(m => m.IsCompatibleWith(pieceType)).ToList();
        }

        /// <summary>
        /// Gets random mutations weighted by rarity.
        /// </summary>
        public List<Mutation> GetRandomMutations(int count, MutationRarity maxRarity = MutationRarity.Legendary)
        {
            var eligible = allMutations.Where(m => m.Rarity <= maxRarity).ToList();
            var result = new List<Mutation>();

            for (int i = 0; i < count && eligible.Count > 0; i++)
            {
                var mutation = GetWeightedRandomMutation(eligible);
                result.Add(mutation);
                eligible.Remove(mutation); // No duplicates
            }

            return result;
        }

        private Mutation GetWeightedRandomMutation(List<Mutation> mutations)
        {
            // Weight: Common=100, Rare=50, Epic=20, Legendary=5
            float totalWeight = 0f;
            var weights = new Dictionary<Mutation, float>();

            foreach (var mutation in mutations)
            {
                float weight = mutation.Rarity switch
                {
                    MutationRarity.Common => 100f,
                    MutationRarity.Rare => 50f,
                    MutationRarity.Epic => 20f,
                    MutationRarity.Legendary => 5f,
                    _ => 50f
                };
                weights[mutation] = weight;
                totalWeight += weight;
            }

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var kvp in weights)
            {
                currentWeight += kvp.Value;
                if (randomValue <= currentWeight)
                {
                    return kvp.Key;
                }
            }

            return mutations[mutations.Count - 1]; // Fallback
        }

        /// <summary>
        /// Detects synergies between a list of mutations.
        /// </summary>
        public List<MutationSynergy> DetectSynergies(List<Mutation> activeMutations)
        {
            var synergies = new List<MutationSynergy>();

            foreach (var mutation in activeMutations)
            {
                if (mutation.SynergyMutations == null) continue;

                foreach (var synergyMutation in mutation.SynergyMutations)
                {
                    if (activeMutations.Contains(synergyMutation))
                    {
                        synergies.Add(new MutationSynergy
                        {
                            mutation1 = mutation,
                            mutation2 = synergyMutation,
                            bonusDescription = $"Synergy: {mutation.MutationName} + {synergyMutation.MutationName}"
                        });
                    }
                }
            }

            return synergies;
        }

        /// <summary>
        /// Adds a mutation to the library.
        /// </summary>
        public void AddMutation(Mutation mutation)
        {
            if (!allMutations.Contains(mutation))
            {
                allMutations.Add(mutation);
            }
        }
    }

    /// <summary>
    /// Represents a synergy between two mutations.
    /// </summary>
    [System.Serializable]
    public class MutationSynergy
    {
        public Mutation mutation1;
        public Mutation mutation2;
        public string bonusDescription;
    }
}
