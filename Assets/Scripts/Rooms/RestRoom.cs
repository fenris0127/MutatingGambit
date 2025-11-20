namespace MutatingGambit.Core.Rooms
{
    /// <summary>
    /// A room where pieces can be repaired
    /// </summary>
    public class RestRoom : Room
    {
        public override RoomType Type => RoomType.Rest;

        public bool CanRepair()
        {
            return true;
        }

        public void RepairPiece(Piece piece)
        {
            if (piece != null)
            {
                piece.Heal(piece.MaxHP);
            }
        }

        public override RoomReward GetReward()
        {
            return new RoomReward
            {
                CanRepairPieces = true
            };
        }
    }
}
