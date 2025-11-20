# 변이하는 기보 (The Mutating Gambit) - TDD 개발 계획

## 개요
이 문서는 변이하는 기보 게임을 TDD(Test-Driven Development) 방식으로 개발하기 위한 계획서입니다.
각 테스트는 Red→Green→Refactor 사이클을 따르며, Kent Beck의 Tidy First 원칙을 적용합니다.

## 개발 순서
1. Core Chess Logic (기본 체스 로직)
2. Board System (보드 시스템)
3. Piece System (기물 시스템)
4. Mutation System (변이 시스템)
5. Artifact System (유물 시스템)
6. Combat/Puzzle System (전투/퍼즐 시스템)
7. Dungeon Map System (던전 맵 시스템)
8. AI System (AI 시스템)
9. UI System (UI 시스템)
10. Meta Progression (메타 프로그레션)

---

## Phase 1: Core Chess Logic (기본 체스 로직)

### Board Representation
- [ ] Test: 보드는 8x8 그리드를 생성할 수 있다
- [ ] Test: 보드는 특정 위치에 기물을 배치할 수 있다
- [ ] Test: 보드는 특정 위치의 기물을 조회할 수 있다
- [ ] Test: 보드는 비어있는 위치를 null로 반환한다
- [ ] Test: 보드는 범위를 벗어난 위치 접근 시 예외를 발생시킨다

### Position System
- [ ] Test: Position은 파일(a-h)과 랭크(1-8)로 생성할 수 있다
- [ ] Test: Position은 좌표(x, y)로 생성할 수 있다
- [ ] Test: Position은 체스 표기법 문자열(예: "e4")로 생성할 수 있다
- [ ] Test: Position은 유효하지 않은 좌표에 대해 예외를 발생시킨다
- [ ] Test: 두 Position 간의 동등성을 비교할 수 있다

### Basic Piece Definition
- [ ] Test: Piece는 색상(White/Black)을 가진다
- [ ] Test: Piece는 타입(King/Queen/Rook/Bishop/Knight/Pawn)을 가진다
- [ ] Test: Piece는 현재 위치를 가진다
- [ ] Test: Piece는 이동 가능한지 여부를 반환할 수 있다

---

## Phase 2: Board System (보드 시스템)

### Standard Board Setup
- [ ] Test: 표준 체스 초기 배치를 설정할 수 있다
- [ ] Test: 백색 폰은 2번째 랭크에 배치된다
- [ ] Test: 흑색 폰은 7번째 랭크에 배치된다
- [ ] Test: 룩은 a1, h1, a8, h8 위치에 배치된다
- [ ] Test: 나이트는 b1, g1, b8, g8 위치에 배치된다
- [ ] Test: 비숍은 c1, f1, c8, f8 위치에 배치된다
- [ ] Test: 퀸은 d1, d8 위치에 배치된다
- [ ] Test: 킹은 e1, e8 위치에 배치된다

### Custom Board Sizes
- [ ] Test: 5x5 크기의 보드를 생성할 수 있다
- [ ] Test: 6x8 크기의 보드를 생성할 수 있다
- [ ] Test: 커스텀 크기 보드에서 위치 유효성을 검증한다
- [ ] Test: 커스텀 보드는 JSON 데이터로부터 로드될 수 있다

### Board Obstacles
- [ ] Test: 보드에 벽(Wall) 타일을 배치할 수 있다
- [ ] Test: 보드에 물(Water) 타일을 배치할 수 있다
- [ ] Test: 장애물 타일은 기물이 차지할 수 없다
- [ ] Test: 장애물 타일은 기물이 통과할 수 없다

---

## Phase 3: Piece System (기물 시스템)

### Pawn Movement
- [ ] Test: 폰은 앞으로 1칸 이동할 수 있다
- [ ] Test: 폰은 첫 이동시 2칸 이동할 수 있다
- [ ] Test: 폰은 대각선 앞 적 기물을 잡을 수 있다
- [ ] Test: 폰은 뒤로 이동할 수 없다
- [ ] Test: 폰은 앞에 기물이 있으면 전진할 수 없다
- [ ] Test: 폰은 8번째(백색) 또는 1번째(흑색) 랭크 도달 시 승급한다

### Rook Movement
- [ ] Test: 룩은 수직으로 이동할 수 있다
- [ ] Test: 룩은 수평으로 이동할 수 있다
- [ ] Test: 룩은 대각선으로 이동할 수 없다
- [ ] Test: 룩은 다른 기물을 뛰어넘을 수 없다
- [ ] Test: 룩은 적 기물을 잡을 수 있다
- [ ] Test: 룩은 아군 기물이 있는 칸으로 이동할 수 없다

### Knight Movement
- [ ] Test: 나이트는 L자 형태로 이동한다
- [ ] Test: 나이트는 다른 기물을 뛰어넘을 수 있다
- [ ] Test: 나이트는 8가지 가능한 위치로 이동할 수 있다
- [ ] Test: 나이트는 보드 경계를 벗어날 수 없다

