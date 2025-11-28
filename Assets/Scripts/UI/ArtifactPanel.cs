using System.Collections.Generic;
using UnityEngine;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Manages the UI panel displaying all collected artifacts.
    /// </summary>
    public class ArtifactPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Transform slotContainer;

        [SerializeField]
        private GameObject slotPrefab;

        [Header("Settings")]
        [SerializeField]
        private int maxArtifactSlots = 20;

        private List<ArtifactSlot> artifactSlots = new List<ArtifactSlot>();
        private ArtifactManager artifactManager;

        private void Awake()
        {
            InitializeSlots();
        }

        private void Start()
        {
            // Use singleton instance if available
            artifactManager = ArtifactManager.Instance;

            if (artifactManager == null)
            {
                Debug.LogWarning("ArtifactPanel: No ArtifactManager found in scene!");
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Initializes artifact slots.
        /// </summary>
        private void InitializeSlots()
        {
            if (slotContainer == null)
            {
                Debug.LogError("ArtifactPanel: Slot container is not assigned!");
                return;
            }

            // Clear existing slots
            foreach (var slot in artifactSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            artifactSlots.Clear();

            // Create new slots
            for (int i = 0; i < maxArtifactSlots; i++)
            {
                GameObject slotObject;

                if (slotPrefab != null)
                {
                    slotObject = Instantiate(slotPrefab, slotContainer);
                }
                else
                {
                    slotObject = new GameObject($"ArtifactSlot_{i}");
                    slotObject.transform.SetParent(slotContainer);
                }

                ArtifactSlot slot = slotObject.GetComponent<ArtifactSlot>();
                if (slot == null)
                {
                    slot = slotObject.AddComponent<ArtifactSlot>();
                }

                artifactSlots.Add(slot);
            }
        }

        /// <summary>
        /// Refreshes the display to show current artifacts.
        /// </summary>
        public void RefreshDisplay()
        {
            if (artifactManager == null)
            {
                return;
            }

            List<Artifact> artifacts = artifactManager.GetAllArtifacts();

            // Update slots
            for (int i = 0; i < artifactSlots.Count; i++)
            {
                if (i < artifacts.Count)
                {
                    artifactSlots[i].SetArtifact(artifacts[i]);
                }
                else
                {
                    artifactSlots[i].ClearArtifact();
                }
            }
        }

        /// <summary>
        /// Adds an artifact to the display.
        /// </summary>
        public void AddArtifact(Artifact artifact)
        {
            if (artifact == null)
            {
                return;
            }

            // Find first empty slot
            foreach (var slot in artifactSlots)
            {
                if (slot.IsEmpty)
                {
                    slot.SetArtifact(artifact);
                    return;
                }
            }

            Debug.LogWarning("ArtifactPanel: No empty slots available!");
        }

        /// <summary>
        /// Removes an artifact from the display.
        /// </summary>
        public void RemoveArtifact(Artifact artifact)
        {
            if (artifact == null)
            {
                return;
            }

            foreach (var slot in artifactSlots)
            {
                if (slot.Artifact == artifact)
                {
                    slot.ClearArtifact();
                    return;
                }
            }
        }

        /// <summary>
        /// Clears all artifacts from the display.
        /// </summary>
        public void ClearAll()
        {
            foreach (var slot in artifactSlots)
            {
                slot.ClearArtifact();
            }
        }

        /// <summary>
        /// Sets the artifact manager reference.
        /// </summary>
        public void SetArtifactManager(ArtifactManager manager)
        {
            artifactManager = manager;
            RefreshDisplay();
        }
    }
}
