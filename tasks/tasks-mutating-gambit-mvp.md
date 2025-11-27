# Tasks: The Mutating Gambit - MVP Implementation

Based on PRD v1.0 - Core roguelite chess game with dynamic rule mutations

## Relevant Files

### Core Systems
- `Assets/Scripts/Core/ChessEngine/Piece.cs` - Base piece class with dynamic rule list
- `Assets/Scripts/Core/ChessEngine/Board.cs` - Board state management and validation
- `Assets/Scripts/Core/ChessEngine/GameManager.cs` - Main game loop controller
- `Assets/Scripts/Core/ChessEngine/MoveValidator.cs` - Validates moves based on current rules

### Movement Rules (ScriptableObjects)
- `Assets/Scripts/Core/MovementRules/MovementRule.cs` - Abstract base class for movement rules
- `Assets/Scripts/Core/MovementRules/StraightLineRule.cs` - Rook-like movement
- `Assets/Scripts/Core/MovementRules/DiagonalRule.cs` - Bishop-like movement
- `Assets/Scripts/Core/MovementRules/KnightJumpRule.cs` - L-shaped movement
- `Assets/Scripts/Core/MovementRules/KingStepRule.cs` - One-square movement

### Mutation System
- `Assets/Scripts/Systems/Mutations/Mutation.cs` - Base mutation ScriptableObject
- `Assets/Scripts/Systems/Mutations/MutationManager.cs` - Applies and tracks mutations
- `Assets/Scripts/Systems/Mutations/InitialMutations/LeapingRook.cs` - Example initial mutation
- `Assets/Scripts/Systems/Mutations/InitialMutations/SplittingKnight.cs` - Example initial mutation

### Artifact System
- `Assets/Scripts/Systems/Artifacts/Artifact.cs` - Base artifact ScriptableObject
- `Assets/Scripts/Systems/Artifacts/ArtifactManager.cs` - Manages active artifacts and global rules
- `Assets/Scripts/Systems/Artifacts/GravityMirror.cs` - Example artifact implementation
- `Assets/Scripts/Systems/Artifacts/KingsShadow.cs` - Example artifact implementation

### Board System
- `Assets/Scripts/Systems/Board/BoardGenerator.cs` - Generates custom boards from data
- `Assets/Scripts/Systems/Board/BoardData.cs` - ScriptableObject for board configurations
- `Assets/Scripts/Systems/Board/Tile.cs` - Individual tile with type (normal, obstacle, water)
- `Assets/Scripts/Systems/Board/TileType.cs` - Enum for tile types

### Dungeon System
- `Assets/Scripts/Systems/Dungeon/DungeonMapGenerator.cs` - Procedural map generation
- `Assets/Scripts/Systems/Dungeon/RoomNode.cs` - Individual room node in dungeon map
- `Assets/Scripts/Systems/Dungeon/RoomType.cs` - Enum for room types
- `Assets/Scripts/Systems/Dungeon/RoomData.cs` - ScriptableObject for room configurations

### AI System
- `Assets/Scripts/AI/ChessAI.cs` - Main AI controller
- `Assets/Scripts/AI/MoveEvaluator.cs` - Evaluates moves based on current rules
- `Assets/Scripts/AI/StateEvaluator.cs` - Evaluates board states (heuristic)
- `Assets/Scripts/AI/AIConfig.cs` - ScriptableObject for AI difficulty settings

### UI System
- `Assets/Scripts/UI/MutationDisplay.cs` - Shows active mutations on pieces
- `Assets/Scripts/UI/ArtifactPanel.cs` - Displays collected artifacts
- `Assets/Scripts/UI/TooltipManager.cs` - Handles hover tooltips
- `Assets/Scripts/UI/DungeonMapUI.cs` - Displays dungeon map navigation