### Bishop Movement
- [ ] Test: 비숍은 대각선으로 이동할 수 있다
- [ ] Test: 비숍은 수직/수평으로 이동할 수 없다
- [ ] Test: 비숍은 다른 기물을 뛰어넘을 수 없다
- [ ] Test: 비숍은 적 기물을 잡을 수 있다

### Queen Movement
- [ ] Test: 퀸은 수직/수평/대각선으로 이동할 수 있다
- [ ] Test: 퀸은 다른 기물을 뛰어넘을 수 없다
- [ ] Test: 퀸은 룩과 비숍의 이동을 결합한 패턴을 가진다

### King Movement
- [ ] Test: 킹은 모든 방향으로 1칸 이동할 수 있다
- [ ] Test: 킹은 체크 상태인 칸으로 이동할 수 없다
- [ ] Test: 킹은 캐슬링을 수행할 수 있다
- [ ] Test: 킹이 체크 상태일 때를 감지할 수 있다
- [ ] Test: 킹이 체크메이트 상태일 때를 감지할 수 있다

---

## Phase 4: Mutation System (변이 시스템)

### Mutation Framework
- [ ] Test: Mutation은 특정 기물 타입에 적용될 수 있다
- [ ] Test: Mutation은 이동 규칙을 추가할 수 있다
- [ ] Test: Mutation은 이동 규칙을 제거할 수 있다
- [ ] Test: Mutation은 이동 규칙을 수정할 수 있다
- [ ] Test: 여러 Mutation이 동시에 적용될 수 있다

### Specific Mutations - Rook
- [ ] Test: "도약하는 룩"은 아군 기물 1개를 뛰어넘을 수 있다
- [ ] Test: "도약하는 룩"은 적 기물은 뛰어넘을 수 없다
- [ ] Test: "도약하는 룩"은 2개 이상의 기물을 뛰어넘을 수 없다

### Specific Mutations - Knight
- [ ] Test: "분열하는 나이트"는 적 기물을 잡을 때 원래 위치에 폰을 생성한다
- [ ] Test: "분열하는 나이트"가 생성한 폰은 생성한 플레이어의 색상이다
- [ ] Test: "분열하는 나이트"는 원래 위치가 비어있을 때만 폰을 생성한다

### Specific Mutations - Bishop
- [ ] Test: "유리 비숍"은 정확히 3칸만 이동할 수 있다
- [ ] Test: "유리 비숍"은 1칸이나 2칸 이동할 수 없다
- [ ] Test: "유리 비숍"은 4칸 이상 이동할 수 없다

### Mutation Persistence
- [ ] Test: 시작 변이는 게임 전체에 걸쳐 유지된다
- [ ] Test: 시작 변이는 저장되고 로드될 수 있다
- [ ] Test: 변이된 기물은 시각적으로 구별된다

---

## Phase 5: Artifact System (유물 시스템)

### Artifact Framework
- [ ] Test: Artifact는 전역 규칙을 변경할 수 있다
- [ ] Test: Artifact는 플레이어와 AI 모두에게 적용된다
- [ ] Test: 여러 Artifact가 중첩될 수 있다
- [ ] Test: Artifact는 활성화/비활성화될 수 있다

### Specific Artifacts
- [ ] Test: "중력 거울"은 턴 종료 시 모든 기물을 하단으로 1칸 이동시킨다
- [ ] Test: "왕의 그림자"는 킹 이동 시 원래 위치에 장애물을 생성한다
- [ ] Test: "기병대의 돌격"은 나이트가 기물을 잡은 후 1칸 더 전진할 수 있게 한다
- [ ] Test: "연쇄 번개"는 기물이 잡힐 때 주변 1칸의 무작위 기물도 제거한다
- [ ] Test: "승급 특권"은 폰이 3개의 적 기물을 잡으면 퀸으로 승급한다

### Artifact Interactions
- [ ] Test: 서로 충돌하는 Artifact 규칙의 우선순위를 처리한다
- [ ] Test: Artifact 조합으로 시너지 효과를 생성한다
- [ ] Test: Artifact 효과를 시각적으로 표시한다

---

## Phase 6: Combat/Puzzle System (전투/퍼즐 시스템)

### Room Types
- [ ] Test: 일반 전투 방을 생성할 수 있다
- [ ] Test: 정예 전투 방을 생성할 수 있다
- [ ] Test: 보스 전투 방을 생성할 수 있다
- [ ] Test: 보물 방을 생성할 수 있다
- [ ] Test: 휴식/수리 방을 생성할 수 있다

### Victory Conditions
- [ ] Test: "N수 안에 체크메이트" 조건을 검증할 수 있다
- [ ] Test: "특정 기물 제거" 조건을 검증할 수 있다
- [ ] Test: "킹을 특정 위치로 이동" 조건을 검증할 수 있다
- [ ] Test: 복합 승리 조건을 처리할 수 있다

