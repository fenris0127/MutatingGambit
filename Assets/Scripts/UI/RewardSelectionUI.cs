using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.UI
{
    /// <summary>
    /// UI for selecting rewards (artifacts) after clearing a room.
    /// Typically shows 3 choices.
    /// </summary>
    public class RewardSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private GameObject rewardPanel;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private Transform rewardCardContainer;

        [SerializeField]
        private GameObject rewardCardPrefab;

        [SerializeField]
        private Button skipButton;

        [Header("Settings")]
        [SerializeField]
        private int numberOfChoices = 3;

        [Header("Events")]
        public UnityEvent<Artifact> OnArtifactSelected;
        public UnityEvent OnSkipped;

        /// <summary>
        /// Alias for OnArtifactSelected for compatibility.
        /// </summary>
        public UnityEvent<Artifact> OnRewardSelected => OnArtifactSelected;

        private List<Artifact> availableRewards = new List<Artifact>();
        private List<RewardCard> rewardCards = new List<RewardCard>();

        private void Awake()
        {
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }
        }

        /// <summary>
        /// Shows the reward selection UI with the given artifact choices.
        /// </summary>
        public void ShowRewards(List<Artifact> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                Debug.LogWarning("RewardSelectionUI: No rewards to display!");
                return;
            }

            availableRewards = new List<Artifact>(rewards);
            CreateRewardCards();

            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = "Choose Your Reward";
            }
        }

        /// <summary>
        /// Creates reward cards for each artifact choice.
        /// </summary>
        private void CreateRewardCards()
        {
            // Clear existing cards
            foreach (var card in rewardCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            rewardCards.Clear();

            // Create new cards
            int cardCount = Mathf.Min(availableRewards.Count, numberOfChoices);

            for (int i = 0; i < cardCount; i++)
            {
                Artifact artifact = availableRewards[i];
                GameObject cardObject;

                if (rewardCardPrefab != null && rewardCardContainer != null)
                {
                    cardObject = Instantiate(rewardCardPrefab, rewardCardContainer);
                }
                else
                {
                    cardObject = new GameObject($"RewardCard_{i}");
                    if (rewardCardContainer != null)
                    {
                        cardObject.transform.SetParent(rewardCardContainer);
                    }
                }

                RewardCard card = cardObject.GetComponent<RewardCard>();
                if (card == null)
                {
                    card = cardObject.AddComponent<RewardCard>();
                }

                card.SetArtifact(artifact);
                card.OnSelected += () => OnRewardCardClicked(artifact);

                rewardCards.Add(card);
            }
        }

        /// <summary>
        /// Called when a reward card is clicked.
        /// </summary>
        private void OnRewardCardClicked(Artifact selectedArtifact)
        {
            OnArtifactSelected?.Invoke(selectedArtifact);
            Hide();
        }

        /// <summary>
        /// Called when skip button is clicked.
        /// </summary>
        private void OnSkipClicked()
        {
            OnSkipped?.Invoke();
            Hide();
        }

        /// <summary>
        /// Hides the reward selection UI.
        /// </summary>
        public void Hide()
        {
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }

            // Clean up cards
            foreach (var card in rewardCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            rewardCards.Clear();
            availableRewards.Clear();
        }

        /// <summary>
        /// Checks if the UI is currently visible.
        /// </summary>
        public bool IsVisible => rewardPanel != null && rewardPanel.activeSelf;
    }

    /// <summary>
    /// Individual reward card component.
    /// </summary>
    public class RewardCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private Image rarityBorder;

        [SerializeField]
        private Button selectButton;

        private Artifact artifact;

        public System.Action OnSelected;

        private void Awake()
        {
            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (selectButton != null)
            {
                selectButton.onClick.AddListener(HandleSelect);
            }
        }

        /// <summary>
        /// Sets the artifact to display on this card.
        /// </summary>
        public void SetArtifact(Artifact newArtifact)
        {
            artifact = newArtifact;
            UpdateDisplay();
        }

        /// <summary>
        /// Updates the visual display.
        /// </summary>
        private void UpdateDisplay()
        {
            if (artifact == null)
            {
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = artifact.Icon;
            }

            if (nameText != null)
            {
                nameText.text = artifact.ArtifactName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = artifact.Description;
            }

            if (rarityBorder != null)
            {
                rarityBorder.color = GetRarityColor(artifact.Rarity);
            }
        }

        /// <summary>
        /// Called when the card is selected.
        /// </summary>
        private void HandleSelect()
        {
            OnSelected?.Invoke();
        }

        /// <summary>
        /// Gets the color for a rarity tier.
        /// </summary>
        private Color GetRarityColor(ArtifactRarity rarity)
        {
            return rarity switch
            {
                ArtifactRarity.Common => Color.white,
                ArtifactRarity.Uncommon => new Color(0f, 1f, 0f), // Green
                ArtifactRarity.Rare => new Color(0f, 0.5f, 1f), // Blue
                ArtifactRarity.Epic => new Color(0.5f, 0f, 1f), // Purple
                ArtifactRarity.Legendary => new Color(1f, 0.84f, 0f), // Gold
                _ => Color.white
            };
        }
    }
}
