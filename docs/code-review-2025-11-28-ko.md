# MutatingGambit 종합 코드 리뷰 보고서
**날짜:** 2025년 11월 28일
**프로젝트:** MutatingGambit - 체스 로그라이크 게임
**리뷰 범위:** Assets/Scripts 전체 코드베이스 (100+ 파일, ~10,000 줄)
**리뷰어:** Claude AI Code Reviewer

---

## 📋 요약 (Executive Summary)

MutatingGambit는 체스 메커니즘에 로그라이크 요소를 접목시킨 혁신적인 게임입니다. 코드베이스는 전반적으로 **양호한 아키텍처 패턴**을 따르고 있으며, Strategy 패턴, ScriptableObject 패턴 등을 적절히 활용하고 있습니다.

### 전체 평가: **7.2/10** (B+)

#### ✅ 주요 강점
- 명확한 네임스페이스 구조와 코드 조직화
- 전략 패턴을 활용한 유연한 무브먼트 룰 시스템
- 잘 구현된 AI 시스템 (Minimax + Alpha-Beta Pruning)
- ScriptableObject를 활용한 데이터 주도 디자인
- 상세한 XML 문서화 주석
- 적절한 Unity 패턴 사용

#### ⚠️ 개선이 필요한 영역
- **과도한 FindObjectOfType 사용** (성능 이슈)
- **싱글톤 패턴 남용** 및 의존성 관리 미흡
- **불완전한 에러 처리** 및 null 체크
- **메모리 관리 이슈** (ScriptableObject 누수)
- **테스트 커버리지 부족** (현재 ~30%)
- **저장/로드 시스템의 취약성**

### 발견된 이슈 통계
- **Critical:** 4개
- **High:** 8개
- **Medium:** 12개
- **Low:** 8개
- **총 이슈:** 32개

---

## 📊 코드 메트릭스

```
총 C# 파일: 100+
총 코드 라인: ~10,000
클래스/구조체/인터페이스: 177개
Debug.Log 호출: 215건
null 참조: 625건
FindObjectOfType 사용: 47회
싱글톤 패턴: 17개
테스트 파일: 8개
테스트 커버리지: ~30% (추정)
```

---

## 1. 🏗️ 아키텍처 및 디자인 패턴

### 1.1 전체 아키텍처 평가

**심각도:** Medium
**파일:** 전체 프로젝트

#### 긍정적 측면
코드는 명확한 계층 구조를 따릅니다:
```
Core/               # 핵심 체스 엔진
  ├── ChessEngine/  # 보드, 피스, 게임 관리
  └── MovementRules/# 이동 규칙
Systems/            # 게임플레이 시스템
  ├── Mutations/    # 피스 변이
  ├── Artifacts/    # 글로벌 효과
  ├── Dungeon/      # 던전 진행
  ├── SaveLoad/     # 저장/로드
  └── ...
UI/                 # UI 컴포넌트
```

네임스페이스가 적절히 사용되어 모듈화가 잘 되어 있습니다.

#### 문제점
**순환 참조 발생**

**위치:** `GameManager.cs:243-250`
```csharp
if (MutatingGambit.Systems.Mutations.MutationManager.Instance != null)
{
    MutatingGambit.Systems.Mutations.MutationManager.Instance.NotifyMove(...);

    if (capturedPiece != null)
    {
        MutatingGambit.Systems.Mutations.MutationManager.Instance.NotifyCapture(...);
    }
}
```

**문제:**
- GameManager → MutationManager 직접 참조
- 강한 결합도로 인한 테스트 어려움
- 시스템 간 의존성 추적 곤란

#### 권장사항

**1. 이벤트 시스템 도입**
```csharp
// GameEvents.cs
public static class GameEvents
{
    public static event Action<Piece, Vector2Int, Vector2Int> OnPieceMove;
    public static event Action<Piece, Piece, Vector2Int, Vector2Int> OnPieceCapture;
}

// GameManager.cs
public bool ExecuteMove(Vector2Int from, Vector2Int to)
{
    // ...
    GameEvents.OnPieceMove?.Invoke(piece, from, to);
    if (capturedPiece != null)
    {
        GameEvents.OnPieceCapture?.Invoke(piece, capturedPiece, from, to);
    }
}

// MutationManager.cs
private void OnEnable()
{
    GameEvents.OnPieceMove += HandlePieceMove;
    GameEvents.OnPieceCapture += HandlePieceCapture;
}
```

**2. Dependency Injection (선택적)**
```csharp
public class GameManager : MonoBehaviour
{
    [Inject] private IMutationManager mutationManager;
    [Inject] private IArtifactManager artifactManager;

    // Zenject 또는 VContainer 사용
}
```

---

### 1.2 싱글톤 패턴 남용

**심각도:** High
**파일:** 다수

#### 발견된 싱글톤 목록
1. `GameManager`
2. `MutationManager`
3. `DungeonManager`
4. `SaveManager`
5. `GlobalDataManager`
6. `MovementRuleFactory`
7. `TutorialManager`
8. `AudioManager`
9. `EffectManager`
10. `TooltipManager`
11. 기타 7개...

#### 문제점

**1. 테스트 어려움**
```csharp
// 테스트 격리 불가능
[Test]
public void Test_SaveGame()
{
    SaveManager.Instance.SaveGame();  // 실제 파일 시스템 사용!
}
```

**2. 숨겨진 의존성**
```csharp
public void SomeMethod()
{
    // 메서드 시그니처에 의존성이 드러나지 않음
    DungeonManager.Instance.DoSomething();
}
```

**3. 멀티씬 문제**
**위치:** `MutationManager.cs:18-34`
```csharp
public static MutationManager Instance
{
    get
    {
        if (instance == null)
        {
            instance = FindObjectOfType<MutationManager>();
            if (instance == null)
            {
                GameObject go = new GameObject("MutationManager");
                instance = go.AddComponent<MutationManager>();
                DontDestroyOnLoad(go);  // ⚠️ 씬 전환 시 누적될 수 있음
            }
        }
        return instance;
    }
}
```

#### 권장사항

**1. 서비스 로케이터 패턴**
```csharp
public class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator instance;

    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MutationManager mutationManager;

    public static ServiceLocator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ServiceLocator>();
            }
            return instance;
        }
    }

    public static T Get<T>() where T : MonoBehaviour
    {
        if (instance == null) return null;

        if (typeof(T) == typeof(DungeonManager))
            return instance.dungeonManager as T;
        if (typeof(T) == typeof(GameManager))
            return instance.gameManager as T;
        // ... 기타

        return null;
    }
}

// 사용
var dungeonManager = ServiceLocator.Get<DungeonManager>();
```

**2. ScriptableObject 기반 이벤트 채널**
```csharp
// GameEventChannel.cs
[CreateAssetMenu(menuName = "Events/Game Event Channel")]
public class GameEventChannel : ScriptableObject
{
    private event Action<Piece, Vector2Int, Vector2Int> onPieceMove;

    public void RaisePieceMove(Piece piece, Vector2Int from, Vector2Int to)
    {
        onPieceMove?.Invoke(piece, from, to);
    }

    public void Subscribe(Action<Piece, Vector2Int, Vector2Int> handler)
    {
        onPieceMove += handler;
    }

    public void Unsubscribe(Action<Piece, Vector2Int, Vector2Int> handler)
    {
        onPieceMove -= handler;
    }
}
```

---

### 1.3 Strategy 패턴 - 우수 사례 ⭐

**심각도:** None (긍정적)
**파일:** `MovementRule.cs`, 관련 파일들

#### 훌륭한 구현
**위치:** `MovementRule.cs:11-41`
```csharp
public abstract class MovementRule : ScriptableObject
{
    public abstract List<Vector2Int> GetValidMoves(
        IBoard board,
        Vector2Int fromPosition,
        ChessEngine.Team pieceTeam
    );

    protected bool IsEnemyPiece(IBoard board, Vector2Int position, Team pieceTeam)
    {
        var piece = board.GetPieceAt(position);
        return piece != null && piece.Team != pieceTeam;
    }
}
```

#### 장점
✅ 런타임에 피스의 이동 규칙을 동적으로 변경 가능
✅ 뮤테이션 시스템의 핵심 구현
✅ 인터페이스(IBoard) 사용으로 결합도 낮음
✅ 확장성이 뛰어남
✅ 단일 책임 원칙 준수

#### 사용 예시
```csharp
// Piece.cs
public void AddMovementRule(MovementRule rule)
{
    if (rule != null && !movementRules.Contains(rule))
    {
        movementRules.Add(rule);
    }
}

// Mutation에서 사용
public override void ApplyToPiece(Piece piece)
{
    piece.AddMovementRule(ScriptableObject.CreateInstance<BackwardPawnRule>());
}
```

---

## 2. ⚡ 성능 및 최적화

### 2.1 과도한 FindObjectOfType 사용

**심각도:** Critical
**파일:** 17개 파일에서 47회 발견

#### 통계
```
DungeonManager.cs: 13회
GameManager.cs: 6회
MutationManager.cs: 1회
SaveManager.cs: 3회
BoardInputHandler.cs: 2회
기타: 22회
```

#### 문제 코드
**위치:** `DungeonManager.cs:134-143`
```csharp
private void Awake()
{
    // ⚠️ FindFirstObjectByType는 전체 씬을 순회 (O(n))
    if (mapGenerator == null) mapGenerator = FindFirstObjectByType<DungeonMapGenerator>();
    if (gameBoard == null) gameBoard = FindFirstObjectByType<Board>();
    if (roomManager == null) roomManager = FindFirstObjectByType<RoomManager>();
    if (repairSystem == null) repairSystem = FindFirstObjectByType<RepairSystem>();
    if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
    if (boardGenerator == null) boardGenerator = FindFirstObjectByType<BoardGenerator>();
    if (dungeonMapUI == null) dungeonMapUI = FindFirstObjectByType<DungeonMapUI>();
    if (rewardUI == null) rewardUI = FindFirstObjectByType<RewardSelectionUI>();
    if (repairUI == null) repairUI = FindFirstObjectByType<RepairUI>();
    if (notificationUI == null) notificationUI = FindFirstObjectByType<NotificationUI>();
}
```

#### 성능 영향
- **Awake/Start 단계**: 초기 로딩 시간 증가
- **복잡도**: O(n) × 호출 횟수
- **예상 지연**: 대형 씬에서 100-500ms 추가

#### 권장사항

**1. Inspector 직렬화 우선**
```csharp
[Header("Required References")]
[SerializeField, Required] private Board gameBoard;
[SerializeField, Required] private RoomManager roomManager;
[SerializeField, Required] private RepairSystem repairSystem;

private void Awake()
{
    ValidateReferences();
}

private void ValidateReferences()
{
    Debug.Assert(gameBoard != null, "Board reference is missing!");
    Debug.Assert(roomManager != null, "RoomManager reference is missing!");
}
```

**2. 캐싱 패턴**
```csharp
private Board _cachedBoard;
public Board GameBoard
{
    get
    {
        if (_cachedBoard == null)
            _cachedBoard = FindFirstObjectByType<Board>();
        return _cachedBoard;
    }
}
```

**3. 서비스 로케이터**
이전 섹션 참조

---

