using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MutatingGambit.Systems.SaveLoad;

namespace MutatingGambit.UI
{
    public class RunHistoryUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform contentContainer;
        [SerializeField] private GameObject runEntryPrefab;

        private void OnEnable()
        {
            RefreshHistory();
        }

        public void RefreshHistory()
        {
            ClearContent();
            if (GlobalDataManager.Instance == null) return;

            var history = GlobalDataManager.Instance.GetRunHistory();
            foreach (var run in history)
            {
                CreateRunEntry(run);
            }
        }

        private void CreateRunEntry(RunRecord run)
        {
            if (runEntryPrefab == null || contentContainer == null) return;

            GameObject entry = Instantiate(runEntryPrefab, contentContainer);
            
            // Setup text
            var text = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                string resultColor = run.IsVictory ? "green" : "red";
                string resultText = run.IsVictory ? "VICTORY" : "DEFEAT";
                
                text.text = $"<color={resultColor}>{resultText}</color> - {run.Date}\n" +
                            $"Floor {run.FloorsCleared}-{run.RoomsCleared} | Score: {run.FinalScore}\n" +
                            $"Artifacts: {run.CollectedArtifacts.Count} | Mutations: {run.FinalMutations.Count}";
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
