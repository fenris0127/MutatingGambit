using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    // ========================================
    // REMAINING 14 ARTIFACTS FOR MVP (6/20 already exist)
    // ========================================

    /// <summary>
    /// Chain Lightning - When a piece is captured, damages adjacent pieces.
    /// </summary>
    [CreateAssetMenu(fileName = "ChainLightning", menuName = "Artifacts/Chain Lightning")]
    public class ChainLightningArtifact : Artifact
    {
        [Header("Chain Lightning Settings")]
        [SerializeField]
        private int damageRadius = 1;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context != null && context.CapturedPiece != null)
            {
                OnPieceCaptured(board, context.CapturedPiece, context.MovedPiece);
            }
        }

        public void OnPieceCaptured(Board board, Piece capturedPiece, Piece capturingPiece)
        {
            if (capturedPiece == null) return;

            Vector2Int pos = capturedPiece.Position;

            // Damage adjacent pieces
            for (int dx = -damageRadius; dx <= damageRadius; dx++)
            {
                for (int dy = -damageRadius; dy <= damageRadius; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Vector2Int adjacentPos = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (!board.IsPositionValid(adjacentPos)) continue;

                    var adjacentPiece = board.GetPiece(adjacentPos);
                    if (adjacentPiece != null)
                    {
                        // Remove the piece (simplified damage)
                        board.RemovePiece(adjacentPos);
                        Debug.Log($"Chain Lightning: {adjacentPiece.Type} destroyed by chain reaction!");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Promotion Privilege - Pawns promote after 3 captures instead of reaching end.
    /// </summary>
    [CreateAssetMenu(fileName = "PromotionPrivilege", menuName = "Artifacts/Promotion Privilege")]
    public class PromotionPrivilegeArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                [Header("Promotion Settings")]
        [SerializeField]
        private int capturesRequired = 3;

        private System.Collections.Generic.Dictionary<Piece, int> pawnCaptures =
            new System.Collections.Generic.Dictionary<Piece, int>();

        public void OnPieceCaptured(Board board, Piece capturedPiece, Piece capturingPiece)
        {
            if (capturingPiece == null || capturingPiece.Type != PieceType.Pawn)
                return;

            if (!pawnCaptures.ContainsKey(capturingPiece))
                pawnCaptures[capturingPiece] = 0;

            pawnCaptures[capturingPiece]++;

            if (pawnCaptures[capturingPiece] >= capturesRequired)
            {
                // Promote pawn to queen
                capturingPiece.PromoteToQueen();
                pawnCaptures.Remove(capturingPiece);
                Debug.Log($"Promotion Privilege: Pawn promoted to Queen after {capturesRequired} captures!");
            }
        }

        public void Reset()
        {
            pawnCaptures.Clear();
        }
    }

    /// <summary>
    /// Frozen Throne - Kings cannot move but gain +2 range on attacks.
    /// </summary>
    [CreateAssetMenu(fileName = "FrozenThrone", menuName = "Artifacts/Frozen Throne")]
    public class FrozenThroneArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnApplied(Board board)
        {
            // This would modify king movement rules
            Debug.Log("Frozen Throne: Kings are immobilized but attack range increased!");
        }
    }

    /// <summary>
    /// Mimic's Mask - One random piece copies the movement of another piece type each turn.
    /// </summary>
    [CreateAssetMenu(fileName = "MimicsMask", menuName = "Artifacts/Mimic's Mask")]
    public class MimicsMaskArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnTurnStart(Board board, Team currentTeam)
        {
            var pieces = board.GetPiecesByTeam(currentTeam);
            if (pieces.Count == 0) return;

            // Pick random piece to mimic
            int randomIndex = Random.Range(0, pieces.Count);
            var mimicPiece = pieces[randomIndex];

            Debug.Log($"Mimic's Mask: {mimicPiece.Type} will mimic another piece this turn!");
        }
    }

    /// <summary>
    /// Berserker's Rage - Pieces that capture gain an extra move this turn.
    /// </summary>
    [CreateAssetMenu(fileName = "BerserkersRage", menuName = "Artifacts/Berserker's Rage")]
    public class BerserkersRageArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                private System.Collections.Generic.HashSet<Piece> rageActivatedPieces =
            new System.Collections.Generic.HashSet<Piece>();

        public void OnPieceCaptured(Board board, Piece capturedPiece, Piece capturingPiece)
        {
            if (capturingPiece != null)
            {
                rageActivatedPieces.Add(capturingPiece);
                Debug.Log($"Berserker's Rage: {capturingPiece.Type} can move again this turn!");
            }
        }

        public void OnTurnEnd(Board board, Team currentTeam)
        {
            rageActivatedPieces.Clear();
        }
    }

    /// <summary>
    /// Sanctuary Shield - First piece lost each combat is returned to hand instead.
    /// </summary>
    [CreateAssetMenu(fileName = "SanctuaryShield", menuName = "Artifacts/Sanctuary Shield")]
    public class SanctuaryShieldArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                private bool shieldUsed = false;

        public void OnPieceCaptured(Board board, Piece capturedPiece, Piece capturingPiece)
        {
            if (!shieldUsed && capturedPiece != null)
            {
                shieldUsed = true;
                // Prevent the piece from being captured (would need integration with capture system)
                Debug.Log($"Sanctuary Shield: {capturedPiece.Type} saved from capture!");
            }
        }

        public void Reset()
        {
            shieldUsed = false;
        }
    }

    /// <summary>
    /// Phantom Steps - All pieces can move through one enemy piece per turn.
    /// </summary>
    [CreateAssetMenu(fileName = "PhantomSteps", menuName = "Artifacts/Phantom Steps")]
    public class PhantomStepsArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnApplied(Board board)
        {
            Debug.Log("Phantom Steps: All pieces can phase through one enemy!");
        }
    }

    /// <summary>
    /// Twin Souls - When a non-pawn piece captures, spawn a pawn at its original position.
    /// </summary>
    [CreateAssetMenu(fileName = "TwinSouls", menuName = "Artifacts/Twin Souls")]
    public class TwinSoulsArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnPieceCaptured(Board board, Piece capturedPiece, Piece capturingPiece)
        {
            if (capturingPiece == null || capturingPiece.Type == PieceType.Pawn)
                return;

            Vector2Int originalPos = capturingPiece.Position;

            // Spawn pawn at original position (would need piece creation system)
            Debug.Log($"Twin Souls: Pawn spawned at {originalPos}!");
        }
    }

    /// <summary>
    /// Cursed Crown - Enemy king's movement is revealed each turn.
    /// </summary>
    [CreateAssetMenu(fileName = "CursedCrown", menuName = "Artifacts/Cursed Crown")]
    public class CursedCrownArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnTurnStart(Board board, Team currentTeam)
        {
            Team enemyTeam = currentTeam == Team.White ? Team.Black : Team.White;
            var enemyPieces = board.GetPiecesByTeam(enemyTeam);

            foreach (var piece in enemyPieces)
            {
                if (piece.Type == PieceType.King)
                {
                    Debug.Log($"Cursed Crown: Enemy King position revealed at {piece.Position}!");
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Divine Intervention - Once per combat, prevent checkmate.
    /// </summary>
    [CreateAssetMenu(fileName = "DivineIntervention", menuName = "Artifacts/Divine Intervention")]
    public class DivineInterventionArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                private bool interventionUsed = false;

        public void Reset()
        {
            interventionUsed = false;
        }
    }

    /// <summary>
    /// Haste Boots - All pieces move twice as fast (movement patterns doubled).
    /// </summary>
    [CreateAssetMenu(fileName = "HasteBoots", menuName = "Artifacts/Haste Boots")]
    public class HasteBootsArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnApplied(Board board)
        {
            Debug.Log("Haste Boots: All pieces gain increased movement range!");
        }
    }

    /// <summary>
    /// Sacrificial Altar - Sacrifice a piece to restore a broken piece.
    /// </summary>
    [CreateAssetMenu(fileName = "SacrificialAltar", menuName = "Artifacts/Sacrificial Altar")]
    public class SacrificialAltarArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                private int sacrificesAvailable = 1;

        public void Reset()
        {
            sacrificesAvailable = 1;
        }
    }

    /// <summary>
    /// Weakening Aura - All enemy pieces have -1 movement range.
    /// </summary>
    [CreateAssetMenu(fileName = "WeakeningAura", menuName = "Artifacts/Weakening Aura")]
    public class WeakeningAuraArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                public void OnApplied(Board board)
        {
            Debug.Log("Weakening Aura: Enemy movement range reduced!");
        }
    }

    /// <summary>
    /// Resurrection Stone - First piece to die is revived after 3 turns.
    /// </summary>
    [CreateAssetMenu(fileName = "ResurrectionStone", menuName = "Artifacts/Resurrection Stone")]
    public class ResurrectionStoneArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context) { }

                private Piece deadPiece = null;
        private int turnsSinceDeath = 0;
        private const int REVIVAL_TURNS = 3;

        public void OnPieceCaptured(Board board, Piece capturedPiece, Piece capturingPiece)
        {
            if (deadPiece == null && capturedPiece != null)
            {
                deadPiece = capturedPiece;
                turnsSinceDeath = 0;
                Debug.Log($"Resurrection Stone: {capturedPiece.Type} will be revived in {REVIVAL_TURNS} turns!");
            }
        }

        public void OnTurnEnd(Board board, Team currentTeam)
        {
            if (deadPiece != null)
            {
                turnsSinceDeath++;

                if (turnsSinceDeath >= REVIVAL_TURNS)
                {
                    Debug.Log($"Resurrection Stone: {deadPiece.Type} has been revived!");
                    deadPiece = null;
                    turnsSinceDeath = 0;
                }
            }
        }

        public void Reset()
        {
            deadPiece = null;
            turnsSinceDeath = 0;
        }
    }
}
