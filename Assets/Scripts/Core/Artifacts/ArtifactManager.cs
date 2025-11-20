using System.Collections.Generic;

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
    }
}