### 2.2 메모리 관리 - ScriptableObject 누수

**심각도:** High
**파일:** `AdvancedMutations.cs`, `Piece.cs`, 테스트 파일들

#### 문제 코드
**위치:** `AdvancedMutations.cs:16-23`
```csharp
public class ReversePawnMutation : Mutation
{
    public override void ApplyToPiece(Piece piece)
    {
        // ⚠️ ScriptableObject 생성 - 메모리 누수!
        var reverseRule = ScriptableObject.CreateInstance<BackwardPawnRule>();
        piece.AddMovementRule(reverseRule);
        // 파괴 코드 없음!
    }

    public override void RemoveFromPiece(Piece piece)
    {
        // ⚠️ 생성한 rule을 파괴하지 않음
    }
}
```

**위치:** `AITests.cs:86-88`
```csharp
var straightRule = ScriptableObject.CreateInstance<StraightLineRule>();
rook.AddMovementRule(straightRule);
// 일부 테스트에서만 Destroy 호출
```

#### 메모리 누수 시나리오
1. 뮤테이션 적용 → ScriptableObject 생성
2. 뮤테이션 제거 → ScriptableObject 남아있음
3. 게임 진행 → 누적된 ScriptableObject로 메모리 증가

#### 해결책 - MovementRuleFactory (우수 사례) ⭐

**위치:** `MovementRuleFactory.cs:11-122`
```csharp
public class MovementRuleFactory : MonoBehaviour
{
    private static MovementRuleFactory instance;
    private readonly Dictionary<Type, MovementRule> ruleCache = new Dictionary<Type, MovementRule>();

    public T GetRule<T>() where T : MovementRule
    {
        var type = typeof(T);

        if (!ruleCache.ContainsKey(type))
        {
            var rule = ScriptableObject.CreateInstance<T>();
            ruleCache[type] = rule;
        }

        return ruleCache[type] as T;
    }

    public MovementRule[] GetQueenRules()
    {
        return new MovementRule[]
        {
            GetRule<StraightLineRule>(),
            GetRule<DiagonalRule>()
        };
    }

    private void OnDestroy()
    {
        ClearCache();
    }

    public void ClearCache()
    {
        foreach (var rule in ruleCache.Values)
        {
            if (rule != null)
            {
                Destroy(rule);
            }
        }
        ruleCache.Clear();
    }
}
```

#### 권장 수정

**Piece.cs의 PromoteToQueen**
```csharp
// 변경 전
public void PromoteToQueen()
{
    pieceType = PieceType.Queen;
    movementRules.Clear();
    AddMovementRule(ScriptableObject.CreateInstance<StraightLineRule>());  // ⚠️
    AddMovementRule(ScriptableObject.CreateInstance<DiagonalRule>());       // ⚠️
}

// 변경 후
public void PromoteToQueen()
{
    pieceType = PieceType.Queen;
    movementRules.Clear();

    var factory = MovementRuleFactory.Instance;
    var queenRules = factory.GetQueenRules();
    foreach (var rule in queenRules)
    {
        AddMovementRule(rule);
    }
}
```

**AdvancedMutations.cs 수정**
```csharp
public override void ApplyToPiece(Piece piece)
{
    var factory = MovementRuleFactory.Instance;
    var reverseRule = factory.GetRule<BackwardPawnRule>();
    piece.AddMovementRule(reverseRule);
}

public override void RemoveFromPiece(Piece piece)
{
    var factory = MovementRuleFactory.Instance;
    var reverseRule = factory.GetRule<BackwardPawnRule>();
    piece.RemoveMovementRule(reverseRule);
}
```

---

### 2.3 Board.Clone() 성능 문제

**심각도:** High
**파일:** `Board.cs`, `ChessAI.cs`

#### 문제 분석

**위치:** `Board.cs:304-339`
```csharp
public Board Clone()
{
    // ⚠️ GameObject 생성 - 느림!
    GameObject clonedObject = new GameObject("ClonedBoard");
    Board clonedBoard = clonedObject.AddComponent<Board>();
    clonedBoard.Initialize(width, height);

    // ⚠️ 모든 피스에 대해 GameObject 생성
    foreach (var piece in allPieces)
    {
        if (piece != null)
        {
            GameObject pieceObject = new GameObject($"Clone_{piece.Type}_{piece.Team}");
            Piece clonedPiece = pieceObject.AddComponent<Piece>();
            clonedPiece.Initialize(piece.Type, piece.Team, piece.Position);

            foreach (var rule in piece.MovementRules)
            {
                clonedPiece.AddMovementRule(rule);
            }

            clonedBoard.PlacePiece(clonedPiece, piece.Position);
        }
    }

    return clonedBoard;
}
```

**AI에서의 사용:** `ChessAI.cs:138-152`
```csharp
foreach (var move in allMoves)  // 평균 30-50회 반복
{
    if (IsTimeExpired()) break;

    Board clonedBoard = board.Clone();  // ⚠️ GameObject 생성 × 30-50
    clonedBoard.MovePiece(move.From, move.To);

    float score = Minimax(clonedBoard, depth - 1, ...);  // 재귀적 Clone 더 많이

    Destroy(clonedBoard.gameObject);  // ⚠️ GC 압박
}
```

#### 성능 측정 (추정)
```
표준 보드 (8×8, 16피스):
- Clone() 1회: ~2-3ms
- Minimax depth 3: ~100-200 Clone 호출
- 총 시간: 200-600ms
- GC 압박: 매 턴 수백 개 GameObject 생성/파괴
```

#### 권장 해결책

**1. 경량 BoardState 구조체**
```csharp
public struct BoardState
{
    public PieceType[,] pieceTypes;
    public Team[,] pieceTeams;
    public bool[,] obstacles;

    public static BoardState FromBoard(Board board)
    {
        int width = board.Width;
        int height = board.Height;

        return new BoardState
        {
            pieceTypes = new PieceType[width, height],
            pieceTeams = new Team[width, height],
            obstacles = new bool[width, height]
        };
    }

    public BoardState Clone()
    {
        return new BoardState
        {
            pieceTypes = (PieceType[,])pieceTypes.Clone(),
            pieceTeams = (Team[,])pieceTeams.Clone(),
            obstacles = (bool[,])obstacles.Clone()
        };
    }
}
```

**2. AI 리팩토링**
```csharp
// ChessAI.cs
private Move DepthLimitedSearch(Board board, int depth)
{
    BoardState state = BoardState.FromBoard(board);
    List<Move> allMoves = GetAllMoves(state, aiTeam);

    foreach (var move in allMoves)
    {
        BoardState clonedState = state.Clone();  // 빠른 배열 복사
        ApplyMove(clonedState, move);

        float score = MinimaxState(clonedState, depth - 1, ...);

        // Destroy 불필요 - 구조체는 스택에서 자동 해제
    }
}
```

**성능 개선 예상:**
- Clone 시간: 2-3ms → 0.1-0.2ms (10-20배 빠름)
- GC 압박: 대폭 감소
- AI 응답 시간: 50-70% 단축

---

### 2.4 AI 최적화 기회

**심각도:** Low
**파일:** `ChessAI.cs`

#### 현재 구현 평가
**위치:** `ChessAI.cs:64-240`

✅ **잘 구현된 부분:**
- Minimax 알고리즘
- Alpha-Beta Pruning
- Iterative Deepening
- Time limit 체크

#### 추가 최적화 기회

**1. Transposition Table**

동일한 보드 상태를 여러 번 평가하는 것을 방지:

```csharp
public class ChessAI : MonoBehaviour
{
    private Dictionary<ulong, TranspositionEntry> transpositionTable
        = new Dictionary<ulong, TranspositionEntry>();

    private struct TranspositionEntry
    {
        public float Score;
        public int Depth;
        public Move BestMove;
    }

    private float Minimax(BoardState state, int depth, float alpha, float beta, bool maximizing)
    {
        ulong hash = state.GetZobristHash();

        if (transpositionTable.TryGetValue(hash, out var entry))
        {
            if (entry.Depth >= depth)
            {
                return entry.Score;  // 캐시 히트!
            }
        }

        // ... 기존 로직

        transpositionTable[hash] = new TranspositionEntry
        {
            Score = evaluation,
            Depth = depth,
            BestMove = bestMove
        };

        return evaluation;
    }
}

// BoardState에 추가
public struct BoardState
{
    public ulong GetZobristHash()
    {
        ulong hash = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (pieceTypes[x, y] != PieceType.None)
                {
                    hash ^= ZobristTable[x, y, (int)pieceTypes[x, y], (int)pieceTeams[x, y]];
                }
            }
        }
        return hash;
    }
}
```

**예상 개선:** 30-50% 속도 향상

**2. Move Ordering**

좋은 수를 먼저 평가하여 Alpha-Beta Pruning 효율 증가:

```csharp
private List<Move> GetAllMoves(BoardState state, Team team)
{
    var moves = GenerateAllMoves(state, team);

    // 우선순위:
    // 1. Captures (특히 높은 가치 피스)
    // 2. 위협 회피
    // 3. 기타
    moves.Sort((a, b) => {
        int scoreA = QuickMoveEvaluation(state, a);
        int scoreB = QuickMoveEvaluation(state, b);
        return scoreB.CompareTo(scoreA);
    });

    return moves;
}

private int QuickMoveEvaluation(BoardState state, Move move)
{
    int score = 0;

    // Capture bonus
    if (state.pieceTypes[move.To.x, move.To.y] != PieceType.None)
    {
        score += GetPieceValue(state.pieceTypes[move.To.x, move.To.y]) * 10;
    }

    // Center control
    if (IsCenter(move.To))
    {
        score += 5;
    }

    return score;
}
```

**예상 개선:** 20-40% 속도 향상

**3. Quiescence Search**

포획이 가능한 불안정한 상태에서 깊이를 추가로 탐색:

```csharp
private float Minimax(BoardState state, int depth, float alpha, float beta, bool maximizing)
{
    if (depth == 0)
    {
        if (IsQuiet(state))
        {
            return stateEvaluator.EvaluateState(state);
        }
        else
        {
            return QuiescenceSearch(state, alpha, beta);
        }
    }

    // ... 기존 로직
}

private float QuiescenceSearch(BoardState state, float alpha, float beta)
{
    float standPat = stateEvaluator.EvaluateState(state);
    if (standPat >= beta) return beta;
    if (alpha < standPat) alpha = standPat;

    var captureMoves = GetCaptureMoves(state);
    foreach (var move in captureMoves)
    {
        var newState = state.Clone();
        ApplyMove(newState, move);

        float score = -QuiescenceSearch(newState, -beta, -alpha);

        if (score >= beta) return beta;
        if (score > alpha) alpha = score;
    }

    return alpha;
}
```

**예상 개선:** 평가 정확도 15-25% 향상

---

## 3. 🐛 버그 및 잠재적 이슈

### 3.1 Null Reference 취약점

**심각도:** High
**파일:** 다수 (625건의 null 참조)

#### 문제 패턴

**패턴 1: 조용히 실패**
**위치:** `Board.cs:177-182`
```csharp
public void PlacePiece(Piece piece, Vector2Int position)
{
    if (!IsPositionValid(position))
    {
        Debug.LogError($"Cannot place piece at invalid position {position}");
        return;  // ⚠️ 조용히 반환 - 호출자는 성공했다고 생각
    }
    // ...
}
```

