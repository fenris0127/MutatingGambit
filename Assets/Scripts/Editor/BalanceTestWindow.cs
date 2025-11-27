using UnityEngine;
using UnityEditor;
using MutatingGambit.Systems.Testing;

namespace MutatingGambit.Editor
{
    public class BalanceTestWindow : EditorWindow
    {
        private BalanceTestManager testManager;
        private Vector2 scrollPosition;

        [MenuItem("MutatingGambit/Balance Tester")]
        public static void ShowWindow()
        {
            GetWindow<BalanceTestWindow>("Balance Tester");
        }

        private void OnGUI()
        {
            GUILayout.Label("Balance Testing System", EditorStyles.boldLabel);

            if (testManager == null)
            {
                testManager = FindObjectOfType<BalanceTestManager>();
            }

            if (testManager == null)
            {
                EditorGUILayout.HelpBox("BalanceTestManager not found in scene. Please add it to a GameObject.", MessageType.Warning);
                if (GUILayout.Button("Create BalanceTestManager"))
                {
                    GameObject go = new GameObject("BalanceTestManager");
                    testManager = go.AddComponent<BalanceTestManager>();
                }
                return;
            }

            // Draw Default Inspector for configuration
            // This allows us to use the List<Mutation> UI from Unity
            ScriptableObject target = testManager;
            SerializedObject so = new SerializedObject(target);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                // Skip script field
                if (prop.name == "m_Script") 
                {
                    enterChildren = false;
                    continue;
                }
                
                EditorGUILayout.PropertyField(prop, true);
                enterChildren = false;
            }
            so.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
            
            if (testManager.IsRunning)
            {
                EditorGUILayout.HelpBox($"Testing in progress... Games: {testManager.TotalGamesPlayed}", MessageType.Info);
            }
            else
            {
                if (GUILayout.Button("Run Test", GUILayout.Height(30)))
                {
                    if (Application.isPlaying)
                    {
                        testManager.RunTest();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Tests must be run in Play Mode.", "OK");
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"White Wins: {testManager.PlayerAWins}");
            EditorGUILayout.LabelField($"Black Wins: {testManager.PlayerBWins}");
            EditorGUILayout.LabelField($"Draws: {testManager.Draws}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            
            // Force repaint to show progress
            if (testManager.IsRunning)
            {
                Repaint();
            }
        }
    }
}
