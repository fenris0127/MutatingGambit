# MutationConfig 에셋 생성 가이드

## 개요

`MutationConfig`는 게임의 모든 변이 설정을 중앙에서 관리하는 ScriptableObject입니다.
드롭 확률, 비용, 제한 사항 등을 Unity Inspector에서 쉽게 조정할 수 있습니다.

---

## 에셋 생성 방법

### 1. Unity Editor에서 생성

1. **Project 창**에서 에셋을 생성할 폴더로 이동
   - 권장 경로: `Assets/Resources/Config/`

2. **우클릭** → `Create` → `MutatingGambit` → `Config` → `Mutation Config`

3. 생성된 에셋 이름을 `MutationConfig`로 변경

---

## 설정 항목

### 드롭 확률 (Drop Rates)

변이 등급별 드롭 확률을 설정합니다. **합계가 1.0(100%)이 되어야 합니다.**

| 필드 | 기본값 | 설명 |
|------|--------|------|
| `commonDropRate` | 0.5 (50%) | 일반 등급 드롭 확률 |
| `rareDropRate` | 0.3 (30%) | 희귀 등급 드롭 확률 |
| `epicDropRate` | 0.15 (15%) | 영웅 등급 드롭 확률 |
| `legendaryDropRate` | 0.05 (5%) | 전설 등급 드롭 확률 |

**검증**: Inspector에서 값을 변경하면 합계가 1.0이 아닐 경우 Console에 경고 메시지가 표시됩니다.

```
[MutationConfig] 드롭 확률의 합이 1.0이 아닙니다: 0.95
```

---

### 제한 (Limits)

| 필드 | 기본값 | 설명 |
|------|--------|------|
| `maxMutationsPerPiece` | 3 | 기물 하나당 최대 변이 개수 |
| `allowDuplicateMutations` | false | 동일 변이 중복 적용 허용 여부 |

**참고**: `maxMutationsPerPiece`는 최소 1 이상이어야 합니다.

---

### 사용 가능한 Mutation 목록

게임에서 사용할 모든 Mutation ScriptableObject를 등록합니다.

**설정 방법**:

1. Inspector에서 `Available Mutations` 리스트 펼치기
2. `+` 버튼 클릭하여 요소 추가
3. Mutation 에셋을 드래그 앤 드롭 또는 선택

**권장 Mutation 목록**:

#### Movement (이동)
- ReversePawnMutation
- SwapPositionMutation
- EchoChamberMutation
- LeapingRookMutation
- TeleportingKnightMutation
- PhantomPawnMutation

#### Attack (공격)
- ExplosiveCaptureMutation
- SniperMutation
- BloodthirstMutation
- SplittingKnightMutation

#### Utility (유틸리티)
- SacrificeWarpMutation
- StealthCloakMutation
- PhoenixRebornMutation
- EvolutionMutation

#### Chaos (혼돈)
- ChaosStepMutation

#### Special (특수)
- BerserkQueenMutation
- ExplosiveKingMutation
- FragileBishopMutation
- FrozenBishopMutation
- DoubleMoveRookMutation
- ShadowPawnMutation

---

### 비용 설정 (Cost Multipliers)

등급별 기본 비용 배율을 설정합니다.

| 필드 | 기본값 | 설명 |
|------|--------|------|
| `commonCostMultiplier` | 1.0 | 일반 등급 비용 배율 |
| `rareCostMultiplier` | 2.0 | 희귀 등급 비용 배율 (2배) |
| `epicCostMultiplier` | 3.5 | 영웅 등급 비용 배율 (3.5배) |
| `legendaryCostMultiplier` | 5.0 | 전설 등급 비용 배율 (5배) |

**계산 방식**: `최종 비용 = Mutation.Cost × 해당 등급의 Multiplier`

---

## 코드에서 사용하기

### MutationConfig 로드

```csharp
// Resources 폴더에서 로드
MutationConfig config = Resources.Load<MutationConfig>("Config/MutationConfig");
```

### 랜덤 등급 선택

