using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Represents a single artifact slot in the UI.
    /// Displays artifact icon and handles tooltips.
    /// </summary>
    public class ArtifactSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private GameObject emptyIndicator;

        [SerializeField]
        private TextMeshProUGUI stackCountText;

        [Header("Visual Settings")]
        [SerializeField]
        private Color emptyColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [SerializeField]
        private Color filledColor = Color.white;

        private Artifact artifact;
        private int stackCount = 1;

        /// <summary>
        /// Gets the artifact in this slot.
        /// </summary>
        public Artifact Artifact => artifact;

        /// <summary>
        /// Gets whether this slot is empty.
        /// </summary>
        public bool IsEmpty => artifact == null;

        private void Awake()
        {
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Sets the artifact to display in this slot.
        /// </summary>
        public void SetArtifact(Artifact newArtifact, int count = 1)
        {
            artifact = newArtifact;
            stackCount = count;
            UpdateVisuals();
        }

        /// <summary>
        /// Clears the artifact from this slot.
        /// </summary>
        public void ClearArtifact()
        {
            artifact = null;
            stackCount = 0;
            UpdateVisuals();
        }

        /// <summary>
        /// Updates the visual representation of the slot.
        /// </summary>
        private void UpdateVisuals()
        {
            if (iconImage == null)
            {
                return;
            }

            if (artifact == null)
            {
                // Empty slot
                iconImage.sprite = null;
                iconImage.color = emptyColor;

                if (emptyIndicator != null)
                {
                    emptyIndicator.SetActive(true);
                }

                if (stackCountText != null)
                {
                    stackCountText.gameObject.SetActive(false);
                }
            }
            else
            {
                // Filled slot
                iconImage.sprite = artifact.Icon;
                iconImage.color = filledColor;

                if (emptyIndicator != null)
                {
                    emptyIndicator.SetActive(false);
                }

                // Show stack count if > 1
                if (stackCountText != null)
                {
                    if (stackCount > 1)
                    {
                        stackCountText.text = stackCount.ToString();
                        stackCountText.gameObject.SetActive(true);
                    }
                    else
                    {
                        stackCountText.gameObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Called when mouse enters the slot.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (artifact != null)
            {
                string description = BuildArtifactTooltip();
                TooltipManager.Instance.ShowTooltip(artifact.ArtifactName, description);
            }
        }

        /// <summary>
        /// Called when mouse exits the slot.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTooltip();
        }

        /// <summary>
        /// Builds the tooltip text for the artifact.
        /// </summary>
        private string BuildArtifactTooltip()
        {
            if (artifact == null)
            {
                return "";
            }

            var sb = new System.Text.StringBuilder();

            // Description
            sb.AppendLine(artifact.Description);

            // Trigger type
            sb.AppendLine();
            sb.AppendLine($"<i>Trigger: {GetTriggerDescription(artifact.Trigger)}</i>");

            // Rarity (if available)
            if (artifact.Rarity != ArtifactRarity.Common)
            {
                sb.AppendLine($"<color={GetRarityColor(artifact.Rarity)}>{artifact.Rarity}</color>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a human-readable description of the artifact trigger.
        /// </summary>
        private string GetTriggerDescription(ArtifactTrigger trigger)
        {
            return trigger switch
            {
                ArtifactTrigger.OnTurnStart => "At the start of each turn",
                ArtifactTrigger.OnTurnEnd => "At the end of each turn",
                ArtifactTrigger.OnPieceMove => "When a piece moves",
                ArtifactTrigger.OnPieceCapture => "When a piece is captured",
                ArtifactTrigger.OnKingMove => "When the king moves",
                ArtifactTrigger.Passive => "Passive effect",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the color for a rarity tier.
        /// </summary>
        private string GetRarityColor(ArtifactRarity rarity)
        {
            return rarity switch
            {
                ArtifactRarity.Common => "white",
                ArtifactRarity.Uncommon => "#00FF00", // Green
                ArtifactRarity.Rare => "#0080FF", // Blue
                ArtifactRarity.Epic => "#8000FF", // Purple
                ArtifactRarity.Legendary => "#FFD700", // Gold
                _ => "white"
            };
        }
    }
}
