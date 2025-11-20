using MutatingGambit.Core.Victory;

namespace MutatingGambit.Core.Rooms
{
    /// <summary>
    /// A room with combat/puzzle challenge
    /// </summary>
    public class CombatRoom : Room
    {
        public override RoomType Type => RoomType.Combat;
        public RoomDifficulty Difficulty { get; }
        public IVictoryCondition VictoryCondition { get; set; }

        public CombatRoom(RoomDifficulty difficulty)
        {
            Difficulty = difficulty;
        }

        public override RoomReward GetReward()
        {
            return RoomReward.CreateCombatReward(Difficulty);
        }
    }
}
