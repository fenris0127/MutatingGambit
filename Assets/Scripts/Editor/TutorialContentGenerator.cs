using UnityEngine;
using UnityEditor;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.Systems.Tutorial;
using MutatingGambit.Systems.BoardSystem;

namespace MutatingGambit.Editor
{
    public class TutorialContentGenerator : EditorWindow
    {
        [MenuItem("MutatingGambit/Generate Tutorial Content")]
        public static void Generate()
        {
            EnsureDirectories();
            
            // 1. Create Tutorial Steps
            CreateTutorialSteps();
            
            // 2. Create Tutorial Room
            CreateTutorialRoom();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Tutorial Content Generated!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Tutorial"))
                AssetDatabase.CreateFolder("Assets/Resources", "Tutorial");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Rooms"))
                AssetDatabase.CreateFolder("Assets/Resources", "Rooms");
        }

        private static void CreateTutorialSteps()
        {
            // Step 1: Welcome
            TutorialStep step1 = ScriptableObject.CreateInstance<TutorialStep>();
            step1.message = "Welcome to the Mutating Gambit! In this dungeon, chess rules change constantly.";
            step1.buttonText = "Let's Go";
            step1.showButton = true;
            step1.restrictMovement = true; // No moving yet
            AssetDatabase.CreateAsset(step1, "Assets/Resources/Tutorial/Step1_Welcome.asset");

            // Step 2: Move Pawn
            TutorialStep step2 = ScriptableObject.CreateInstance<TutorialStep>();
            step2.message = "Let's start by moving your Pawn forward. Click the Pawn at E2 and move it to E4.";
            step2.showButton = false;
            step2.restrictMovement = true;
            step2.requiredMoveFrom = new Vector2Int(4, 1); // E2
            step2.requiredMoveTo = new Vector2Int(4, 3); // E4
            step2.completeOnMove = true;
            AssetDatabase.CreateAsset(step2, "Assets/Resources/Tutorial/Step2_MovePawn.asset");

            // Step 3: Capture
            TutorialStep step3 = ScriptableObject.CreateInstance<TutorialStep>();
            step3.message = "Good! Now, capture the enemy Pawn.";
            step3.showButton = false;
            step3.restrictMovement = true;
            // Assuming enemy moves to D4 or similar, but for tutorial we might force a scenario
            // For now, just generic capture instruction
            step3.completeOnMove = true;
            AssetDatabase.CreateAsset(step3, "Assets/Resources/Tutorial/Step3_Capture.asset");
        }

        private static void CreateTutorialRoom()
        {
            // TODO: This method needs to use SerializedObject to set readonly properties
            // For now, create the asset manually in the Unity Editor
            // Or use reflection/SerializedObject API

            Debug.LogWarning("TutorialRoom creation skipped - properties are readonly. Create manually in Unity Editor.");

            /* Example of how to use SerializedObject:
            RoomData room = ScriptableObject.CreateInstance<RoomData>();
            room.name = "TutorialRoom";

            SerializedObject serializedRoom = new SerializedObject(room);
            serializedRoom.FindProperty("roomName").stringValue = "Training Grounds";
            serializedRoom.FindProperty("roomDescription").stringValue = "Learn the basics of combat.";
            serializedRoom.FindProperty("roomType").enumValueIndex = (int)RoomType.Start;
            serializedRoom.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(room, "Assets/Resources/Rooms/TutorialRoom.asset");
            */
        }
    }
}