**패턴 2: bool 반환 (일관성 없음)**
**위치:** `Board.cs:204`
```csharp
public bool MovePiece(Vector2Int from, Vector2Int to)
{
    if (!IsPositionValid(from) || !IsPositionValid(to))
    {
        return false;  // 실패 알림
    }
    // ...
}
```

**패턴 3: null 반환**
**위치:** `Board.cs:102-110`
```csharp
public Piece GetPiece(Vector2Int position)
{
    if (!IsPositionValid(position))
    {
        return null;  // null 반환
    }

    return pieces[position.x, position.y];
}
```

#### 구체적 문제

**위치:** `GameManager.cs:228-238`
```csharp
public bool ExecuteMove(Vector2Int from, Vector2Int to)
{
    if (board == null)
    {
        return false;  // ⚠️ 로그도 없음
    }

    var movingPiece = board.GetPiece(from);  // ⚠️ null 가능
    var capturedPiece = board.GetPiece(to);

    bool success = board.MovePiece(from, to);  // ⚠️ movingPiece가 null이면?

    if (success)
    {
        // ⚠️ movingPiece.Type 접근 시 NullReferenceException 가능
        if (MutationManager.Instance != null)
        {
            MutationManager.Instance.NotifyMove(movingPiece, from, to, board);
        }
    }
}
```

#### 권장사항

**1. Null Object Pattern**
```csharp
public class Piece : MonoBehaviour, IPiece
{
    public static readonly Piece Null = new NullPiece();

    private class NullPiece : Piece
    {
        public NullPiece()
        {
            pieceType = PieceType.None;
            team = Team.White;
            position = Vector2Int.zero;
        }

        public override List<Vector2Int> GetValidMoves(IBoard board)
        {
            return new List<Vector2Int>();
        }

        public override string ToString() => "Null Piece";
    }
}

// 사용
public Piece GetPiece(Vector2Int position)
{
    if (!IsPositionValid(position))
        return Piece.Null;  // null 대신 Null Object

    return pieces[position.x, position.y] ?? Piece.Null;
}
```

**2. C# 8.0 Nullable Reference Types**
```csharp
#nullable enable

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Board? board;  // nullable 명시

    public bool ExecuteMove(Vector2Int from, Vector2Int to)
    {
        if (board == null)
        {
            Debug.LogError("Board is null!");
            return false;
        }

        Piece? movingPiece = board.GetPiece(from);
        if (movingPiece == null)
        {
            Debug.LogError($"No piece at {from}");
            return false;
        }

        // 이후 movingPiece는 non-null 보장
    }
}
```

**3. Debug.Assert (개발 빌드)**
```csharp
public void ApplyMutation(Piece piece, Mutation mutation)
{
    Debug.Assert(piece != null, "Piece cannot be null");
    Debug.Assert(mutation != null, "Mutation cannot be null");

    // Release 빌드에서는 제거됨
    if (piece == null || mutation == null)
    {
        Debug.LogError("Invalid parameters");
        return;
    }

    // ...
}
```

**4. Result 패턴 (함수형 접근)**
```csharp
public struct Result<T>
{
    public bool Success { get; }
    public T Value { get; }
    public string Error { get; }

    public static Result<T> Ok(T value)
        => new Result<T> { Success = true, Value = value };

    public static Result<T> Fail(string error)
        => new Result<T> { Success = false, Error = error };
}

public Result<bool> ExecuteMove(Vector2Int from, Vector2Int to)
{
    if (board == null)
        return Result<bool>.Fail("Board is null");

    var movingPiece = board.GetPiece(from);
    if (movingPiece == null)
        return Result<bool>.Fail($"No piece at {from}");

    bool success = board.MovePiece(from, to);
    if (!success)
        return Result<bool>.Fail("Move failed");

    return Result<bool>.Ok(true);
}

// 사용
var result = gameManager.ExecuteMove(from, to);
if (!result.Success)
{
    Debug.LogError(result.Error);
    return;
}
```

---

### 3.2 동기화 문제 - 이벤트 구독

**심각도:** Medium
**파일:** `DungeonManager.cs`

#### 문제점

**위치:** `DungeonManager.cs:146-169`
```csharp
private void Start()
{
    // 씬 재로드 시 중복 구독 가능
    if (dungeonMapUI != null)
    {
        dungeonMapUI.OnNodeSelected.AddListener(OnNodeSelected);  // ⚠️
    }

    if (rewardUI != null)
    {
        rewardUI.OnRewardSelected.AddListener(OnRewardSelected);  // ⚠️
    }

    if (repairUI != null)
    {
        repairUI.OnRepairCompleted.AddListener(ContinueAfterRest);  // ⚠️
    }

    if (gameManager != null)
    {
        gameManager.OnVictory.AddListener(OnRoomVictory);  // ⚠️
        gameManager.OnDefeat.AddListener(OnRoomDefeat);  // ⚠️
    }
}
```

**시나리오:**
1. 게임 시작 → Start() 호출 → 이벤트 구독
2. 씬 재로드 → Start() 다시 호출 → **중복 구독**
3. 이벤트 발생 → 핸들러가 여러 번 호출됨!

#### 권장사항

**방법 1: OnEnable/OnDisable 사용**
```csharp
private void OnEnable()
{
    // 구독 전 명시적 해제
    if (dungeonMapUI != null)
    {
        dungeonMapUI.OnNodeSelected.RemoveListener(OnNodeSelected);
        dungeonMapUI.OnNodeSelected.AddListener(OnNodeSelected);
    }

    if (gameManager != null)
    {
        gameManager.OnVictory.RemoveListener(OnRoomVictory);
        gameManager.OnVictory.AddListener(OnRoomVictory);
    }
}

private void OnDisable()
{
    // 명시적 해제
    if (dungeonMapUI != null)
    {
        dungeonMapUI.OnNodeSelected.RemoveListener(OnNodeSelected);
    }

    if (gameManager != null)
    {
        gameManager.OnVictory.RemoveListener(OnRoomVictory);
        gameManager.OnDefeat.RemoveListener(OnRoomDefeat);
    }
}
```

**방법 2: 구독 상태 추적**
```csharp
private bool isSubscribed = false;

private void Start()
{
    if (!isSubscribed)
    {
        SubscribeToEvents();
        isSubscribed = true;
    }
}

private void SubscribeToEvents()
{
    if (dungeonMapUI != null)
        dungeonMapUI.OnNodeSelected.AddListener(OnNodeSelected);
    if (gameManager != null)
        gameManager.OnVictory.AddListener(OnRoomVictory);
}

private void OnDestroy()
{
    UnsubscribeFromEvents();
}

private void UnsubscribeFromEvents()
{
    if (dungeonMapUI != null)
        dungeonMapUI.OnNodeSelected.RemoveListener(OnNodeSelected);
    if (gameManager != null)
        gameManager.OnVictory.RemoveListener(OnRoomVictory);

    isSubscribed = false;
}
```

---

### 3.3 Race Condition - 싱글톤 초기화

**심각도:** Medium
**파일:** 여러 매니저 클래스

#### 문제 시나리오

```
GameManager.Awake()
  └─> MutationManager.Instance 접근
      └─> MutationManager.Awake() 실행 전
          └─> GlobalDataManager.Instance 접근
              └─> GlobalDataManager.Awake() 실행 전
                  └─> DungeonManager.Instance 접근
                      └─> GameManager.Instance 접근 (순환!)
```

#### 구체적 문제

**위치:** 여러 파일
```csharp
// GameManager.cs Awake()
if (MutationManager.Instance != null)  // MutationManager가 아직 초기화 안 됨
{
    // ...
}

// MutationManager.cs Awake()
if (GlobalDataManager.Instance != null)  // GlobalDataManager가 아직 초기화 안 됨
{
    // ...
}
```

#### 권장사항

**방법 1: 명시적 초기화 순서**
```csharp
public class GameBootstrap : MonoBehaviour
{
    [Header("Initialization Order")]
    [SerializeField] private int order = -100;

    [Header("Managers")]
    [SerializeField] private GlobalDataManager globalDataManager;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private MutationManager mutationManager;
    [SerializeField] private ArtifactManager artifactManager;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        // 순서대로 초기화
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        Debug.Log("Initializing managers in order...");

        // 1. 데이터 매니저
        globalDataManager?.Initialize();
        saveManager?.Initialize();

        // 2. 게임플레이 매니저
        mutationManager?.Initialize();
        artifactManager?.Initialize();

        // 3. 던전 매니저
        dungeonManager?.Initialize();

        // 4. 게임 매니저 (마지막)
        gameManager?.Initialize();

        Debug.Log("Manager initialization complete!");
    }
}
```

**방법 2: Script Execution Order (Unity)**
```
Unity Editor:
Edit → Project Settings → Script Execution Order

설정:
-100: GameBootstrap
-50: GlobalDataManager
-40: SaveManager
-30: MutationManager
-20: ArtifactManager
-10: DungeonManager
0: GameManager (기본값)
10: 기타
```

**방법 3: Lazy Initialization (신중히)**
```csharp
public class MutationManager : MonoBehaviour
{
    private static MutationManager instance;
    private bool isInitialized = false;

    public static MutationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MutationManager>();
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
    }

    public void Initialize()
    {
        if (isInitialized) return;

        // 안전한 초기화 (다른 싱글톤에 의존하지 않음)
        pieceMutations = new Dictionary<Piece, List<Mutation>>();
        mutationStacks = new Dictionary<Piece, Dictionary<Mutation, int>>();

        isInitialized = true;
    }

    public void ApplyMutation(Piece piece, Mutation mutation)
    {
        if (!isInitialized)
        {
            Debug.LogError("MutationManager not initialized!");
            return;
        }

        // ... 로직
    }
}
```

---

### 3.4 BoardInputHandler - 입력 처리

**심각도:** Medium
**파일:** `BoardInputHandler.cs`

#### 문제 1: 입력 시스템 제한

**위치:** `BoardInputHandler.cs:30-41`
```csharp
private void Update()
{
    if (gameManager.CurrentTurn != gameManager.PlayerTeam ||
        gameManager.State != GameManager.GameState.PlayerTurn)
    {
        return;
    }

    if (Input.GetMouseButtonDown(0))  // ⚠️ 마우스만 지원
    {
        HandleInput();
    }
}
```

**문제:**
- 터치 입력 미지원
- 키보드 입력 미지원
- Old Input System 사용
- 모바일 포팅 불가

#### 문제 2: 좌표 변환 부정확

**위치:** `BoardInputHandler.cs:65-71`
```csharp
private Vector2Int GetGridPosition(Vector2 worldPos)
{
    // ⚠️ 가정: 1 unit per tile, origin at (0,0)
    return new Vector2Int(
        Mathf.RoundToInt(worldPos.x),
        Mathf.RoundToInt(worldPos.y)
    );
    // 보드의 실제 크기/위치 고려 안 함!
}
```

**문제:**
- 보드 오프셋 미고려
- 타일 크기 하드코딩
- 카메라 투영 미고려
- 스케일 변경 시 작동 안 함

