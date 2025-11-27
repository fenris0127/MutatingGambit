using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MutatingGambit.UI
{
    /// <summary>
    /// Manages tooltip display for UI elements and game objects.
    /// Singleton pattern for easy access throughout the game.
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        private static TooltipManager instance;

        [Header("Tooltip UI")]
        [SerializeField]
        private GameObject tooltipPanel;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private RectTransform tooltipRect;

        [Header("Settings")]
        [SerializeField]
        private float offsetX = 10f;

        [SerializeField]
        private float offsetY = 10f;

        [SerializeField]
        private float showDelay = 0.3f;

        private float hoverTimer = 0f;
        private bool isHovering = false;
        private string pendingTitle = "";
        private string pendingDescription = "";

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static TooltipManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TooltipManager>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject("TooltipManager");
                        instance = go.AddComponent<TooltipManager>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (isHovering)
            {
                hoverTimer += Time.deltaTime;

                if (hoverTimer >= showDelay && !tooltipPanel.activeSelf)
                {
                    ShowTooltipImmediate(pendingTitle, pendingDescription);
                }

                // Update tooltip position to follow mouse
                if (tooltipPanel.activeSelf)
                {
                    UpdateTooltipPosition();
                }
            }
        }

        /// <summary>
        /// Shows a tooltip with the given title and description.
        /// </summary>
        public void ShowTooltip(string title, string description)
        {
            pendingTitle = title;
            pendingDescription = description;
            isHovering = true;
            hoverTimer = 0f;
        }

        /// <summary>
        /// Shows a tooltip immediately without delay.
        /// </summary>
        public void ShowTooltipImmediate(string title, string description)
        {
            if (tooltipPanel == null)
            {
                Debug.LogWarning("TooltipManager: Tooltip panel is not assigned!");
                return;
            }

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            tooltipPanel.SetActive(true);
            UpdateTooltipPosition();
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void HideTooltip()
        {
            isHovering = false;
            hoverTimer = 0f;

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Updates the tooltip position to follow the mouse cursor.
        /// </summary>
        private void UpdateTooltipPosition()
        {
            if (tooltipRect == null)
            {
                return;
            }

            Vector2 mousePosition = Input.mousePosition;

            // Calculate pivot based on screen position to keep tooltip on screen
            float pivotX = mousePosition.x < Screen.width / 2 ? 0f : 1f;
            float pivotY = mousePosition.y < Screen.height / 2 ? 0f : 1f;

            tooltipRect.pivot = new Vector2(pivotX, pivotY);

            // Apply offset
            Vector2 offset = new Vector2(
                pivotX == 0 ? offsetX : -offsetX,
                pivotY == 0 ? offsetY : -offsetY
            );

            tooltipRect.position = mousePosition + offset;
        }

        /// <summary>
        /// Sets up the tooltip panel reference (useful for runtime initialization).
        /// </summary>
        public void SetTooltipPanel(GameObject panel, TextMeshProUGUI title, TextMeshProUGUI description)
        {
            tooltipPanel = panel;
            titleText = title;
            descriptionText = description;
            tooltipRect = panel.GetComponent<RectTransform>();

            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
    }
}
