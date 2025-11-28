using UnityEngine;
using UnityEditor;
using System.IO;
using MutatingGambit.Systems.Mutations;
using MutatingGambit.Systems.Artifacts;
using MutatingGambit.Systems.BoardSystem;
using MutatingGambit.Systems.Dungeon;
using MutatingGambit.AI;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Editor
{
    /// <summary>
    /// Unity Editor tool to automatically create all ScriptableObject assets needed for MVP.
    /// Menu: Tools/Mutating Gambit/Create All MVP Assets
    /// </summary>
    public class MVPAssetCreator : EditorWindow
    {
        private const string DATA_PATH = "Assets/Data";
        private const string MUTATIONS_PATH = DATA_PATH + "/Mutations";
        private const string ARTIFACTS_PATH = DATA_PATH + "/Artifacts";
        private const string BOARDS_PATH = DATA_PATH + "/Boards";
        private const string ROOMS_PATH = DATA_PATH + "/Rooms";
        private const string AI_PATH = DATA_PATH + "/AI";
        private const string VICTORY_PATH = DATA_PATH + "/VictoryConditions";

        private bool createMutations = true;
        private bool createArtifacts = true;
        private bool createBoards = true;
        private bool createRooms = true;
        private bool createAI = true;
        private bool createVictoryConditions = true;

        [MenuItem("Tools/Mutating Gambit/Asset Creator")]
        public static void ShowWindow()
        {
            GetWindow<MVPAssetCreator>("MVP Asset Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create MVP ScriptableObject Assets", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will create all necessary ScriptableObject assets for MVP.\n" +
                "Assets will be created in Assets/Data/ subfolders.",
                MessageType.Info
            );

            GUILayout.Space(10);

            createMutations = EditorGUILayout.Toggle("Create Mutations (10)", createMutations);
            createArtifacts = EditorGUILayout.Toggle("Create Artifacts (20)", createArtifacts);
            createBoards = EditorGUILayout.Toggle("Create Board Data (3)", createBoards);
            createRooms = EditorGUILayout.Toggle("Create Room Data (5)", createRooms);
            createAI = EditorGUILayout.Toggle("Create AI Configs (3)", createAI);
            createVictoryConditions = EditorGUILayout.Toggle("Create Victory Conditions", createVictoryConditions);

            GUILayout.Space(20);

            if (GUILayout.Button("Create All Selected Assets", GUILayout.Height(40)))
            {
                CreateAssets();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Create Folders Only"))
            {
                CreateFolders();
                EditorUtility.DisplayDialog("Success", "Folders created successfully!", "OK");
            }
        }

        private void CreateAssets()
        {
            CreateFolders();

            int assetsCreated = 0;

            if (createMutations) assetsCreated += CreateMutations();
            if (createArtifacts) assetsCreated += CreateArtifacts();
            if (createBoards) assetsCreated += CreateBoardData();
            if (createRooms) assetsCreated += CreateRoomData();
            if (createAI) assetsCreated += CreateAIConfigs();
            if (createVictoryConditions) assetsCreated += CreateVictoryConditions();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Assets Created",
                $"Successfully created {assetsCreated} ScriptableObject assets!\n\n" +
                $"Check Assets/Data/ subfolders.",
                "OK"
            );
        }

        private void CreateFolders()
        {
            CreateFolderIfNotExists(DATA_PATH);
            CreateFolderIfNotExists(MUTATIONS_PATH);
            CreateFolderIfNotExists(ARTIFACTS_PATH);
            CreateFolderIfNotExists(BOARDS_PATH);
            CreateFolderIfNotExists(ROOMS_PATH);
            CreateFolderIfNotExists(AI_PATH);
            CreateFolderIfNotExists(VICTORY_PATH);
        }

        private void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentPath = Path.GetDirectoryName(path).Replace('\\', '/');
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        // ========================================
        // MUTATIONS (10 total)
        // ========================================
        private int CreateMutations()
        {
            Debug.Log("Creating Mutation assets...");

            CreateAsset<LeapingRookMutation>(MUTATIONS_PATH, "LeapingRook");
            CreateAsset<SplittingKnightMutation>(MUTATIONS_PATH, "SplittingKnight");
            CreateAsset<FragileBishopMutation>(MUTATIONS_PATH, "FragileBishop");
            CreateAsset<PhantomPawnMutation>(MUTATIONS_PATH, "PhantomPawn");
            CreateAsset<BerserkQueenMutation>(MUTATIONS_PATH, "BerserkQueen");
            CreateAsset<TeleportingKnightMutation>(MUTATIONS_PATH, "TeleportingKnight");
            CreateAsset<FrozenBishopMutation>(MUTATIONS_PATH, "FrozenBishop");
            CreateAsset<DoubleMoveRookMutation>(MUTATIONS_PATH, "DoubleMoveRook");
            CreateAsset<ExplosiveKingMutation>(MUTATIONS_PATH, "ExplosiveKing");
            CreateAsset<ShadowPawnMutation>(MUTATIONS_PATH, "ShadowPawn");

            return 10;
        }

        // ========================================
        // ARTIFACTS (20 total)
        // ========================================
        private int CreateArtifacts()
        {
            Debug.Log("Creating Artifact assets...");

            // Existing artifacts
            CreateAsset<GravityMirrorArtifact>(ARTIFACTS_PATH, "GravityMirror");
            CreateAsset<KingsShadowArtifact>(ARTIFACTS_PATH, "KingsShadow");
            CreateAsset<CavalryChargeArtifact>(ARTIFACTS_PATH, "CavalryCharge");
            CreateAsset<TimeWarpArtifact>(ARTIFACTS_PATH, "TimeWarp");
            CreateAsset<MagneticFieldArtifact>(ARTIFACTS_PATH, "MagneticField");
            CreateAsset<PhoenixFeatherArtifact>(ARTIFACTS_PATH, "PhoenixFeather");
            CreateAsset<BloodMoonArtifact>(ARTIFACTS_PATH, "BloodMoon");

            // New artifacts
            CreateAsset<ChainLightningArtifact>(ARTIFACTS_PATH, "ChainLightning");
            CreateAsset<PromotionPrivilegeArtifact>(ARTIFACTS_PATH, "PromotionPrivilege");
            CreateAsset<FrozenThroneArtifact>(ARTIFACTS_PATH, "FrozenThrone");
            CreateAsset<MimicsMaskArtifact>(ARTIFACTS_PATH, "MimicsMask");
            CreateAsset<BerserkersRageArtifact>(ARTIFACTS_PATH, "BerserkersRage");
            CreateAsset<SanctuaryShieldArtifact>(ARTIFACTS_PATH, "SanctuaryShield");
            CreateAsset<PhantomStepsArtifact>(ARTIFACTS_PATH, "PhantomSteps");
            CreateAsset<TwinSoulsArtifact>(ARTIFACTS_PATH, "TwinSouls");
            CreateAsset<CursedCrownArtifact>(ARTIFACTS_PATH, "CursedCrown");
            CreateAsset<DivineInterventionArtifact>(ARTIFACTS_PATH, "DivineIntervention");
            CreateAsset<HasteBootsArtifact>(ARTIFACTS_PATH, "HasteBoots");
            CreateAsset<SacrificialAltarArtifact>(ARTIFACTS_PATH, "SacrificialAltar");
            CreateAsset<WeakeningAuraArtifact>(ARTIFACTS_PATH, "WeakeningAura");
            CreateAsset<ResurrectionStoneArtifact>(ARTIFACTS_PATH, "ResurrectionStone");

            return 21; // 7 existing + 14 new
        }

        // ========================================
        // BOARD DATA (3 variations)
        // ========================================
        private int CreateBoardData()
        {
            Debug.Log("Creating BoardData assets...");

            // 8x8 Standard
            var standard = CreateAsset<BoardData>(BOARDS_PATH, "Standard_8x8");
            if (standard != null)
            {
                // Use SerializedObject to set readonly properties
                SerializedObject so = new SerializedObject(standard);
                so.FindProperty("width").intValue = 8;
                so.FindProperty("height").intValue = 8;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(standard);
            }

            // 6x6 Small
            var small = CreateAsset<BoardData>(BOARDS_PATH, "Small_6x6");
            if (small != null)
            {
                SerializedObject so = new SerializedObject(small);
                so.FindProperty("width").intValue = 6;
                so.FindProperty("height").intValue = 6;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(small);
            }

            // 5x8 with obstacles
            var obstacles = CreateAsset<BoardData>(BOARDS_PATH, "Obstacles_5x8");
            if (obstacles != null)
            {
                SerializedObject so = new SerializedObject(obstacles);
                so.FindProperty("width").intValue = 5;
                so.FindProperty("height").intValue = 8;
                so.ApplyModifiedProperties();
                // You can set specific obstacles in the inspector later
                EditorUtility.SetDirty(obstacles);
            }

            return 3;
        }

        // ========================================
        // ROOM DATA (5 basic rooms)
        // ========================================
        private int CreateRoomData()
        {
            Debug.Log("Creating RoomData assets...");

            // Normal Combat 1
            var combat1 = CreateAsset<RoomData>(ROOMS_PATH, "Combat_BasicEncounter");
            if (combat1 != null)
            {
                // Set properties via SerializedObject to access private fields
                SerializedObject so = new SerializedObject(combat1);
                so.FindProperty("roomName").stringValue = "Basic Encounter";
                so.FindProperty("roomDescription").stringValue = "A simple chess battle.";
                so.FindProperty("roomType").enumValueIndex = (int)RoomType.NormalCombat;
                so.ApplyModifiedProperties();
            }

            // Normal Combat 2
            var combat2 = CreateAsset<RoomData>(ROOMS_PATH, "Combat_PawnAdvance");
            if (combat2 != null)
            {
                SerializedObject so = new SerializedObject(combat2);
                so.FindProperty("roomName").stringValue = "Pawn Advance";
                so.FindProperty("roomDescription").stringValue = "Enemy pawns march forward.";
                so.FindProperty("roomType").enumValueIndex = (int)RoomType.NormalCombat;
                so.ApplyModifiedProperties();
            }

            // Elite Combat
            var elite = CreateAsset<RoomData>(ROOMS_PATH, "Elite_MutatedKnight");
            if (elite != null)
            {
                SerializedObject so = new SerializedObject(elite);
                so.FindProperty("roomName").stringValue = "Mutated Knight";
                so.FindProperty("roomDescription").stringValue = "Face a knight with strange powers.";
                so.FindProperty("roomType").enumValueIndex = (int)RoomType.EliteCombat;
                so.ApplyModifiedProperties();
            }

            // Boss
            var boss = CreateAsset<RoomData>(ROOMS_PATH, "Boss_DarkKing");
            if (boss != null)
            {
                SerializedObject so = new SerializedObject(boss);
                so.FindProperty("roomName").stringValue = "The Dark King";
                so.FindProperty("roomDescription").stringValue = "Final boss encounter.";
                so.FindProperty("roomType").enumValueIndex = (int)RoomType.Boss;
                so.ApplyModifiedProperties();
            }

            // Rest
            var rest = CreateAsset<RoomData>(ROOMS_PATH, "Rest_RepairStation");
            if (rest != null)
            {
                SerializedObject so = new SerializedObject(rest);
                so.FindProperty("roomName").stringValue = "Repair Station";
                so.FindProperty("roomDescription").stringValue = "Repair broken pieces.";
                so.FindProperty("roomType").enumValueIndex = (int)RoomType.Rest;
                so.ApplyModifiedProperties();
            }

            return 5;
        }

        // ========================================
        // AI CONFIGS (3 difficulty levels)
        // ========================================
        private int CreateAIConfigs()
        {
            Debug.Log("Creating AIConfig assets...");

            // Easy
            var easy = CreateAsset<AIConfig>(AI_PATH, "AI_Easy");
            if (easy != null)
            {
                SerializedObject so = new SerializedObject(easy);
                so.FindProperty("searchDepth").intValue = 2;
                so.FindProperty("maxTimePerMove").floatValue = 500f;
                so.FindProperty("randomnessFactor").floatValue = 0.3f;
                so.ApplyModifiedProperties();
            }

            // Normal
            var normal = CreateAsset<AIConfig>(AI_PATH, "AI_Normal");
            if (normal != null)
            {
                SerializedObject so = new SerializedObject(normal);
                so.FindProperty("searchDepth").intValue = 3;
                so.FindProperty("maxTimePerMove").floatValue = 1000f;
                so.FindProperty("randomnessFactor").floatValue = 0.1f;
                so.ApplyModifiedProperties();
            }

            // Hard
            var hard = CreateAsset<AIConfig>(AI_PATH, "AI_Hard");
            if (hard != null)
            {
                SerializedObject so = new SerializedObject(hard);
                so.FindProperty("searchDepth").intValue = 4;
                so.FindProperty("maxTimePerMove").floatValue = 2000f;
                so.FindProperty("randomnessFactor").floatValue = 0.05f;
                so.ApplyModifiedProperties();
            }

            return 3;
        }

        // ========================================
        // VICTORY CONDITIONS
        // ========================================
        private int CreateVictoryConditions()
        {
            Debug.Log("Creating VictoryCondition assets...");

            // Checkmate in 5 moves
            var checkmate5 = CreateAsset<CheckmateInNMovesCondition>(VICTORY_PATH, "CheckmateIn5Moves");
            if (checkmate5 != null)
            {
                SerializedObject so = new SerializedObject(checkmate5);
                so.FindProperty("conditionName").stringValue = "Checkmate in 5 Moves";
                so.FindProperty("description").stringValue = "Achieve checkmate within 5 moves.";
                so.FindProperty("maxMoves").intValue = 5;
                so.ApplyModifiedProperties();
            }

            // Capture specific piece (Queen)
            var captureQueen = CreateAsset<CaptureSpecificPieceCondition>(VICTORY_PATH, "CaptureEnemyQueen");
            if (captureQueen != null)
            {
                SerializedObject so = new SerializedObject(captureQueen);
                so.FindProperty("conditionName").stringValue = "Capture Enemy Queen";
                so.FindProperty("description").stringValue = "Capture the enemy Queen to win.";
                so.FindProperty("targetPieceType").enumValueIndex = (int)PieceType.Queen;
                so.ApplyModifiedProperties();
            }

            return 2;
        }

        // ========================================
        // HELPER METHOD
        // ========================================
        private T CreateAsset<T>(string folderPath, string assetName) where T : ScriptableObject
        {
            string assetPath = $"{folderPath}/{assetName}.asset";

            // Check if asset already exists
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existingAsset != null)
            {
                Debug.Log($"Asset already exists: {assetPath}");
                return existingAsset;
            }

            // Create new asset
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log($"Created asset: {assetPath}");

            return asset;
        }
    }
}
