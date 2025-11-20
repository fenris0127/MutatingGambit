using System.Collections.Generic;

namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// Pawns promote after 3 captures
    /// </summary>
    public class PromotionPrivilegeArtifact : Artifact
    {
        private readonly Dictionary<Piece, int> _captureCount = new Dictionary<Piece, int>();

        public override string Name => "Promotion Privilege";
        public override string Description => "Pawns promote after 3 captures";
        public override int Cost => 90;
        public override ArtifactTrigger Trigger => ArtifactTrigger.OnCapture;

        public void TrackCapture(Piece piece)
        {
            if (!IsActive)
            {
                return;
            }

            if (piece.Type == PieceType.Pawn)
            {
                if (!_captureCount.ContainsKey(piece))
                {
                    _captureCount[piece] = 0;
                }
                _captureCount[piece]++;
            }
        }

        public bool ShouldPromote(Piece piece)
        {
            if (!IsActive || piece.Type != PieceType.Pawn)
            {
                return false;
            }

            if (_captureCount.TryGetValue(piece, out int count))
            {
                return count >= 3;
            }

            return false;
        }

        public int GetCaptureCount(Piece piece)
        {
            if (_captureCount.TryGetValue(piece, out int count))
            {
                return count;
            }
            return 0;
        }

        public void ResetCaptures(Piece piece)
        {
            _captureCount.Remove(piece);
        }
    }
}