```csharp
MutationRarity rarity = config.GetRandomRarity();
```

### 특정 등급의 변이 가져오기

```csharp
List<Mutation> epicMutations = config.GetMutationsByRarity(MutationRarity.Epic);
```

### 기물 타입 호환 변이 가져오기

```csharp
List<Mutation> knightMutations = config.GetCompatibleMutations(PieceType.Knight);
```

### 드롭 확률 가져오기

```csharp
float rareDropRate = config.GetDropRate(MutationRarity.Rare);
```

### 비용 배율 가져오기

```csharp
float legendaryCost = config.GetCostMultiplier(MutationRarity.Legendary);
```

---

## 예제 설정

### 균형잡힌 기본 설정

```
드롭 확률:
- Common: 50%
- Rare: 30%
- Epic: 15%
- Legendary: 5%

제한:
- Max Mutations Per Piece: 3
- Allow Duplicate Mutations: false

비용 배율:
- Common: 1.0
- Rare: 2.0
- Epic: 3.5
- Legendary: 5.0
```

### 하드코어 설정 (고난이도)

```
드롭 확률:
- Common: 70%
- Rare: 20%
- Epic: 8%
- Legendary: 2%

제한:
- Max Mutations Per Piece: 2
- Allow Duplicate Mutations: false

비용 배율:
- Common: 1.5
- Rare: 3.0
- Epic: 5.0
- Legendary: 8.0
```

### 캐주얼 설정 (쉬운 난이도)

```
드롭 확률:
- Common: 30%
- Rare: 35%
- Epic: 25%
- Legendary: 10%

제한:
- Max Mutations Per Piece: 5
- Allow Duplicate Mutations: true

비용 배율:
- Common: 0.8
- Rare: 1.5
- Epic: 2.5
- Legendary: 3.5
```

---

## 주의사항

### 1. 드롭 확률 합계
- 반드시 합계가 **정확히 1.0**이어야 합니다
- 소수점 오차는 0.01 이내로 허용됩니다
- 검증은 `OnValidate()` 메서드에서 자동으로 수행됩니다

### 2. Available Mutations 리스트
- **비어있으면 안 됩니다**
- null 요소가 없도록 주의하세요
- 중복된 Mutation을 추가하지 마세요

### 3. 런타임 변경
- ScriptableObject이므로 **런타임에 값을 변경하면 에셋이 영구적으로 수정됩니다**
- 런타임에 임시로 값을 변경하려면 별도의 설정 데이터 클래스를 사용하세요

### 4. Resources 폴더
- `Resources.Load()`를 사용하려면 반드시 `Resources/` 폴더 내에 있어야 합니다
- 또는 직접 참조를 통해 사용할 수도 있습니다

---

## 트러블슈팅

### 문제: "드롭 확률의 합이 1.0이 아닙니다" 경고

**해결 방법**:
1. Inspector에서 모든 드롭 확률 값을 확인
2. 합계가 정확히 1.0이 되도록 조정
3. 예: 0.5 + 0.3 + 0.15 + 0.05 = 1.0

### 문제: GetRandomRarity()가 항상 같은 값을 반환

**원인**: 드롭 확률이 잘못 설정됨

**해결 방법**:
1. 각 등급의 드롭 확률이 0보다 큰지 확인
2. 누적 확률이 올바른지 확인

### 문제: GetCompatibleMutations()가 빈 리스트 반환

**원인**: Available Mutations 리스트가 비어있거나 호환되는 Mutation이 없음

**해결 방법**:
1. Available Mutations 리스트에 Mutation이 추가되어 있는지 확인
2. 각 Mutation의 `compatiblePieceTypes` 설정 확인

---

## 추가 리소스

- **Mutation 클래스 문서**: `docs/Mutation_System_Guide.md`
- **리팩토링 계획**: `docs/MutationSystem_Refactoring_Plan.md`
- **MovementRule 가이드**: `docs/MovementRule_Guide.md`

---

## 변경 이력

- **2025-12-02**: 초기 작성 (Phase 2 완료)
