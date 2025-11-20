using System;
using System.Collections.Generic;

namespace MutatingGambit.Core
{
    /// <summary>
    /// Represents a chess piece with potential mutations
    /// </summary>
    public class Piece
    {
        public PieceColor Color { get; }
        public PieceType Type { get; }
        public Position CurrentPosition { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; }
        public bool HasMoved { get; set; }

        private readonly List<Mutations.Mutation> _mutations = new List<Mutations.Mutation>();

        public IReadOnlyList<Mutations.Mutation> Mutations => _mutations.AsReadOnly();
        public bool IsBroken => HP <= 0;

        public Piece(PieceColor color, PieceType type, int maxHP = 3)
        {
            Color = color;
            Type = type;
            MaxHP = maxHP;
            HP = maxHP;
            HasMoved = false;
        }

        /// <summary>
        /// Adds a mutation to this piece
        /// </summary>
        public void AddMutation(Mutations.Mutation mutation)
        {
            if (mutation == null)
            {
                throw new ArgumentNullException(nameof(mutation));
            }
            _mutations.Add(mutation);
        }

        /// <summary>
        /// Removes a mutation from this piece
        /// </summary>
        public bool RemoveMutation(Mutations.Mutation mutation)
        {
            return _mutations.Remove(mutation);
        }

        /// <summary>
        /// Clears all mutations from this piece
        /// </summary>
        public void ClearMutations()
        {
            _mutations.Clear();
        }

        /// <summary>
        /// Takes damage
        /// </summary>
        public void TakeDamage(int amount)
        {
            HP = Math.Max(0, HP - amount);
        }

        /// <summary>
        /// Heals this piece
        /// </summary>
        public void Heal(int amount)
        {
            HP = Math.Min(MaxHP, HP + amount);
        }

        /// <summary>
        /// Creates a deep copy of this piece
        /// </summary>
        public Piece Clone()
        {
            var clone = new Piece(Color, Type, MaxHP)
            {
                HP = this.HP,
                HasMoved = this.HasMoved
            };

            // Clone mutations
            foreach (var mutation in _mutations)
            {
                clone.AddMutation(mutation);
            }

            return clone;
        }

        public override string ToString()
        {
            string mutationMarker = _mutations.Count > 0 ? "*" : "";
            return $"{Color} {Type}{mutationMarker}";
        }
    }
}
