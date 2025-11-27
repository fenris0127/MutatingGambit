using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Shield Of Faith Artifact: King is protected by a holy barrier.
    /// Squares adjacent to the king cannot be attacked.
    /// </summary>
    [CreateAssetMenu(fileName = "ShieldOfFaithArtifact", menuName = "Mutating Gambit/Artifacts/Shield Of Faith")]
    public class ShieldOfFaithArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // Passive effect - implementation would prevent attacks on king-adjacent squares
            Debug.Log("Shield Of Faith protects the king!");
        }
    }

    /// <summary>
    /// Chaos Orb Artifact: Randomly swaps two pieces each turn.
    /// Creates unpredictable board states.
    /// </summary>
    [CreateAssetMenu(fileName = "ChaosOrbArtifact", menuName = "Mutating Gambit/Artifacts/Chaos Orb")]
    public class ChaosOrbArtifact : Artifact
    {
        private System.Random random = new System.Random();

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (board == null) return;

            var allPieces = board.GetAllPieces();
            if (allPieces.Count < 2) return;

            // Pick two random pieces
            int index1 = random.Next(allPieces.Count);
            int index2 = random.Next(allPieces.Count);

            if (index1 != index2 && allPieces[index1] != null && allPieces[index2] != null)
            {
                Vector2Int pos1 = allPieces[index1].Position;
                Vector2Int pos2 = allPieces[index2].Position;

                // Swap positions
                var temp = board.GetPiece(pos1);
                board.MovePiece(pos1, pos2);

                Debug.Log($"Chaos Orb swapped pieces at {pos1} and {pos2}!");
            }
        }
    }

    /// <summary>
    /// Lightning Strike Artifact: Damages random enemy piece each turn.
    /// </summary>
    [CreateAssetMenu(fileName = "LightningStrikeArtifact", menuName = "Mutating Gambit/Artifacts/Lightning Strike")]
    public class LightningStrikeArtifact : Artifact
    {
        private System.Random random = new System.Random();

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (board == null || context == null) return;

            Team enemyTeam = context.CurrentTeam == Team.White ? Team.Black : Team.White;
            var enemyPieces = board.GetPiecesByTeam(enemyTeam);

            if (enemyPieces.Count > 0)
            {
                int index = random.Next(enemyPieces.Count);
                Debug.Log($"Lightning strikes {enemyPieces[index].Type}!");
                // In full implementation: apply damage
            }
        }
    }

    /// <summary>
    /// Mirror Realm Artifact: Board is vertically mirrored.
    /// Disorients players and creates new strategies.
    /// </summary>
    [CreateAssetMenu(fileName = "MirrorRealmArtifact", menuName = "Mutating Gambit/Artifacts/Mirror Realm")]
    public class MirrorRealmArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // Passive visual effect - implementation would flip board display
            Debug.Log("Mirror Realm active - board is mirrored!");
        }
    }

    /// <summary>
    /// Tactical Nuke Artifact: Destroys a 3x3 area every 3 turns.
    /// High risk, high reward artifact.
    /// </summary>
    [CreateAssetMenu(fileName = "TacticalNukeArtifact", menuName = "Mutating Gambit/Artifacts/Tactical Nuke")]
    public class TacticalNukeArtifact : Artifact
    {
        private int turnCounter = 0;
        private System.Random random = new System.Random();

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            turnCounter++;

            if (turnCounter >= 3)
            {
                turnCounter = 0;

                // Random explosion position
                Vector2Int explosionCenter = new Vector2Int(
                    random.Next(board.Width),
                    random.Next(board.Height)
                );

                Debug.Log($"Tactical Nuke explodes at {explosionCenter}!");

                // Destroy 3x3 area
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        Vector2Int pos = explosionCenter + new Vector2Int(dx, dy);
                        if (board.IsPositionValid(pos))
                        {
                            board.RemovePiece(pos);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Double Pawn Artifact: When moving a pawn, move two pawns simultaneously.
    /// </summary>
    [CreateAssetMenu(fileName = "DoublePawnArtifact", menuName = "Mutating Gambit/Artifacts/Double Pawn")]
    public class DoublePawnArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context != null && context.MovedPiece != null && context.MovedPiece.Type == PieceType.Pawn)
            {
                // Find another pawn of the same team
                var pawns = board.GetPiecesByTeam(context.MovedPiece.Team);
                foreach (var pawn in pawns)
                {
                    if (pawn.Type == PieceType.Pawn && pawn != context.MovedPiece)
                    {
                        // Move this pawn in the same direction
                        Vector2Int movement = context.ToPosition - context.FromPosition;
                        Vector2Int newPos = pawn.Position + movement;

                        if (board.IsPositionValid(newPos))
                        {
                            board.MovePiece(pawn.Position, newPos);
                            Debug.Log("Double Pawn moved second pawn!");
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Clone Factory Artifact: Creates a copy of captured pieces.
    /// </summary>
    [CreateAssetMenu(fileName = "CloneFactoryArtifact", menuName = "Mutating Gambit/Artifacts/Clone Factory")]
    public class CloneFactoryArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context != null && context.CapturedPiece != null)
            {
                Debug.Log($"Clone Factory creates copy of {context.CapturedPiece.Type}!");
                // In full implementation: spawn a copy on your side
            }
        }
    }

    /// <summary>
    /// Enchanted Armor Artifact: All pieces get one-time damage immunity.
    /// </summary>
    [CreateAssetMenu(fileName = "EnchantedArmorArtifact", menuName = "Mutating Gambit/Artifacts/Enchanted Armor")]
    public class EnchantedArmorArtifact : Artifact
    {
        private HashSet<Piece> protectedPieces = new HashSet<Piece>();

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (context != null && context.CapturedPiece != null)
            {
                if (protectedPieces.Contains(context.CapturedPiece))
                {
                    Debug.Log("Enchanted Armor saved the piece!");
                    protectedPieces.Remove(context.CapturedPiece);
                    // Prevent capture
                }
            }
        }

        public override void OnAcquired(Board board)
        {
            // Grant immunity to all current pieces
            var allPieces = board.GetAllPieces();
            foreach (var piece in allPieces)
            {
                protectedPieces.Add(piece);
            }

            Debug.Log("All pieces gain Enchanted Armor!");
        }
    }

    /// <summary>
    /// Rapid Deployment Artifact: Start with extra pawns.
    /// </summary>
    [CreateAssetMenu(fileName = "RapidDeploymentArtifact", menuName = "Mutating Gambit/Artifacts/Rapid Deployment")]
    public class RapidDeploymentArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // Passive effect applied at game start
        }

        public override void OnAcquired(Board board)
        {
            Debug.Log("Rapid Deployment grants extra starting pieces!");
        }
    }

    /// <summary>
    /// Sacrifice Altar Artifact: Sacrifice piece to power up another.
    /// </summary>
    [CreateAssetMenu(fileName = "SacrificeAltarArtifact", menuName = "Mutating Gambit/Artifacts/Sacrifice Altar")]
    public class SacrificeAltarArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            Debug.Log("Sacrifice Altar allows piece sacrifice for power!");
            // Implementation would add UI for selecting pieces to sacrifice
        }
    }

    /// <summary>
    /// Time Reverse Artifact: Can undo last move once per game.
    /// </summary>
    [CreateAssetMenu(fileName = "TimeReverseArtifact", menuName = "Mutating Gambit/Artifacts/Time Reverse")]
    public class TimeReverseArtifact : Artifact
    {
        private bool used = false;

        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            if (!used)
            {
                Debug.Log("Time Reverse available - can undo one move!");
            }
        }

        public void UseTimeReverse()
        {
            if (!used)
            {
                used = true;
                Debug.Log("Time Reverse activated!");
                // Implementation would restore previous board state
            }
        }
    }

    /// <summary>
    /// Fog Of War Artifact: Enemy pieces are partially hidden.
    /// </summary>
    [CreateAssetMenu(fileName = "FogOfWarArtifact", menuName = "Mutating Gambit/Artifacts/Fog Of War")]
    public class FogOfWarArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // Passive visual effect - implementation would hide enemy pieces
            Debug.Log("Fog Of War obscures enemy positions!");
        }
    }

    /// <summary>
    /// Frozen Time Artifact: AI gets less time to think.
    /// </summary>
    [CreateAssetMenu(fileName = "FrozenTimeArtifact", menuName = "Mutating Gambit/Artifacts/Frozen Time")]
    public class FrozenTimeArtifact : Artifact
    {
        public override void ApplyEffect(Board board, ArtifactContext context)
        {
            // Passive effect - reduces AI thinking time
            Debug.Log("Frozen Time slows enemy thinking!");
        }

        public float GetAITimeMultiplier()
        {
            return 0.5f; // AI gets half the normal time
        }
    }
}
