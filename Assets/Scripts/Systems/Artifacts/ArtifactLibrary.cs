using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Central library for all artifacts in the game.
    /// Provides querying, filtering, and synergy detection.
    /// </summary>
    [CreateAssetMenu(fileName = "ArtifactLibrary", menuName = "MutatingGambit/Artifact Library")]
    public class ArtifactLibrary : ScriptableObject
    {
        [SerializeField]
        private List<Artifact> allArtifacts = new List<Artifact>();

        /// <summary>
        /// Gets all artifacts in the library.
        /// </summary>
        public List<Artifact> AllArtifacts => allArtifacts;

        /// <summary>
        /// Gets an artifact by its name.
        /// </summary>
        public Artifact GetArtifactByName(string name)
        {
            return allArtifacts.FirstOrDefault(a => a.ArtifactName == name);
        }

        /// <summary>
        /// Gets artifacts by rarity.
        /// </summary>
        public List<Artifact> GetArtifactsByRarity(ArtifactRarity rarity)
        {
            return allArtifacts.Where(a => a.Rarity == rarity).ToList();
        }

        /// <summary>
        /// Gets artifacts by tag.
        /// </summary>
        public List<Artifact> GetArtifactsByTag(ArtifactTag tag)
        {
            return allArtifacts.Where(a => a.Tags != null && a.Tags.Contains(tag)).ToList();
        }

        /// <summary>
        /// Gets artifacts by trigger type.
        /// </summary>
        public List<Artifact> GetArtifactsByTrigger(ArtifactTrigger trigger)
        {
            return allArtifacts.Where(a => a.Trigger == trigger).ToList();
        }

        /// <summary>
        /// Gets random artifacts weighted by rarity.
        /// </summary>
        public List<Artifact> GetRandomArtifacts(int count, ArtifactRarity maxRarity = ArtifactRarity.Legendary)
        {
            var eligible = allArtifacts.Where(a => a.Rarity <= maxRarity).ToList();
            var result = new List<Artifact>();

            for (int i = 0; i < count && eligible.Count > 0; i++)
            {
                var artifact = GetWeightedRandomArtifact(eligible);
                result.Add(artifact);
                eligible.Remove(artifact);
            }

            return result;
        }

        private Artifact GetWeightedRandomArtifact(List<Artifact> artifacts)
        {
            float totalWeight = 0f;
            var weights = new Dictionary<Artifact, float>();

            foreach (var artifact in artifacts)
            {
                float weight = artifact.Rarity switch
                {
                    ArtifactRarity.Common => 100f,
                    ArtifactRarity.Uncommon => 70f,
                    ArtifactRarity.Rare => 40f,
                    ArtifactRarity.Epic => 15f,
                    ArtifactRarity.Legendary => 5f,
                    _ => 50f
                };
                weights[artifact] = weight;
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

            return artifacts[artifacts.Count - 1];
        }

        /// <summary>
        /// Detects synergies between artifacts.
        /// </summary>
        public List<ArtifactSynergy> DetectSynergies(List<Artifact> activeArtifacts)
        {
            var synergies = new List<ArtifactSynergy>();

            foreach (var artifact in activeArtifacts)
            {
                if (artifact.SynergyArtifacts == null) continue;

                foreach (var synergyArtifact in artifact.SynergyArtifacts)
                {
                    if (activeArtifacts.Contains(synergyArtifact))
                    {
                        synergies.Add(new ArtifactSynergy
                        {
                            artifact1 = artifact,
                            artifact2 = synergyArtifact,
                            bonusDescription = $"Synergy: {artifact.ArtifactName} + {synergyArtifact.ArtifactName}"
                        });
                    }
                }
            }

            return synergies;
        }

        /// <summary>
        /// Adds an artifact to the library.
        /// </summary>
        public void AddArtifact(Artifact artifact)
        {
            if (!allArtifacts.Contains(artifact))
            {
                allArtifacts.Add(artifact);
            }
        }
    }

    /// <summary>
    /// Represents a synergy between two artifacts.
    /// </summary>
    [System.Serializable]
    public class ArtifactSynergy
    {
        public Artifact artifact1;
        public Artifact artifact2;
        public string bonusDescription;
        public float bonusMultiplier = 1.2f; // 20% bonus by default
    }
}