#### 권장사항

**1. New Input System 사용**
```csharp
using UnityEngine.InputSystem;

public class BoardInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    private InputAction selectAction;

    private void OnEnable()
    {
        selectAction = inputActions.FindAction("Select");
        selectAction.performed += OnSelectPerformed;
        selectAction.Enable();
    }

    private void OnDisable()
    {
        selectAction.performed -= OnSelectPerformed;
        selectAction.Disable();
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        HandleInput(screenPos);
    }

    private void HandleInput(Vector2 screenPos)
    {
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        Vector2Int gridPos = GetGridPosition(worldPos);

        if (board.IsPositionValid(gridPos))
        {
            HandleClick(gridPos);
        }
    }
}
```

**2. 정확한 좌표 변환**
```csharp
[Header("Board Transform")]
[SerializeField] private Transform boardTransform;
[SerializeField] private float tileSize = 1f;
[SerializeField] private Vector2 boardOrigin = Vector2.zero;

private Vector2Int GetGridPosition(Vector2 worldPos)
{
    // 보드 로컬 좌표로 변환
    Vector2 localPos = worldPos - boardOrigin;

    // 보드가 회전/스케일되었을 경우
    if (boardTransform != null)
    {
        localPos = boardTransform.InverseTransformPoint(worldPos);
    }

    // 그리드 좌표 계산
    int x = Mathf.FloorToInt(localPos.x / tileSize);
    int y = Mathf.FloorToInt(localPos.y / tileSize);

    return new Vector2Int(x, y);
}
```

**3. Raycast 기반 (3D 보드용)**
```csharp
private Vector2Int? GetGridPositionFromRaycast(Vector2 screenPos)
{
    Ray ray = mainCamera.ScreenPointToRay(screenPos);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 100f, boardLayer))
    {
        // 타일 컴포넌트에서 그리드 위치 가져오기
        Tile tile = hit.collider.GetComponent<Tile>();
        if (tile != null)
        {
            return tile.GridPosition;
        }
    }

    return null;
}
```

---

## 4. 💾 저장/로드 시스템

### 4.1 SaveManager - 불완전한 구현

**심각도:** High
**파일:** `SaveManager.cs`, `SaveData.cs`, `DungeonManager.cs`

#### 문제 1: 하드코딩된 팀

**위치:** `SaveManager.cs:67`
```csharp
var playerPieces = board.GetPiecesByTeam(Team.White);  // ⚠️ 항상 White?
```

**문제:** 플레이어가 Black 팀일 경우 저장 안 됨

#### 문제 2: 던전 시드 불일치

**SaveData.cs:15**
```csharp
public int DungeonSeed;  // 필드 정의
```

**DungeonManager.cs:235**
```csharp
int seed = data.DungeonSeed != 0 ? data.DungeonSeed : ...;  // 사용
```

**SaveManager.cs:52**
```csharp
// Seed saving would go here if implemented  // ⚠️ 주석만 있음
```

**문제:** 시드가 실제로 저장되지 않음 → 로드 시 다른 던전 생성

#### 문제 3: 리소스 경로 하드코딩

**DungeonManager.cs:211-212**
```csharp
var mutationLib = Resources.Load<MutationLibrary>("MutationLibrary");  // ⚠️ 하드코딩
var artifactLib = Resources.Load<ArtifactLibrary>("ArtifactLibrary");  // ⚠️ 하드코딩
```

**문제:**
- 경로 변경 시 코드 수정 필요
- 오타 위험
- Inspector에서 설정 불가

#### 권장사항

**1. 완전한 저장 데이터 구조**
```csharp
[Serializable]
public class GameSaveData
{
    // 메타데이터
    public string SaveDate;
    public string GameVersion = "1.0.0";  // 버전 호환성

    // 던전 상태
    public int DungeonSeed;  // 필수!
    public Team PlayerTeam;  // 팀도 저장
    public int CurrentFloor;
    public int CurrentRoomIndex;

    // 플레이어 상태
    public PlayerSaveData PlayerData;
    public List<string> ActiveArtifactNames;
    public int Gold;

    // 통계
    public int TotalMoves;
    public int PiecesLost;
    public float PlayTime;
}
```

**2. 저장 검증**
```csharp
public class SaveManager : MonoBehaviour
{
    public void SaveGame()
    {
        GameSaveData data = CollectSaveData();

        if (!ValidateSaveData(data))
        {
            Debug.LogError("Save data validation failed!");
            ShowSaveErrorUI("저장 데이터가 유효하지 않습니다.");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = GetSavePath();
            File.WriteAllText(path, json);

            Debug.Log($"Game saved successfully to {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
            ShowSaveErrorUI("저장에 실패했습니다.");
        }
    }

    private bool ValidateSaveData(GameSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("Save data is null");
            return false;
        }

        if (data.PlayerData == null)
        {
            Debug.LogError("Player data is null");
            return false;
        }

        if (data.PlayerData.Pieces == null || data.PlayerData.Pieces.Count == 0)
        {
            Debug.LogError("No pieces to save");
            return false;
        }

        if (data.DungeonSeed == 0)
        {
            Debug.LogError("Dungeon seed is 0");
            return false;
        }

        return true;
    }

    private GameSaveData CollectSaveData()
    {
        var data = new GameSaveData
        {
            SaveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            GameVersion = Application.version,
            DungeonSeed = dungeonManager.CurrentDungeonSeed,  // 실제 저장!
            PlayerTeam = gameManager.PlayerTeam,  // 팀 저장!
            CurrentFloor = dungeonManager.CurrentFloor,
            CurrentRoomIndex = dungeonManager.CurrentRoomIndex,
            Gold = dungeonManager.PlayerState.Currency
        };

        // 피스 저장
        data.PlayerData = SavePlayerData();

        // 아티팩트 저장
        data.ActiveArtifactNames = SaveArtifacts();

        return data;
    }
}
```

**3. 리소스 관리 개선**
```csharp
public class LibraryManager : MonoBehaviour
{
    private static LibraryManager instance;

    [Header("Libraries")]
    [SerializeField] private MutationLibrary mutationLibrary;
    [SerializeField] private ArtifactLibrary artifactLibrary;

    public static LibraryManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LibraryManager>();
            }
            return instance;
        }
    }

    public static MutationLibrary MutationLibrary => Instance?.mutationLibrary;
    public static ArtifactLibrary ArtifactLibrary => Instance?.artifactLibrary;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        ValidateLibraries();
    }

    private void ValidateLibraries()
    {
        if (mutationLibrary == null)
        {
            Debug.LogError("MutationLibrary is not assigned!");
        }

        if (artifactLibrary == null)
        {
            Debug.LogError("ArtifactLibrary is not assigned!");
        }
    }
}

// 사용
var mutation = LibraryManager.MutationLibrary.GetMutationByName(name);
```

---

### 4.2 JSON 직렬화 제한

**심각도:** Medium
**파일:** `SaveManager.cs`

#### 현재 문제

**위치:** `SaveManager.cs:136-138`
```csharp
string json = JsonUtility.ToJson(data, true);
string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
File.WriteAllText(path, json);
```

**Unity JsonUtility 제한:**
- ❌ Dictionary 직렬화 불가
- ❌ 다형성 지원 안 함
- ❌ null 값 처리 문제
- ❌ 순환 참조 처리 안 됨
- ❌ 커스텀 직렬화 제한적

#### 권장사항

**방법 1: Newtonsoft.Json (JSON.NET)**
```csharp
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SaveManager : MonoBehaviour
{
    public void SaveGame()
    {
        GameSaveData data = CollectSaveData();

        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,  // 다형성 지원
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,  // 순환 참조 방지
            Formatting = Formatting.Indented,  // 가독성
            NullValueHandling = NullValueHandling.Ignore  // null 제외
        };

        try
        {
            string json = JsonConvert.SerializeObject(data, settings);
            string path = GetSavePath();
            File.WriteAllText(path, json);

            Debug.Log($"Game saved: {path}");
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON serialization failed: {ex.Message}");
        }
    }

    public GameSaveData LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };

            var data = JsonConvert.DeserializeObject<GameSaveData>(json, settings);

            // 버전 검증
            if (data.GameVersion != Application.version)
            {
                Debug.LogWarning($"Save file version mismatch: {data.GameVersion} vs {Application.version}");
                // 마이그레이션 로직 필요
            }

            return data;
        }
        catch (JsonException ex)
        {
            Debug.LogError($"JSON deserialization failed: {ex.Message}");
            return null;
        }
    }
}
```

**방법 2: 암호화 추가 (치트 방지)**
```csharp
using System.Security.Cryptography;
using System.Text;
using System.IO;

public class SecureSaveManager : MonoBehaviour
{
    // ⚠️ 실제 프로젝트에서는 더 안전한 키 관리 필요
    private static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("YourSecureKey123456");  // 16, 24, or 32 bytes
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("YourIV1234567890");  // 16 bytes

    public void SaveGame(GameSaveData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        byte[] encrypted = Encrypt(json);

        string path = GetSavePath();
        File.WriteAllBytes(path, encrypted);

        Debug.Log("Game saved securely");
    }

    public GameSaveData LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found");
            return null;
        }

        try
        {
            byte[] encrypted = File.ReadAllBytes(path);
            string json = Decrypt(encrypted);
            return JsonConvert.DeserializeObject<GameSaveData>(json);
        }
        catch (CryptographicException ex)
        {
            Debug.LogError($"Save file may be corrupted or tampered: {ex.Message}");
            return null;
        }
    }

    private byte[] Encrypt(string plainText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (StreamWriter sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
                sw.Close();
                return ms.ToArray();
            }
        }
    }

    private string Decrypt(byte[] cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(cipherText))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
```

**방법 3: 체크섬 검증**
```csharp
using System.Security.Cryptography;

[Serializable]
public class GameSaveData
{
    public string SaveDate;
    public int DungeonSeed;
    // ... 기타 데이터

    public string Checksum;  // SHA256 해시

    public void CalculateChecksum()
    {
        // 주요 데이터를 문자열로 연결
        string dataString = $"{SaveDate}|{DungeonSeed}|{CurrentFloor}|{Gold}|{PlayerTeam}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(dataString);
            byte[] hash = sha256.ComputeHash(bytes);
            Checksum = Convert.ToBase64String(hash);
        }
    }

    public bool ValidateChecksum()
    {
        string originalChecksum = Checksum;
        CalculateChecksum();
        bool isValid = Checksum == originalChecksum;

        if (!isValid)
        {
            Debug.LogWarning("Save file checksum mismatch - possible tampering!");
        }

        return isValid;
    }
}

// SaveManager에서 사용
public void SaveGame(GameSaveData data)
{
    data.CalculateChecksum();
    string json = JsonConvert.SerializeObject(data);
    File.WriteAllText(GetSavePath(), json);
}

public GameSaveData LoadGame()
{
    string json = File.ReadAllText(GetSavePath());
    var data = JsonConvert.DeserializeObject<GameSaveData>(json);

    if (!data.ValidateChecksum())
    {
        // 변조된 저장 파일
        return null;
    }

    return data;
}
```

---

## 5. 🎮 Unity 특화 Best Practices

### 5.1 SerializeField vs Public

