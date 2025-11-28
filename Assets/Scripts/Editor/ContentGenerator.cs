using UnityEngine;
using UnityEditor;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.Systems.BoardSystem;
using MutatingGambit.Core.ChessEngine;
using System.IO;

namespace MutatingGambit.Editor
{
    public class ContentGenerator : EditorWindow
    {
        [MenuItem("MutatingGambit/Generate Content")]
        public static void ShowWindow()
        {
            GetWindow<ContentGenerator>("Content Generator");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Generate Boss Room"))
            {
                GenerateBossRoom();
            }

            if (GUILayout.Button("Generate Additional Rooms"))
            {
                GenerateAdditionalRooms();
            }
        }

        private static void GenerateBossRoom()
        {
            string folderPath = "Assets/Data/Rooms";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            RoomData room = ScriptableObject.CreateInstance<RoomData>();
            room.name = "BossRoom_TheGrandmaster";
            
            // We need to set private fields via SerializedObject since they are private
            SerializedObject so = new SerializedObject(room);
            so.FindProperty("roomName").stringValue = "The Grandmaster's Chamber";
            so.FindProperty("roomDescription").stringValue = "The final challenge. Defeat the Grandmaster to escape the dungeon.";
            so.FindProperty("roomType").enumValueIndex = (int)RoomType.Boss;
            so.FindProperty("currencyReward").intValue = 100;
            
            // Create specific board for boss
            BoardData board = CreateBoardData("BossBoard", 8, 8);
            so.FindProperty("boardData").objectReferenceValue = board;

            // Create victory condition
            CheckmateInNMovesCondition victory = CreateVictoryCondition<CheckmateInNMovesCondition>("BossVictory");
            SerializedObject soVictory = new SerializedObject(victory);
            soVictory.FindProperty("maxMoves").intValue = 50; // Long battle
            soVictory.ApplyModifiedProperties();
            so.FindProperty("victoryCondition").objectReferenceValue = victory;

            so.ApplyModifiedProperties();

            AssetDatabase.CreateAsset(room, $"{folderPath}/{room.name}.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Boss Room Generated!");
        }

        private static void GenerateAdditionalRooms()
        {
            string folderPath = "Assets/Data/Rooms";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            for (int i = 1; i <= 10; i++)
            {
                RoomData room = ScriptableObject.CreateInstance<RoomData>();
                room.name = $"Room_Generated_{i}";
                
                SerializedObject so = new SerializedObject(room);
                so.FindProperty("roomName").stringValue = $"Chamber {i}";
                so.FindProperty("roomDescription").stringValue = "A challenging room with unique constraints.";
                so.FindProperty("roomType").enumValueIndex = (int)(i % 3 == 0 ? RoomType.EliteCombat : RoomType.NormalCombat);
                so.FindProperty("currencyReward").intValue = 10 + i * 5;

                // Create board
                BoardData board = CreateBoardData($"Board_Gen_{i}", 8, 8);
                // Add some obstacles randomly
                if (i > 5)
                {
                    AddRandomObstacles(board, 5);
                }
                so.FindProperty("boardData").objectReferenceValue = board;

                // Create victory condition
                CheckmateInNMovesCondition victory = CreateVictoryCondition<CheckmateInNMovesCondition>($"Victory_Gen_{i}");
                SerializedObject soVictory = new SerializedObject(victory);
                soVictory.FindProperty("maxMoves").intValue = 10 + i;
                soVictory.ApplyModifiedProperties();
                so.FindProperty("victoryCondition").objectReferenceValue = victory;

                so.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(room, $"{folderPath}/{room.name}.asset");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Additional Rooms Generated!");
        }

        private static BoardData CreateBoardData(string name, int width, int height)
        {
            string folderPath = "Assets/Data/Boards";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            BoardData board = ScriptableObject.CreateInstance<BoardData>();
            board.name = name;
            
            // Initialize via reflection or SerializedObject because fields are private
            SerializedObject so = new SerializedObject(board);
            so.FindProperty("width").intValue = width;
            so.FindProperty("height").intValue = height;
            so.ApplyModifiedProperties();
            
            // Force initialization of tiles
            board.InitializeTiles();
            
            // Save asset
            string path = $"{folderPath}/{name}.asset";
            // Check if exists
            BoardData existing = AssetDatabase.LoadAssetAtPath<BoardData>(path);
            if (existing != null) return existing;

            AssetDatabase.CreateAsset(board, path);
            return board;
        }

        private static void AddRandomObstacles(BoardData board, int count)
        {
            for(int i=0; i<count; i++)
            {
                int x = Random.Range(0, board.Width);
                int y = Random.Range(2, board.Height - 2); // Avoid starting zones
                board.SetTileType(x, y, TileType.Obstacle);
            }
            EditorUtility.SetDirty(board);
        }

        private static T CreateVictoryCondition<T>(string name) where T : VictoryCondition
        {
            string folderPath = "Assets/Data/VictoryConditions";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string path = $"{folderPath}/{name}.asset";
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            T condition = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(condition, path);
            return condition;
        }
    }
}
