using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;
using System.Linq;

namespace MutatingGambit.Systems.Artifacts.Advanced
{
    // ==================== GLOBAL MODIFIERS ====================

    /// <summary>
    /// Every 3rd turn, all pieces can move twice.
    /// </summary>
    [CreateAssetMenu(fileName = "TemporalAccelerator", menuName = "MutatingGambit/Artifacts/Advanced/Temporal Accelerator")]
    public class TemporalAcceleratorArtifact : Artifact
    {
        private int turnCounter = 0;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context.CurrentTeam == Team.White) // Only count player turns
            {
                turnCounter++;
                if (turnCounter % 3 == 0)
                {
                    Debug.Log("Temporal Accelerator activated! Double movement this turn!");
                    // Would need to modify GameManager to allow extra actions
                }
            }
        }
    }

    /// <summary>
    /// Pieces cannot be captured, instead they are "frozen" for 2 turns.
    /// </summary>
    [CreateAssetMenu(fileName = "CrystalPrison", menuName = "MutatingGambit/Artifacts/Advanced/Crystal Prison")]
    public class CrystalPrisonArtifact : Artifact
    {
        private Dictionary<Piece, int> frozenPieces = new Dictionary<Piece, int>();

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context.CapturedPiece != null)
            {
                // Instead of capturing, freeze the piece
                frozenPieces[context.CapturedPiece] = 2;
                Debug.Log($"{context.CapturedPiece.Type} is frozen for 2 turns!");
                // Cancel the capture and freeze piece
            }

            // Unfreeze countdown
            var toRemove = new List<Piece>();
            foreach (var kvp in frozenPieces.ToList())
            {
                frozenPieces[kvp.Key]--;
                if (frozenPieces[kvp.Key] <= 0)
                {
                    toRemove.Add(kvp.Key);
                    Debug.Log($"{kvp.Key.Type} is unfrozen!");
                }
            }
            foreach (var piece in toRemove)
            {
                frozenPieces.Remove(piece);
            }
        }
    }

    /// <summary>
    /// Board rotates 90 degrees clockwise after every 5 moves.
    /// </summary>
    [CreateAssetMenu(fileName = "RotatingRealm", menuName = "MutatingGambit/Artifacts/Advanced/Rotating Realm")]
    public class RotatingRealmArtifact : Artifact
    {
        private int moveCount = 0;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            moveCount++;
            if (moveCount % 5 == 0)
            {
                Debug.Log("Board rotating 90 degrees!");
                RotateBoard(board);
            }
        }

        private void RotateBoard(Board board)
        {
            // Rotate all piece positions 90 degrees clockwise
            var allPieces = board.GetAllPieces();
            var newPositions = new Dictionary<Piece, Vector2Int>();

            foreach (var piece in allPieces)
            {
                Vector2Int oldPos = piece.Position;
                // Rotate: (x, y) -> (y, width-1-x)
                Vector2Int newPos = new Vector2Int(oldPos.y, board.Width - 1 - oldPos.x);
                newPositions[piece] = newPos;
            }

            // Move all pieces to new positions
            foreach (var kvp in newPositions)
            {
                board.MovePiece(kvp.Key.Position, kvp.Value);
            }
        }
    }

    // ==================== COMBAT MODIFIERS ====================

    /// <summary>
    /// Whenever a piece is captured, both the attacker and defender take 1 splash damage.
    /// </summary>
    [CreateAssetMenu(fileName = "MutualDestruction", menuName = "MutatingGambit/Artifacts/Advanced/Mutual Destruction")]
    public class MutualDestructionArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context.CapturedPiece != null && context.MovedPiece != null)
            {
                Debug.Log($"Mutual Destruction! Both {context.MovedPiece.Type} and {context.CapturedPiece.Type} take damage!");
                // Apply damage to both (requires PieceHealth system)
            }
        }
    }

    /// <summary>
    /// Pieces adjacent to the King gain +1 attack range.
    /// </summary>
    [CreateAssetMenu(fileName = "RoyalGuard", menuName = "MutatingGambit/Artifacts/Advanced/Royal Guard")]
    public class RoyalGuardArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // Passive effect - would need to check adjacency to king when calculating moves
        }

        public override void OnAcquired(Board board)
        {
            Debug.Log("Royal Guard active! Pieces near King are empowered!");
        }
    }

    /// <summary>
    /// Capturing an enemy piece heals 1 HP to all friendly pieces.
    /// </summary>
    [CreateAssetMenu(fileName = "VampiricAura", menuName = "MutatingGambit/Artifacts/Advanced/Vampiric Aura")]
    public class VampiricAuraArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context.CapturedPiece != null)
            {
                var friendlyPieces = board.GetPiecesByTeam(context.MovedPiece.Team);
                foreach (var piece in friendlyPieces)
                {
                    // Heal 1 HP (requires PieceHealth system)
                    Debug.Log($"Vampiric Aura: {piece.Type} healed!");
                }
            }
        }
    }

    // ==================== ECONOMIC / RESOURCE ====================

    /// <summary>
    /// Gain 10 gold every turn, lose 5 gold whenever a piece is captured.
    /// </summary>
    [CreateAssetMenu(fileName = "GoldMine", menuName = "MutatingGambit/Artifacts/Advanced/Gold Mine")]
    public class GoldMineArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // OnTurnStart: +10 gold
            // OnPieceCapture: -5 gold
            // Would integrate with PlayerState.Currency
        }
    }

    /// <summary>
    /// Can sacrifice 30 gold to resurrect a broken piece (once per 3 turns).
    /// </summary>
    [CreateAssetMenu(fileName = "NecromancersCoins", menuName = "MutatingGambit/Artifacts/Advanced/Necromancers Coins")]
    public class NecromancersCoinsArtifact : Artifact
    {
        private int cooldownTurns = 0;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (cooldownTurns > 0)
            {
                cooldownTurns--;
            }
        }

        public bool CanResurrect()
        {
            return cooldownTurns == 0;
        }

        public void UseResurrection()
        {
            cooldownTurns = 3;
            Debug.Log("Necromancer's Coins used! Resurrect a piece!");
        }
    }

    // ==================== CHAOS / RANDOM ====================

    /// <summary>
    /// 20% chance after each move to spawn a random pawn on an empty square.
    /// </summary>
    [CreateAssetMenu(fileName = "PawnStorm", menuName = "MutatingGambit/Artifacts/Advanced/Pawn Storm")]
    public class PawnStormArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (Random.value < 0.2f)
            {
                Debug.Log("Pawn Storm! A wild pawn appears!");
                // Find empty square and spawn pawn
                for (int x = 0; x < board.Width; x++)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        if (board.GetPiece(new Vector2Int(x, y)) == null && !board.IsObstacle(new Vector2Int(x, y)))
                        {
                            // Spawn pawn (would need piece creation system)
                            return;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Every turn, two random pieces swap positions.
    /// </summary>
    [CreateAssetMenu(fileName = "ChaoticSwap", menuName = "MutatingGambit/Artifacts/Advanced/Chaotic Swap")]
    public class ChaoticSwapArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            var allPieces = board.GetAllPieces();
            if (allPieces.Count >= 2)
            {
                var piece1 = allPieces[Random.Range(0, allPieces.Count)];
                var piece2 = allPieces[Random.Range(0, allPieces.Count)];

                if (piece1 != piece2)
                {
                    Vector2Int pos1 = piece1.Position;
                    Vector2Int pos2 = piece2.Position;
                    
                    board.MovePiece(pos1, pos2);
                    board.MovePiece(pos2, pos1);
                    
                    Debug.Log($"Chaotic Swap! {piece1.Type} and {piece2.Type} swapped!");
                }
            }
        }
    }

    /// <summary>
    /// All pieces have a 10% chance to move to a random adjacent square instead of their intended destination.
    /// </summary>
    [CreateAssetMenu(fileName = "DistortionField", menuName = "MutatingGambit/Artifacts/Advanced/Distortion Field")]
    public class DistortionFieldArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (Random.value < 0.1f)
            {
                Debug.Log("Distortion Field! Movement redirected!");
                // Redirect to random adjacent square
            }
        }
    }

    // ==================== DEFENSIVE ====================

    /// <summary>
    /// King is surrounded by an invisible shield. First attack on King each game is nullified.
    /// </summary>
    [CreateAssetMenu(fileName = "DivineProtection", menuName = "MutatingGambit/Artifacts/Advanced/Divine Protection")]
    public class DivineProtectionArtifact : Artifact
    {
        private bool shieldActive = true;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context.CapturedPiece != null && context.CapturedPiece.Type == PieceType.King && shieldActive)
            {
                Debug.Log("Divine Protection activated! King is saved!");
                // Nullify the capture
                shieldActive = false;
            }
        }

        public override void OnAcquired(Board board)
        {
            shieldActive = true;
        }
    }

    /// <summary>
    /// Broken pieces have a 30% chance to auto-repair at the start of each turn.
    /// </summary>
    [CreateAssetMenu(fileName = "AutoRepair", menuName = "MutatingGambit/Artifacts/Advanced/Auto Repair")]
    public class AutoRepairArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // OnTurnStart: check broken pieces and repair with 30% chance
            if (Random.value < 0.3f)
            {
                Debug.Log("Auto Repair activated!");
                // Repair random broken piece
            }
        }
    }
}