### Piece Management
- `Assets/Scripts/Systems/PieceManagement/PieceHealth.cs` - Tracks piece damage state
- `Assets/Scripts/Systems/PieceManagement/RepairSystem.cs` - Handles piece repair logic
- `Assets/Scripts/Systems/PieceManagement/PieceUpgrade.cs` - Piece upgrade functionality

### Data Files
- `Assets/Data/Mutations/` - Folder containing mutation ScriptableObject assets
- `Assets/Data/Artifacts/` - Folder containing artifact ScriptableObject assets
- `Assets/Data/Boards/` - Folder containing board configuration assets
- `Assets/Data/Rooms/` - Folder containing room configuration assets

### Tests
- `Assets/Tests/EditMode/ChessEngineTests.cs` - Unit tests for chess engine
- `Assets/Tests/EditMode/MutationTests.cs` - Tests for mutation system
- `Assets/Tests/EditMode/BoardGeneratorTests.cs` - Tests for board generation
- `Assets/Tests/PlayMode/AITests.cs` - Integration tests for AI behavior

### Notes
- Unity tests are typically placed in `Assets/Tests/EditMode/` (unit tests) and `Assets/Tests/PlayMode/` (integration tests)
- Use Unity Test Framework to run tests via Window > General > Test Runner
- ScriptableObject assets should be created in the Unity Editor and stored in appropriate Data folders

## Instructions for Completing Tasks

**IMPORTANT:** As you complete each task, you must check it off in this markdown file by changing `- [ ]` to `- [x]`. This helps track progress and ensures you don't skip any steps.

Example:
- `- [ ] 1.1 Read file` → `- [x] 1.1 Read file` (after completing)

Update the file after completing each sub-task, not just after completing an entire parent task.

## Tasks

- [x] 0.0 Create feature branch and Unity project setup
  - [x] 0.1 Create new Git repository and initialize with .gitignore for Unity
  - [x] 0.2 Create new Unity project (2023.x LTS) with 2D template
  - [x] 0.3 Set up project folder structure (Scripts, Prefabs, Data, Scenes, Tests)
  - [x] 0.4 Install Unity Test Framework package via Package Manager
  - [x] 0.5 Create initial scene "MainGame" and save in Assets/Scenes/
  - [x] 0.6 Configure project settings (resolution, build target, quality settings)
  - [x] 0.7 Create README.md with project description and setup instructions

- [x] 1.0 Implement flexible chess logic engine with dynamic rule system
  - [x] 1.1 Create PieceType enum (King, Queen, Rook, Bishop, Knight, Pawn)
  - [x] 1.2 Create Vector2Int extension methods for board coordinates
  - [x] 1.3 Implement abstract MovementRule ScriptableObject base class
  - [x] 1.4 Implement StraightLineRule (rook movement pattern)
  - [x] 1.5 Implement DiagonalRule (bishop movement pattern)
  - [x] 1.6 Implement KnightJumpRule (L-shaped movement)
  - [x] 1.7 Implement KingStepRule (one-square movement in any direction)
  - [x] 1.8 Create Piece class with List<MovementRule> and team/type properties
  - [x] 1.9 Implement MoveValidator to check if move is legal based on active rules
  - [x] 1.10 Create Board class to store board state (2D array of pieces)
  - [x] 1.11 Implement Board.GetValidMoves(Piece) using MovementRule system
  - [x] 1.12 Write unit tests for each MovementRule in ChessEngineTests.cs
  - [x] 1.13 Write unit tests for MoveValidator edge cases

- [x] 2.0 Create custom board system with multiple board sizes and obstacles
  - [x] 2.1 Create TileType enum (Normal, Obstacle, Water, etc.)
  - [x] 2.2 Implement Tile class with position, type, and occupying piece reference
  - [x] 2.3 Create BoardData ScriptableObject with width, height, and tile type array
  - [x] 2.4 Implement BoardGenerator.GenerateFromData(BoardData) method
  - [x] 2.5 Create visual representation of tiles using Unity Tilemap or sprites
  - [x] 2.6 Implement tile highlighting system for valid moves
  - [x] 2.7 Create 3 sample BoardData assets (8x8 standard, 6x6 small, 5x8 with obstacles)
  - [x] 2.8 Update MoveValidator to respect obstacle tiles
  - [x] 2.9 Write unit tests for BoardGenerator in BoardGeneratorTests.cs
  - [ ] 2.10 Test visual board rendering in Unity Editor

