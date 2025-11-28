# 코드 리뷰 - MutatingGambit 테스트 수정
**날짜:** 2025-11-28  
**리뷰어:** AI 코드 리뷰어  
**범위:** 테스트 인프라 및 핵심 시스템 수정

## 요약

테스트 스위트 및 핵심 게임 시스템의 컴파일 오류를 수정한 최근 변경사항을 검토했습니다. 6개 파일에서 8개의 개별 이슈를 수정했습니다. 대부분의 변경사항은 테스트 인프라 설정 및 누락된 의존성에 초점을 맞춘 중간 우선순위입니다.

## 검토된 파일

### 1. `MutationManager.cs`
**상태:** ✅ 수정 완료  
**변경사항:** 누락된 네임스페이스 import 추가

#### 치명적 이슈
없음

#### 경고
- **누락된 네임스페이스 import** (수정 완료)
  - **문제:** `using MutatingGambit.Systems.SaveLoad;` 누락으로 인한 `GlobalDataManager` 참조 실패
  - **적용된 수정:** 4번째 줄에 네임스페이스 import 추가
  - **영향:** Codex 시스템에서 뮤테이션 잠금 해제 기능 활성화

#### 제안사항
- 직접적인 싱글톤 접근 대신 `GlobalDataManager.Instance`에 대한 의존성 주입 고려
- `GlobalDataManager.Instance.UnlockMutation()` 호출 전 null 체크 추가

```csharp
// 현재 코드
if (GlobalDataManager.Instance != null)
{
    GlobalDataManager.Instance.UnlockMutation(mutation.MutationName);
}

// 개선 방안
private IGlobalDataManager globalDataManager;

public void Initialize(IGlobalDataManager dataManager) 
{
    globalDataManager = dataManager;
}

// 사용 시
globalDataManager?.UnlockMutation(mutation.MutationName);
```

---

### 2. `Piece.cs`
**상태:** ✅ 개선됨  
**변경사항:** `PromoteToQueen()` 메서드 추가

#### 치명적 이슈
없음

#### 경고
- **PromoteToQueen의 하드코딩된 이동 규칙**
  - **문제:** 매번 `StraightLineRule`과 `DiagonalRule`의 새 인스턴스 생성
  - **위험:** 규칙이 제대로 파괴되지 않으면 메모리 누수 가능성
  - **권장사항:** 규칙 팩토리 또는 캐시된 규칙 인스턴스 사용

```csharp
// 현재 방식
public void PromoteToQueen()
{
    pieceType = PieceType.Queen;
    movementRules.Clear();
    AddMovementRule(ScriptableObject.CreateInstance<StraightLineRule>());
    AddMovementRule(ScriptableObject.CreateInstance<DiagonalRule>());
}

// 개선 방안
public void PromoteToQueen(IPieceRuleFactory ruleFactory)
{
    pieceType = PieceType.Queen;
    movementRules.Clear();
    AddMovementRule(ruleFactory.GetQueenRules());
}
```

#### 제안사항
- 매개변수 유효성 검사 추가 (null 체크)
- 승급을 더 유연하게 만들기 (모든 기물 타입으로 승급 가능)
- 승급에 대한 이벤트/콜백 추가 (UI/오디오 피드백용)

---

### 3. `BackwardPawnRule.cs`
**상태:** ✅ 신규 파일  
**변경사항:** 역방향 폰 이동을 위한 새로운 이동 규칙 생성

#### 치명적 이슈
없음

#### 경고
없음 - 깔끔한 구현

#### 제안사항
- **전방 폰 규칙과의 코드 중복**
  - 공통 로직을 기본 폰 규칙 클래스로 추출 고려
  - 방향 매개변수를 가진 `DirectionalPawnRule` 생성

