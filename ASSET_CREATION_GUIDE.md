# ScriptableObject 에셋 생성 가이드

The Mutating Gambit MVP에 필요한 모든 ScriptableObject 에셋을 생성하는 방법입니다.

---

## 방법 1: 자동 생성 (추천) ⚡

### 사용 방법

1. **Unity Editor를 엽니다**

2. **상단 메뉴에서 선택:**
   ```
   Tools > Mutating Gambit > Asset Creator
   ```

3. **Asset Creator 창이 열리면:**
   - 생성할 에셋 카테고리를 선택 (기본적으로 모두 선택됨)
   - `Create All Selected Assets` 버튼 클릭

4. **완료!**
   - 모든 에셋이 `Assets/Data/` 폴더에 생성됩니다
   - Console에서 생성 로그를 확인할 수 있습니다

### 생성되는 에셋 목록

- ✅ **Mutations** (10개) - `Assets/Data/Mutations/`
- ✅ **Artifacts** (21개) - `Assets/Data/Artifacts/`
- ✅ **Board Data** (3개) - `Assets/Data/Boards/`
- ✅ **Room Data** (5개) - `Assets/Data/Rooms/`
- ✅ **AI Configs** (3개) - `Assets/Data/AI/`
- ✅ **Victory Conditions** (2개) - `Assets/Data/VictoryConditions/`

---

## 방법 2: 수동 생성 📝

자동 생성이 작동하지 않거나 개별 에셋을 커스터마이즈하고 싶을 때 사용합니다.

### 기본 생성 방법

1. **Project 창에서 폴더 생성**
   ```
   Assets/
   └── Data/
       ├── Mutations/
       ├── Artifacts/
       ├── Boards/
       ├── Rooms/
       ├── AI/
       └── VictoryConditions/
   ```

2. **에셋 생성**
   - Project 창에서 해당 폴더에 우클릭
   - `Create > [카테고리]` 선택
   - 에셋 이름 입력

---

## 1. Mutation 에셋 생성 (10개)

### 생성 위치
`Assets/Data/Mutations/`

### 생성 방법

각 Mutation마다:

1. **Project 창에서:**
   ```
   우클릭 > Create > Mutations > [Mutation 이름]
   ```

2. **파일 이름 지정**

### 필요한 Mutations

| 번호 | 에셋 이름 | 메뉴 경로 | 설명 |
|------|-----------|-----------|------|
| 1 | LeapingRook | `Create > Mutations > Leaping Rook` | 룩이 아군 1개를 뛰어넘을 수 있음 |
| 2 | SplittingKnight | `Create > Mutations > Splitting Knight` | 나이트가 말을 잡으면 폰 생성 |
| 3 | FragileBishop | `Create > Mutations > Fragile Bishop` | 비숍이 정확히 3칸만 이동 |
| 4 | PhantomPawn | `Create > Mutations > Phantom Pawn` | 폰이 적 1개를 관통 가능 |
| 5 | BerserkQueen | `Create > Mutations > Berserk Queen` | 퀸이 강력하지만 HP 감소 |
| 6 | TeleportingKnight | `Create > Mutations > Teleporting Knight` | 나이트가 텔레포트 가능 |
| 7 | FrozenBishop | `Create > Mutations > Frozen Bishop` | 비숍이 이동 불가, 대신 사거리 증가 |
| 8 | DoubleMoveRook | `Create > Mutations > Double Move Rook` | 룩이 턴당 2번 이동 |
| 9 | ExplosiveKing | `Create > Mutations > Explosive King` | 킹이 주변에 피해 |
| 10 | ShadowPawn | `Create > Mutations > Shadow Pawn` | 폰이 그림자 분신 생성 |

---

## 2. Artifact 에셋 생성 (20개)

### 생성 위치
`Assets/Data/Artifacts/`

### 생성 방법

1. **Project 창에서:**
   ```
   우클릭 > Create > Artifacts > [Artifact 이름]
   ```

2. **파일 이름 지정**

3. **Inspector에서 설정:**
   - **Artifact Name**: 표시될 이름
   - **Description**: 효과 설명
   - **Icon**: 아이콘 스프라이트 (optional)
   - **Rarity**: Common/Uncommon/Rare/Epic/Legendary

### 필요한 Artifacts

