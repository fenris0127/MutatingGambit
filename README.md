# The Mutating Gambit (변이하는 기보)

A roguelike chess game where the rules mutate as you play, rewarding **creative adaptation** over perfect moves.

![Tests](https://img.shields.io/badge/tests-247%2F247%20passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-6.0-blue)
![TDD](https://img.shields.io/badge/methodology-TDD-orange)
![Optimized](https://img.shields.io/badge/performance-optimized-green)
![CI/CD](https://img.shields.io/badge/CI%2FCD-automated-success)
![License](https://img.shields.io/badge/license-MIT-blue)

## Overview

The Mutating Gambit combines traditional chess mechanics with roguelike elements and dynamic rule mutations. Each piece can gain unique abilities, global artifacts modify game rules, and every run presents new strategic challenges.

## Features

### ✨ Core Systems

- **🎮 Complete Chess Engine**: All standard piece movements with custom board sizes
- **🧬 Mutation System**: 9 unique mutations that modify piece movement rules
- **⚡ Artifact System**: 9 global effects that change game rules for all pieces
- **🗺️ Procedural Generation**: Dynamic dungeon layouts with balanced room distribution
- **🏰 Dungeon Progression**: Node-based map navigation with branching paths
- **🎯 Victory Conditions**: Flexible win conditions (checkmate in N moves, capture specific pieces, etc.)
- **💔 HP/Damage System**: Pieces have HP and can be damaged/repaired
- **🤖 AI Opponents**: 4 difficulty levels with mutation-aware strategy
- **📊 Meta Progression**: Unlock new mutations, artifacts, and features
- **💾 Save/Load System**: JSON-based persistence
- **⚡ Performance**: Optimized with object pooling and reduced allocations
- **🎮 Complete Game Loop**: Full roguelike run integration with session management
- **🎲 Random Events**: Choice-based encounters with risk/reward mechanics
- **🛒 Shop System**: Purchase upgrades, consumables, and mutations during runs
- **♔ Checkmate Detection**: Authentic chess rules with check, checkmate, and stalemate detection
- **🤖 CI/CD Automation**: Automated testing, builds, and releases via GitHub Actions
- **✏️ Level Editor**: Create custom dungeon maps with visual text-based editor
- **🎨 Graphical UI**: MonoGame-powered 2D graphics with mouse/keyboard controls

### 🧬 Available Mutations

#### Starting Mutations
| Mutation | Effect | Cost |
|----------|--------|------|
| **Leaping Rook** | Can jump over ONE friendly piece | 50 |
| **Splitting Knight** | Spawns a pawn when capturing | 75 |
| **Glass Bishop** | Moves exactly 3 squares (no more, no less) | 60 |

#### Advanced Mutations
| Mutation | Effect | Cost |
|----------|--------|------|
| **Phantom Pawn** | Can phase through one enemy piece | 80 |
| **Sideways King** | Can move 2 squares horizontally/vertically | 100 |
| **Diagonal Knight** | Can also move 1 square diagonally | 70 |

#### Elite Mutations (Phase 17)
| Mutation | Effect | Cost |
|----------|--------|------|
| **Teleporting Queen** | Can swap positions with any friendly piece | 120 |
| **Berserker Pawn** | Can also capture straight ahead | 85 |
| **Chain Bishop** | Captures chain to adjacent enemies on diagonals | 110 |

### ⚡ Available Artifacts

#### Starting Artifacts
| Artifact | Effect | Cost |
|----------|--------|------|
| **King's Shadow** | King leaves an obstacle when moving | 100 |
| **Cavalry Charge** | Knights move again after capturing | 120 |
| **Promotion Privilege** | Pawns promote after 3 captures | 90 |

#### Advanced Artifacts
| Artifact | Effect | Cost |
|----------|--------|------|
| **Fortune's Favor** | Every 4th move is free | 150 |
| **Weakening Curse** | All pieces start with -1 HP | 110 |
| **Piece Revival** | First destroyed piece is revived to 1 HP | 140 |

#### Elite Artifacts (Phase 17)
| Artifact | Effect | Cost |
|----------|--------|------|
| **Time Warp** | First captured enemy revives at end with 1 HP | 160 |
| **Piece Magnet** | Adjacent same-type pieces get +1 HP each turn | 130 |
| **Chaos Tiles** | Random empty tile becomes obstacle each turn | 100 |

### 📊 Complete Content Library

| Category | Count | Details |
|----------|-------|---------|
| **Mutations** | 9 | 3 Starting, 3 Advanced, 3 Elite |
| **Artifacts** | 9 | 3 Starting, 3 Advanced, 3 Elite |
| **QoL Features** | 3 | Move Undo, Save Anywhere, AI Hints |
| **Room Types** | 7 | Normal/Elite/Boss Combat, Treasure, Rest, Event, Shop |
| **Events** | 4 | Mysterious Chest, Stranger, Ancient Shrine, Shop |
| **AI Difficulties** | 4 | Easy, Normal, Hard, Master |
| **Victory Conditions** | 5 | Checkmate, Capture Piece, King Position, Stalemate, Time Limit |
| **Total Unlockables** | 21 | Complete meta progression system |

## Quick Start

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)

### Run the Demo

```bash
# Clone the repository
git clone <repository-url>
cd MutatingGambit

# Run the interactive demo
cd src/MutatingGambit.Demo
dotnet run
```

### Run the Tests

```bash
cd tests
dotnet run
```

Expected output:
```
=== Running MutatingGambit Tests ===
...
=== Test Results ===
Passed: 227
Failed: 0
Total:  227
```

## Project Structure

```
MutatingGambit/
├── src/
│   ├── MutatingGambit/           # Core game engine
│   │   └── Core/
│   │       ├── Board.cs           # Chess board
│   │       ├── Piece.cs           # Chess pieces with HP/mutations
│   │       ├── Position.cs        # Board positions
│   │       ├── GameSession.cs     # Roguelike run management
│   │       ├── RoguelikeGameLoop.cs # Room execution and progression
│   │       ├── CheckmateDetector.cs # Check/checkmate/stalemate detection
│   │       ├── Movement/          # Piece movement rules
│   │       ├── Mutations/         # Piece mutation system (9 mutations)
│   │       ├── Artifacts/         # Global game modifiers (9 artifacts)
│   │       ├── Rooms/             # Dungeon room types
│   │       ├── Victory/           # Victory conditions (including checkmate)
│   │       ├── Map/               # Dungeon map system & procedural generation
│   │       ├── Events/            # Random events & shop system
│   │       ├── AI/                # Chess AI
│   │       ├── UI/                # Text-based display
│   │       └── Meta/              # Meta progression
│   └── MutatingGambit.Demo/      # Interactive demo
└── tests/
    └── SimpleTestRunner.cs        # 227 comprehensive tests
```

## Development

This project was built using **Test-Driven Development (TDD)** with Kent Beck's Red-Green-Refactor methodology.

### Development Phases

1. **Phase 1-3**: Chess Foundation (64 tests)
2. **Phase 4**: Mutation System (14 tests)
3. **Phase 5**: Artifact System (15 tests)
4. **Phase 6**: Combat/Puzzle System (14 tests)
5. **Phase 7**: Dungeon Map System (12 tests)
6. **Phase 8**: AI System (12 tests)
7. **Phase 9**: UI System (7 tests)
8. **Phase 10**: Meta Progression (10 tests)
9. **Phase 11**: Performance Optimizations (refactoring - PositionCache, SlidingMoveHelper)
10. **Phase 12**: Content Expansion (18 tests - 3 mutations, 3 artifacts)
11. **Phase 13**: Procedural Generation (12 tests - DungeonGenerator)
12. **Phase 14**: Game Loop Integration (16 tests - GameSession, RoguelikeGameLoop)
13. **Phase 15**: Events & Shop System (18 tests - 4 events, shop mechanics)
14. **Phase 16**: Checkmate Detection (15 tests - authentic chess rules)
15. **Phase 17**: Extended Content Library (3 elite mutations, 3 elite artifacts)
16. **Phase 18**: CI/CD Automation (GitHub Actions workflows, automated testing & releases)
17. **Phase 19**: Level Editor (20 tests - custom map creation, validation, save/load)
18. **Phase 20**: Graphical UI (MonoGame framework - 2D rendering, UI system, input handling)

**Total: 247+ tests (base system), 100% passing, fully automated**

### Code Example

```csharp
// Create a custom board with mutations
var board = new Board(6, 6);

// Add a mutated rook
var rook = new Piece(PieceColor.White, PieceType.Rook);
rook.AddMutation(new LeapingRookMutation());
board.PlacePiece(rook, Position.FromNotation("a1"));

// Add global artifacts
board.ArtifactManager.AddArtifact(new KingsShadowArtifact());

// Get legal moves (respects mutations)
var moves = MoveValidator.GetLegalMoves(board, Position.FromNotation("a1"));

// Create an AI opponent
var ai = new AdvancedAI(PieceColor.Black, AdvancedAI.Difficulty.Hard);
var aiMove = ai.SelectBestMove(board);

// Generate a procedural dungeon
var generator = new DungeonGenerator(seed: 42);
var map = generator.Generate();
Console.WriteLine($"Generated {map.Nodes.Count} rooms across {map.MaxLayers + 1} layers");

// Run a complete roguelike session
var metaProgression = new MetaProgression();
var session = new GameSession(metaProgression, seed: 123);
var gameLoop = new RoguelikeGameLoop(session);

// Navigate and complete rooms
var nextRoom = session.GetAvailableRooms()[0];
session.MoveToRoom(nextRoom);

var board = gameLoop.EnterRoom(nextRoom.Room);
var victory = gameLoop.GetCurrentVictoryCondition();

// Play the room... then complete it
session.CompleteRoom(new RoomResult(true, currencyReward: 50));
Console.WriteLine($"HP: {session.CurrentHP} | Currency: {session.Currency}");
```

## Architecture Highlights

### Modular Design

- **Strategy Pattern**: `IMoveRule` for extensible piece movement
- **Decorator Pattern**: Mutations wrap and modify movement rules
- **Observer Pattern**: Artifact trigger system for events
- **Composition**: Multiple mutations can stack on a single piece
- **Object Pooling**: PositionCache reduces memory allocations
- **DRY Principle**: SlidingMoveHelper eliminates code duplication

### Extensibility

Adding new mutations:
```csharp
public class CustomMutation : Mutation
{
    public override string Name => "Custom Mutation";

    public override IMoveRule ApplyToMoveRule(IMoveRule baseRule, Piece piece)
    {
        // Modify movement behavior
        return new CustomMoveRule(baseRule);
    }
}
```

Adding new artifacts:
```csharp
public class CustomArtifact : Artifact
{
    public override string Name => "Custom Artifact";
    public override ArtifactTrigger Trigger => ArtifactTrigger.OnTurnStart;

    public override void ApplyEffect(Board board, ArtifactContext context)
    {
        // Modify game state
    }
}
```

## Testing

The project includes comprehensive tests covering all systems:

```bash
# Run all 148 tests
cd tests
dotnet run

# Example test output
--- Mutation Framework Tests ---
  ✓ Mutation can be applied to a piece
  ✓ Multiple mutations can be applied to a piece
  ✓ Mutations can be removed from a piece
  ✓ All mutations can be cleared from a piece
  ✓ Piece with mutations is visually distinguished
```

## Roadmap

- [x] ~~Procedural dungeon generation~~ ✅ **Complete** (Phase 13)
- [x] ~~More mutations and artifacts~~ ✅ **Complete** (9 of each - Phases 12, 17)
- [x] ~~Performance optimizations~~ ✅ **Complete** (Phase 11)
- [x] ~~Complete roguelike game loop~~ ✅ **Complete** (Phase 14)
- [x] ~~Events and shop system~~ ✅ **Complete** (Phase 15)
- [x] ~~Authentic checkmate detection~~ ✅ **Complete** (Phase 16)
- [x] ~~Extended content library~~ ✅ **Complete** (Phase 17)
- [x] ~~CI/CD automation~~ ✅ **Complete** (Phase 18)
- [x] ~~Level editor~~ ✅ **Complete** (Phase 19)
- [x] ~~Graphical UI (MonoGame)~~ ✅ **Complete** (Phase 20)
- [ ] Online multiplayer
- [ ] Tournament mode
- [ ] Daily challenges with leaderboards

## License

This project is open source and available under the MIT License.

## Credits

Developed using Test-Driven Development methodology with comprehensive test coverage.

---

**"In The Mutating Gambit, the board is your canvas, and chaos is your palette."**
