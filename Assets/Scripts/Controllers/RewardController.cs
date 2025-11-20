using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using MutatingGambit.Core;
using MutatingGambit.Core.Rooms;
using MutatingGambit.Core.Artifacts;
using MutatingGambit.Core.Mutations;

namespace MutatingGambit
{
    /// <summary>
    /// Displays rewards and handles selection after combat
    /// </summary>
    public class RewardController : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI rewardInfoText;
        public Transform rewardButtonContainer;
        public Button skipButton;

        private RoomReward _reward;
        private Board _playerBoard;
        private List<Button> _rewardButtons = new List<Button>();

        public event Action OnRewardSelected;

        public void Initialize(RoomReward reward, Board playerBoard)
        {
            _reward = reward;
            _playerBoard = playerBoard;

            DisplayRewards();
        }

        void DisplayRewards()
        {
            // Clear old buttons
            foreach (var btn in _rewardButtons)
            {
                if (btn != null)
                    Destroy(btn.gameObject);
            }
            _rewardButtons.Clear();

            if (titleText != null)
            {
                titleText.text = "=== CHOOSE YOUR REWARD ===";
            }

            // Show currency
            string infoText = "";
            if (_reward.CurrencyAmount > 0)
            {
                infoText += $"💰 Earned: {_reward.CurrencyAmount} Gambit Fragments\n\n";
            }

            // Show guaranteed artifact
            if (_reward.GuaranteedArtifact != null)
            {
                infoText += "Guaranteed Artifact:\n";
                infoText += $"✨ {_reward.GuaranteedArtifact.Name}\n";
                infoText += $"   {_reward.GuaranteedArtifact.Description}\n\n";

                // Auto-add guaranteed artifact
                _playerBoard.ArtifactManager.AddArtifact(_reward.GuaranteedArtifact);
                Debug.Log($"Added artifact: {_reward.GuaranteedArtifact.Name}");
            }

            // Show choices
            if (_reward.ArtifactChoices != null && _reward.ArtifactChoices.Count > 0)
            {
                infoText += "Choose one:\n";
            }

            if (rewardInfoText != null)
            {
                rewardInfoText.text = infoText;
            }

            // Create buttons for artifact choices
            if (_reward.ArtifactChoices != null)
            {
                for (int i = 0; i < _reward.ArtifactChoices.Count; i++)
                {
                    var artifact = _reward.ArtifactChoices[i];
                    CreateRewardButton(artifact, i);
                }
            }

            // Setup skip button
            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();
                skipButton.onClick.AddListener(OnSkipClicked);

                var skipText = skipButton.GetComponentInChildren<TextMeshProUGUI>();
                if (skipText != null)
                {
                    skipText.text = _reward.ArtifactChoices != null && _reward.ArtifactChoices.Count > 0
                        ? "Skip Reward"
                        : "Continue";
                }
            }
        }

        void CreateRewardButton(Artifact artifact, int index)
        {
            GameObject btnObj = new GameObject($"RewardButton_{index}");
            btnObj.transform.SetParent(rewardButtonContainer != null ? rewardButtonContainer : transform);

            var rectTransform = btnObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(500, 100);
            rectTransform.anchoredPosition = new Vector2(0, -index * 120);

            var image = btnObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.2f, 0.4f);

            var button = btnObj.AddComponent<Button>();

            // Text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.1f);
            textRect.anchorMax = new Vector2(0.9f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = $"<b>{artifact.Name}</b>\n<size=16>{artifact.Description}</size>";
            tmp.fontSize = 20;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = Color.white;

            button.onClick.AddListener(() => OnRewardButtonClicked(artifact));
            _rewardButtons.Add(button);
        }

        void OnRewardButtonClicked(Artifact selectedArtifact)
        {
            // Add artifact to player's board
            _playerBoard.ArtifactManager.AddArtifact(selectedArtifact);
            Debug.Log($"Player selected artifact: {selectedArtifact.Name}");

            // Notify completion
            OnRewardSelected?.Invoke();
        }

        void OnSkipClicked()
        {
            Debug.Log("Player skipped reward");
            OnRewardSelected?.Invoke();
        }
    }
}
