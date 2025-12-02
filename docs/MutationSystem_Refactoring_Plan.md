# Mutation 시스템 전체 리팩터링 계획

## 문서 정보
- **작성일**: 2025-12-02
- **대상 시스템**: Assets/Scripts/Systems/Mutations
- **목표**: 코드 품질, 유지보수성, 확장성 향상
- **예상 기간**: 4주 (Phase 1-4)

---

## 목차
1. [개요](#개요)
2. [Phase 1: 긴급 버그 수정](#phase-1-긴급-버그-수정)
3. [Phase 2: 구조 개선](#phase-2-구조-개선)
4. [Phase 3: 코드 품질 향상](#phase-3-코드-품질-향상)
5. [Phase 4: 기능 확장 및 최적화](#phase-4-기능-확장-및-최적화)
6. [테스트 전략](#테스트-전략)
7. [롤백 계획](#롤백-계획)

---

## 개요

### 현재 시스템 구조
```
Mutations/
├── Mutation.cs (기본 클래스)
├── MutationManager.cs (Singleton, 주요 관리자)
├── MutationApplicator.cs (얇은 래퍼)
├── MutationLibrary.cs (Mutation 목록 관리)
├── MutationHelpers.cs (유틸리티 + 확장 메서드)
├── AdvancedMutations.cs (고급 Mutation들)
└── InitialMutations/
    └── DoubleMoveRookMutation.cs (초기 Mutation 예제)
```

### 주요 문제점
1. **치명적 버그**: Singleton 무한 재귀, 상태 관리 오류
2. **구조적 문제**: 역할 중복, 불필요한 클래스
3. **구현 미완성**: 빈 메서드들, 누락된 로직
4. **유지보수성**: 중복 코드, 하드코딩, 문서화 부족

### 목표 시스템 구조
```
Mutations/
├── Core/
│   ├── Mutation.cs (기본 클래스)
│   ├── MutationState.cs (NEW - 상태 관리)
│   └── IMutationEffect.cs (NEW - 효과 인터페이스)
├── Management/
│   ├── MutationManager.cs (통합 관리자)
│   ├── MutationStateTracker.cs (NEW - 상태 추적)
│   └── MutationConfig.cs (NEW - 설정 ScriptableObject)
├── Library/
│   ├── MutationLibrary.cs (목록 관리)
│   └── MutationQueryService.cs (NEW - 조회 서비스)
├── Implementations/
│   ├── BasicMutations.cs (기본 Mutations)
│   ├── AdvancedMutations.cs (고급 Mutations - 구현 완성)
│   └── SpecialMutations.cs (NEW - 특수 Mutations)
└── Utilities/
    └── MutationUtilities.cs (정리된 유틸리티)
```

---

## Phase 1: 긴급 버그 수정

### 목표
치명적인 버그를 수정하여 시스템 안정화

### 기간
1주 (Week 1)

### 작업 항목

#### 1.1 MutationManager Singleton 무한 재귀 수정
**파일**: `MutationManager.cs`

**현재 코드** (Line 28):
```csharp
if (instance == null)
{
    instance = MutationManager.Instance; // 무한 재귀!
}
```

**수정 코드**:
```csharp
if (instance == null)
{
    instance = FindFirstObjectByType<MutationManager>();
    if (instance == null)
    {
        GameObject go = new GameObject("MutationManager");
        instance = go.AddComponent<MutationManager>();
        DontDestroyOnLoad(go);
    }
}
```

**테스트**:
- [ ] Singleton 생성 테스트
- [ ] 중복 생성 방지 확인
- [ ] Scene 전환 시 유지 확인

---

#### 1.2 ScriptableObject 상태 관리 버그 수정
**파일**: `DoubleMoveRookMutation.cs`, `AdvancedMutations.cs`

**문제**: ScriptableObject는 에셋이므로 상태를 저장하면 모든 인스턴스가 공유됨

**해결 방안**: MutationState 클래스 도입

**새 파일**: `Mutations/Core/MutationState.cs`
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Piece별 Mutation 상태를 관리하는 클래스
    /// ScriptableObject에서 상태를 분리하여 인스턴스별 관리
    /// </summary>
    public class MutationState
    {
        public Mutation Mutation { get; private set; }
        public Dictionary<string, object> Data { get; private set; }

        public MutationState(Mutation mutation)
        {
            Mutation = mutation;
            Data = new Dictionary<string, object>();
        }

        public T GetData<T>(string key, T defaultValue = default)
        {
            if (Data.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        public void SetData<T>(string key, T value)
        {
            Data[key] = value;
        }

        public bool HasData(string key)
        {
            return Data.ContainsKey(key);
        }
    }
}
```

**MutationManager 수정**:
```csharp
// 추가 필드
private Dictionary<Piece, List<MutationState>> pieceMutationStates
    = new Dictionary<Piece, List<MutationState>>();

// 기존 pieceMutations를 pieceMutationStates로 대체
```

**DoubleMoveRookMutation 수정**:
```csharp
// 기존: private bool hasBonusMove = false; (삭제)

// 수정된 ApplyToPiece
public override void ApplyToPiece(Piece piece)
{
    if (piece == null) return;

    // MutationState를 통해 상태 관리
    var state = MutationManager.Instance.GetMutationState(piece, this);
    state.SetData("hasBonusMove", false);

    // 기존 로직
    piece.onMoveCompleted.AddListener(() => OnRookMoved(piece, state));
}

private void OnRookMoved(Piece piece, MutationState state)
{
    if (!state.GetData("hasBonusMove", false))
    {
        state.SetData("hasBonusMove", true);
        // 보너스 이동 로직
    }
}
```

**테스트**:
- [ ] 여러 Piece에 동일 Mutation 적용 시 독립적 상태 확인
- [ ] Scene 재로드 후 상태 초기화 확인
- [ ] 상태 저장/로드 정상 작동 확인

---

#### 1.3 MovementRule 메모리 누수 방지
**파일**: `MutationManager.cs`, `AdvancedMutations.cs`

**문제**: MovementRule을 생성하지만 추적하지 않아 제거 시 정리되지 않음

**해결 방안**: MovementRule 추적 시스템 구축

**MutationState 확장**:
```csharp
public class MutationState
{
    // 기존 코드...

    public List<MovementRule> AddedRules { get; private set; }
        = new List<MovementRule>();

    public void TrackRule(MovementRule rule)
    {
        if (!AddedRules.Contains(rule))
            AddedRules.Add(rule);
    }
}
```

**Mutation 기본 클래스 수정**:
```csharp
public abstract class Mutation : ScriptableObject
{
    // Helper 메서드 추가
    protected void AddAndTrackRule(Piece piece, MovementRule rule, MutationState state)
    {
        if (piece != null && rule != null)
        {
            piece.movementRules.Add(rule);
            state.TrackRule(rule);
        }
    }

    protected void RemoveTrackedRules(Piece piece, MutationState state)
    {
        if (piece != null && state != null)
        {
            foreach (var rule in state.AddedRules)
            {
                piece.movementRules.Remove(rule);
            }
            state.AddedRules.Clear();
        }
    }
}
```

**AdvancedMutations 적용 예시**:
```csharp
public class BackwardPawnMutation : Mutation
{
    public override void ApplyToPiece(Piece piece)
    {
        var state = MutationManager.Instance.GetMutationState(piece, this);
        var rule = ScriptableObject.CreateInstance<BackwardPawnRule>();
        AddAndTrackRule(piece, rule, state);
    }

    public override void RemoveFromPiece(Piece piece)
    {
        var state = MutationManager.Instance.GetMutationState(piece, this);
        RemoveTrackedRules(piece, state);
    }
}
```

**테스트**:
- [ ] Rule 추가 후 추적 목록에 존재 확인
- [ ] Mutation 제거 시 Rule도 제거 확인
- [ ] 메모리 프로파일러로 누수 없음 확인

---

### Phase 1 완료 기준
- [ ] 모든 치명적 버그 수정 완료
- [ ] 단위 테스트 통과 (커버리지 80% 이상)
- [ ] 기존 기능 정상 작동 확인
- [ ] 코드 리뷰 완료

---

## Phase 2: 구조 개선

### 목표
시스템 아키텍처를 명확히 하고 불필요한 복잡성 제거

### 기간
1주 (Week 2)

### 작업 항목

#### 2.1 MutationApplicator 제거 및 MutationManager 통합
**삭제 파일**: `MutationApplicator.cs`

**MutationManager.cs 통합**:
```csharp
public class MutationManager : MonoBehaviour
{
    // MutationApplicator의 기능 직접 통합

    #region Mutation Application

    public void ApplyMutation(Piece piece, Mutation mutation)
    {
        if (piece == null || mutation == null)
        {
            Debug.LogWarning("Cannot apply null mutation or to null piece");
            return;
        }

        // 상태 생성
        var state = GetOrCreateMutationState(piece, mutation);

        // Mutation 적용
        mutation.ApplyToPiece(piece);

        // 리스트에 추가
        if (!pieceMutationStates.ContainsKey(piece))
            pieceMutationStates[piece] = new List<MutationState>();

        if (!pieceMutationStates[piece].Contains(state))
            pieceMutationStates[piece].Add(state);

        // 이벤트 발생
        OnMutationApplied?.Invoke(piece, mutation);
    }

    public void RemoveMutation(Piece piece, Mutation mutation)
    {
        if (piece == null || mutation == null) return;

        var state = GetMutationState(piece, mutation);
        if (state == null) return;

        // Mutation 제거
        mutation.RemoveFromPiece(piece);

        // 상태에서 제거
        pieceMutationStates[piece].Remove(state);

        // 이벤트 발생
        OnMutationRemoved?.Invoke(piece, mutation);
    }

    #endregion
}
```

**영향 받는 파일 업데이트**:
- 모든 `MutationApplicator` 참조를 `MutationManager`로 변경

**테스트**:
- [ ] Mutation 적용/제거 정상 작동
- [ ] 이벤트 발생 확인
- [ ] 기존 코드 호환성 확인

---

#### 2.2 중복 메서드 제거
**파일**: `MutationManager.cs`

**제거할 메서드**: `GetMutations` (103-111번 줄)

**유지할 메서드**: `GetMutationsForPiece` (113-118번 줄)을 개선

```csharp
/// <summary>
/// 특정 Piece에 적용된 모든 Mutation을 반환합니다
/// </summary>
public List<Mutation> GetMutationsForPiece(Piece piece)
{
    if (piece == null)
    {
        Debug.LogWarning("Cannot get mutations for null piece");
        return new List<Mutation>();
    }

    if (!pieceMutationStates.TryGetValue(piece, out var states))
        return new List<Mutation>();

    return states.Select(s => s.Mutation).ToList();
}

/// <summary>
/// 특정 Piece의 MutationState 목록을 반환합니다
/// </summary>
public List<MutationState> GetMutationStatesForPiece(Piece piece)
{
    if (piece == null) return new List<MutationState>();

    if (!pieceMutationStates.TryGetValue(piece, out var states))
        return new List<MutationState>();

    return new List<MutationState>(states);
}
```

**MutationHelpers.cs 업데이트**:
```csharp
// GetMutations 호출을 GetMutationsForPiece로 변경
public static List<Mutation> GetMutationsForPiece(this MutationManager manager, Piece piece)
{
    return manager.GetMutationsForPiece(piece);
}
```

**테스트**:
- [ ] 모든 조회 기능 정상 작동
- [ ] null 처리 확인
- [ ] 성능 저하 없음 확인

---

#### 2.3 MutationConfig ScriptableObject 생성
**새 파일**: `Mutations/Management/MutationConfig.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace MutatingGambit.Systems.Mutations
{
    /// <summary>
    /// Mutation 시스템의 중앙 설정을 관리하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MutationConfig", menuName = "MutatingGambit/Mutation Config")]
    public class MutationConfig : ScriptableObject
    {
        [Header("Rarity Weights")]
        [Tooltip("Common 등급 Mutation의 가중치")]
        public float commonWeight = 100f;

        [Tooltip("Uncommon 등급 Mutation의 가중치")]
        public float uncommonWeight = 50f;

        [Tooltip("Rare 등급 Mutation의 가중치")]
        public float rareWeight = 25f;

        [Tooltip("Epic 등급 Mutation의 가중치")]
        public float epicWeight = 10f;

        [Tooltip("Legendary 등급 Mutation의 가중치")]
        public float legendaryWeight = 5f;

        [Header("Rarity Colors")]
        public Color commonColor = Color.white;
        public Color uncommonColor = Color.green;
        public Color rareColor = Color.blue;
        public Color epicColor = new Color(0.5f, 0f, 0.5f); // Purple
        public Color legendaryColor = new Color(1f, 0.5f, 0f); // Orange

        [Header("System Settings")]
        [Tooltip("Piece당 최대 Mutation 개수")]
        public int maxMutationsPerPiece = 5;

        [Tooltip("Mutation 시너지 활성화 여부")]
        public bool enableSynergies = true;

        [Tooltip("디버그 로그 출력 여부")]
        public bool enableDebugLogs = false;

        /// <summary>
        /// 등급에 따른 가중치를 반환합니다
        /// </summary>
        public float GetWeightForRarity(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Common => commonWeight,
                MutationRarity.Uncommon => uncommonWeight,
                MutationRarity.Rare => rareWeight,
                MutationRarity.Epic => epicWeight,
                MutationRarity.Legendary => legendaryWeight,
                _ => commonWeight
            };
        }

        /// <summary>
        /// 등급에 따른 색상을 반환합니다
        /// </summary>
        public Color GetColorForRarity(MutationRarity rarity)
        {
            return rarity switch
            {
                MutationRarity.Common => commonColor,
                MutationRarity.Uncommon => uncommonColor,
                MutationRarity.Rare => rareColor,
                MutationRarity.Epic => epicColor,
                MutationRarity.Legendary => legendaryColor,
                _ => commonColor
            };
        }
    }
}
```

**MutationManager에 Config 참조 추가**:
```csharp
public class MutationManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private MutationConfig config;

    public MutationConfig Config => config;

    private void Awake()
    {
        if (config == null)
        {
            Debug.LogError("MutationConfig not assigned! Creating default...");
            config = ScriptableObject.CreateInstance<MutationConfig>();
        }
    }
}
```

**MutationHelpers 업데이트**:
```csharp
public static float GetRarityWeight(MutationRarity rarity)
{
    var config = MutationManager.Instance?.Config;
    if (config != null)
        return config.GetWeightForRarity(rarity);

    // Fallback to hardcoded values
    return rarity switch
    {
        MutationRarity.Common => 100f,
        // ... (기존 하드코딩 값 유지)
    };
}

public static Color GetRarityColor(MutationRarity rarity)
{
    var config = MutationManager.Instance?.Config;
    if (config != null)
        return config.GetColorForRarity(rarity);

    // Fallback
    return Color.white;
}
```

**에셋 생성**:
- Resources/MutationConfig.asset 생성
- 기본 값 설정

**테스트**:
- [ ] Config 로드 확인
- [ ] 가중치/색상 적용 확인
- [ ] Inspector에서 실시간 수정 반영 확인

---

#### 2.4 폴더 구조 재정리
**변경 전**:
```
Mutations/
├── Mutation.cs
├── MutationManager.cs
├── MutationApplicator.cs (삭제됨)
├── MutationLibrary.cs
├── MutationHelpers.cs
├── AdvancedMutations.cs
└── InitialMutations/
```

**변경 후**:
```
Mutations/
├── Core/
│   ├── Mutation.cs
│   ├── MutationState.cs
│   └── MutationRarity.cs (enum 분리)
├── Management/
│   ├── MutationManager.cs
│   └── MutationConfig.cs
├── Library/
│   └── MutationLibrary.cs
├── Utilities/
│   └── MutationUtilities.cs (MutationHelpers 개명)
└── Implementations/
    ├── BasicMutations/ (InitialMutations 개명)
    │   └── DoubleMoveRookMutation.cs
    └── AdvancedMutations.cs
```

**마이그레이션 스크립트**:
```bash
# Git을 통한 파일 이동 (히스토리 유지)
git mv Mutation.cs Core/Mutation.cs
git mv MutationState.cs Core/MutationState.cs
git mv MutationManager.cs Management/MutationManager.cs
# ...
```

**네임스페이스 업데이트**:
```csharp
// 기존
namespace MutatingGambit.Systems.Mutations

// 세부 구분
namespace MutatingGambit.Systems.Mutations.Core
namespace MutatingGambit.Systems.Mutations.Management
namespace MutatingGambit.Systems.Mutations.Library
```

**테스트**:
- [ ] 모든 참조 정상 작동
- [ ] 빌드 성공
- [ ] Git 히스토리 유지 확인

---

### Phase 2 완료 기준
- [ ] 구조 개선 완료
- [ ] 모든 참조 업데이트 완료
- [ ] 단위 테스트 통과
- [ ] 문서 업데이트 완료

---

## Phase 3: 코드 품질 향상

### 목표
구현 완성, 문서화, 코드 품질 개선

### 기간
1주 (Week 3)

### 작업 항목

#### 3.1 AdvancedMutations 구현 완성
**파일**: `AdvancedMutations.cs`

각 Mutation의 빈 메서드를 완전히 구현하거나 명시적으로 미구현 표시

**구현 예시**: BackwardPawnMutation
```csharp
/// <summary>
/// 폰이 뒤로도 이동할 수 있게 하는 Mutation
/// </summary>
[CreateAssetMenu(fileName = "BackwardPawnMutation", menuName = "MutatingGambit/Mutations/Advanced/Backward Pawn")]
public class BackwardPawnMutation : Mutation
{
    public override void ApplyToPiece(Piece piece)
    {
        if (piece == null || piece.Type != PieceType.Pawn)
        {
            Debug.LogWarning($"BackwardPawnMutation can only be applied to Pawns");
            return;
        }

        var state = MutationManager.Instance.GetMutationState(piece, this);
        var rule = ScriptableObject.CreateInstance<BackwardPawnRule>();
        rule.Initialize(piece);

        AddAndTrackRule(piece, rule, state);

        if (MutationManager.Instance.Config.enableDebugLogs)
            Debug.Log($"Applied BackwardPawnMutation to {piece.name}");
    }

    public override void RemoveFromPiece(Piece piece)
    {
        if (piece == null) return;

        var state = MutationManager.Instance.GetMutationState(piece, this);
        RemoveTrackedRules(piece, state);

        if (MutationManager.Instance.Config.enableDebugLogs)
            Debug.Log($"Removed BackwardPawnMutation from {piece.name}");
    }

    public override string GetDetailedDescription(Piece piece)
    {
        return $"{description}\n\n이 폰은 앞뿐만 아니라 뒤로도 한 칸 이동할 수 있습니다.";
    }
}
```

**미구현 표시 예시**: (나중에 구현할 Mutation)
```csharp
/// <summary>
/// [미구현] 킹 주변에 보호막을 생성하는 Mutation
/// TODO: 보호막 시각화 및 로직 구현 필요
/// </summary>
[CreateAssetMenu(fileName = "ShieldMutation", menuName = "MutatingGambit/Mutations/Advanced/Shield")]
public class ShieldMutation : Mutation
{
    public override void ApplyToPiece(Piece piece)
    {
        Debug.LogWarning($"ShieldMutation is not yet implemented");
        // TODO: 보호막 로직 구현
    }

    public override void RemoveFromPiece(Piece piece)
    {
        Debug.LogWarning($"ShieldMutation is not yet implemented");
        // TODO: 보호막 제거 로직 구현
    }
}
```

**구현 우선순위**:
1. **높음**: BackwardPawnMutation, DiagonalRookMutation, StraightBishopMutation
2. **중간**: TeleportKnightMutation, BloodthirstMutation
3. **낮음**: PhantomPieceMutation, TimeWarpMutation

**테스트**:
- [ ] 각 구현된 Mutation 동작 확인
- [ ] 미구현 Mutation 경고 메시지 확인
- [ ] 엣지 케이스 처리 확인

---

#### 3.2 한국어 문서화 완성
**대상 파일**: 모든 public API

**문서화 표준**:
```csharp
/// <summary>
/// [한글로 간단한 설명]
/// </summary>
/// <param name="paramName">[파라미터 설명]</param>
/// <returns>[반환값 설명]</returns>
/// <remarks>
/// [추가 정보, 사용 예시, 주의사항]
/// </remarks>
/// <example>
/// <code>
/// // 사용 예시
/// var mutation = ...;
/// mutation.ApplyToPiece(piece);
/// </code>
/// </example>
```

**주요 클래스 문서화**:

**Mutation.cs**:
```csharp
/// <summary>
/// Mutation의 기본 클래스
/// 모든 Mutation은 이 클래스를 상속하여 구현합니다
/// </summary>
/// <remarks>
/// Mutation은 체스 말에 특수한 능력을 부여하는 시스템입니다.
/// ScriptableObject로 구현되어 에셋으로 관리되며, 재사용 가능합니다.
///
/// 구현 시 주의사항:
/// 1. ScriptableObject이므로 상태를 저장하지 마세요 (MutationState 사용)
/// 2. ApplyToPiece와 RemoveFromPiece를 반드시 구현하세요
/// 3. MovementRule을 추가할 때는 AddAndTrackRule을 사용하세요
/// </remarks>
public abstract class Mutation : ScriptableObject
{
    /// <summary>
    /// Mutation을 Piece에 적용합니다
    /// </summary>
    /// <param name="piece">Mutation을 적용할 체스 말</param>
    /// <remarks>
    /// 이 메서드는 MutationManager가 호출하며, 직접 호출하지 마세요.
    /// 상태가 필요한 경우 MutationState를 통해 관리하세요.
    /// </remarks>
    public abstract void ApplyToPiece(Piece piece);

    // ...
}
```

**MutationManager.cs**:
```csharp
/// <summary>
/// Mutation 시스템의 중앙 관리자
/// Singleton 패턴으로 구현되어 씬 전체에서 단일 인스턴스로 작동합니다
/// </summary>
/// <remarks>
/// 주요 기능:
/// - Mutation 적용/제거
/// - Piece별 Mutation 상태 관리
/// - Mutation 이벤트 발생
/// - 설정 관리 (MutationConfig)
///
/// 사용 예시:
/// <code>
/// var manager = MutationManager.Instance;
/// manager.ApplyMutation(piece, mutation);
/// var mutations = manager.GetMutationsForPiece(piece);
/// </code>
/// </remarks>
public class MutationManager : MonoBehaviour
{
    // ...
}
```

**문서 생성**:
- API 레퍼런스 자동 생성 (Doxygen 또는 DocFX)
- 사용자 가이드 작성 (`docs/MutationSystem_Guide.md`)

**테스트**:
- [ ] 모든 public API 문서화 완료
- [ ] 문서 생성 확인
- [ ] 예시 코드 실행 확인

---

#### 3.3 Region 주석으로 논리적 그룹화
**적용 파일**: `MutationManager.cs`, `MutationLibrary.cs`

**MutationManager.cs 예시**:
```csharp
public class MutationManager : MonoBehaviour
{
    #region Singleton

    private static MutationManager instance;
    public static MutationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MutationManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("MutationManager");
                    instance = go.AddComponent<MutationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    #endregion

    #region Fields

    [Header("Configuration")]
    [SerializeField] private MutationConfig config;

    private Dictionary<Piece, List<MutationState>> pieceMutationStates
        = new Dictionary<Piece, List<MutationState>>();

    #endregion

    #region Events

    public static event System.Action<Piece, Mutation> OnMutationApplied;
    public static event System.Action<Piece, Mutation> OnMutationRemoved;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // 초기화
    }

    private void OnDestroy()
    {
        // 정리
    }

    #endregion

    #region Mutation Application

    public void ApplyMutation(Piece piece, Mutation mutation)
    {
        // ...
    }

    public void RemoveMutation(Piece piece, Mutation mutation)
    {
        // ...
    }

    #endregion

    #region Mutation Queries

    public List<Mutation> GetMutationsForPiece(Piece piece)
    {
        // ...
    }

    public MutationState GetMutationState(Piece piece, Mutation mutation)
    {
        // ...
    }

    #endregion

    #region State Management

    private MutationState GetOrCreateMutationState(Piece piece, Mutation mutation)
    {
        // ...
    }

    #endregion

    #region Validation

    public bool CanApplyMutation(Piece piece, Mutation mutation)
    {
        // ...
    }

    #endregion
}
```

**그룹화 기준**:
1. Singleton
2. Fields (필드)
3. Properties (프로퍼티)
4. Events (이벤트)
5. Unity Lifecycle (Unity 생명주기)
6. Public Methods (공개 메서드)
7. Private Methods (비공개 메서드)
8. Validation (검증)
9. Utilities (유틸리티)

**테스트**:
- [ ] 코드 가독성 향상 확인
- [ ] 기능 정상 작동

---

#### 3.4 Null 체크 및 오류 처리 강화
**모든 public 메서드에 일관된 검증**:

```csharp
public void ApplyMutation(Piece piece, Mutation mutation)
{
    // Validation
    if (piece == null)
    {
        Debug.LogError($"[MutationManager] Cannot apply mutation to null piece");
        return;
    }

    if (mutation == null)
    {
        Debug.LogError($"[MutationManager] Cannot apply null mutation to {piece.name}");
        return;
    }

    if (!CanApplyMutation(piece, mutation))
    {
        Debug.LogWarning($"[MutationManager] Cannot apply {mutation.name} to {piece.name}. " +
                        $"Maximum mutations reached or mutation already applied.");
        return;
    }

    // 실제 로직
    try
    {
        var state = GetOrCreateMutationState(piece, mutation);
        mutation.ApplyToPiece(piece);

        // ...

        OnMutationApplied?.Invoke(piece, mutation);
    }
    catch (System.Exception e)
    {
        Debug.LogError($"[MutationManager] Error applying {mutation.name} to {piece.name}: {e.Message}");
        throw;
    }
}
```

**검증 메서드 추가**:
```csharp
#region Validation

/// <summary>
/// Mutation을 적용할 수 있는지 검증합니다
/// </summary>
public bool CanApplyMutation(Piece piece, Mutation mutation)
{
    if (piece == null || mutation == null)
        return false;

    // 최대 개수 확인
    var currentMutations = GetMutationsForPiece(piece);
    if (currentMutations.Count >= config.maxMutationsPerPiece)
        return false;

    // 중복 확인
    if (currentMutations.Contains(mutation))
        return false;

    // Piece 타입 호환성 확인
    if (!mutation.IsCompatibleWithPiece(piece))
        return false;

    return true;
}

#endregion
```

**테스트**:
- [ ] null 입력 시 적절한 오류 메시지
- [ ] 예외 상황 처리 확인
- [ ] 로그 출력 확인

---

### Phase 3 완료 기준
- [ ] AdvancedMutations 구현 완료 또는 명시적 표시
- [ ] 문서화 100% 완료
- [ ] 코드 리뷰 통과
- [ ] 모든 테스트 통과

---

## Phase 4: 기능 확장 및 최적화

### 목표
새로운 기능 추가 및 시스템 최적화

### 기간
1주 (Week 4)

### 작업 항목

#### 4.1 MutationQueryService 분리
**새 파일**: `Mutations/Library/MutationQueryService.cs`

```csharp
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MutatingGambit.Systems.Mutations.Library
{
    /// <summary>
    /// Mutation 조회를 전담하는 서비스 클래스
    /// 복잡한 쿼리를 간편하게 수행할 수 있습니다
    /// </summary>
    public class MutationQueryService
    {
        private MutationLibrary library;
        private MutationManager manager;

        public MutationQueryService(MutationLibrary library, MutationManager manager)
        {
            this.library = library;
            this.manager = manager;
        }

        /// <summary>
        /// Query Builder 시작
        /// </summary>
        public MutationQuery Query()
        {
            return new MutationQuery(library, manager);
        }
    }

    /// <summary>
    /// Mutation 조회를 위한 Query Builder
    /// </summary>
    public class MutationQuery
    {
        private MutationLibrary library;
        private MutationManager manager;
        private List<Mutation> results;

        internal MutationQuery(MutationLibrary library, MutationManager manager)
        {
            this.library = library;
            this.manager = manager;
            this.results = new List<Mutation>(library.allMutations);
        }

        /// <summary>
        /// 특정 등급으로 필터링
        /// </summary>
        public MutationQuery WithRarity(MutationRarity rarity)
        {
            results = results.Where(m => m.rarity == rarity).ToList();
            return this;
        }

        /// <summary>
        /// 특정 Piece 타입과 호환되는 것만 필터링
        /// </summary>
        public MutationQuery CompatibleWith(PieceType pieceType)
        {
            results = results.Where(m => m.IsCompatibleWithPieceType(pieceType)).ToList();
            return this;
        }

        /// <summary>
        /// 특정 Piece에 아직 적용되지 않은 것만 필터링
        /// </summary>
        public MutationQuery NotAppliedTo(Piece piece)
        {
            var applied = manager.GetMutationsForPiece(piece);
            results = results.Where(m => !applied.Contains(m)).ToList();
            return this;
        }

        /// <summary>
        /// 가중치 기반 랜덤 선택
        /// </summary>
        public Mutation GetRandomWeighted()
        {
            if (results.Count == 0) return null;

            float totalWeight = results.Sum(m =>
                manager.Config.GetWeightForRarity(m.rarity));
            float random = Random.Range(0f, totalWeight);
            float current = 0f;

            foreach (var mutation in results)
            {
                current += manager.Config.GetWeightForRarity(mutation.rarity);
                if (random <= current)
                    return mutation;
            }

            return results[results.Count - 1];
        }

        /// <summary>
        /// 모든 결과 반환
        /// </summary>
        public List<Mutation> GetAll()
        {
            return new List<Mutation>(results);
        }

        /// <summary>
        /// 첫 번째 결과 반환
        /// </summary>
        public Mutation GetFirst()
        {
            return results.Count > 0 ? results[0] : null;
        }

        /// <summary>
        /// 랜덤하게 하나 선택
        /// </summary>
        public Mutation GetRandom()
        {
            return results.Count > 0 ? results[Random.Range(0, results.Count)] : null;
        }
    }
}
```

**사용 예시**:
```csharp
// MutationManager에 QueryService 추가
public class MutationManager : MonoBehaviour
{
    private MutationQueryService queryService;

    public MutationQueryService Query => queryService;

    private void Awake()
    {
        queryService = new MutationQueryService(mutationLibrary, this);
    }
}

// 사용
var mutation = MutationManager.Instance.Query()
    .WithRarity(MutationRarity.Rare)
    .CompatibleWith(PieceType.Pawn)
    .NotAppliedTo(myPiece)
    .GetRandomWeighted();
```

**테스트**:
- [ ] Query Builder 정상 작동
- [ ] 체이닝 확인
- [ ] 성능 테스트

---

#### 4.2 Event System 확장
**파일**: `MutationManager.cs`

**추가 이벤트**:
```csharp
#region Events

// 기존
public static event System.Action<Piece, Mutation> OnMutationApplied;
public static event System.Action<Piece, Mutation> OnMutationRemoved;

// 새로 추가
public static event System.Action<Piece, List<Mutation>> OnMutationStackChanged;
public static event System.Action<Piece, Mutation, Mutation> OnSynergyActivated;
public static event System.Action<Mutation> OnMutationCreated;
public static event System.Action OnSystemInitialized;

#endregion
```

**Synergy 시스템**:
```csharp
#region Synergies

/// <summary>
/// Mutation 간의 시너지를 확인하고 활성화합니다
/// </summary>
private void CheckSynergies(Piece piece)
{
    if (!config.enableSynergies) return;

    var mutations = GetMutationsForPiece(piece);

    // 예시: BackwardPawn + DiagonalRook = 완전 자유 이동
    var backwardPawn = mutations.FirstOrDefault(m => m is BackwardPawnMutation);
    var diagonalRook = mutations.FirstOrDefault(m => m is DiagonalRookMutation);

    if (backwardPawn != null && diagonalRook != null)
    {
        OnSynergyActivated?.Invoke(piece, backwardPawn, diagonalRook);
        // 시너지 효과 적용
        ApplySynergyEffect(piece, backwardPawn, diagonalRook);
    }
}

private void ApplySynergyEffect(Piece piece, Mutation m1, Mutation m2)
{
    // 시너지 효과 구현
    Debug.Log($"Synergy activated: {m1.name} + {m2.name} on {piece.name}");
}

#endregion
```

**테스트**:
- [ ] 모든 이벤트 발생 확인
- [ ] 시너지 활성화 확인
- [ ] 이벤트 리스너 정상 작동

---

#### 4.3 성능 최적화
**최적화 항목**:

1. **Dictionary 조회 최적화**:
```csharp
// 기존: 매번 새 리스트 생성
public List<Mutation> GetMutationsForPiece(Piece piece)
{
    return pieceMutationStates[piece].Select(s => s.Mutation).ToList();
}

// 최적화: 캐싱
private Dictionary<Piece, List<Mutation>> mutationCache
    = new Dictionary<Piece, List<Mutation>>();

public List<Mutation> GetMutationsForPiece(Piece piece)
{
    if (!mutationCache.TryGetValue(piece, out var cached))
    {
        cached = pieceMutationStates[piece].Select(s => s.Mutation).ToList();
        mutationCache[piece] = cached;
    }
    return new List<Mutation>(cached); // 방어적 복사
}

private void InvalidateCache(Piece piece)
{
    mutationCache.Remove(piece);
}
```

2. **Object Pooling (MovementRule)**:
```csharp
public class MovementRulePool
{
    private Dictionary<System.Type, Queue<MovementRule>> pools
        = new Dictionary<System.Type, Queue<MovementRule>>();

    public T Get<T>() where T : MovementRule
    {
        var type = typeof(T);
        if (!pools.TryGetValue(type, out var pool))
        {
            pool = new Queue<MovementRule>();
            pools[type] = pool;
        }

        if (pool.Count > 0)
            return pool.Dequeue() as T;

        return ScriptableObject.CreateInstance<T>();
    }

    public void Return(MovementRule rule)
    {
        var type = rule.GetType();
        if (!pools.ContainsKey(type))
            pools[type] = new Queue<MovementRule>();

        pools[type].Enqueue(rule);
    }
}
```

3. **이벤트 최적화** (불필요한 발생 방지):
```csharp
public void ApplyMutation(Piece piece, Mutation mutation)
{
    // 변경 전 상태 저장
    int prevCount = GetMutationsForPiece(piece).Count;

    // Mutation 적용
    // ...

    // 실제 변경된 경우에만 이벤트 발생
    if (GetMutationsForPiece(piece).Count != prevCount)
    {
        OnMutationApplied?.Invoke(piece, mutation);
        OnMutationStackChanged?.Invoke(piece, GetMutationsForPiece(piece));
    }
}
```

**성능 테스트**:
- [ ] 프로파일러로 병목 지점 확인
- [ ] 100개 Piece에 Mutation 적용 시 성능 측정
- [ ] 메모리 사용량 확인

---

#### 4.4 단위 테스트 작성
**새 파일**: `Tests/Editor/MutationSystemTests.cs`

```csharp
using NUnit.Framework;
using UnityEngine;
using MutatingGambit.Systems.Mutations;

public class MutationSystemTests
{
    private MutationManager manager;
    private Piece testPiece;
    private Mutation testMutation;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject("TestManager");
        manager = go.AddComponent<MutationManager>();

        testPiece = new GameObject("TestPiece").AddComponent<Piece>();
        testMutation = ScriptableObject.CreateInstance<TestMutation>();
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(manager.gameObject);
        Object.DestroyImmediate(testPiece.gameObject);
        Object.DestroyImmediate(testMutation);
    }

    [Test]
    public void ApplyMutation_AddsToList()
    {
        manager.ApplyMutation(testPiece, testMutation);
        var mutations = manager.GetMutationsForPiece(testPiece);

        Assert.AreEqual(1, mutations.Count);
        Assert.Contains(testMutation, mutations);
    }

    [Test]
    public void ApplyMutation_FiresEvent()
    {
        bool eventFired = false;
        MutationManager.OnMutationApplied += (p, m) => eventFired = true;

        manager.ApplyMutation(testPiece, testMutation);

        Assert.IsTrue(eventFired);
    }

    [Test]
    public void RemoveMutation_RemovesFromList()
    {
        manager.ApplyMutation(testPiece, testMutation);
        manager.RemoveMutation(testPiece, testMutation);

        var mutations = manager.GetMutationsForPiece(testPiece);
        Assert.AreEqual(0, mutations.Count);
    }

    [Test]
    public void ApplyMutation_NullPiece_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => manager.ApplyMutation(null, testMutation));
    }

    [Test]
    public void MutationState_IndependentPerPiece()
    {
        var piece1 = new GameObject("Piece1").AddComponent<Piece>();
        var piece2 = new GameObject("Piece2").AddComponent<Piece>();

        manager.ApplyMutation(piece1, testMutation);
        manager.ApplyMutation(piece2, testMutation);

        var state1 = manager.GetMutationState(piece1, testMutation);
        var state2 = manager.GetMutationState(piece2, testMutation);

        state1.SetData("test", 100);
        state2.SetData("test", 200);

        Assert.AreEqual(100, state1.GetData<int>("test"));
        Assert.AreEqual(200, state2.GetData<int>("test"));

        Object.DestroyImmediate(piece1.gameObject);
        Object.DestroyImmediate(piece2.gameObject);
    }

    // ... 더 많은 테스트
}

// 테스트용 Mutation
public class TestMutation : Mutation
{
    public override void ApplyToPiece(Piece piece) { }
    public override void RemoveFromPiece(Piece piece) { }
}
```

**테스트 커버리지 목표**: 80% 이상

**테스트**:
- [ ] 모든 테스트 통과
- [ ] 커버리지 80% 달성
- [ ] CI/CD 통합

---

### Phase 4 완료 기준
- [ ] 모든 신기능 구현 완료
- [ ] 성능 최적화 완료 (20% 이상 향상)
- [ ] 단위 테스트 커버리지 80% 달성
- [ ] 최종 코드 리뷰 통과

---

## 테스트 전략

### 단위 테스트
- **도구**: NUnit, Unity Test Framework
- **목표**: 80% 코드 커버리지
- **주요 테스트**:
  - Mutation 적용/제거
  - 상태 관리
  - 이벤트 발생
  - 검증 로직
  - Query Builder

### 통합 테스트
- **시나리오**:
  - 여러 Mutation을 Piece에 적용
  - Mutation 제거 후 재적용
  - Scene 전환 시 상태 유지
  - 시너지 활성화

### 성능 테스트
- **벤치마크**:
  - 100개 Piece에 Mutation 적용: < 10ms
  - 1000번 조회: < 5ms
  - 메모리 사용량: < 50MB

### 수동 테스트
- **체크리스트**:
  - [ ] UI에서 Mutation 적용 확인
  - [ ] 게임플레이 정상 작동
  - [ ] 시각적 피드백 확인
  - [ ] 에러 없음

---

## 롤백 계획

### Git 브랜치 전략
```
main
├── feature/mutation-refactor-phase1
├── feature/mutation-refactor-phase2
├── feature/mutation-refactor-phase3
└── feature/mutation-refactor-phase4
```

각 Phase는 별도 브랜치에서 작업하고, 완료 후 main에 머지

### 백업
- 리팩터링 시작 전 전체 프로젝트 백업
- 각 Phase 시작 전 해당 폴더 백업

### 롤백 절차
1. 문제 발생 Phase 확인
2. 해당 브랜치로 체크아웃
3. 문제 수정 또는 이전 Phase로 롤백
4. 재테스트 후 재머지

---

## 마일스톤

### Week 1 (Phase 1)
- [x] 계획 수립 완료
- [ ] 치명적 버그 수정
- [ ] Phase 1 테스트 통과

### Week 2 (Phase 2)
- [ ] 구조 개선 완료
- [ ] MutationApplicator 제거
- [ ] Phase 2 테스트 통과

### Week 3 (Phase 3)
- [ ] 구현 완성
- [ ] 문서화 100%
- [ ] Phase 3 테스트 통과

### Week 4 (Phase 4)
- [ ] 신기능 추가
- [ ] 최적화 완료
- [ ] 최종 테스트 통과
- [ ] 프로덕션 배포

---

## 성공 지표

### 코드 품질
- 코드 커버리지: 80% 이상
- 코드 복잡도: 평균 10 이하 (Cyclomatic Complexity)
- 중복 코드: 5% 이하

### 성능
- Mutation 적용 시간: 20% 감소
- 메모리 사용량: 현재 대비 유지 또는 감소
- 조회 속도: 30% 향상

### 유지보수성
- 문서화: 모든 public API
- 코드 가독성: 4.5/5
- 확장성: 새 Mutation 추가 30분 이내

---

## 참고 자료

- [Unity 스크립팅 가이드](https://docs.unity3d.com/Manual/ScriptingSection.html)
- [C# 코딩 컨벤션](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- [SOLID 원칙](https://en.wikipedia.org/wiki/SOLID)
- [디자인 패턴 (Gang of Four)](https://en.wikipedia.org/wiki/Design_Patterns)

---

## 변경 이력

| 날짜 | 버전 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 2025-12-02 | 1.0 | 초기 계획 수립 | Claude |

---

**다음 단계**: Phase 1 시작 - 긴급 버그 수정