**심각도:** None (긍정적)
**파일:** 대부분

#### 우수 사례 ⭐

**위치:** `GameManager.cs:18-42`
```csharp
[Header("Core References")]
[SerializeField]
private Board board;

[SerializeField]
private ChessAI aiPlayer;

[Header("Game State")]
[SerializeField]
private Team playerTeam = Team.White;

[SerializeField]
private Team currentTurn = Team.White;
```

**장점:**
✅ Encapsulation 유지
✅ Inspector에서 설정 가능
✅ 외부에서 직접 접근 불가
✅ Header로 그룹화

#### 추가 개선 제안

```csharp
// Odin Inspector 사용 시
using Sirenix.OdinInspector;

[Header("Core References")]
[SerializeField, Required, AssetsOnly]
private Board board;

[SerializeField, Required, SceneObjectsOnly]
private ChessAI aiPlayer;

[Header("Game State")]
[SerializeField, EnumToggleButtons]
private Team playerTeam = Team.White;

[ShowInInspector, ReadOnly]
private Team currentTurn = Team.White;

// 또는 NaughtyAttributes 사용
using NaughtyAttributes;

[Header("Core References")]
[SerializeField, Required]
private Board board;

[SerializeField, ValidateInput("IsNotNull", "AI Player is required!")]
private ChessAI aiPlayer;

[Header("Game State")]
[SerializeField]
[OnValueChanged("OnPlayerTeamChanged")]
private Team playerTeam = Team.White;

private bool IsNotNull(ChessAI value)
{
    return value != null;
}

private void OnPlayerTeamChanged()
{
    Debug.Log($"Player team changed to {playerTeam}");
}
```

---

### 5.2 GameObject 생성 및 파괴

**심각도:** Medium
**파일:** `Board.cs`, `ChessAI.cs`

#### 문제 분석

이미 섹션 2.3에서 다뤘지만, 추가 권장사항:

**Object Pooling 패턴**
```csharp
public class PiecePool : MonoBehaviour
{
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private int initialPoolSize = 32;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        // 초기 풀 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject piece = Instantiate(piecePrefab);
            piece.SetActive(false);
            pool.Enqueue(piece);
        }
    }

    public GameObject GetPiece()
    {
        if (pool.Count > 0)
        {
            GameObject piece = pool.Dequeue();
            piece.SetActive(true);
            return piece;
        }
        else
        {
            // 풀이 비었으면 새로 생성
            return Instantiate(piecePrefab);
        }
    }

    public void ReturnPiece(GameObject piece)
    {
        piece.SetActive(false);
        pool.Enqueue(piece);
    }
}

// Board.cs에서 사용
public class Board : MonoBehaviour
{
    [SerializeField] private PiecePool piecePool;

    public Piece SpawnPiece(PieceType type, Team team, Vector2Int position)
    {
        GameObject pieceObject = piecePool.GetPiece();
        pieceObject.name = $"{team}_{type}";

        Piece piece = pieceObject.GetComponent<Piece>();
        if (piece == null)
        {
            piece = pieceObject.AddComponent<Piece>();
        }

        piece.Initialize(type, team, position);
        PlacePiece(piece, position);

        return piece;
    }

    public void RemovePiece(Vector2Int position)
    {
        if (!IsPositionValid(position))
        {
            return;
        }

        Piece piece = pieces[position.x, position.y];
        if (piece != null)
        {
            pieces[position.x, position.y] = null;
            allPieces.Remove(piece);

            piecePool.ReturnPiece(piece.gameObject);
        }
    }
}
```

---

### 5.3 코루틴 사용

**심각도:** Low
**파일:** `GameManager.cs`

#### 현재 문제

**위치:** `GameManager.cs:189-198`
```csharp
if (!IsPlayerTurn && aiPlayer != null)
{
    if (simulationMode)
    {
        ExecuteAITurn();
    }
    else
    {
        Invoke(nameof(ExecuteAITurn), 0.5f);  // ⚠️ Invoke 사용
    }
}
```

**Invoke의 문제점:**
- 문자열 기반 (리팩토링 시 오류)
- 취소 어려움
- 매개변수 전달 불가
- 디버깅 어려움

#### 권장사항

**코루틴 사용**
```csharp
private Coroutine aiTurnCoroutine;

private void StartTurn()
{
    turnNumber++;
    state = IsPlayerTurn ? GameState.PlayerTurn : GameState.AITurn;
    OnTurnStart?.Invoke(currentTurn);

    if (!IsPlayerTurn && aiPlayer != null)
    {
        if (simulationMode)
        {
            ExecuteAITurn();
        }
        else
        {
            // 코루틴 사용
            aiTurnCoroutine = StartCoroutine(ExecuteAITurnWithDelay(0.5f));
        }
    }
}

private IEnumerator ExecuteAITurnWithDelay(float delay)
{
    // 시각적 효과 표시
    if (turnIndicator != null)
    {
        turnIndicator.ShowAIThinking();
    }

    yield return new WaitForSeconds(delay);

    ExecuteAITurn();

    if (turnIndicator != null)
    {
        turnIndicator.HideAIThinking();
    }
}

// 게임 종료 시 코루틴 정리
private void OnDestroy()
{
    if (aiTurnCoroutine != null)
    {
        StopCoroutine(aiTurnCoroutine);
    }
}

// 긴급 중단 가능
public void CancelAITurn()
{
    if (aiTurnCoroutine != null)
    {
        StopCoroutine(aiTurnCoroutine);
        aiTurnCoroutine = null;
    }
}
```

**UniTask 사용 (고급)**
```csharp
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameManager : MonoBehaviour
{
    private CancellationTokenSource aiCancellation;

    private async void StartTurn()
    {
        turnNumber++;
        state = IsPlayerTurn ? GameState.PlayerTurn : GameState.AITurn;
        OnTurnStart?.Invoke(currentTurn);

        if (!IsPlayerTurn && aiPlayer != null)
        {
            if (simulationMode)
            {
                ExecuteAITurn();
            }
            else
            {
                aiCancellation = new CancellationTokenSource();
                await ExecuteAITurnAsync(0.5f, aiCancellation.Token);
            }
        }
    }

    private async UniTask ExecuteAITurnAsync(float delay, CancellationToken token)
    {
        try
        {
            if (turnIndicator != null)
            {
                turnIndicator.ShowAIThinking();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: token);

            ExecuteAITurn();

            if (turnIndicator != null)
            {
                turnIndicator.HideAIThinking();
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("AI turn cancelled");
        }
    }

    public void CancelAITurn()
    {
        aiCancellation?.Cancel();
        aiCancellation?.Dispose();
        aiCancellation = null;
    }

    private void OnDestroy()
    {
        CancelAITurn();
    }
}
```

---

## 6. 📝 코드 품질 및 가독성

### 6.1 문서화 - 우수 사례 ⭐

**심각도:** None (긍정적)
**파일:** 대부분

#### 훌륭한 XML 문서화

**위치:** `Board.cs:86-97`
```csharp
/// <summary>
/// Gets the piece at the specified position.
/// </summary>
public IPiece GetPieceAt(Vector2Int position)
{
    if (!IsPositionValid(position))
    {
        return null;
    }

    return pieces[position.x, position.y];
}
```

**통계:**
✅ 대부분의 public 메서드에 XML 주석
✅ 파라미터 설명 포함
✅ 반환값 설명 포함
✅ 클래스 레벨 설명

#### 추가 개선 제안

```csharp
/// <summary>
/// Gets the piece at the specified position on the board.
/// </summary>
/// <param name="position">
/// Grid position in board coordinates (0,0 = bottom-left).
/// </param>
/// <returns>
/// The piece at the specified position, or null if the position is
/// empty or invalid.
/// </returns>
/// <example>
/// <code>
/// Vector2Int pos = new Vector2Int(4, 4);
/// Piece piece = board.GetPieceAt(pos);
/// if (piece != null)
/// {
///     Debug.Log($"Found {piece.Type} at {pos}");
/// }
/// </code>
/// </example>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown when DEBUG is defined and position is outside board bounds.
/// </exception>
public IPiece GetPieceAt(Vector2Int position)
{
    #if DEBUG
    if (!IsPositionValid(position))
    {
        throw new ArgumentOutOfRangeException(
            nameof(position),
            $"Position {position} is outside board bounds (0-{Width-1}, 0-{Height-1})"
        );
    }
    #endif

    if (!IsPositionValid(position))
    {
        return null;
    }

    return pieces[position.x, position.y];
}
```

---

### 6.2 매직 넘버 제거

**심각도:** Low
**파일:** 여러 파일

#### 문제 코드

**위치:** `DungeonManager.cs:426-445`
```csharp
// Random event system - 40% treasure, 30% curse, 30% blessing
int roll = UnityEngine.Random.Range(0, 100);

if (roll < 40)  // ⚠️ 매직 넘버
{
    EnterTreasureRoom(roomNode);
}
else if (roll < 70)  // ⚠️ 매직 넘버 (40 + 30)
{
    HandleCurseEvent();
}
else
{
    HandleBlessingEvent();
}
```

**위치:** `DungeonManager.cs:453-455`
```csharp
int currencyLoss = UnityEngine.Random.Range(10, 31);  // ⚠️ 매직 넘버
```

**위치:** `DungeonManager.cs:466`
```csharp
int currencyGain = UnityEngine.Random.Range(20, 51);  // ⚠️ 매직 넘버
```

#### 권장사항

**방법 1: 상수 정의**
```csharp
public class DungeonManager : MonoBehaviour
{
    // 신비 방 확률
    private const int MYSTERY_TREASURE_CHANCE = 40;
    private const int MYSTERY_CURSE_CHANCE = 30;
    private const int MYSTERY_BLESSING_CHANCE = 30;

    // 저주 효과
    private const int CURSE_MIN_CURRENCY_LOSS = 10;
    private const int CURSE_MAX_CURRENCY_LOSS = 30;

    // 축복 효과
    private const int BLESSING_MIN_CURRENCY_GAIN = 20;
    private const int BLESSING_MAX_CURRENCY_GAIN = 50;

    private void EnterMysteryRoom(RoomNode roomNode)
    {
        int roll = UnityEngine.Random.Range(0, 100);

        if (roll < MYSTERY_TREASURE_CHANCE)
        {
            EnterTreasureRoom(roomNode);
        }
        else if (roll < MYSTERY_TREASURE_CHANCE + MYSTERY_CURSE_CHANCE)
        {
            HandleCurseEvent();
        }
        else
        {
            HandleBlessingEvent();
        }
    }

    private void HandleCurseEvent()
    {
        int currencyLoss = UnityEngine.Random.Range(
            CURSE_MIN_CURRENCY_LOSS,
            CURSE_MAX_CURRENCY_LOSS + 1
        );
        playerState.Currency = Mathf.Max(0, playerState.Currency - currencyLoss);

        ShowNotification($"저주! {currencyLoss} 골드를 잃었습니다.");
    }
}
```

