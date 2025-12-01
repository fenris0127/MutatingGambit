# Mutating Gambit - 스크립트 장착 가이드

## Unity에서 스크립트를 GameObject에 장착하는 방법

### 방법 1: Drag & Drop (가장 쉬움)
1. **Project 창**에서 스크립트 파일 찾기
   - `Assets/Scripts/` 폴더 내에서 원하는 스크립트 찾기
2. **Hierarchy 창**에서 GameObject 선택
3. **스크립트 파일을 드래그**하여 **Inspector 창**에 드롭

### 방법 2: Add Component 버튼
1. **Hierarchy 창**에서 GameObject 선택
2. **Inspector 창** 하단의 **"Add Component"** 버튼 클릭
3. 검색창에 스크립트 이름 입력 (예: "DungeonMapUI")
4. 나타난 스크립트 클릭

### 방법 3: Inspector에서 직접 드래그
1. **Hierarchy 창**에서 GameObject 선택
2. **Project 창**에서 스크립트 찾기
3. **스크립트를 드래그**하여 **Inspector 창의 빈 공간**에 드롭

---

## MutatingGambit 프로젝트 스크립트 장착 가이드

### 1. UI 스크립트 장착

#### Canvas 구조
```
Canvas (Screen Space - Overlay)
├── LeftSidebar (GameObject with Image)
│   ├── PlayerPiecesPanel (GameObject with Image)
│   ├── MutationsPanel (GameObject with Image)
│   └── CurrencyDisplay (GameObject with Image)
├── CenterArea (GameObject)
│   ├── TurnIndicator (GameObject with Image + Text)
│   └── BoardContainer (GameObject)
├── RightSidebar (GameObject with Image)
│   ├── ArtifactsPanel (GameObject with Image)
│   ├── MoveHistoryPanel (GameObject with Image + ScrollRect)
│   └── VictoryConditionPanel (GameObject with Image)
└── DungeonMapPanel (GameObject with Image)
```

#### A. 던전 맵 UI
**GameObject**: `DungeonMapPanel`
- **스크립트**: `DungeonMapUI.cs`
- **필수 참조**:
  - Map Panel (자기 자신)
  - Node Container (자식 GameObject)
  - Node Button Prefab (Resources 또는 직접 할당)
  - Line Prefab (연결선 프리팹)

**설정 방법**:
```
1. DungeonMapPanel GameObject 선택
2. Add Component → DungeonMapUI 검색 후 추가
3. Inspector에서 참조 설정:
   - Map Panel: 자기 자신 드래그
   - Node Container: 자식 "NodeContainer" 드래그
   - Node Button Prefab: Prefabs 폴더에서 드래그
   - Line Prefab: Prefabs 폴더에서 드래그
```

#### B. 휴식 방 UI
**GameObject**: `RepairPanel` (부모 Canvas 아래)
- **스크립트**: `RepairUI.cs`
- **필수 참조**:
  - Repair Panel (자기 자신)
  - Broken Piece Container (자식 GameObject)
  - Title Text, Repairs Text (Text 컴포넌트들)
  - Continue Button (Button)

**설정 방법**:
```
1. RepairPanel GameObject 선택
2. Add Component → RepairUI
3. Inspector에서 모든 UI 참조 드래그 앤 드롭
```

### 2. 게임 매니저 스크립트 장착

#### A. DungeonManager (싱글톤)
**GameObject**: 새로운 Empty GameObject 생성 (`DungeonManager`)
- **위치**: Hierarchy 최상단 (어떤 부모도 없음)
- **스크립트**: `DungeonManager.cs`

**설정 방법**:
```
1. Hierarchy 빈 공간 우클릭 → Create Empty
2. 이름을 "DungeonManager"로 변경
3. Add Component → DungeonManager
4. Inspector에서 참조 설정:
   - UI 매니저들
   - 보드 매니저
   - 현재 던전 설정 등
```

#### B. Board (체스 보드)
**GameObject**: `Board` (Scene에 이미 있거나 새로 생성)
- **스크립트**: `Board.cs`

**설정 방법**:
```
1. Board GameObject 선택
2. Add Component → Board
3. Inspector에서:
   - Artifact Manager 참조
   - Piece Prefab 할당
```