| 번호 | 에셋 이름 | 메뉴 경로 | Rarity | 설명 |
|------|-----------|-----------|--------|------|
| 1 | GravityMirror | `Create > Artifacts > Gravity Mirror` | Rare | 모든 기물을 하단으로 끌어당김 |
| 2 | KingsShadow | `Create > Artifacts > Kings Shadow` | Uncommon | 킹이 그림자 장애물 생성 |
| 3 | CavalryCharge | `Create > Artifacts > Cavalry Charge` | Common | 나이트가 capture 후 추가 이동 |
| 4 | ChainLightning | `Create > Artifacts > Chain Lightning` | Epic | 말이 잡힐 때 주변 피해 |
| 5 | PromotionPrivilege | `Create > Artifacts > Promotion Privilege` | Rare | 폰이 3회 capture 시 승급 |
| 6 | FrozenThrone | `Create > Artifacts > Frozen Throne` | Legendary | 킹이 고정되지만 사거리 증가 |
| 7 | MimicsMask | `Create > Artifacts > Mimics Mask` | Rare | 랜덤 기물이 다른 기물 모방 |
| 8 | BerserkersRage | `Create > Artifacts > Berserkers Rage` | Uncommon | Capture 시 추가 이동 |
| 9 | SanctuaryShield | `Create > Artifacts > Sanctuary Shield` | Epic | 첫 기물 손실 방지 |
| 10 | PhantomSteps | `Create > Artifacts > Phantom Steps` | Rare | 적 기물 관통 가능 |
| 11 | TwinSouls | `Create > Artifacts > Twin Souls` | Uncommon | Capture 시 폰 생성 |
| 12 | CursedCrown | `Create > Artifacts > Cursed Crown` | Common | 적 킹 위치 표시 |
| 13 | DivineIntervention | `Create > Artifacts > Divine Intervention` | Legendary | 체크메이트 1회 방지 |
| 14 | HasteBoots | `Create > Artifacts > Haste Boots` | Rare | 모든 기물 이동력 증가 |
| 15 | SacrificialAltar | `Create > Artifacts > Sacrificial Altar` | Epic | 기물 희생으로 복구 |
| 16 | WeakeningAura | `Create > Artifacts > Weakening Aura` | Uncommon | 적 이동력 감소 |
| 17 | ResurrectionStone | `Create > Artifacts > Resurrection Stone` | Legendary | 죽은 기물 3턴 후 부활 |
| 18 | TimeWarp | `Create > Artifacts > Time Warp` | Epic | 시간 조작 |
| 19 | MagneticField | `Create > Artifacts > Magnetic Field` | Rare | 자기장 효과 |
| 20 | PhoenixFeather | `Create > Artifacts > Phoenix Feather` | Legendary | 불사조 깃털 |

---

## 3. BoardData 에셋 생성 (3개)

### 생성 위치
`Assets/Data/Boards/`

### 생성 방법

1. **Project 창에서:**
   ```
   우클릭 > Create > Mutating Gambit > Board Data
   ```

2. **Inspector에서 설정**

### 필요한 Boards

#### 3.1 Standard_8x8.asset
```
Width: 8
Height: 8
Obstacles: (비워둠 - 표준 체스판)
```

#### 3.2 Small_6x6.asset
```
Width: 6
Height: 6
Obstacles: (비워둠 - 작은 체스판)
```

#### 3.3 Obstacles_5x8.asset
```
Width: 5
Height: 8
Obstacles: (Inspector에서 설정 - 중앙에 장애물 배치)
```

**장애물 설정 방법:**
1. Inspector에서 `Obstacles` 배열 크기 설정: `Size = 3`
2. 각 요소에 장애물 위치 입력:
   - Element 0: `(2, 3)`
   - Element 1: `(2, 4)`
   - Element 2: `(2, 5)`

---

## 4. RoomData 에셋 생성 (5개)

### 생성 위치
`Assets/Data/Rooms/`

### 생성 방법

1. **Project 창에서:**
   ```
   우클릭 > Create > Mutating Gambit > Room Data
   ```

2. **Inspector에서 설정**

### 필요한 Rooms

#### 4.1 Combat_BasicEncounter.asset

```yaml
Room Name: "Basic Encounter"
Room Description: "A simple chess battle."
Room Type: NormalCombat
Board Data: [Standard_8x8 연결]
Victory Condition: [CheckmateIn5Moves 연결]
Enemy Pieces:
  - Size: 8
  - [Inspector에서 적 기물 배치 설정]
Use Standard Player Setup: true
```

#### 4.2 Combat_PawnAdvance.asset

```yaml
Room Name: "Pawn Advance"
Room Description: "Enemy pawns march forward."
Room Type: NormalCombat
Board Data: [Small_6x6 연결]
Victory Condition: [CaptureEnemyQueen 연결]
Enemy Pieces: [적 폰 위주로 설정]
Use Standard Player Setup: true
```

#### 4.3 Elite_MutatedKnight.asset

```yaml
Room Name: "Mutated Knight"
Room Description: "Face a knight with strange powers."
Room Type: EliteCombat
Board Data: [Standard_8x8 연결]
Enemy Mutations:
  - [TeleportingKnight 연결]
Possible Artifact Rewards: [3개 선택]
```

