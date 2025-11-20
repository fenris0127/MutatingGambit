using System.Collections.Generic;
using System.Linq;

namespace MutatingGambit.Core.Artifacts
{
    /// <summary>
    /// Manages global artifacts that affect the game
    /// </summary>
    public class ArtifactManager
    {
        private readonly List<Artifact> _artifacts = new List<Artifact>();

        public IReadOnlyList<Artifact> Artifacts => _artifacts.AsReadOnly();

        public void AddArtifact(Artifact artifact)
        {
            if (artifact != null && !_artifacts.Contains(artifact))
            {
                _artifacts.Add(artifact);
            }
        }

        public bool RemoveArtifact(Artifact artifact)
        {
            return _artifacts.Remove(artifact);
        }

        public void ClearArtifacts()
        {
            _artifacts.Clear();
        }

        /// <summary>
        /// Gets artifacts sorted by priority (highest first)
        /// </summary>
        public List<Artifact> GetArtifactsByPriority()
        {
            return _artifacts.OrderByDescending(a => a.Priority).ToList();
        }

        /// <summary>
        /// Gets active artifacts for a specific trigger
        /// </summary>
        public List<Artifact> GetArtifactsForTrigger(ArtifactTrigger trigger)
        {
            return _artifacts
                .Where(a => a.IsActive && a.Trigger == trigger)
                .OrderByDescending(a => a.Priority)
                .ToList();
        }
    }
}
