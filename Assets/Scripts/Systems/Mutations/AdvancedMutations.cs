using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Core.MovementRules;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations.Advanced
{
    // ==================== MOVEMENT MUTATIONS ====================
    
    /// <summary>
    /// Pawn can move backwards, breaking the traditional pawn limitation.
    /// </summary>
    [CreateAssetMenu(fileName = "ReversePawnMutation", menuName = "MutatingGambit/Mutations/Advanced/Reverse Pawn")]
    public class ReversePawnMutation : Mutation
    {
        public override void ApplyToPiece(Piece piece)
        {
            // Add reverse movement (backward by 1)
            // Add reverse movement (backward by 1)
            var reverseRule = ScriptableObject.CreateInstance<BackwardPawnRule>();
            piece.AddMovementRule(reverseRule);
        }

        public override void RemoveFromPiece(Piece piece)
        {
            // Remove added rule
        }
    }

    /// <summary>
    /// Piece can swap places with an adjacent friendly piece once per turn.
    /// </summary>
    [CreateAssetMenu(fileName = "SwapPositionMutation", menuName = "MutatingGambit/Mutations/Advanced/Swap Position")]
    public class SwapPositionMutation : Mutation
    {
        private bool usedThisTurn = false;

        public override void ApplyToPiece(Piece piece)
        {
            // Custom logic handled in OnMove
        }

        public override void RemoveFromPiece(Piece piece)
        {
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            usedThisTurn = false; // Reset for next turn
        }
    }

    /// <summary>
    /// Piece leaves a clone at its starting position for 2 turns (cannot move, blocks movement).
    /// </summary>
    [CreateAssetMenu(fileName = "EchoChamberMutation", menuName = "MutatingGambit/Mutations/Advanced/Echo Chamber")]
    public class EchoChamberMutation : Mutation
    {
        private Dictionary<Vector2Int, int> echoPositions = new Dictionary<Vector2Int, int>();

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
            echoPositions.Clear();
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            // Leave an "echo" obstacle at the from position
            board.SetObstacle(from, true);
            echoPositions[from] = 2; // Lasts 2 turns
            
            // Cleanup old echoes
            var toRemove = new List<Vector2Int>();
            foreach (var kvp in echoPositions)
            {
                echoPositions[kvp.Key]--;
                if (echoPositions[kvp.Key] <= 0)
                {
                    board.SetObstacle(kvp.Key, false);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var pos in toRemove)
            {
                echoPositions.Remove(pos);
            }
        }
    }

    // ==================== ATTACK MUTATIONS ====================

    /// <summary>
    /// When capturing, also damage all pieces adjacent to the captured piece.
    /// </summary>
    [CreateAssetMenu(fileName = "ExplosiveCaptureMutation", menuName = "MutatingGambit/Mutations/Advanced/Explosive Capture")]
    public class ExplosiveCaptureMutation : Mutation
    {
        [SerializeField]
        private int splashDamage = 1;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            // Deal splash damage to adjacent pieces
            Vector2Int capturePos = capturedPiece.Position;
            Vector2Int[] adjacentOffsets = new Vector2Int[]
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0), new Vector2Int(1, 0),
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
            };

            foreach (var offset in adjacentOffsets)
            {
                Vector2Int checkPos = capturePos + offset;
                var adjacentPiece = board.GetPiece(checkPos);
                if (adjacentPiece != null)
                {
                    // Apply damage (would need PieceHealth system)
                    Debug.Log($"Splash damage to {adjacentPiece.Type} at {checkPos}");
                }
            }
        }
    }

    /// <summary>
    /// Piece can capture pieces 2 squares away in cardinal directions (sniper).
    /// </summary>
    [CreateAssetMenu(fileName = "SniperMutation", menuName = "MutatingGambit/Mutations/Advanced/Sniper")]
    public class SniperMutation : Mutation
    {
        public override void ApplyToPiece(Piece piece)
        {
            // Add long-range capture ability (custom rule needed)
        }

        public override void RemoveFromPiece(Piece piece)
        {
        }
    }

    /// <summary>
    /// Capturing a piece grants +1 movement range permanently (stacks).
    /// </summary>
    [CreateAssetMenu(fileName = "BloodthirstMutation", menuName = "MutatingGambit/Mutations/Advanced/Bloodthirst")]
    public class BloodthirstMutation : Mutation
    {
        private int captureCount = 0;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
            captureCount = 0;
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            captureCount++;
            // Extend movement range (would need custom rule modification)
            Debug.Log($"Bloodthirst: {captureCount} kills, increased range");
        }
    }

    // ==================== UTILITY MUTATIONS ====================

    /// <summary>
    /// Once per game, this piece can be sacrificed to teleport any friendly piece.
    /// </summary>
    [CreateAssetMenu(fileName = "SacrificeWarpMutation", menuName = "MutatingGambit/Mutations/Advanced/Sacrifice Warp")]
    public class SacrificeWarpMutation : Mutation
    {
        private bool used = false;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
            used = false;
        }

        public void ActivateSacrifice(Piece sacrificePiece, Piece targetPiece, Vector2Int destination, Board board)
        {
            if (used) return;
            
            // Remove sacrifice piece
            board.RemovePiece(sacrificePiece.Position);
            
            // Teleport target
            board.MovePiece(targetPiece.Position, destination);
            
            used = true;
            Debug.Log($"Sacrifice Warp: {sacrificePiece.Type} sacrificed to teleport {targetPiece.Type}");
        }
    }

    /// <summary>
    /// Piece is invisible to enemy AI for 3 turns after first move.
    /// </summary>
    [CreateAssetMenu(fileName = "StealthCloakMutation", menuName = "MutatingGambit/Mutations/Advanced/Stealth Cloak")]
    public class StealthCloakMutation : Mutation
    {
        private int stealthTurnsRemaining = 0;
        private bool activated = false;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
            activated = false;
            stealthTurnsRemaining = 0;
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (!activated)
            {
                activated = true;
                stealthTurnsRemaining = 3;
                Debug.Log($"{mutatedPiece.Type} is now stealthed for 3 turns");
            }
            else if (stealthTurnsRemaining > 0)
            {
                stealthTurnsRemaining--;
            }
        }

        public bool IsStealthed => stealthTurnsRemaining > 0;
    }

    /// <summary>
    /// When this piece is captured, it respawns at a random empty square after 2 turns.
    /// </summary>
    [CreateAssetMenu(fileName = "PhoenixRebornMutation", menuName = "MutatingGambit/Mutations/Advanced/Phoenix Reborn")]
    public class PhoenixRebornMutation : Mutation
    {
        private bool used = false;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
            used = false;
        }

        // Would need to implement respawn logic in game manager
    }

    /// <summary>
    /// Piece transforms into a random different piece type after capturing 3 enemies.
    /// </summary>
    [CreateAssetMenu(fileName = "EvolutionMutation", menuName = "MutatingGambit/Mutations/Advanced/Evolution")]
    public class EvolutionMutation : Mutation
    {
        private int captureCount = 0;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
            captureCount = 0;
        }

        public override void OnCapture(Piece mutatedPiece, Piece capturedPiece, Vector2Int fromPos, Vector2Int toPos, Board board)
        {
            captureCount++;
            if (captureCount >= 3)
            {
                // Transform piece (would need piece transformation system)
                Debug.Log($"{mutatedPiece.Type} is evolving!");
                captureCount = 0;
            }
        }
    }

    // ==================== CHAOS MUTATIONS ====================

    /// <summary>
    /// Each move, this piece has a 30% chance to move to a random valid square instead.
    /// </summary>
    [CreateAssetMenu(fileName = "ChaosStepMutation", menuName = "MutatingGambit/Mutations/Advanced/Chaos Step")]
    public class ChaosStepMutation : Mutation
    {
        [SerializeField]
        private float chaosChance = 0.3f;

        public override void ApplyToPiece(Piece piece)
        {
        }

        public override void RemoveFromPiece(Piece piece)
        {
        }

        public override void OnMove(Piece mutatedPiece, Vector2Int from, Vector2Int to, Board board)
        {
            if (Random.value < chaosChance)
            {
                // Randomize destination
                var validMoves = mutatedPiece.GetValidMoves(board);
                if (validMoves.Count > 0)
                {
                    var randomMove = validMoves[Random.Range(0, validMoves.Count)];
                    Debug.Log($"Chaos Step! Moving to {randomMove} instead of {to}");
                    // Would need to override the actual move
                }
            }
        }
    }
}
