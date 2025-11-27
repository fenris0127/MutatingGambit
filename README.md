# The Mutating Gambit (변이하는 기보)

A Unity-based roguelite chess game where the rules mutate as you play, rewarding **creative adaptation** over perfect moves.

![Unity](https://img.shields.io/badge/Unity-2023.x-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-PC-blue)
![Status](https://img.shields.io/badge/Status-In%20Development-yellow)
![License](https://img.shields.io/badge/license-MIT-blue)

## Overview

The Mutating Gambit combines traditional chess mechanics with roguelite elements and dynamic rule mutations. Each piece can gain unique abilities, global artifacts modify game rules, and every dungeon run presents new strategic challenges.

### Core Concept

- **Traditional 8x8 chess board** is replaced with custom-sized boards (5x5, 6x8, etc.) with obstacles
- **Piece mutations** modify movement rules dynamically (Leaping Rook, Splitting Knight, etc.)
- **Artifacts** apply global rule changes affecting both player and AI
- **Dungeon exploration** through node-based maps (Slay the Spire style)
- **Piece damage system** - pieces become "broken" instead of captured, requiring repair at rest rooms
- **Adaptive AI** that understands and exploits mutated rules

## Quick Start

### Prerequisites

- **Unity 2023.x LTS** or newer
- **Git** for version control

### Setup

```bash
# Clone the repository
git clone <repository-url>
cd MutatingGambit

# Open in Unity Hub
# File > Open Project > Select 'MutatingGambit' folder

# Open the main scene
# Assets/Scenes/MainGame.unity
```

### Running Tests

1. Open Unity Test Runner: `Window > General > Test Runner`
2. Click "Run All" to execute all tests
3. View results in Edit Mode and Play Mode tabs

## Project Structure

```
MutatingGambit/
├── Assets/
│   ├── Scenes/
│   │   └── MainGame.unity              # Main game scene
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── ChessEngine/           # Core chess logic
│   │   │   │   ├── Piece.cs           # Piece with dynamic rules
│   │   │   │   ├── Board.cs           # Board state management
│   │   │   │   ├── GameManager.cs     # Game loop controller
│   │   │   │   └── MoveValidator.cs   # Move validation
│   │   │   └── MovementRules/         # ScriptableObject-based rules
│   │   │       ├── MovementRule.cs    # Abstract base class
│   │   │       ├── StraightLineRule.cs
│   │   │       ├── DiagonalRule.cs
│   │   │       ├── KnightJumpRule.cs
│   │   │       └── KingStepRule.cs
│   │   ├── Systems/
│   │   │   ├── Mutations/             # Mutation system
│   │   │   │   ├── Mutation.cs
│   │   │   │   ├── MutationManager.cs
│   │   │   │   └── InitialMutations/
│   │   │   ├── Artifacts/             # Artifact system
│   │   │   │   ├── Artifact.cs
│   │   │   │   └── ArtifactManager.cs
│   │   │   ├── Board/                 # Custom board system
│   │   │   │   ├── BoardGenerator.cs
│   │   │   │   ├── BoardData.cs
│   │   │   │   └── Tile.cs
│   │   │   ├── Dungeon/               # Dungeon map system
│   │   │   │   ├── DungeonMapGenerator.cs
│   │   │   │   ├── RoomNode.cs
│   │   │   │   └── RoomData.cs
│   │   │   └── PieceManagement/       # HP & repair system
│   │   │       ├── PieceHealth.cs
│   │   │       └── RepairSystem.cs
│   │   ├── AI/                        # Chess AI
│   │   │   ├── ChessAI.cs
│   │   │   ├── MoveEvaluator.cs
│   │   │   └── StateEvaluator.cs
│   │   └── UI/                        # UI components
│   │       ├── MutationDisplay.cs
│   │       ├── ArtifactPanel.cs
│   │       ├── TooltipManager.cs
│   │       └── DungeonMapUI.cs
│   ├── Data/                          # ScriptableObject assets
│   │   ├── Mutations/
│   │   ├── Artifacts/
│   │   ├── Boards/
│   │   └── Rooms/
│   ├── Prefabs/                       # Game object prefabs
│   ├── Sprites/                       # 2D art assets
│   └── Tests/                         # Unit & integration tests
│       ├── EditMode/
│       └── PlayMode/
└── Packages/
    └── manifest.json                  # Package dependencies
```

## MVP Feature List

### Implemented (0.0 - 0.6)
- ✅ Unity 2023.x project setup
- ✅ Basic folder structure
- ✅ Git repository initialization
- ✅ Unity Test Framework installed

### In Development (0.7+)
- 🔨 **Phase 1**: Flexible Chess Logic Engine
- ⏳ **Phase 2**: Custom Board System
- ⏳ **Phase 3**: Mutation & Artifact System
- ⏳ **Phase 4**: Dungeon Exploration
- ⏳ **Phase 5**: Adaptive AI
- ⏳ **Phase 6**: UI/UX System
- ⏳ **Phase 7**: Piece Damage & Repair
- ⏳ **Phase 8**: Content Creation (10 mutations, 20 artifacts)

## Game Features

### 🧬 Mutations (Planned: 10 for MVP)

Example mutations:
- **Leaping Rook**: Can jump over ONE friendly piece
- **Splitting Knight**: Spawns a pawn when capturing
- **Glass Bishop**: Moves exactly 3 squares (no more, no less)
- **Phantom Pawn**: Can phase through one enemy piece
- **Sideways King**: Can move 2 squares horizontally/vertically

### ⚡ Artifacts (Planned: 20 for MVP)

Example artifacts:
- **Gravity Mirror**: Pulls all pieces toward bottom rows each turn
- **King's Shadow**: King leaves an obstacle when moving
- **Cavalry Charge**: Knights move again after capturing
- **Chain Lightning**: Captured piece damages adjacent pieces
- **Promotion Privilege**: Pawns promote after 3 captures instead of reaching end

### 🗺️ Room Types

- **Normal Combat**: Simple puzzles or small battles
- **Elite Combat**: Mutated enemies with better rewards
- **Boss**: Final encounter with powerful mutations
- **Treasure**: Immediate artifact acquisition
- **Rest**: Repair broken pieces or upgrade
- **Mystery**: Random events

### 🎯 Victory Conditions

- Checkmate in N moves
- Capture specific piece
- Move King to target position
- Survive N turns
- Custom puzzle objectives

## Development

### Architecture Highlights

**Dynamic Rule System**
- Uses `ScriptableObject` for modular movement rules
- Pieces hold `List<MovementRule>` that can be modified at runtime
- Mutations add/remove/modify rules using Strategy Pattern
- Artifacts apply global effects through event hooks

**AI System**
- Custom AI that reads current `MovementRule` configurations
- No hardcoded chess engine (Stockfish, etc.)
- Uses Minimax with Alpha-Beta pruning or MCTS
- Mutation-aware move evaluation

**Procedural Generation**
- Node-based dungeon map (Slay the Spire style)
- Custom board configurations per room
- Balanced difficulty progression

### Code Example

```csharp
// Creating a mutated piece
var rook = new Piece(PieceType.Rook, Team.White);

// Add base movement rule
var straightRule = ScriptableObject.CreateInstance<StraightLineRule>();
rook.AddMovementRule(straightRule);

// Apply mutation
var leapingMutation = ScriptableObject.CreateInstance<LeapingRookMutation>();
MutationManager.Instance.ApplyMutation(rook, leapingMutation);

// Get valid moves (respects mutation)
var validMoves = MoveValidator.GetValidMoves(board, rook.Position);
```

### Adding New Mutations

```csharp
[CreateAssetMenu(fileName = "NewMutation", menuName = "Mutations/NewMutation")]
public class NewMutation : Mutation
{
    public override string MutationName => "New Mutation";
    public override string Description => "Description of effect";

    public override void ModifyMovementRules(Piece piece)
    {
        // Add/remove/modify movement rules
        var customRule = ScriptableObject.CreateInstance<CustomMoveRule>();
        piece.AddMovementRule(customRule);
    }
}
```

### Testing

```csharp
// Example test in ChessEngineTests.cs
[Test]
public void Rook_WithLeapingMutation_CanJumpOverFriendlyPiece()
{
    // Arrange
    var board = new Board(8, 8);
    var rook = CreatePiece(PieceType.Rook, Team.White);
    var blockingPiece = CreatePiece(PieceType.Pawn, Team.White);

    board.PlacePiece(rook, new Position(0, 0));
    board.PlacePiece(blockingPiece, new Position(0, 2));

    var mutation = ScriptableObject.CreateInstance<LeapingRookMutation>();
    MutationManager.Instance.ApplyMutation(rook, mutation);

    // Act
    var moves = MoveValidator.GetValidMoves(board, new Position(0, 0));

    // Assert
    Assert.IsTrue(moves.Contains(new Position(0, 3)));
}
```

## Roadmap

### MVP (Current Focus)
- [ ] Core chess engine with dynamic rules
- [ ] 10 initial mutations
- [ ] 20 artifacts
- [ ] Basic AI (depth 3 minimax)
- [ ] Dungeon map generation
- [ ] 1 boss encounter
- [ ] Piece damage/repair system
- [ ] Basic UI/UX

### Post-MVP
- [ ] Meta progression (unlock system)
- [ ] More content (50+ mutations/artifacts)
- [ ] Advanced AI (MCTS)
- [ ] Mobile port (iOS/Android)
- [ ] Multiplayer mode
- [ ] Daily challenges
- [ ] Achievements

## Technical Requirements

- **Unity Version**: 2023.x LTS or higher
- **Scripting Backend**: Mono or IL2CPP
- **Target Platform**: PC (Windows/Mac/Linux)
- **Rendering**: URP (Universal Render Pipeline) 2D
- **Required Packages**:
  - Unity Test Framework
  - TextMesh Pro
  - Input System (optional)

## Contributing

This is currently a personal project. Contributions, bug reports, and feature requests are welcome through GitHub issues.

## License

This project is open source and available under the MIT License.

## Credits

**Game Design & Development**: Based on PRD v1.0
**Inspiration**: Slay the Spire, Into the Breach, Chess

---

**"Your chess knowledge is your foundation. Adaptation is your weapon."**