#### 4.4 Boss_DarkKing.asset

```yaml
Room Name: "The Dark King"
Room Description: "Final boss encounter."
Room Type: Boss
Board Data: [Obstacles_5x8 연결]
Enemy Mutations:
  - [ExplosiveKing 연결]
Possible Artifact Rewards: [Legendary 등급 3개]
```

#### 4.5 Rest_RepairStation.asset

```yaml
Room Name: "Repair Station"
Room Description: "Repair broken pieces."
Room Type: Rest
Board Data: [아무거나]
Victory Condition: (비워둠)
```

---

## 5. AIConfig 에셋 생성 (3개)

### 생성 위치
`Assets/Data/AI/`

### 생성 방법

1. **Project 창에서:**
   ```
   우클릭 > Create > Mutating Gambit > AI Config
   ```

2. **Inspector에서 설정**

### 필요한 AI Configs

#### 5.1 AI_Easy.asset

```yaml
Config Name: "Easy AI"
Search Depth: 2
Max Time Per Move: 500
Use Iterative Deepening: false
Randomness Factor: 0.3
Piece Values:
  - Pawn: 100
  - Knight: 300
  - Bishop: 300
  - Rook: 500
  - Queen: 900
  - King: 10000
```

#### 5.2 AI_Normal.asset

```yaml
Config Name: "Normal AI"
Search Depth: 3
Max Time Per Move: 1000
Use Iterative Deepening: true
Randomness Factor: 0.1
Piece Values: [Easy와 동일]
```

#### 5.3 AI_Hard.asset

```yaml
Config Name: "Hard AI"
Search Depth: 4
Max Time Per Move: 2000
Use Iterative Deepening: true
Randomness Factor: 0.05
Piece Values: [Easy와 동일]
```

---

## 6. VictoryCondition 에셋 생성

### 생성 위치
`Assets/Data/VictoryConditions/`

### 필요한 Victory Conditions

#### 6.1 CheckmateIn5Moves.asset

```
메뉴: Create > Victory Conditions > Checkmate In N Moves

Condition Name: "Checkmate in 5 Moves"
Description: "Achieve checkmate within 5 moves."
Condition Type: CheckmateInNMoves
Max Moves: 5
```

#### 6.2 CaptureEnemyQueen.asset

```
메뉴: Create > Victory Conditions > Capture Specific Piece

Condition Name: "Capture Enemy Queen"
Description: "Capture the enemy Queen to win."
Condition Type: CaptureSpecificPiece
Target Piece Type: Queen
Target Team: (적 팀)
```

---

## 검증 체크리스트 ✓

생성 후 다음을 확인하세요:

### 폴더 구조
```
Assets/Data/
├── Mutations/      ✓ 10개 에셋
├── Artifacts/      ✓ 20개 에셋
├── Boards/         ✓ 3개 에셋
├── Rooms/          ✓ 5개 에셋
├── AI/             ✓ 3개 에셋
└── VictoryConditions/ ✓ 2개 에셋
```

### RoomData 연결 확인
- [ ] 각 RoomData에 BoardData가 연결되어 있음
- [ ] Combat 방에 VictoryCondition이 연결되어 있음
- [ ] Elite/Boss 방에 Enemy Mutations이 설정되어 있음
- [ ] Possible Artifact Rewards가 3개씩 설정되어 있음

### 테스트
- [ ] DungeonMapGenerator 실행 시 맵이 생성됨
- [ ] Room에 진입 시 에러가 없음
- [ ] AI가 정상적으로 동작함

---

## 문제 해결 🔧

### "Create 메뉴에 항목이 없어요"
- Unity Editor를 재시작하세요
- `Assets > Refresh` 실행
- ScriptableObject 클래스에 `[CreateAssetMenu]` 속성이 있는지 확인

### "에셋이 연결되지 않아요"
- Inspector에서 작은 동그라미 아이콘 클릭
- 목록에서 에셋 선택
- 또는 Project 창에서 드래그 앤 드롭

### "자동 생성이 작동하지 않아요"
- `Assets/Scripts/Editor/` 폴더가 있는지 확인
- Unity Console에서 에러 메시지 확인
- 수동 생성 방법 사용

---

## 다음 단계

에셋 생성 후:

1. **DungeonManager 설정**
   - Hierarchy에 DungeonManager GameObject 생성
   - 필요한 참조 연결

2. **게임 시작**
   - DungeonManager.StartNewRun() 호출
   - 던전 맵 확인

3. **밸런싱**
   - AI 난이도 조정
   - Artifact 효과 테스트
   - Victory Condition 난이도 조정

---

**Good Luck! 🎮**
