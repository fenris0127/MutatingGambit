using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Systems.Tutorial
{
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "MutatingGambit/Tutorial/Step")]
    public class TutorialStep : ScriptableObject
    {
        [Header("Display")]
        [TextArea(3, 10)]
        public string message;
        public string buttonText = "Next";
        public bool showButton = true;

        [Header("Constraints")]
        public bool restrictMovement = false;
        public Vector2Int requiredMoveFrom = new Vector2Int(-1, -1);
        public Vector2Int requiredMoveTo = new Vector2Int(-1, -1);
        
        [Header("Highlighting")]
        public Vector2Int[] highlightedTiles;

        [Header("Completion")]
        public bool completeOnMove = false;
        public bool completeOnButtonPress = true;
    }
}
