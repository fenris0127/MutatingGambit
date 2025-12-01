using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Extension methods for ArtifactManager.
    /// </summary>
    public static class ArtifactManagerExtensions
    {
        /// <summary>
        /// Gets all artifacts with a specific tag.
        /// </summary>
        public static List<Artifact> GetArtifactsByTag(this ArtifactManager manager, ArtifactTag tag)
        {
            // Would need access to active artifacts list
            return new List<Artifact>();
        }

        /// <summary>
        /// Calculates total synergy bonuses.
        /// </summary>
        public static float CalculateTotalSynergyBonus(this ArtifactManager manager, ArtifactLibrary library, List<Artifact> activeArtifacts)
        {
            var synergies = library.DetectSynergies(activeArtifacts);
            float totalBonus = 1f;
            
            foreach (var synergy in synergies)
            {
                totalBonus *= synergy.bonusMultiplier;
            }
            
            return totalBonus;
        }
    }

    /// <summary>
    /// Helper methods for artifact-related calculations.
    /// </summary>
    public static class ArtifactHelper
    {
        /// <summary>
        /// Gets the color associated with an artifact rarity.
        /// </summary>
        public static Color GetRarityColor(ArtifactRarity rarity)
        {
            return rarity switch
            {
                ArtifactRarity.Common => new Color(0.8f, 0.8f, 0.8f), // Light Gray
                ArtifactRarity.Uncommon => new Color(0.5f, 0.9f, 0.5f), // Green
                ArtifactRarity.Rare => new Color(0.4f, 0.7f, 1f), // Blue
                ArtifactRarity.Epic => new Color(0.9f, 0.4f, 1f), // Purple  
                ArtifactRarity.Legendary => new Color(1f, 0.9f, 0.3f), // Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// Gets a particle effect color for the rarity.
        /// </summary>
        public static Color GetRarityParticleColor(ArtifactRarity rarity)
        {
            var baseColor = GetRarityColor(rarity);
            return new Color(baseColor.r, baseColor.g, baseColor.b, 0.8f);
        }

        /// <summary>
        /// Gets a display name for an artifact tag (Korean).
        /// </summary>
        public static string GetTagDisplayName(ArtifactTag tag)
        {
            return tag switch
            {
                ArtifactTag.Global => "전역",
                ArtifactTag.Movement => "이동",
                ArtifactTag.Combat => "전투",
                ArtifactTag.Economic => "경제",
                ArtifactTag.Defensive => "방어",
                ArtifactTag.Aggressive => "공격",
                ArtifactTag.Chaos => "혼돈",
                ArtifactTag.Temporal => "시간",
                _ => tag.ToString()
            };
        }

        /// <summary>
        /// Calculates the combined effect multiplier from multiple artifacts.
        /// </summary>
        public static float CalculateCombinedMultiplier(List<Artifact> artifacts, ArtifactTag tag)
        {
            float multiplier = 1f;
            int count = artifacts.Count(a => a.Tags != null && a.Tags.Contains(tag));
            
            // Each artifact with the tag adds 15% multiplicatively
            for (int i = 0; i < count; i++)
            {
                multiplier *= 1.15f;
            }
            
            return multiplier;
        }

        /// <summary>
        /// Gets the shop price for an artifact based on rarity and current floor.
        /// </summary>
        public static int CalculateShopPrice(Artifact artifact, int currentFloor)
        {
            float basePrice = artifact.Rarity switch
            {
                ArtifactRarity.Common => 50f,
                ArtifactRarity.Uncommon => 100f,
                ArtifactRarity.Rare => 200f,
                ArtifactRarity.Epic => 400f,
                ArtifactRarity.Legendary => 800f,
                _ => 100f
            };

            // Price increases with floor
            float floorMultiplier = 1f + (currentFloor * 0.1f);
            
            return Mathf.RoundToInt(basePrice * floorMultiplier);
        }

        /// <summary>
        /// Checks if an artifact should appear in the reward pool based on floor.
        /// </summary>
        public static bool IsAvailableAtFloor(Artifact artifact, int currentFloor)
        {
            // Higher rarity artifacts unlock at higher floors
            int minFloor = artifact.Rarity switch
            {
                ArtifactRarity.Common => 1,
                ArtifactRarity.Uncommon => 1,
                ArtifactRarity.Rare => 2,
                ArtifactRarity.Epic => 3,
                ArtifactRarity.Legendary => 5,
                _ => 1
            };

            return currentFloor >= minFloor;
        }

        /// <summary>
        /// Gets artifacts that counter a specific enemy strategy.
        /// </summary>
        public static List<Artifact> GetCounterArtifacts(ArtifactLibrary library, string enemyStrategy)
        {
            // Example: if enemy is aggressive, suggest defensive artifacts
            return enemyStrategy switch
            {
                "Aggressive" => library.GetArtifactsByTag(ArtifactTag.Defensive),
                "Defensive" => library.GetArtifactsByTag(ArtifactTag.Aggressive),
                "Control" => library.GetArtifactsByTag(ArtifactTag.Chaos),
                _ => new List<Artifact>()
            };
        }

        /// <summary>
        /// Formats artifact description with dynamic values.
        /// </summary>
        public static string FormatDescription(Artifact artifact, Dictionary<string, string> values = null)
        {
            string desc = artifact.Description;
            
            if (values != null)
            {
                foreach (var kvp in values)
                {
                    desc = desc.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
            }
            
            return desc;
        }

        /// <summary>
        /// Generates a tooltip text for an artifact.
        /// </summary>
        public static string GenerateTooltip(Artifact artifact, bool includePrice = false, int currentFloor = 1)
        {
            var tooltip = $"<b>{artifact.ArtifactName}</b>n";
            tooltip += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor(artifact.Rarity))}>{artifact.Rarity}</color>nn";
            tooltip += $"{artifact.Description}nn";
            tooltip += $"<i>Trigger: {GetTriggerDisplayName(artifact.Trigger)}</i>";
            
            if (artifact.Tags != null && artifact.Tags.Length > 0)
            {
                tooltip += $"nTags: {string.Join(", ", artifact.Tags.Select(GetTagDisplayName))}";
            }

            if (includePrice)
            {
                int price = CalculateShopPrice(artifact, currentFloor);
                tooltip += $"nnPrice: {price} Gold";
            }

            return tooltip;
        }

        private static string GetTriggerDisplayName(ArtifactTrigger trigger)
        {
            return trigger switch
            {
                ArtifactTrigger.OnTurnStart => "턴 시작",
                ArtifactTrigger.OnTurnEnd => "턴 종료",
                ArtifactTrigger.OnPieceMove => "기물 이동",
                ArtifactTrigger.OnPieceCapture => "기물 포획",
                ArtifactTrigger.OnKingMove => "킹 이동",
                ArtifactTrigger.Passive => "상시",
                _ => trigger.ToString()
            };
        }
    }
}