```csharp
// 리팩토링 제안
public class DirectionalPawnRule : MovementRule
{
    [SerializeField] private int direction = 1; // 1은 전진, -1은 후진
    
    public override List<Vector2Int> GetValidMoves(...)
    {
        int targetY = fromPosition.y + direction;
        // 나머지 로직
    }
}
```

- 매개변수 및 반환 값에 대한 XML 문서화 추가
- 엣지 케이스 고려: 보드 가장자리에서는 어떻게 되는가?

---

### 4. `DungeonRunTests.cs`
**상태:** ✅ 수정 완료  
**변경사항:** 구체적인 `TestMutation` 클래스 추가

#### 치명적 이슈
없음 (컴파일 방지 중이었음)

#### 경고
- **테스트 헬퍼 위치**
  - `TestMutation` 클래스가 테스트 클래스 내부가 아닌 파일 레벨에 있음
  - 주의하지 않으면 네임스페이스를 오염시킬 수 있음
  - **권장사항:** 별도의 테스트 헬퍼 파일로 이동하거나 테스트 클래스 내부에 중첩

```csharp
// 현재
namespace MutatingGambit.Tests.PlayMode
{
    public class DungeonRunTests { ... }
    
    public class TestMutation : Mutation { ... }
}

// 제안
namespace MutatingGambit.Tests.PlayMode
{
    public class DungeonRunTests 
    {
        private class TestMutation : Mutation { ... }
    }
}
```

#### 제안사항
- 뮤테이션 지속성에 대한 더 포괄적인 테스트 추가
- 뮤테이션 스택 동작 테스트
- 뮤테이션 호환성 검사 테스트

---

### 5. `AITests.cs`
**상태:** ✅ 개선됨  
**변경사항:** 리플렉션 기반 초기화를 사용하는 `CreateTestAIConfig()` 헬퍼 추가

#### 치명적 이슈
없음

#### 경고
- **리플렉션의 과도한 사용** (중간 우선순위)
  - **문제:** private 필드를 설정하기 위한 리플렉션 사용은 취약함
  - **위험:** 필드 이름이 변경되면 중단됨, 성능 오버헤드
  - **권장사항:** `AIConfig`에 공개 테스트 생성자 추가 또는 객체 초기화자 사용

```csharp
// 현재 방식 (취약함)
type.GetField("searchDepth", BindingFlags.NonPublic | BindingFlags.Instance)
    ?.SetValue(config, 3);

// 더 나은 방식: AIConfig에 추가
#if UNITY_INCLUDE_TESTS
public static AIConfig CreateForTesting(int searchDepth = 3, ...)
{
    var config = CreateInstance<AIConfig>();
    config.searchDepth = searchDepth;
    // ... 다른 필드 설정
    return config;
}
#endif
```

- **테스트 설정의 코드 중복**
  - 여러 테스트가 유사한 기물 구성을 생성함
  - `SetupStandardPosition()`과 같은 헬퍼 메서드로 추출 가능

#### 제안사항
- 모든 테스트에서 재생성을 피하기 위해 생성된 이동 규칙 캐시
- 잘못된 보드 상태를 처리하는 AI 테스트 추가
- 0ms 시간 제한으로 AI 동작 테스트 (엣지 케이스)
- 최종 결과뿐만 아니라 중간 상태에 대한 어설션 추가

---

### 6. `MutatingGambit.Tests.PlayMode.asmdef`
**상태:** ✅ 신규 파일  
**변경사항:** PlayMode 테스트를 위한 어셈블리 정의 생성

#### 치명적 이슈
없음

#### 경고
- **누락된 GUID** (낮은 우선순위)
  - Unity가 자동 생성하지만, 명시적 GUID가 버전 관리에 더 좋음
  - 치명적이지는 않지만 좋은 관행

#### 제안사항
- 기능별로 테스트를 여러 어셈블리로 분할 고려 (AI, Systems, UI)
- Unity 버전별 테스트 코드를 위한 버전 정의 추가

---

## 전체 평가

