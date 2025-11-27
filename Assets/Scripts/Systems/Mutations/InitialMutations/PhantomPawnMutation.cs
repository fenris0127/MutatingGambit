using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Phantom Pawn Mutation: Pawn can move 3 squares forward instead of 1.
    /// Creates a ghostly, aggressive pawn.
    /// </summary>
    [CreateAssetMenu(fileName = "PhantomPawnMutation", menuName = "Mutating Gambit/Mutations/Phantom Pawn")]
    public class PhantomPawnMutation : Mutation
    {
        private StraightLineRule phantomMoveRule;

        public override void ApplyToPiece(Piece piece)
        {
            if (piece.Type != PieceType.Pawn)
            {
                Debug.LogWarning("Phantom Pawn mutation can only be applied to pawns!");
                return;
            }

            // Create extended forward movement rule
            phantomMoveRule = ScriptableObject.CreateInstance<StraightLineRule>();

            // Pawn can now move up to 3 squares forward
            piece.AddMovementRule(phantomMoveRule);

            Debug.Log($"Applied Phantom Pawn mutation to {piece.Team} pawn");
        }

        public override void RemoveFromPiece(Piece piece)
        {
            if (phantomMoveRule != null)
            {
                piece.RemoveMovementRule(phantomMoveRule);
            }
        }
    }
}
