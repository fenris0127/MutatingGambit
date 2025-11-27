# Mutating Gambit - UI Layout & Mutation System Guide

## UI Layout Overview

![UI Layout](C:/Users/김민재/.gemini/antigravity/brain/311c84e4-3494-4eab-972f-fc6715c014b8/game_ui_layout_1764204568500.png)

### Screen Layout Structure (1920x1080 기준)

#### 1. 중앙 보드 영역 (Center Board Area)
- **위치**: 화면 중앙 (360, 100) ~ (1560, 980)
- **크기**: 1200x880px
- **내용**: 8x8 체스보드
- **인터랙션**: 클릭으로 기물 선택 및 이동

#### 2. 왼쪽 사이드바 (Left Sidebar) - 360px width
**A. Player Pieces Panel** (상단)
- **위치**: (10, 10) ~ (350, 250)
- **내용**:
  - 살아있는 기물 아이콘 (8개)
  - 파손된 기물 아이콘 (빨간 테두리 표시)
  - 각 기물 위에 마우스 오버 시 적용된 Mutation 툴팁 표시

**B. Active Mutations Panel** (중앙)
- **위치**: (10, 260) ~ (350, 550)
- **내용**:
  - 현재 활성화된 Mutation 리스트 (3-5개)
  - 각 Mutation: 아이콘 + 이름 + 간단한 설명
  - 마우스 오버로 상세 설명 표시
- **스타일**: 카드 형태, 보라색 그라데이션 배경

**C. Currency Display** (하단)
- **위치**: (10, 560) ~ (350, 620)
- **내용**: 골드/화폐 아이콘 + 숫자

#### 3. 오른쪽 사이드바 (Right Sidebar) - 360px width
**A. Artifacts Panel** (상단)
- **위치**: (1570, 10) ~ (1910, 280)
- **내용**:
  - 획득한 Artifact 아이콘들 (2-4개)
  - 각 Artifact 툴팁으로 효과 설명
- **스타일**: 황금색 테두리

**B. Move History Panel** (중앙)
- **위치**: (1570, 290) ~ (1910, 650)
- **내용**:
  - 스크롤 가능한 이동 기록
  - 대수학 표기법 (e.g., "1. e4 e5", "2. Nf3 Nc6")
  - 최근 5-10개 이동 표시

**C. Victory Condition Panel** (하단)
- **위치**: (1570, 660) ~ (1910, 800)
- **내용**:
  - 현재 방의 승리 조건 (예: "적 킹을 파괴하라", "5턴 안에 체크메이트")
  - 진행도 표시 (해당되는 경우)

#### 4. 상단 바 (Top Bar)
**A. Turn Indicator** (중앙)
- **위치**: (760, 10) ~ (1160, 80)
- **내용**: "White's Turn" / "Black's Turn" + 턴 번호
- **스타일**: 팀 색상으로 강조 (White: 밝은 배경, Black: 어두운 배경)

**B. Pause/Settings Button** (우측상단)
- **위치**: (1870, 10) ~ (1910, 50)
- **아이콘**: ⚙️ 또는 ⏸️

#### 5. 하단 액션 바 (Bottom Action Bar) - 선택사항
- **위치**: (360, 1000) ~ (1560, 1070)
- **내용**: 특수 능력 버튼, 턴 종료 버튼 등

---

## Mutation System - 개선 사항

### 1. Mutation 시각적 피드백
- 기물에 Mutation이 적용되면 **파티클 이펙트** 표시
- Mutation 타입별 색상 코딩:
  - 이동 관련: 파란색
  - 공격 관련: 빨간색
  - 특수 효과: 보라색

### 2. Mutation UI 컴포넌트
새롭게 추가할 스크립트:
- `MutationTooltip.cs`: Mutation 정보를 보여주는 툴팁
- `MutationIconDisplay.cs`: Mutation 아이콘 렌더링
- `PieceMutationIndicator.cs`: 기물 위에 작은 Mutation 마크 표시

### 3. Mutation 적용 플로우
```
1. 방 클리어 → Reward UI 표시
2. Mutation 3개 제시 (카드 형태)
3. 플레이어가 1개 선택
4. 선택한 Mutation을 적용할 기물 선택
5. 애니메이션과 함께 적용 (파티클 + 사운드)
6. Left Sidebar의 Active Mutations Panel에 추가
```

### 4. Unity Scene 구성
```
Canvas (Screen Space - Overlay)
├── LeftSidebar
│   ├── PlayerPiecesPanel
│   │   └── Grid (2x4) - PieceIcon prefabs
│   ├── MutationsPanel
│   │   └── Vertical Layout - MutationCard prefabs
│   └── CurrencyDisplay
├── CenterArea
│   ├── TurnIndicator
│   └── BoardContainer
│       └── Board (8x8 Grid)
├── RightSidebar
│   ├── ArtifactsPanel
│   │   └── Grid - ArtifactIcon prefabs
│   ├── MoveHistoryPanel
│   │   └── Scroll View - MoveHistoryEntry prefabs
│   └── VictoryConditionPanel
└── TopBar
    └── PauseButton
```

### 5. 권장 컴포넌트 크기
- Board Tile: 100x100px
- Piece Icon (Sidebar): 60x60px
- Mutation Card: 320x80px
- Artifact Icon: 80x80px
- Move History Entry: 320x30px

---

## 개선된 Mutation 사용 예시

### 적용 방법
```csharp
// 1. Mutation 가져오기
Mutation teleportingKnight = Resources.Load<Mutation>("Mutations/TeleportingKnight");

// 2. 기물에 적용
Piece myKnight = board.GetPiece(new Vector2Int(1, 0));
MutationManager.Instance.ApplyMutation(myKnight, teleportingKnight);

// 3. UI 업데이트는 자동으로 이루어짐 (MutationsPanel이 구독 중)
```

### Mutation 제거
```csharp
MutationManager.Instance.RemoveMutation(myKnight, teleportingKnight);
```

### 현재 적용된 Mutation 조회
```csharp
List<Mutation> mutations = MutationManager.Instance.GetMutations(myKnight);
foreach (var m in mutations)
{
    Debug.Log($"Mutation: {m.MutationName}");
}
```
