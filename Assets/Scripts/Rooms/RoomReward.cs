using System.Collections.Generic;
using MutatingGambit.Core.Artifacts;

namespace MutatingGambit.Core.Rooms
{
    /// <summary>
    /// Rewards given upon completing a room
    /// </summary>
    public class RoomReward
    {
        public List<Artifact> ArtifactChoices { get; set; } = new List<Artifact>();
        public Artifact GuaranteedArtifact { get; set; }
        public int CurrencyAmount { get; set; }
        public bool CanRepairPieces { get; set; }

        public RoomReward()
        {
        }

        public static RoomReward CreateCombatReward(RoomDifficulty difficulty)
        {
            var reward = new RoomReward();

            switch (difficulty)
            {
                case RoomDifficulty.Normal:
                    reward.CurrencyAmount = 25;
                    reward.ArtifactChoices = new List<Artifact>
                    {
                        new KingsShadowArtifact(),
                        new CavalryChargeArtifact(),
                        new PromotionPrivilegeArtifact()
                    };
                    break;

                case RoomDifficulty.Elite:
                    reward.CurrencyAmount = 50;
                    reward.ArtifactChoices = new List<Artifact>
                    {
                        new KingsShadowArtifact(),
                        new CavalryChargeArtifact(),
                        new PromotionPrivilegeArtifact()
                    };
                    break;

                case RoomDifficulty.Boss:
                    reward.CurrencyAmount = 100;
                    reward.GuaranteedArtifact = new KingsShadowArtifact();
                    break;
            }

            return reward;
        }

        public static RoomReward CreateTreasureReward()
        {
            return new RoomReward
            {
                GuaranteedArtifact = new KingsShadowArtifact(),
                CurrencyAmount = 30
            };
        }
    }
}