### Piece Damage System
- [ ] Test: 기물은 HP를 가진다
- [ ] Test: 기물이 잡히면 "파손" 상태가 된다
- [ ] Test: 파손된 기물은 전투에서 사용할 수 없다
- [ ] Test: 휴식/수리 방에서 기물을 수리할 수 있다
- [ ] Test: 킹이 파손되면 게임 오버가 된다

---

## Phase 7: Dungeon Map System (던전 맵 시스템)

### Map Generation
- [ ] Test: 노드 기반 맵을 생성할 수 있다
- [ ] Test: 맵은 시작점과 끝점을 가진다
- [ ] Test: 노드 간 연결이 유효하다
- [ ] Test: 플레이어는 인접한 노드로만 이동할 수 있다

### Map Navigation
- [ ] Test: 플레이어의 현재 위치를 추적한다
- [ ] Test: 방문한 노드를 기록한다
- [ ] Test: 선택 가능한 다음 노드를 표시한다
- [ ] Test: 층 완료 시 다음 층으로 이동한다

### Room Rewards
- [ ] Test: 전투 완료 시 유물 선택지 3개를 제공한다
- [ ] Test: 정예 전투는 더 좋은 유물을 제공한다
- [ ] Test: 보물 방은 즉시 유물을 제공한다
- [ ] Test: 보상 선택이 저장된다

---

## Phase 8: AI System (AI 시스템)

### Basic AI
- [ ] Test: AI는 가능한 모든 수를 계산할 수 있다
- [ ] Test: AI는 현재 변이 규칙을 적용한다
- [ ] Test: AI는 현재 유물 규칙을 적용한다
- [ ] Test: AI는 합법적인 수만 선택한다

### Evaluation Function
- [ ] Test: AI는 보드 상태를 평가할 수 있다
- [ ] Test: AI는 기물 가치를 계산한다
- [ ] Test: AI는 위치 우위를 평가한다
- [ ] Test: AI는 변이/유물 시너지를 고려한다

### Advanced AI (MCTS or Alpha-Beta)
- [ ] Test: AI는 여러 수 앞을 예측한다
- [ ] Test: AI는 시간 제한 내에 수를 결정한다
- [ ] Test: AI 난이도를 조절할 수 있다
- [ ] Test: AI는 특정 전략 패턴을 따른다

---

## Phase 9: UI System (UI 시스템)

### Board Display
- [ ] Test: 보드와 기물을 화면에 표시한다
- [ ] Test: 가능한 이동을 하이라이트한다
- [ ] Test: 변이된 기물을 시각적으로 구별한다
- [ ] Test: 현재 적용된 유물 목록을 표시한다

### User Input
- [ ] Test: 마우스 클릭으로 기물을 선택할 수 있다
- [ ] Test: 드래그 앤 드롭으로 기물을 이동할 수 있다
- [ ] Test: 불법적인 수를 시도하면 피드백을 제공한다
- [ ] Test: 이동 취소(Undo) 기능을 제공한다

### Game State Display
- [ ] Test: 현재 턴을 표시한다
- [ ] Test: 승리 조건을 표시한다
- [ ] Test: 남은 수를 표시한다
- [ ] Test: 파손된 기물 상태를 표시한다

---

## Phase 10: Meta Progression (메타 프로그레션)

### Currency System
- [ ] Test: 게임 오버 시 "기보 조각"을 획득한다
- [ ] Test: 획득량은 진행 거리에 비례한다
- [ ] Test: 기보 조각은 계정에 저장된다

### Unlock System
- [ ] Test: 새로운 시작 변이를 해금할 수 있다
- [ ] Test: 새로운 유물을 해금할 수 있다
- [ ] Test: 편의 기능을 해금할 수 있다
- [ ] Test: 해금 상태가 저장된다

### Save/Load System
- [ ] Test: 게임 진행 상황을 저장할 수 있다
- [ ] Test: 저장된 게임을 불러올 수 있다
- [ ] Test: 메타 진행 상황이 유지된다

---

## Implementation Notes

### 테스트 실행 규칙
1. 각 테스트는 독립적으로 실행 가능해야 함
2. 테스트는 빠르게 실행되어야 함 (< 100ms)
3. 장시간 실행 테스트는 별도로 표시
4. 모든 커밋 전에 전체 테스트 스위트 실행

### 리팩토링 체크포인트
- 각 Phase 완료 시 코드 중복 제거
- 매 5개 테스트마다 구조 개선 검토
- 성능 최적화는 기능 완성 후 진행

### 기술 스택
- Unity 2023.x LTS
- 테스트 프레임워크: Unity Test Framework
- 코드 스타일: C# 표준 명명 규칙
- 버전 관리: Git (커밋 메시지에 [TEST], [IMPL], [REFACTOR] 태그 사용)

---

## 다음 단계
"go" 명령을 입력하면 다음 미완료 테스트를 찾아 구현을 시작합니다.
각 테스트는 Red→Green→Refactor 사이클을 따릅니다.
