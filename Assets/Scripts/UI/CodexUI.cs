using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MutatingGambit.Systems.SaveLoad;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.UI
{
    public class CodexUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MutationLibrary mutationLibrary;
        [SerializeField] private ArtifactLibrary artifactLibrary;
        
        [Header("UI Elements")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject itemPrefab; // Simple prefab with Image and Text
        [SerializeField] private Button mutationsTabButton;
        [SerializeField] private Button artifactsTabButton;
        [SerializeField] private TextMeshProUGUI detailsText;

        private void Start()
        {
            if (mutationsTabButton != null)
                mutationsTabButton.onClick.AddListener(ShowMutations);
            
            if (artifactsTabButton != null)
                artifactsTabButton.onClick.AddListener(ShowArtifacts);

            // Default view
            ShowMutations();
        }

        public void ShowMutations()
        {
            ClearContent();
            if (mutationLibrary == null) return;

            foreach (var mutation in mutationLibrary.AllMutations)
            {
                CreateItemEntry(
                    mutation.MutationName, 
                    mutation.Icon, 
                    GlobalDataManager.Instance.IsMutationDiscovered(mutation.MutationName),
                    mutation.Description
                );
            }
        }

        public void ShowArtifacts()
        {
            ClearContent();
            if (artifactLibrary == null) return;

            foreach (var artifact in artifactLibrary.AllArtifacts)
            {
                CreateItemEntry(
                    artifact.ArtifactName, 
                    artifact.Icon, 
                    GlobalDataManager.Instance.IsArtifactDiscovered(artifact.ArtifactName),
                    artifact.Description
                );
            }
        }

        private void CreateItemEntry(string name, Sprite icon, bool isDiscovered, string description)
        {
            if (itemPrefab == null || contentContainer == null) return;

            GameObject entry = Instantiate(itemPrefab, contentContainer);
            
            // Setup UI
            var texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            var images = entry.GetComponentsInChildren<Image>();

            if (isDiscovered)
            {
                if (texts.Length > 0) texts[0].text = name;
                if (images.Length > 1) images[1].sprite = icon; // Assuming 0 is bg, 1 is icon
                
                // Add click listener for details
                var btn = entry.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => ShowDetails(name, description));
                }
            }
            else
            {
                if (texts.Length > 0) texts[0].text = "???";
                if (images.Length > 1) images[1].color = Color.black; // Silhouette
                
                var btn = entry.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }
        }

        private void ShowDetails(string name, string desc)
        {
            if (detailsText != null)
            {
                detailsText.text = $"<b>{name}</b>\n\n{desc}";
            }
        }

        private void ClearContent()
        {
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
