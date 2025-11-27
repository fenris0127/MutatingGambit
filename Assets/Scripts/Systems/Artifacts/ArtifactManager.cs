using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Artifacts
{
    /// <summary>
    /// Manages all active artifacts and applies their global effects.
    /// </summary>
    public class ArtifactManager : MonoBehaviour
    {
        private List<Artifact> activeArtifacts = new List<Artifact>();
        private Board currentBoard;

        /// <summary>
        /// Gets all currently active artifacts.
        /// </summary>
        public List<Artifact> ActiveArtifacts => new List<Artifact>(activeArtifacts);

        /// <summary>
        /// Gets all artifacts (alias for ActiveArtifacts).
        /// </summary>
        public List<Artifact> GetAllArtifacts() => new List<Artifact>(activeArtifacts);

        /// <summary>
        /// Gets the number of active artifacts.
        /// </summary>
        public int ArtifactCount => activeArtifacts.Count;

        /// <summary>
        /// Sets the board that this manager is managing artifacts for.
        /// </summary>
        public void SetBoard(Board board)
        {
            currentBoard = board;
        }

        /// <summary>
        /// Adds an artifact to the active collection.
        /// </summary>
        public void AddArtifact(Artifact artifact)
        {
            if (artifact == null)
            {
                Debug.LogError("Cannot add null artifact.");
                return;
            }

            // Check if artifact can stack
            bool canAdd = true;
            foreach (var existing in activeArtifacts)
            {
                if (!artifact.CanStackWith(existing))
                {
                    Debug.LogWarning($"Artifact '{artifact.ArtifactName}' cannot stack with existing '{existing.ArtifactName}'.");
                    canAdd = false;
                    break;
                }
            }

            if (!canAdd)
            {
                return;
            }

            activeArtifacts.Add(artifact);
            artifact.OnAcquired(currentBoard);

            Debug.Log($"Acquired artifact: {artifact.ArtifactName}");
        }

        /// <summary>
        /// Removes an artifact from the active collection.
        /// </summary>
        public void RemoveArtifact(Artifact artifact)
        {
            if (artifact == null || !activeArtifacts.Contains(artifact))
            {
                return;
            }

            artifact.OnRemoved(currentBoard);
            activeArtifacts.Remove(artifact);

            Debug.Log($"Removed artifact: {artifact.ArtifactName}");
        }

        /// <summary>
        /// Removes all artifacts.
        /// </summary>
        public void ClearArtifacts()
        {
            var artifactsCopy = new List<Artifact>(activeArtifacts);
            foreach (var artifact in artifactsCopy)
            {
                artifact.OnRemoved(currentBoard);
            }

            activeArtifacts.Clear();
        }

        /// <summary>
        /// Checks if a specific artifact is active.
        /// </summary>
        public bool HasArtifact(Artifact artifact)
        {
            return activeArtifacts.Contains(artifact);
        }

        /// <summary>
        /// Checks if any artifact of a specific type is active.
        /// </summary>
        public bool HasArtifactOfType<T>() where T : Artifact
        {
            foreach (var artifact in activeArtifacts)
            {
                if (artifact is T)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all artifacts that match a specific trigger.
        /// </summary>
        public List<Artifact> GetArtifactsByTrigger(ArtifactTrigger trigger)
        {
            var result = new List<Artifact>();
            foreach (var artifact in activeArtifacts)
            {
                if (artifact.Trigger == trigger)
                {
                    result.Add(artifact);
                }
            }
            return result;
        }

        /// <summary>
        /// Triggers all artifacts that match the specified trigger type.
        /// </summary>
        public void TriggerArtifacts(ArtifactTrigger trigger, ArtifactContext context)
        {
            if (currentBoard == null)
            {
                Debug.LogWarning("ArtifactManager has no board set. Cannot trigger artifacts.");
                return;
            }

            var triggeredArtifacts = GetArtifactsByTrigger(trigger);
            foreach (var artifact in triggeredArtifacts)
            {
                artifact.ApplyEffect(currentBoard, context);
            }
        }

        /// <summary>
        /// Notifies all artifacts that a turn has started.
        /// </summary>
        public void NotifyTurnStart(Team team, int turnNumber)
        {
            var context = new ArtifactContext
            {
                CurrentTeam = team,
                TurnNumber = turnNumber
            };

            TriggerArtifacts(ArtifactTrigger.OnTurnStart, context);
        }

        /// <summary>
        /// Notifies all artifacts that a turn has ended.
        /// </summary>
        public void NotifyTurnEnd(Team team, int turnNumber)
        {
            var context = new ArtifactContext
            {
                CurrentTeam = team,
                TurnNumber = turnNumber
            };

            TriggerArtifacts(ArtifactTrigger.OnTurnEnd, context);
        }

        /// <summary>
        /// Notifies all artifacts that a piece has moved.
        /// </summary>
        public void NotifyPieceMove(Piece piece, Vector2Int from, Vector2Int to)
        {
            var context = new ArtifactContext(piece, from, to);

            // Trigger general piece move artifacts
            TriggerArtifacts(ArtifactTrigger.OnPieceMove, context);

            // Trigger king-specific artifacts if it was a king
            if (piece.Type == PieceType.King)
            {
                TriggerArtifacts(ArtifactTrigger.OnKingMove, context);
            }
        }

        /// <summary>
        /// Notifies all artifacts that a piece was captured.
        /// </summary>
        public void NotifyPieceCapture(Piece attacker, Piece captured, Vector2Int capturePosition)
        {
            var context = new ArtifactContext
            {
                MovedPiece = attacker,
                CapturedPiece = captured,
                ToPosition = capturePosition
            };

            TriggerArtifacts(ArtifactTrigger.OnPieceCapture, context);
        }

        /// <summary>
        /// Returns a string listing all active artifacts.
        /// </summary>
        public override string ToString()
        {
            if (activeArtifacts.Count == 0)
            {
                return "ArtifactManager (no active artifacts)";
            }

            var result = $"ArtifactManager ({activeArtifacts.Count} artifacts):\n";
            foreach (var artifact in activeArtifacts)
            {
                result += $"  - {artifact.ArtifactName}\n";
            }
            return result;
        }
    }
}