**방법 2: ScriptableObject Config (더 나음)**
```csharp
[CreateAssetMenu(fileName = "MysteryRoomConfig", menuName = "MutatingGambit/Configs/Mystery Room")]
public class MysteryRoomConfig : ScriptableObject
{
    [Header("Event Probabilities")]
    [Range(0, 100)]
    [Tooltip("확률: 보물")]
    public int treasureChance = 40;

    [Range(0, 100)]
    [Tooltip("확률: 저주")]
    public int curseChance = 30;

    [Range(0, 100)]
    [Tooltip("확률: 축복")]
    public int blessingChance = 30;

    [Header("Curse Effect")]
    [Tooltip("저주로 잃을 최소 골드")]
    public int curseMinGoldLoss = 10;

    [Tooltip("저주로 잃을 최대 골드")]
    public int curseMaxGoldLoss = 30;

    [Header("Blessing Effect")]
    [Tooltip("축복으로 얻을 최소 골드")]
    public int blessingMinGoldGain = 20;

    [Tooltip("축복으로 얻을 최대 골드")]
    public int blessingMaxGoldGain = 50;

    private void OnValidate()
    {
        // 확률 합계 검증
        int total = treasureChance + curseChance + blessingChance;
        if (total != 100)
        {
            Debug.LogWarning($"Mystery room probabilities don't sum to 100% (current: {total}%)");
        }
    }
}

// DungeonManager.cs
public class DungeonManager : MonoBehaviour
{
    [Header("Mystery Room")]
    [SerializeField] private MysteryRoomConfig mysteryConfig;

    private void EnterMysteryRoom(RoomNode roomNode)
    {
        int roll = UnityEngine.Random.Range(0, 100);

        if (roll < mysteryConfig.treasureChance)
        {
            EnterTreasureRoom(roomNode);
        }
        else if (roll < mysteryConfig.treasureChance + mysteryConfig.curseChance)
        {
            HandleCurseEvent();
        }
        else
        {
            HandleBlessingEvent();
        }
    }

    private void HandleCurseEvent()
    {
        int currencyLoss = UnityEngine.Random.Range(
            mysteryConfig.curseMinGoldLoss,
            mysteryConfig.curseMaxGoldLoss + 1
        );

        playerState.Currency = Mathf.Max(0, playerState.Currency - currencyLoss);
        ShowNotification($"저주! {currencyLoss} 골드를 잃었습니다.");
    }

    private void HandleBlessingEvent()
    {
        int currencyGain = UnityEngine.Random.Range(
            mysteryConfig.blessingMinGoldGain,
            mysteryConfig.blessingMaxGoldGain + 1
        );

        playerState.Currency += currencyGain;
        ShowNotification($"축복! {currencyGain} 골드를 획득했습니다.");
    }
}
```

**장점:**
✅ 게임 디자이너가 코드 수정 없이 밸런싱 가능
✅ 값 검증 (OnValidate)
✅ Inspector 툴팁으로 설명
✅ 여러 프리셋 생성 가능

---

### 6.3 메서드 길이 및 복잡도

**심각도:** Low
**파일:** `DungeonManager.cs`

#### 문제

**위치:** `DungeonManager.cs:276-386` (110줄!)
```csharp
public void EnterRoom(RoomNode roomNode)
{
    // ... 110줄의 로직
}
```

**복잡도 지표:**
- 줄 수: 110줄
- 분기: ~15개
- 책임: 방 검증, 상태 업데이트, 타입별 처리, 이벤트 발생

#### 권장 리팩토링

**Single Responsibility Principle 적용**
```csharp
public void EnterRoom(RoomNode roomNode)
{
    // 1. 검증
    if (!ValidateRoom(roomNode))
    {
        Debug.LogError("Cannot enter invalid room");
        return;
    }

    // 2. 상태 업데이트
    UpdateRoomState(roomNode);

    // 3. 타입별 처리 (Strategy Pattern)
    ProcessRoom(roomNode);

    // 4. 이벤트 발생
    OnRoomEntered?.Invoke(roomNode);
}

private bool ValidateRoom(RoomNode roomNode)
{
    if (roomNode == null)
    {
        Debug.LogError("Room node is null");
        return false;
    }

    if (roomNode.Room == null)
    {
        Debug.LogError("Room data is null");
        return false;
    }

    return true;
}

private void UpdateRoomState(RoomNode roomNode)
{
    // 이전 방을 cleared로 마크
    if (currentRoomNode != null)
    {
        currentRoomNode.IsCleared = true;
    }

    currentRoomNode = roomNode;
}

private void ProcessRoom(RoomNode roomNode)
{
    switch (roomNode.Type)
    {
        case RoomType.Rest:
            ProcessRestRoom(roomNode);
            break;

        case RoomType.Treasure:
            ProcessTreasureRoom(roomNode);
            break;

        case RoomType.NormalCombat:
        case RoomType.EliteCombat:
        case RoomType.Boss:
        case RoomType.Start:
        case RoomType.Tutorial:
            ProcessCombatRoom(roomNode);
            break;

        case RoomType.Mystery:
            ProcessMysteryRoom(roomNode);
            break;

        default:
            Debug.LogError($"Unknown room type: {roomNode.Type}");
            break;
    }
}

private void ProcessCombatRoom(RoomNode roomNode)
{
    SetupBoard(roomNode.Room);
    RestorePlayerPieces();
    SetupEnemyPieces(roomNode.Room);
    ApplyArtifacts();
    StartCombat(roomNode.Room);
}

private void SetupBoard(RoomData roomData)
{
    if (gameBoard == null)
    {
        Debug.LogError("Game board is null!");
        return;
    }

    gameBoard.Clear();

    if (roomData.BoardData != null)
    {
        InitializeBoardWithData(roomData.BoardData);
    }
    else
    {
        gameBoard.Initialize(8, 8);  // Default
    }
}

private void InitializeBoardWithData(BoardData boardData)
{
    gameBoard.Initialize(boardData.Width, boardData.Height);

    // Place obstacles
    for (int y = 0; y < boardData.Height; y++)
    {
        for (int x = 0; x < boardData.Width; x++)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (boardData.GetTileType(pos) == TileType.Obstacle)
            {
                if (gameBoard.IsPositionValid(pos))
                {
                    gameBoard.SetObstacle(pos, true);
                }
            }
        }
    }
}

// ... 나머지 헬퍼 메서드들
```

**개선 결과:**
- EnterRoom: 110줄 → 20줄
- 각 메서드가 단일 책임
- 가독성 향상
- 테스트 용이
- 재사용 가능

---

## 7. 🔒 보안 및 데이터 무결성

### 7.1 치트 방지

**심각도:** Medium
**파일:** `SaveManager.cs`

이미 섹션 4.2에서 다룬 내용을 참조.

추가로:

#### API 통신 보안 (온라인 기능용)

```csharp
public class SecureAPIClient : MonoBehaviour
{
    private const string API_URL = "https://api.yourgame.com";
    private string apiKey;  // 서버에서 발급받은 키

    private void Awake()
    {
        // ⚠️ API 키를 코드에 하드코딩하지 말 것!
        // 런타임에 서버에서 받아오거나, 암호화된 파일에서 로드
        LoadAPIKey();
    }

    private void LoadAPIKey()
    {
        // 안전한 방법으로 API 키 로드
        // 예: PlayerPrefs 암호화, Keychain (iOS), Keystore (Android)
    }

    public async Task<bool> ValidateSaveWithServer(GameSaveData data)
    {
        using (UnityWebRequest request = new UnityWebRequest($"{API_URL}/validate", "POST"))
        {
            // 데이터 암호화
            string json = JsonConvert.SerializeObject(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // 헤더 설정
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-API-Key", apiKey);
            request.SetRequestHeader("X-Game-Version", Application.version);

            // 요청 전송
            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<ValidationResponse>(
                    request.downloadHandler.text
                );
                return response.IsValid;
            }

            return false;
        }
    }
}

[Serializable]
public class ValidationResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
}
```

---

### 7.2 입력 검증 및 살균

**심각도:** Low
**파일:** 여러 파일

#### 문제

사용자 입력(뮤테이션 이름, 플레이어 이름 등)에 대한 검증이 없음.

#### 권장사항

```csharp
public static class InputValidator
{
    private static readonly Regex ValidNamePattern = new Regex(@"^[a-zA-Z0-9가-힣s]{1,20}$");

    public static bool IsValidPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        if (name.Length > 20)
            return false;

        if (!ValidNamePattern.IsMatch(name))
            return false;

        // 금지어 체크
        if (ContainsProfanity(name))
            return false;

        return true;
    }

    public static string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // HTML 태그 제거
        input = Regex.Replace(input, @"<[^>]*>", string.Empty);

        // SQL 인젝션 방지 (DB 사용 시)
        input = input.Replace("'", "''");
        input = input.Replace("--", "");

        // 길이 제한
        if (input.Length > 100)
            input = input.Substring(0, 100);

        return input.Trim();
    }

    private static bool ContainsProfanity(string text)
    {
        // 금지어 목록 확인
        // 실제로는 외부 파일이나 서버에서 로드
        string[] profanityList = { "badword1", "badword2" };

        string lowerText = text.ToLower();
        foreach (var word in profanityList)
        {
            if (lowerText.Contains(word.ToLower()))
                return true;
        }

        return false;
    }
}

// 사용
public class PlayerProfile : MonoBehaviour
{
    public void SetPlayerName(string name)
    {
        if (!InputValidator.IsValidPlayerName(name))
        {
            Debug.LogWarning("Invalid player name");
            ShowError("플레이어 이름이 유효하지 않습니다.");
            return;
        }

        string sanitized = InputValidator.SanitizeString(name);
        // ... 저장
    }
}
```

---

## 8. 🧪 테스트 및 품질 보증

### 8.1 테스트 커버리지

**심각도:** Medium
**파일:** Tests 폴더

#### 현재 상태

**기존 테스트 파일:**
```
EditMode/
  ├── BoardGeneratorTests.cs
  ├── ChessEngineTests.cs
  ├── DungeonSystemTests.cs
  ├── MutationTests.cs
  └── PieceManagementTests.cs

PlayMode/
  ├── AITests.cs (14 tests) ⭐
  ├── DungeonRunTests.cs (2 tests)
  └── SystemIntegrationTests.cs
```

**통계:**
- 총 테스트: ~30-40개 (추정)
- 커버리지: ~30% (추정)
- AI 테스트: 우수 ⭐
- 기타: 기본적

#### 커버리지 부족 영역

**1. UI 컴포넌트** - 테스트 없음
- MainMenuUI
- GameOverScreen
- DungeonMapUI
- RewardSelectionUI
- 기타 15+ UI 클래스

**2. 저장/로드 시스템** - 테스트 없음
- SaveManager.SaveGame()
- SaveManager.LoadGame()
- 데이터 무결성
- 버전 호환성

**3. 아티팩트 시스템** - 테스트 없음
- 15+ 아티팩트
- ArtifactManager
- 아티팩트 효과 검증

**4. 뮤테이션** - 제한적
- 25+ 뮤테이션 중 일부만
- 뮤테이션 스택
- 호환성 검사

**5. 던전 생성** - 제한적
- DungeonMapGenerator
- 시드 결정성
- 경로 검증

#### 권장 테스트 추가

