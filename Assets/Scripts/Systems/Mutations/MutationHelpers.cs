using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Extension methods for advanced mutation features.
    /// </summary>
    public static class MutationManagerExtensions
    {
        /// <summary>
        /// Gets the stack count of a specific mutation on a piece.
        /// </summary>
        public static int GetMutationStackCount(this MutationManager manager, Piece piece, Mutation mutation)
        {
            // Would need to expose mutationStacks dictionary or add a method in MutationManager
            return 1; // Placeholder
        }

        /// <summary>
        /// Gets all mutations of a specific rarity from all pieces.
        /// </summary>
        public static List<Mutation> GetMutationsByRarity(this MutationManager manager, Board board, MutationRarity rarity)
        {
            var result = new List<Mutation>();
            if (board != null)
            {
                var allPieces = board.GetAllPieces();
                foreach (var piece in allPieces)
                {
                    var mutations = manager.GetMutations(piece);
                    result.AddRange(mutations.Where(m => m.Rarity == rarity));
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets all mutations with a specific tag.
        /// </summary>
        public static List<Mutation> GetMutationsByTag(this MutationManager manager, Board board, MutationTag tag)
        {
            var result = new List<Mutation>();
            if (board != null)
            {
                var allPieces = board.GetAllPieces();
                foreach (var piece in allPieces)
                {
                    var mutations = manager.GetMutations(piece);
                    result.AddRange(mutations.Where(m => m.Tags != null && m.Tags.Contains(tag)));
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Checks if two mutations would create a synergy.
        /// </summary>
        public static bool HasSynergy(this Mutation mutation1, Mutation mutation2)
        {
            if (mutation1.SynergyMutations != null && mutation1.SynergyMutations.Contains(mutation2))
            {
                return true;
            }
            if (mutation2.SynergyMutations != null && mutation2.SynergyMutations.Contains(mutation1))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the total count of a specific mutation tag across all pieces.
        /// </summary>
        public static int GetTagCount(this MutationManager manager, Board board, MutationTag tag, Team team)
        {
            int count = 0;
            if (board != null)
            {
                var teamPieces = board.GetPiecesByTeam(team);
                foreach (var piece in teamPieces)
                {
                    var mutations = manager.GetMutations(piece);
                    count += mutations.Count(m => m.Tags != null && m.Tags.Contains(tag));
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Helper class for mutation-related calculations.
    /// </summary>
    public static class MutationHelper
    {
        /// <summary>
        /// Gets the color associated with a mutation rarity.
        /// </summary>
        public static Color GetRarityColor(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Common => new Color(0.7f, 0.7f, 0.7f), // Gray
                MutationRarity.Rare => new Color(0.3f, 0.6f, 1f), // Blue
                MutationRarity.Epic => new Color(0.8f, 0.3f, 0.9f), // Purple
                MutationRarity.Legendary => new Color(1f, 0.8f, 0.2f), // Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// Gets a descriptive name for a mutation tag.
        /// </summary>
        public static string GetTagDisplayName(MutationTag tag)
        {
            return tag switch
            {
                MutationTag.Movement => "이동",
                MutationTag.Attack => "공격",
                MutationTag.Defense => "방어",
                MutationTag.Utility => "유틸리티",
                MutationTag.Teleport => "순간이동",
                MutationTag.Area => "광역",
                MutationTag.Sacrifice => "희생",
                MutationTag.Summon => "소환",
                MutationTag.Transform => "변형",
                MutationTag.Chaos => "혼돈",
                _ => tag.ToString()
            };
        }

        /// <summary>
        /// Calculates synergy bonus damage/effect multiplier.
        /// </summary>
        public static float CalculateSynergyBonus(int synergyCount)
        {
            // Each synergy adds 20% bonus, capped at 100%
            return Mathf.Min(1f + (synergyCount * 0.2f), 2f);
        }

        /// <summary>
        /// Gets the drop weight for a rarity tier.
        /// </summary>
        public static float GetRarityWeight(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Common => 100f,
                MutationRarity.Rare => 50f,
                MutationRarity.Epic => 20f,
                MutationRarity.Legendary => 5f,
                _ => 50f
            };
        }
    }
}