### 코드 품질: 7/10
- **장점:**
  - 명확하고 읽기 쉬운 코드
  - 우수한 관심사 분리
  - Unity 패턴의 적절한 사용 (ScriptableObject, MonoBehaviour)
  - 대부분의 경우 적절한 오류 처리

- **약점:**
  - 일부 하드코딩된 의존성 (싱글톤)
  - 테스트의 리플렉션 사용이 취약함
  - 제한적인 입력 유효성 검사
  - 리팩토링 기회가 있는 일부 코드 중복

### 보안: 9/10
- 노출된 비밀키나 API 키 없음
- SQL 인젝션 위험 없음 (데이터베이스 없음)
- 이동 유효성 검사기에 입력 유효성 검사 존재
- **사소한 우려:** 사용자 제공 문자열 (뮤테이션 이름 등)에 대한 살균 처리 없음

### 유지보수성: 7/10
- 좋은 명명 규칙
- 합리적인 파일 구성
- **개선 필요:**
  - 더 많은 XML 문서화 추가
  - 인터페이스를 통한 결합도 감소
  - 매직 넘버를 상수로 추출

### 테스트 커버리지: 6/10
- 좋은 AI 동작 테스트
- 기본적인 통합 테스트 존재
- **누락:**
  - 엣지 케이스 테스트
  - 성능 벤치마크
  - 네거티브 테스트 케이스 (잘못된 입력)
  - 뮤테이션 호환성 매트릭스 테스트

## 우선순위 작업 항목

### 반드시 수정 (릴리스 전)
1. 없음 - 모든 치명적 이슈 해결됨

### 수정해야 함 (스프린트 우선순위)
1. `AITests.cs`의 리플렉션을 적절한 테스트 팩토리로 교체
2. `Piece.PromoteToQueen()`에서 동적으로 생성된 ScriptableObject에 대한 메모리 관리 추가
3. 더 나은 디버깅을 위해 `MutationManager`의 오류 메시지 개선

### 개선 고려 (기술 부채)
1. 공통 테스트 설정 코드를 기본 테스트 클래스로 추출
2. 이동 규칙에 대한 규칙 팩토리 패턴 생성
3. 싱글톤 결합도를 줄이기 위한 인터페이스 추상화 추가
4. 설명 주석으로 복잡한 테스트 시나리오 문서화
5. 큰 보드 상태에 대한 AI 성능 테스트 추가

## 다음 단계 권장사항

1. **통합 테스트 추가**
   - 시작부터 승리까지의 전체 게임 흐름
   - 저장/로드 사이클 테스트
   - 뮤테이션 적용 및 제거 사이클

2. **테스트 인프라 개선**
   - 복잡한 객체에 대한 테스트 데이터 빌더 생성
   - 게임별 검사를 위한 사용자 정의 어설션 추가
   - CI/CD 테스트 자동화 설정

3. **문서화**
   - 아키텍처 의사결정 기록(ADR) 추가
   - 테스트 전략 및 커버리지 목표 문서화
   - 개발자 온보딩 가이드 생성

4. **코드 건강성**
   - 정적 분석 도구 실행 (예: Roslyn Analyzers)
   - 코드 커버리지 측정 및 개선 (목표 80%+)
   - 자동화된 코드 리뷰 검사 설정

## 결론

최근 수정사항은 컴파일 오류를 성공적으로 해결하고 테스트 인프라를 개선했습니다. 코드 품질은 의존성 관리 및 테스트 견고성 면에서 개선의 여지가 있는 좋은 수준입니다. 치명적인 보안 문제는 확인되지 않았습니다. "개선 고려" 항목에 대한 기술 부채 백로그를 구축하면서 다음 스프린트에서 "수정해야 함" 항목을 처리하는 것을 권장합니다.

**전체 등급: B+**  
코드는 장기적인 유지보수성을 위한 권장 개선사항과 함께 프로덕션 준비가 되어 있습니다.