**저장/로드 테스트**
```csharp
[TestFixture]
public class SaveLoadTests
{
    private SaveManager saveManager;
    private string testSavePath;

    [SetUp]
    public void Setup()
    {
        saveManager = new GameObject().AddComponent<SaveManager>();
        testSavePath = Path.Combine(Application.temporaryCachePath, "test_save.json");
    }

    [TearDown]
    public void Teardown()
    {
        if (File.Exists(testSavePath))
        {
            File.Delete(testSavePath);
        }
        Object.Destroy(saveManager.gameObject);
    }

    [Test]
    public void SaveGame_ValidData_CreatesFile()
    {
        // Arrange
        var gameData = CreateTestGameData();

        // Act
        saveManager.SaveGame(gameData);

        // Assert
        Assert.IsTrue(File.Exists(testSavePath), "Save file should be created");
    }

    [Test]
    public void LoadGame_ValidSave_RestoresData()
    {
        // Arrange
        var originalData = CreateTestGameData();
        saveManager.SaveGame(originalData);

        // Act
        var loadedData = saveManager.LoadGame();

        // Assert
        Assert.IsNotNull(loadedData);
        Assert.AreEqual(originalData.DungeonSeed, loadedData.DungeonSeed);
        Assert.AreEqual(originalData.CurrentFloor, loadedData.CurrentFloor);
        Assert.AreEqual(originalData.Gold, loadedData.Gold);
    }

    [Test]
    public void LoadGame_CorruptedFile_ReturnsNull()
    {
        // Arrange
        File.WriteAllText(testSavePath, "corrupted data {{{");

        // Act
        var loadedData = saveManager.LoadGame();

        // Assert
        Assert.IsNull(loadedData, "Corrupted save should return null");
    }

    [Test]
    public void SaveGame_NullData_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => saveManager.SaveGame(null));
    }

    private GameSaveData CreateTestGameData()
    {
        return new GameSaveData
        {
            SaveDate = DateTime.Now.ToString(),
            DungeonSeed = 12345,
            CurrentFloor = 3,
            CurrentRoomIndex = 5,
            Gold = 150,
            PlayerTeam = Team.White
        };
    }
}
```

**아티팩트 효과 테스트**
```csharp
[TestFixture]
public class ArtifactEffectTests
{
    private Board board;
    private ArtifactManager artifactManager;

    [SetUp]
    public void Setup()
    {
        var boardObj = new GameObject("TestBoard");
        board = boardObj.AddComponent<Board>();
        board.Initialize(8, 8);

        artifactManager = boardObj.AddComponent<ArtifactManager>();
        artifactManager.SetBoard(board);
    }

    [Test]
    public void PhoenixFeatherArtifact_OnPieceCapture_RevivesPiece()
    {
        // Arrange
        var artifact = ScriptableObject.CreateInstance<PhoenixFeatherArtifact>();
        var piece = CreateTestPiece(PieceType.Queen, Team.White, new Vector2Int(3, 3));
        board.PlacePiece(piece, piece.Position);
        artifactManager.AddArtifact(artifact);

        // Act
        board.RemovePiece(piece.Position);

        // Simulate 3 turns passing
        for (int i = 0; i < 3; i++)
        {
            artifactManager.NotifyTurnEnd(Team.White, i);
        }

        // Assert
        var revivedPiece = board.GetPiece(piece.Position);
        Assert.IsNotNull(revivedPiece, "Piece should be revived after 3 turns");
        Assert.AreEqual(PieceType.Queen, revivedPiece.Type);
    }

    [Test]
    public void TimeWarpArtifact_OnTurnEnd_AllowsExtraTurn()
    {
        // Arrange
        var artifact = ScriptableObject.CreateInstance<TimeWarpArtifact>();
        artifactManager.AddArtifact(artifact);

        int turnCount = 0;
        int extraTurns = 0;

        // Act
        for (int i = 0; i < 10; i++)
        {
            var context = new ArtifactContext { TurnNumber = i, CurrentTeam = Team.White };
            artifactManager.TriggerArtifacts(ArtifactTrigger.OnTurnEnd, context);

            turnCount++;
            // Check if artifact granted extra turn (implementation dependent)
        }

        // Assert
        Assert.Greater(extraTurns, 0, "TimeWarp should grant at least one extra turn");
    }
}
```

**뮤테이션 호환성 테스트**
```csharp
[TestFixture]
public class MutationCompatibilityTests
{
    [Test]
    public void ApplyMutation_IncompatiblePieceType_LogsWarning()
    {
        // Arrange
        var mutation = ScriptableObject.CreateInstance<BerserkQueenMutation>();
        var pawn = CreateTestPiece(PieceType.Pawn);  // Queen 뮤테이션을 Pawn에 적용

        // Act
        MutationManager.Instance.ApplyMutation(pawn, mutation);

        // Assert
        Assert.IsFalse(MutationManager.Instance.HasMutation(pawn, mutation),
            "Incompatible mutation should not be applied");
    }

    [Test]
    public void ApplyMutation_Stacking_RespectsMaxStacks()
    {
        // Arrange
        var stackableMutation = ScriptableObject.CreateInstance<SomeStackableMutation>();
        stackableMutation.MaxStacks = 3;
        var piece = CreateTestPiece(PieceType.Knight);

        // Act
        for (int i = 0; i < 5; i++)  // 5번 적용 시도
        {
            MutationManager.Instance.ApplyMutation(piece, stackableMutation);
        }

        // Assert
        var stackCount = MutationManager.Instance.GetStackCount(piece, stackableMutation);
        Assert.AreEqual(3, stackCount, "Stack count should not exceed MaxStacks");
    }
}
```

---

### 8.2 통합 테스트

**심각도:** Medium

#### 권장 시나리오

**1. 전체 게임 흐름 테스트**
```csharp
[UnityTest]
public IEnumerator FullGameFlow_StartToVictory()
{
    // Arrange
    var gameManager = Object.FindFirstObjectByType<GameManager>();
    var dungeonManager = Object.FindFirstObjectByType<DungeonManager>();

    // Act
    dungeonManager.StartNewRun();
    yield return new WaitForSeconds(1f);

    // 첫 번째 방 진입
    var firstRoom = dungeonManager.CurrentMap.StartNode;
    dungeonManager.EnterRoom(firstRoom);
    yield return new WaitForSeconds(1f);

    // 게임 플레이 (AI vs AI)
    gameManager.SetPlayerTeam(Team.White);
    gameManager.StartGame();

    // 승리 조건까지 진행 (타임아웃 설정)
    float timeout = 60f;
    float elapsed = 0f;
    while (gameManager.State != GameManager.GameState.Victory && elapsed < timeout)
    {
        yield return new WaitForSeconds(0.1f);
        elapsed += 0.1f;
    }

    // Assert
    Assert.AreNotEqual(GameManager.GameState.NotStarted, gameManager.State,
        "Game should have started");
    Assert.Less(elapsed, timeout, "Game should complete within timeout");
}
```

**2. 저장/로드 사이클 테스트**
```csharp
[UnityTest]
public IEnumerator SaveLoadCycle_PreservesState()
{
    // Arrange
    var dungeonManager = Object.FindFirstObjectByType<DungeonManager>();
    var saveManager = Object.FindFirstObjectByType<SaveManager>();

    dungeonManager.StartNewRun();
    yield return new WaitForSeconds(1f);

    // 몇 턴 진행
    for (int i = 0; i < 5; i++)
    {
        // ... 턴 진행
        yield return new WaitForSeconds(0.5f);
    }

    // 현재 상태 기록
    int originalFloor = dungeonManager.CurrentFloor;
    int originalGold = dungeonManager.PlayerState.Currency;
    int originalRoomIndex = dungeonManager.CurrentRoomIndex;

    // Act: 저장
    saveManager.SaveGame();
    yield return null;

    // 씬 리셋 (실제로는 씬 재로드)
    dungeonManager.Reset();
    yield return null;

    // 로드
    var saveData = saveManager.LoadGame();
    dungeonManager.LoadRun(saveData);
    yield return new WaitForSeconds(1f);

    // Assert
    Assert.AreEqual(originalFloor, dungeonManager.CurrentFloor);
    Assert.AreEqual(originalGold, dungeonManager.PlayerState.Currency);
    Assert.AreEqual(originalRoomIndex, dungeonManager.CurrentRoomIndex);
}
```

---

### 8.3 성능 벤치마크

**심각도:** Low

#### 권장 테스트

```csharp
[TestFixture]
public class PerformanceBenchmarks
{
    [Test, Performance]
    public void Benchmark_BoardClone()
    {
        // Arrange
        var board = CreateStandardBoard();

        // Act
        Measure.Method(() => {
            var cloned = board.Clone();
            Object.Destroy(cloned.gameObject);
        })
        .WarmupCount(10)
        .MeasurementCount(100)
        .Run();

        // 목표: < 5ms per clone
    }

    [Test, Performance]
    public void Benchmark_AIDecision_Depth3()
    {
        // Arrange
        var board = CreateComplexBoard();
        var ai = CreateTestAI(depth: 3);

        // Act
        Measure.Method(() => {
            ai.MakeMove(board);
        })
        .WarmupCount(5)
        .MeasurementCount(20)
        .Run();

        // 목표: < 500ms per move
    }

    [Test, Performance]
    public void Benchmark_FindObjectOfType()
    {
        // 성능 비교: FindObjectOfType vs 직접 참조
        CreateTestScene(objectCount: 100);

        // FindObjectOfType
        Measure.Method(() => {
            var manager = Object.FindFirstObjectByType<DungeonManager>();
        })
        .WarmupCount(10)
        .MeasurementCount(100)
        .SampleGroup("FindObjectOfType")
        .Run();

        // 직접 참조
        DungeonManager cachedManager = Object.FindFirstObjectByType<DungeonManager>();
        Measure.Method(() => {
            var manager = cachedManager;
        })
        .WarmupCount(10)
        .MeasurementCount(100)
        .SampleGroup("DirectReference")
        .Run();

        // FindObjectOfType는 100배 이상 느림을 증명
    }
}
```

---

## 9. 📂 구체적 파일별 이슈 상세

### 9.1 DungeonManager.cs

**파일 크기:** 769줄
**복잡도:** 높음
**이슈 수:** 8개

#### 이슈 1: God Object
**줄:** 전체
**심각도:** High

**문제:**
너무 많은 책임:
- 던전 맵 관리
- 룸 전환
- 플레이어 상태 관리
- UI 제어
- 보드 설정
- 리워드 처리
- 적 생성
- 아티팩트 적용

**권장:**
책임 분리
```
DungeonManager       → 전체 조율
RoomTransitionManager → 룸 전환
PlayerStateManager   → 플레이어 상태
RewardManager        → 리워드 처리
EnemySpawner         → 적 생성
```

#### 이슈 2: FindObjectOfType 과다
**줄:** 134-143
**심각도:** Critical

이미 섹션 2.1에서 다룸.

#### 이슈 3: 매직 넘버
**줄:** 236, 426-445, 453, 466
**심각도:** Low

이미 섹션 6.2에서 다룸.

---

### 9.2 Piece.cs

**파일 크기:** 172줄
**복잡도:** 중간
**이슈 수:** 2개

#### 이슈 1: PromoteToQueen 제한
**줄:** 91-105
**심각도:** Medium

**문제:**
퀸으로만 승급 가능

