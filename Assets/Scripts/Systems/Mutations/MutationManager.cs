using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Singleton manager for tracking and applying mutations to pieces.
    /// </summary>
    public class MutationManager : MonoBehaviour
    {
        private static MutationManager instance;

        /// <summary>
        /// Gets the singleton instance of the MutationManager.
        /// </summary>
        public static MutationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MutationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("MutationManager");
                        instance = go.AddComponent<MutationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        // Tracks which mutations are applied to which pieces
        private Dictionary<Piece, List<Mutation>> pieceMutations = new Dictionary<Piece, List<Mutation>>();

        // Tracks stack counts for stackable mutations
        private Dictionary<Piece, Dictionary<Mutation, int>> mutationStacks = new Dictionary<Piece, Dictionary<Mutation, int>>();

        /// <summary>
        /// Event fired when a mutation is applied to a piece.
        /// Args: Piece, Mutation
        /// </summary>
        public event System.Action<Piece, Mutation> OnMutationApplied;

        /// <summary>
        /// Event fired when a mutation is removed from a piece.
        /// Args: Piece, Mutation
        /// </summary>
        public event System.Action<Piece, Mutation> OnMutationRemoved;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Applies a mutation to a piece.
        /// </summary>
        public void ApplyMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null)
            {
                Debug.LogError("Cannot apply null piece or mutation.");
                return;
            }

            // Check compatibility
            if (!mutation.IsCompatibleWith(piece.Type))
            {
                Debug.LogWarning($"Mutation '{mutation.MutationName}' is not compatible with {piece.Type}.");
                return;
            }

            // Initialize mutations list for this piece if needed
            if (!pieceMutations.ContainsKey(piece))
            {
                pieceMutations[piece] = new List<Mutation>();
                mutationStacks[piece] = new Dictionary<Mutation, int>();
            }

            // Check if mutation is already applied
            if (pieceMutations[piece].Contains(mutation))
            {
                // Handle stacking
                if (mutation.CanStack)
                {
                    int currentStacks = mutationStacks[piece][mutation];
                    if (currentStacks < mutation.MaxStacks)
                    {
                        mutationStacks[piece][mutation]++;
                        Debug.Log($"Stacked '{mutation.MutationName}' on {piece.Type} (Stack: {mutationStacks[piece][mutation]})");
                        
                        // Reapply for stack bonus
                        mutation.ApplyToPiece(piece);
                        OnMutationApplied?.Invoke(piece, mutation);
                    }
                    else
                    {
                        Debug.LogWarning($"Mutation '{mutation.MutationName}' is already at max stacks ({mutation.MaxStacks}).");
                    }
                }
                else
                {
                    Debug.LogWarning($"Mutation '{mutation.MutationName}' is already applied and cannot stack.");
                }
                return;
            }

            // Apply the mutation to the piece
            mutation.ApplyToPiece(piece);

            // Add to tracking
            pieceMutations[piece].Add(mutation);
            mutationStacks[piece][mutation] = 1;

            // Fire event
            OnMutationApplied?.Invoke(piece, mutation);

            // Unlock in Codex
            if (GlobalDataManager.Instance != null)
            {
                GlobalDataManager.Instance.UnlockMutation(mutation.MutationName);
            }

            Debug.Log($"Applied mutation '{mutation.MutationName}' to {piece.Type} at {piece.Position}");
        }

        /// <summary>
        /// Removes a mutation from a piece.
        /// </summary>
        public void RemoveMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null)
            {
                return;
            }

            if (!pieceMutations.ContainsKey(piece))
            {
                return;
            }

            if (pieceMutations[piece].Contains(mutation))
            {
                mutation.RemoveFromPiece(piece);
                pieceMutations[piece].Remove(mutation);

                // Fire event
                OnMutationRemoved?.Invoke(piece, mutation);

                Debug.Log($"Removed mutation '{mutation.MutationName}' from {piece}");
            }
        }

        /// <summary>
        /// Removes all mutations from a piece.
        /// </summary>
        public void ClearMutations(Piece piece)
        {
            if (piece == null || !pieceMutations.ContainsKey(piece))
            {
                return;
            }

            var mutations = new List<Mutation>(pieceMutations[piece]);
            foreach (var mutation in mutations)
            {
                mutation.RemoveFromPiece(piece);
            }

            pieceMutations[piece].Clear();
        }

        /// <summary>
        /// Gets all mutations applied to a piece.
        /// </summary>
        public List<Mutation> GetMutations(Piece piece)
        {
            if (piece == null || !pieceMutations.ContainsKey(piece))
            {
                return new List<Mutation>();
            }

            return new List<Mutation>(pieceMutations[piece]);
        }

        /// <summary>
        /// Checks if a piece has any mutations.
        /// </summary>
        public bool HasMutations(Piece piece)
        {
            return piece != null && pieceMutations.ContainsKey(piece) && pieceMutations[piece].Count > 0;
        }

        /// <summary>
        /// Checks if a piece has a specific mutation.
        /// </summary>
        public bool HasMutation(Piece piece, Mutation mutation)
        {
            if (piece == null || mutation == null || !pieceMutations.ContainsKey(piece))
            {
                return false;
            }

            return pieceMutations[piece].Contains(mutation);
        }

        /// <summary>
        /// Notifies all mutations on a piece that it captured an enemy.
        /// </summary>
        public void NotifyCapture(Piece attacker, Piece captured, Core.ChessEngine.Board board)
        {
            if (attacker == null || !pieceMutations.ContainsKey(attacker))
            {
                return;
            }

            foreach (var mutation in pieceMutations[attacker])
            {
                mutation.OnCapture(attacker, captured, board);
            }
        }

        /// <summary>
        /// Notifies all mutations on a piece that it moved.
        /// </summary>
        public void NotifyMove(Piece piece, Vector2Int from, Vector2Int to, Core.ChessEngine.Board board)
        {
            if (piece == null || !pieceMutations.ContainsKey(piece))
            {
                return;
            }

            foreach (var mutation in pieceMutations[piece])
            {
                mutation.OnMove(piece, from, to, board);
            }
        }

        /// <summary>
        /// Clears all mutation data. Used when starting a new game.
        /// </summary>
        public void Reset()
        {
            pieceMutations.Clear();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