### 3. 비주얼 스크립트 장착

#### PieceVisualizer (기물 시각화)
**GameObject**: 각 Piece GameObject
- **스크립트**: `PieceVisualizer.cs`
- **자동 생성**: Board.SpawnPiece()에서 자동으로 추가됨

**수동 설정 (필요시)**:
```
1. Piece GameObject 선택
2. Add Component → PieceVisualizer
3. 스크립트가 자동으로 SpriteRenderer 찾음
```

### 4. 에디터 스크립트 (Editor 폴더)

#### MVPAssetCreator
**사용 방법**:
```
1. Unity 상단 메뉴: Tools → Create MVP Assets
2. 창이 열리면 원하는 에셋 타입 체크
3. "Create Selected Assets" 버튼 클릭
```

**주의**: `Editor` 폴더의 스크립트는 GameObject에 장착하지 않습니다!

---

## 주요 스크립트 장착 체크리스트

### 필수 스크립트 (게임 실행 필요)
- [ ] **Canvas**에 EventSystem 있는지 확인 (UI 상호작용용)
- [ ] **DungeonManager** GameObject 생성 및 스크립트 장착
- [ ] **Board** GameObject에 Board.cs 장착
- [ ] **DungeonMapUI** Panel에 스크립트 장착 및 참조 설정
- [ ] **RepairUI** Panel에 스크립트 장착 및 참조 설정

### 선택 스크립트 (기능별)
- [ ] **CurrencyDisplay** - 화폐 표시용
- [ ] **MutationPanel** - Mutation 표시용
- [ ] **ArtifactPanel** - Artifact 표시용

---

## 자주 하는 실수 및 해결 방법

### 1. "Missing Reference" 에러
**원인**: Inspector에서 참조가 설정되지 않음
**해결**: 
- Inspector 확인하여 빈 참조 찾기
- 해당 GameObject나 Prefab을 드래그하여 할당

### 2. "NullReferenceException" 에러
**원인**: 스크립트가 필요한 컴포넌트를 찾지 못함
**해결**:
- GameObject에 필수 컴포넌트 있는지 확인
- 예: UI 스크립트는 RectTransform 필요 (Canvas 하위에 있어야 함)

### 3. 스크립트가 Add Component에 안 나타남
**원인**: 
- 스크립트에 컴파일 에러가 있음
- `Editor` 폴더의 스크립트임 (GameObject에 장착 불가)

**해결**:
- Console 창에서 에러 확인 및 수정
- Editor 스크립트는 메뉴에서 실행

### 4. UI가 클릭되지 않음
**원인**: EventSystem이 없음
**해결**:
```
Hierarchy → 우클릭 → UI → Event System
```

---

## Inspector 참조 설정 팁

### Prefab 할당
```
1. Project 창에서 Prefab 찾기
2. Inspector의 참조 필드로 드래그
```

### Scene의 GameObject 할당
```
1. Hierarchy에서 GameObject 찾기
2. Inspector의 참조 필드로 드래그

또는

1. Inspector 참조 필드 우측 ⊙ 버튼 클릭
2. 팝업 창에서 GameObject 선택
```

### 자식 GameObject 빠르게 할당
```
1. Inspector에서 참조 필드 ⊙ 버튼 클릭
2. 검색창에 GameObject 이름 입력
3. 결과에서 선택
```

---

## 스크립트 실행 순서

Unity에서 스크립트 실행 순서 변경:
```
Edit → Project Settings → Script Execution Order

추천 순서:
1. DungeonManager (가장 먼저)
2. Board
3. UI 스크립트들
```

---

## 추가 도움말

### 스크립트가 제대로 장착되었는지 확인
1. GameObject 선택
2. Inspector에서 스크립트 이름이 **회색**이 아닌 **밝은 글씨**로 표시되어야 함
3. 스크립트 옆에 ⚙️ 아이콘 클릭 → "Edit Script"로 파일 열기 가능하면 정상

### Debug 모드로 참조 확인
```
Inspector 우측 상단 ⋮ 클릭 → Debug 모드
→ 모든 private 변수까지 볼 수 있음
```