**권장:**
```csharp
public void Promote(PieceType targetType)
{
    if (pieceType != PieceType.Pawn)
    {
        Debug.LogWarning("Only pawns can be promoted!");
        return;
    }

    if (targetType == PieceType.King || targetType == PieceType.Pawn)
    {
        Debug.LogWarning("Cannot promote to King or Pawn!");
        return;
    }

    pieceType = targetType;
    movementRules.Clear();

    var factory = MovementRuleFactory.Instance;
    var rules = factory.GetRulesForPieceType(targetType);

    foreach (var rule in rules)
    {
        AddMovementRule(rule);
    }

    Debug.Log($"{team} Pawn promoted to {targetType}!");
}
```

#### 이슈 2: ScriptableObject 생성
**줄:** 97-102
**심각도:** High

이미 섹션 2.2에서 다룸.

---

### 9.3 ChessAI.cs

**파일 크기:** 330줄
**복잡도:** 높음 (알고리즘)
**이슈 수:** 1개

#### 긍정적 평가 ⭐
- 잘 구현된 Minimax
- Alpha-Beta Pruning
- Time limit 처리
- Iterative Deepening

#### 이슈: 최적화 기회
**줄:** 전체
**심각도:** Low

이미 섹션 2.4에서 다룸.

---

### 9.4 MutationManager.cs

**파일 크기:** 280줄
**복잡도:** 중간
**이슈 수:** 2개

#### 이슈 1: 메모리 누수 가능성
**줄:** 37-40
**심각도:** High

**문제:**
Piece가 파괴되어도 Dictionary에서 제거 안 됨

**권장:**
```csharp
// Piece.cs OnDestroy에서 호출
private void OnDestroy()
{
    if (MutationManager.Instance != null)
    {
        MutationManager.Instance.OnPieceDestroyed(this);
    }
}

// MutationManager.cs에 추가
public void OnPieceDestroyed(Piece piece)
{
    if (pieceMutations.ContainsKey(piece))
    {
        // 모든 뮤테이션 제거
        ClearMutations(piece);

        // Dictionary에서 제거
        pieceMutations.Remove(piece);
        mutationStacks.Remove(piece);
    }
}
```

#### 이슈 2: 스택 카운트 검증 미흡
**줄:** 111-124
**심각도:** Medium

**문제:**
```csharp
int currentStacks = mutationStacks[piece][mutation];  // KeyNotFoundException 가능
```

**권장:**
```csharp
if (mutationStacks[piece].TryGetValue(mutation, out int currentStacks))
{
    if (currentStacks < mutation.MaxStacks)
    {
        mutationStacks[piece][mutation]++;
        Debug.Log($"Stacked '{mutation.MutationName}' (Stack: {currentStacks + 1})");
    }
}
```

---

### 9.5 AdvancedMutations.cs

**파일 크기:** 332줄
**복잡도:** 중간
**이슈 수:** 1개

#### 이슈: ScriptableObject 누수
**줄:** 20
**심각도:** High

이미 섹션 2.2에서 다룸.

---

## 10. ✨ 긍정적 측면 (Highlights)

### 1. 아키텍처 패턴 활용 ⭐⭐⭐

**Strategy Pattern** - MovementRule 시스템
- 런타임 룰 변경
- 뮤테이션 핵심
- 확장성 뛰어남

**ScriptableObject** - 데이터 주도 설계
- AIConfig
- Mutation
- Artifact
- MovementRule

### 2. AI 구현 ⭐⭐⭐

**Minimax + Alpha-Beta Pruning**
- 표준 체스 AI 알고리즘
- 시간 제한 처리
- Iterative Deepening
- 뮤테이션 지원

### 3. 문서화 ⭐⭐

**XML 주석**
- 대부분의 public API
- 파라미터 설명
- 반환값 설명

### 4. 이벤트 시스템 ⭐⭐

**UnityEvent 활용**
- UI/로직 분리
- 느슨한 결합
- Inspector 연결

### 5. 테스트 프레임워크 ⭐

**AI 테스트**
- 14개 테스트 메서드
- 다양한 시나리오
- 성능 테스트

### 6. MovementRuleFactory ⭐⭐

**메모리 관리**
- 규칙 캐싱
- 재사용
- 명시적 정리

---

## 11. 📊 우선순위별 액션 아이템

### 🔴 Critical (즉시 수정 - 1-2주)

1. **FindObjectOfType 제거**
   - 파일: 17개
   - 예상 시간: 1주
   - 방법: ServiceLocator 도입

2. **ScriptableObject 메모리 누수 수정**
   - 파일: AdvancedMutations.cs, Piece.cs
   - 예상 시간: 2일
   - 방법: MovementRuleFactory 사용

3. **Board.Clone() 최적화**
   - 파일: Board.cs, ChessAI.cs
   - 예상 시간: 3일
   - 방법: BoardState 구조체 도입

4. **저장 시스템 완성**
   - 파일: SaveManager.cs, DungeonManager.cs
   - 예상 시간: 3일
   - 방법: 시드 저장, 검증 추가

### 🟠 High (빠른 시일 내 - 2-4주)

5. **싱글톤 개선**
   - 파일: 17개 매니저
   - 예상 시간: 1주
   - 방법: 이벤트 시스템 또는 DI

6. **Null 참조 체크 강화**
   - 파일: 다수
   - 예상 시간: 1주
   - 방법: Nullable Reference Types

7. **MutationManager 메모리 관리**
   - 파일: MutationManager.cs, Piece.cs
   - 예상 시간: 2일
   - 방법: OnDestroy 훅

8. **에러 처리 일관성**
   - 파일: 다수
   - 예상 시간: 1주
   - 방법: Result 패턴

### 🟡 Medium (점진적 개선 - 1-2개월)

9. **테스트 커버리지 확대**
   - 목표: 30% → 70%
   - 예상 시간: 2주
   - 영역: 저장/로드, 아티팩트, UI

10. **코드 리팩토링**
    - 파일: DungeonManager.cs
    - 예상 시간: 1주
    - 방법: 책임 분리

11. **로깅 시스템 도입**
    - 예상 시간: 3일
    - 방법: GameLogger 클래스

12. **입력 시스템 개선**
    - 파일: BoardInputHandler.cs
    - 예상 시간: 1주
    - 방법: New Input System

### 🟢 Low (선택적 - 장기)

13. **AI 추가 최적화**
    - Transposition Table
    - Move Ordering
    - Quiescence Search

14. **암호화 저장**
    - 치트 방지
    - 체크섬 검증

15. **매직 넘버 제거**
    - ScriptableObject Config

16. **주석 개선**
    - 예제 코드 추가

---

## 12. 📈 개선 로드맵

### Phase 1: 안정화 (1개월)
- Critical 이슈 해결
- 핵심 시스템 안정화
- 기본 테스트 추가

**목표:**
- FindObjectOfType 제거 100%
- 메모리 누수 수정 100%
- 저장/로드 완성

### Phase 2: 최적화 (1개월)
- High 이슈 해결
- 성능 개선
- 테스트 커버리지 향상

**목표:**
- 성능 50% 향상
- 테스트 커버리지 50%+
- 에러 처리 표준화

### Phase 3: 고도화 (1-2개월)
- Medium 이슈 해결
- 코드 품질 향상
- 문서화 개선

**목표:**
- 테스트 커버리지 70%+
- 코드 리팩토링 완료
- 로깅 시스템 도입

### Phase 4: 완성도 (지속적)
- Low 이슈 해결
- 고급 기능 추가
- 유지보수성 향상

**목표:**
- AI 최적화
- 보안 강화
- 완벽한 문서화

---

## 13. 📚 추천 리소스

### Unity Best Practices
- [Unity Manual - Best Practices](https://docs.unity3d.com/Manual/BestPracticeGuides.html)
- [Unite Talks - Performance Optimization](https://www.youtube.com/playlist?list=PLX2vGYjWbI0R_X4FZ3AWShp8zqtW_pRt4)

### Design Patterns
- "Game Programming Patterns" by Robert Nystrom
- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns)

### Testing
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- "The Art of Unit Testing" by Roy Osherove

### 성능 최적화
- [Unity Profiler](https://docs.unity3d.com/Manual/Profiler.html)
- [Memory Profiler](https://docs.unity3d.com/Packages/com.unity.memoryprofiler@latest)

---

## 14. 🎯 결론

MutatingGambit는 **혁신적인 게임 컨셉**과 **견고한 기술 기반**을 가진 프로젝트입니다. 코드 품질은 전반적으로 **양호**하며, 특히 아키텍처 패턴 활용과 AI 구현이 우수합니다.

### 최종 평가

**종합 점수: 7.2/10 (B+)**

| 영역 | 점수 | 비고 |
|------|------|------|
| 아키텍처 | 8/10 | Strategy 패턴 우수 |
| 성능 | 5/10 | 최적화 필요 (Critical) |
| 버그/안정성 | 6/10 | Null 체크 강화 필요 |
| 저장/로드 | 4/10 | 불완전 (High) |
| Unity 활용 | 8/10 | 적절한 패턴 사용 |
| 코드 품질 | 7/10 | 문서화 우수 |
| 보안 | 6/10 | 치트 방지 미흡 |
| 테스트 | 5/10 | 커버리지 부족 |

### 주요 강점
1. ⭐⭐⭐ 전략 패턴 기반 유연한 설계
2. ⭐⭐⭐ 잘 구현된 AI 시스템
3. ⭐⭐ ScriptableObject 활용
4. ⭐⭐ 상세한 XML 문서화

### 주요 약점
1. ⚠️⚠️⚠️ 과도한 FindObjectOfType (성능)
2. ⚠️⚠️ ScriptableObject 메모리 누수
3. ⚠️⚠️ 싱글톤 남용
4. ⚠️ 테스트 커버리지 부족

### 권장 우선순위

**즉시 (1-2주):**
1. FindObjectOfType 제거
2. 메모리 누수 수정
3. Board.Clone() 최적화
4. 저장 시스템 완성

**단기 (1개월):**
5. 싱글톤 개선
6. Null 체크 강화
7. 테스트 추가 (50%)

**중기 (2-3개월):**
8. 코드 리팩토링
9. 테스트 확대 (70%)
10. 문서화 개선

### 마무리

이러한 개선사항들을 **점진적으로** 적용하면, 프로젝트의 안정성과 유지보수성이 **크게 향상**될 것입니다. 특히 **Critical 이슈들을 우선 해결**하는 것이 중요합니다.

프로젝트는 이미 **훌륭한 기반**을 가지고 있으며, 제시된 개선사항들을 반영하면 **프로덕션 레디** 수준에 도달할 수 있습니다.

---

**리뷰 작성자:** Claude AI Code Reviewer
**리뷰 날짜:** 2025년 11월 28일
**검토 파일 수:** 100+ 파일
**코드 라인 수:** ~10,000+ 줄
**발견 이슈:** Critical: 4, High: 8, Medium: 12, Low: 8
**총 이슈:** 32개
**긍정적 패턴:** 6개 주요 영역

---

**다음 리뷰:** 1개월 후 (Phase 1 완료 시)

**문의:** 추가 질문이나 특정 영역에 대한 심층 분석이 필요하시면 언제든 요청해주세요.