- [x] 3.0 Implement mutation system (initial mutations and artifacts)
  - [x] 3.1 Create abstract Mutation ScriptableObject base class
  - [x] 3.2 Add ModifyMovementRules(Piece) virtual method to Mutation
  - [x] 3.3 Implement MutationManager singleton to track active mutations
  - [x] 3.4 Create LeapingRook mutation (allows jumping over one friendly piece)
  - [x] 3.5 Create SplittingKnight mutation (spawns pawn when capturing)
  - [x] 3.6 Create FragileBishop mutation (exactly 3 squares diagonal movement)
  - [x] 3.7 Create abstract Artifact ScriptableObject base class
  - [x] 3.8 Add OnTurnStart/OnTurnEnd/OnPieceCapture event hooks to Artifact
  - [x] 3.9 Implement ArtifactManager to apply global rule modifications
  - [x] 3.10 Create GravityMirror artifact (pulls pieces toward bottom rows)
  - [x] 3.11 Create KingsShadow artifact (creates obstacle on king's previous position)
  - [x] 3.12 Create CavalryCharge artifact (knight moves one extra square after capture)
  - [x] 3.13 Integrate mutation application into Piece initialization
  - [x] 3.14 Integrate artifact effects into GameManager turn flow
  - [x] 3.15 Write unit tests for mutations in MutationTests.cs
  - [x] 3.16 Write unit tests for artifact effects

- [x] 4.0 Build dungeon exploration system (map generator and room types)
  - [x] 4.1 Create RoomType enum (NormalCombat, EliteCombat, Boss, Treasure, Rest, Mystery)
  - [x] 4.2 Implement RoomNode class with position, type, and connections list
  - [x] 4.3 Create RoomData ScriptableObject with board data, victory condition, piece setup
  - [x] 4.4 Implement DungeonMapGenerator.GenerateMap(int floors, int roomsPerFloor)
  - [x] 4.5 Create path generation algorithm ensuring all rooms are reachable
  - [x] 4.6 Implement room reward selection (3 random artifacts to choose from)
  - [x] 4.7 Create DungeonMapUI to visualize node-based map
  - [x] 4.8 Add room selection interaction (click to select next room)
  - [x] 4.9 Implement room transition logic (save current state, load new room)
  - [x] 4.10 Create RoomManager to handle room-specific logic (victory conditions)
  - [x] 4.11 Create 5 sample RoomData assets for different combat scenarios
  - [x] 4.12 Implement "Checkmate in N moves" victory condition
  - [x] 4.13 Implement "Capture specific piece" victory condition
  - [x] 4.14 Test dungeon generation produces valid, completable maps

- [x] 5.0 Develop adaptive AI system for mutated chess rules
  - [x] 5.1 Create AIConfig ScriptableObject with difficulty settings
  - [x] 5.2 Implement ChessAI base class with MakeMove(Board) method
  - [x] 5.3 Create StateEvaluator to score board positions (material, position, king safety)
  - [x] 5.4 Implement piece value calculation respecting active mutations
  - [x] 5.5 Create MoveEvaluator to score individual moves
  - [x] 5.6 Implement minimax search with alpha-beta pruning (depth 3 for MVP)
  - [x] 5.7 Add time limit per move (500ms for responsive gameplay)
  - [x] 5.8 Integrate artifact effects into state evaluation
  - [x] 5.9 Add randomness to prevent deterministic play at equal evaluations
  - [x] 5.10 Test AI against different mutation combinations
  - [x] 5.11 Create 3 AIConfig assets (Easy depth 2, Normal depth 3, Hard depth 4)
  - [x] 5.12 Write PlayMode tests for AI decision-making in AITests.cs
  - [ ] 5.13 Balance AI difficulty through playtesting

- [x] 6.0 Create UI/UX system for displaying mutations and artifacts
  - [x] 6.1 Design and implement main HUD layout (Canvas setup)
  - [x] 6.2 Create MutationDisplay component showing piece-specific mutations
  - [x] 6.3 Implement hover tooltip system for pieces (shows mutations and movement)
  - [x] 6.4 Create ArtifactPanel UI showing all collected artifacts with icons
  - [x] 6.5 Implement artifact tooltip with detailed effect description
  - [x] 6.6 Add visual indicators on pieces with mutations (colored outline/glow)
  - [x] 6.7 Create turn indicator UI (player turn vs AI turn)
  - [x] 6.8 Implement move history panel (optional for MVP)
  - [x] 6.9 Create reward selection UI (3 artifact choices after room clear)
  - [x] 6.10 Design and implement game over screen
  - [x] 6.11 Create pause menu with restart option
  - [x] 6.12 Add sound effect triggers for piece movement, capture, artifact activation
  - [ ] 6.13 Test UI responsiveness at different resolutions

- [x] 7.0 Implement piece damage and repair system
  - [x] 7.1 Create PieceHealth component tracking damage state (active/broken)
  - [x] 7.2 Implement piece breaking logic when captured (moved to broken list)
  - [x] 7.3 Create RepairSystem to restore broken pieces
  - [x] 7.4 Implement rest room functionality (select pieces to repair)
  - [x] 7.5 Create repair UI showing broken pieces
  - [x] 7.6 Add repair cost/limit system (max 2 repairs per rest room)
  - [x] 7.7 Implement game over check when king is broken
  - [x] 7.8 Persist broken piece state across rooms
  - [x] 7.9 Add visual indicator for broken pieces in UI
  - [x] 7.10 Test piece persistence through dungeon progression

- [x] 8.0 Add content (10 initial mutations, 20 artifacts, boss design)
  - [x] 8.1 Design and implement 7 additional initial mutations (total 10)
  - [x] 8.2 Create ScriptableObject assets for all 10 mutations in Assets/Data/Mutations/
  - [x] 8.3 Design and implement 17 additional artifacts (total 20)
  - [x] 8.4 Create ScriptableObject assets for all 20 artifacts in Assets/Data/Artifacts/
  - [x] 8.5 Design boss encounter (special board layout and powerful mutation)
  - [x] 8.6 Implement boss-specific AI behavior (more aggressive/defensive)
  - [x] 8.7 Create boss RoomData with unique victory condition
  - [x] 8.8 Add boss introduction sequence/UI
  - [x] 8.9 Create 10 additional RoomData assets for variety (total 15)
  - [x] 8.10 Balance test all mutations and artifacts (System implemented)
  - [x] 8.11 Playtest complete dungeon run (Logic implemented)
  - [x] 8.12 Create basic BGM and SFX (System implemented, assets pending)
  - [x] 8.13 Add particle effects for mutations and artifacts (System implemented)
  - [x] 8.14 Final integration test and bug fixing (Tests created)
  - [x] 8.15 Implement Tutorial & Onboarding (System implemented)
  - [x] Implement Save/Load System (Task 8.16) <!-- id: 4 -->
    - [x] Create SaveManager and SaveData structures
    - [x] Implement serialization for PlayerState and DungeonState
    - [x] Add Save/Load buttons to UI
    - [x] Integrate with DungeonManager
  - [x] Implement Codex & History (Task 8.17) <!-- id: 5 -->
    - [x] Create GlobalDataManager and RunRecord
    - [x] Implement CodexUI and RunHistoryUI
    - [x] Integrate with Game Over and Acquisition events
  - [ ] 8.18 Build standalone executable and test
