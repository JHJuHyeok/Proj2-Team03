# 스킬 그리드 시스템 사용자 가이드

## 이 가이드에 대하여

이 가이드는 스킬 그리드 시스템을 처음 사용하는 초보자도 쉽게 이해할 수 있도록 작성되었습니다.

---

## 1. 시스템 소개

### 1.1 스킬 그리드 시스템이란?

스킬 그리드 시스템은 **백팩 히어로(Backpack Hero)** 게임처럼 스킬을 격자(그리드)에 배치하는 시스템입니다.

**특징:**
- 스킬마다 다른 모양과 크기 (테트리스처럼)
- 드래그 앤 드롭으로 쉽게 배치
- R키 또는 우클릭으로 회전 가능
- 배치한 스킬은 자동 저장

### 1.2 스킬 모양 종류

| 모양 | 크기 | 설명 |
|------|------|------|
| 1x1 | 1칸 | 가장 기본적인 모양 |
| 2x1 | 2칸 | 가로로 긴 모양 |
| 1x2 | 2칸 | 세로로 긴 모양 |
| 2x2 | 4칸 | 정사각형 모양 |
| L자 | 3칸 | L 모양 |
| T자 | 4칸 | T 모양 |

---

## 2. 시작하기

### 2.1 프리팹 생성하기

첫 번째로 해야 할 일은 프리팹(게임 오브젝트 템플릿)을 생성하는 것입니다.

**단계:**

1. Unity 상단 메뉴에서 `Tools` 클릭
2. `Skill Grid` 클릭
3. `Create All Prefabs` 클릭
4. 콘솔에 "모든 프리팹 생성 완료!" 메시지가 뜨면 성공

**생성되는 프리팹:**
```
Assets/2. Scripts/System/Skill/UI/Grid/Prefabs/
├── SkillGridCell.prefab       # 그리드 셀
├── SkillDraggableItem.prefab  # 드래그 가능한 스킬
├── InventorySlot.prefab       # 인벤토리 슬롯
└── SkillGridSystem.prefab     # 전체 시스템 (이것만 배치하면 됨)
```

### 2.2 씬에 배치하기

1. `Prefabs` 폴더에서 `SkillGridSystem` 프리팹 찾기
2. 씬(Scene)으로 드래그 앤 드롭
3. Canvas 안에 배치되었는지 확인

---

## 2.5 새 테스트 씬 만들기 (권장)

기존 씬에서 문제가 발생하거나 스킬 그리드 시스템만 독립적으로 테스트하고 싶을 때, 새 씬을 만들어 테스트할 수 있습니다.

### 단계 1: 새 씬 생성

1. `File` > `New Scene` (또는 Ctrl+N)
2. `Basic (URP)` 또는 `Built-In` 템플릿 선택
3. 씬 이름을 `SkillGridTest` 등으로 저장

### 단계 2: 카메라 설정

1. `Main Camera` 선택
2. Inspector에서 다음 설정:
   - **Clear Flags**: Solid Color
   - **Background**: 어두운 색 (예: #1A1A2E)
   - **Projection**: Orthographic (2D) 또는 Perspective

### 단계 3: Canvas 생성

1. Hierarchy에서 우클릭 > `UI` > `Canvas`
2. Canvas 선택 후 Inspector 설정:
   - **Render Mode**: `Screen Space - Overlay` (간단한 테스트용)
   - 또는 `Screen Space - Camera`:
     - Render Mode를 `Screen Space - Camera`로 설정
     - **Render Camera**: Main Camera 드래그 할당
     - **Plane Distance**: 100
3. Canvas에 `GraphicRaycaster` 컴포넌트가 있는지 확인 (자동 추가됨)

### 단계 4: EventSystem 확인

1. Hierarchy에 `EventSystem`이 있는지 확인
2. 없다면: `GameObject` > `UI` > `Event System` 생성

### 단계 5: TestSkillDataLoader 배치

스킬 데이터를 로드하려면 `TestSkillDataLoader`가 필요합니다.

1. Hierarchy에서 우클릭 > `Create Empty`
2. 이름을 `TestSkillDataLoader`로 변경
3. `Add Component` > `Test Skill Data Loader` 검색하여 추가

**참고:** 이 컴포넌트는 별도의 Inspector 설정이 없습니다. Awake 시 자동으로 Addressables에서 데이터를 로드합니다.

### 단계 6: SkillGridSystem 배치

1. `Assets/2. Scripts/System/Skill/UI/Grid/Prefabs/SkillGridSystem.prefab` 찾기
2. Canvas 안으로 드래그 앤 드롭
3. RectTransform 설정:
   - **Anchors**: Stretch/Stretch (부모 전체 채우기)
   - 또는 원하는 위치/크기로 조정

### 단계 7: 최종 Hierarchy 구조

```
SkillGridTest (Scene)
├── Main Camera
├── EventSystem
├── TestSkillDataLoader
└── Canvas
    └── SkillGridSystem
        ├── GridContainer
        │   └── GridArea (SkillGridManager)
        └── InventoryContainer
            ├── Header
            └── ScrollView
                └── Viewport
                    └── Content
```

### 단계 8: 테스트 실행

1. `Play` 버튼 클릭
2. 콘솔 로그 확인:
   - `[TestSkillDataLoader] 테스트 스킬 데이터 로드 완료` → OK
   - `[SkillGridInitializer] 스킬 그리드 초기화 완료` → OK
3. 오른쪽 인벤토리에서 스킬을 드래그하여 왼쪽 그리드에 배치 테스트

### 문제 해결 체크리스트

| 문제 | 확인 사항 |
|------|----------|
| 스킬이 안 보임 | TestSkillDataLoader가 씬에 있는지 확인, 콘솔에서 로드 완료 로그 확인 |
| 드래그 안 됨 | EventSystem이 있는지, Canvas에 GraphicRaycaster가 있는지 확인 |
| 배치 실패 (범위 벗어남) | Canvas Render Mode 확인 (Overlay 권장) |
| 셀이 안 보임 | SkillGridSystem > GridArea의 cellPrefab, cellContainer가 할당되어 있는지 확인 |
| 데이터 로드 실패 | Addressables에 스킬 JSON이 등록되어 있는지 확인 |

### Render Mode별 설정

| 모드 | 특징 | 카메라 설정 |
|------|------|------------|
| **Screen Space - Overlay** | 화면에 직접 렌더링, 카메라 불필요 | 필요 없음 |
| **Screen Space - Camera** | 카메라 앞에 렌더링 | Render Camera 필수 |
| **World Space** | 3D 공간에 배치 | 카메라 필요 + 거리 설정 |

---

## 3. 인스펙터 설정

### 3.1 SkillGridInitializer (초기화 담당)

| 설정 (필드명) | 설명 | 기본값 |
|------|------|--------|
| Grid Controller (gridController) | 그리드 컨트롤러 참조 | 자동 할당 |
| Inventory UI (inventoryUI) | 인벤토리 UI 참조 | 자동 할당 |
| Load On Start (loadOnStart) | 시작 시 자동 로드 | true |
| Load Saved Grid Data (loadSavedGridData) | 저장된 데이터 로드 | true |

### 3.2 SkillGridManager (그리드 관리)

| 설정 (필드명) | 설명 | 기본값 |
|------|------|--------|
| Grid Width (gridWidth) | 그리드 가로 크기 | 6 |
| Grid Height (gridHeight) | 그리드 세로 크기 | 6 |
| Cell Size (cellSize) | 셀 하나의 크기 (픽셀) | 80 |
| Cell Prefab (cellPrefab) | 셀 프리팹 | 자동 할당 |

### 3.3 SkillGridController (전체 컨트롤)

| 설정 (필드명) | 설명 | 기본값 |
|------|------|--------|
| Grid Manager (gridManager) | 그리드 매니저 참조 | 자동 할당 |
| Inventory Container (inventoryContainer) | 인벤토리 컨테이너 | 자동 할당 |
| Draggable Item Prefab (draggableItemPrefab) | 드래그 아이템 프리팹 | 자동 할당 |
| Save Key (saveKey) | 저장 키 이름 | "SkillGridData" |

---

## 4. 게임 플레이 중 사용법

### 4.1 스킬 배치하기

1. **오른쪽 인벤토리**에서 스킬을 찾습니다
2. 스킬을 **마우스 왼쪽 클릭 + 드래그** 합니다
3. **왼쪽 그리드**의 원하는 위치로 이동합니다
4. 마우스를 놓으면 배치됩니다

### 4.2 스킬 회전하기

두 가지 방법이 있습니다:

| 방법 | 설명 |
|------|------|
| R키 | 드래그 중에 R키를 누르면 90도 회전 |
| 우클릭 | 그리드에 있는 스킬을 우클릭하면 회전 |

### 4.3 스킬 제거하기

1. 그리드에 있는 스킬을 **드래그** 합니다
2. 그리드 밖으로 이동하면 제거됩니다
3. 인벤토리로 돌아갑니다

### 4.4 인벤토리 필터링

인벤토리 상단의 필터를 사용할 수 있습니다:

| 필터 | 기능 |
|------|------|
| 액티브 토글 | 액티브 스킬만 표시 |
| 패시브 토글 | 패시브 스킬만 표시 |
| 등급 드롭다운 | 특정 등급만 표시 |

---

## 5. 코드에서 사용하기

### 5.1 스킬 그리드 초기화

```csharp
// SkillGridInitializer에 접근
SkillGridInitializer initializer = GetComponent<SkillGridInitializer>();

// 수동으로 초기화
initializer.Initialize();

// 초기화 완료 이벤트 구독
initializer.OnInitializationComplete += () => {
    Debug.Log("초기화 완료!");
};
```

### 5.2 배치된 스킬 확인하기

```csharp
// 특정 스킬이 배치되어 있는지 확인
bool isPlaced = initializer.IsSkillPlaced("skill_id");

// 배치된 모든 스킬 ID 가져오기
List<string> placedIds = initializer.GetPlacedSkillIds();

// 그리드 저장 데이터 가져오기
SkillGridSaveData saveData = initializer.GetGridSaveData();
```

### 5.3 저장 및 초기화

```csharp
// 수동 저장
initializer.SaveAll();

// 전체 초기화 (저장 데이터도 삭제)
initializer.ResetAll();
```

### 5.4 그리드 컨트롤러 직접 사용

```csharp
SkillGridController controller = initializer.GridController;

// 배치된 스킬 목록
List<SkillDraggableItem> placedItems = controller.GetPlacedItems();

// 특정 스킬 조회
SkillDraggableItem item = controller.GetItem("skill_id");

// 저장 데이터 삭제
controller.ClearSavedData();
```

---

## 6. DataManager와 연동

### 6.1 자동 연동

`SkillGridInitializer`는 `DataManager.skills`에서 자동으로 스킬을 로드합니다.

**전제 조건:**
- `DataManager.LoadAllDatabase()`가 먼저 호출되어야 함
- `DataManager.skills`가 null이 아니어야 함

### 6.2 초기화 순서

```
1. DataManager.LoadAllDatabase() 호출
2. SkillGridInitializer.Initialize() 호출
3. DataManager.skills.GetAll()로 스킬 목록 가져옴
4. 인벤토리에 스킬 추가
5. 저장된 그리드 데이터 로드
```

---

## 7. 스킬 모양 커스터마이징

### 7.1 SkillDataGridExtensions.cs 수정

스킬 ID에 따른 모양을 변경하려면 `GetShapeType` 메서드를 수정합니다:

```csharp
public static SkillShapeType GetShapeType(this SkillData skillData)
{
    return skillData.id switch
    {
        "meteor" => SkillShapeType.TwoByTwo,       // 메테오: 2x2
        "ice_spear" => SkillShapeType.OneByTwo,    // 얼음 창: 1x2 (세로)
        "burn" => SkillShapeType.TwoByOne,         // 화상: 2x1 (가로)
        "freeze_blast" => SkillShapeType.L_Shape,  // 빙결 폭발: L자
        _ => GetShapeByGrade(skillData.grade)      // 기본값: 등급별
    };
}
```

**참고**: 위 스킬 ID는 예시입니다. 실제 프로젝트의 스킬 ID로 교체하여 사용하세요.

### 7.2 등급별 기본 모양

등급에 따라 자동으로 모양이 결정됩니다:

| 등급 | 모양 |
|------|------|
| Common (일반) | 1x1 |
| Uncommon (고급) | 1x1 |
| Rare (희귀) | 2x1 |
| Hero (영웅) | 1x2 |
| Legend (전설) | L자 |
| Myth (신화) | T자 |

---

## 8. 문제 해결

### 8.1 스킬이 로드되지 않을 때

**확인 사항:**
1. DataManager.LoadAllDatabase()가 호출되었는지 확인
2. 스킬 JSON 파일이 올바른지 확인
3. 콘솔 에러 메시지 확인

### 8.2 프리팹 생성 실패

**확인 사항:**
1. Prefabs 폴더가 존재하는지 확인
2. 쓰기 권한이 있는지 확인
3. Unity 콘솔에서 에러 확인

### 8.3 저장 데이터가 로드되지 않을 때

**확인 사항:**
1. PlayerPrefs에 "SkillGridData" 키가 있는지 확인
2. JSON 형식이 올바른지 확인

### 8.4 드래그가 작동하지 않을 때

**확인 사항:**
1. Canvas에 GraphicRaycaster가 있는지 확인
2. EventSystem이 씬에 있는지 확인
3. 스킬 아이템에 CanvasGroup이 있는지 확인

---

## 9. 용어 설명

| 용어 | 설명 |
|------|------|
| 그리드 (Grid) | 스킬을 배치하는 격자판 (6x6) |
| 셀 (Cell) | 그리드의 한 칸 |
| 프리팹 (Prefab) | 게임 오브젝트 템플릿 |
| 드래그 앤 드롭 | 마우스로 끌어서 놓는 동작 |
| 인벤토리 | 배치하지 않은 스킬 목록 |
| 배치 | 스킬을 그리드에 놓는 것 |
| 점유 | 셀이 스킬에 의해 차지된 상태 |
| PlayerPrefs | Unity의 간단한 데이터 저장소 |
| SerializedProperty | 인스펙터에 표시되는 변수 |

---

## 10. 자주 묻는 질문 (FAQ)

**Q: 그리드 크기를 변경할 수 있나요?**
A: 네, SkillGridManager의 Grid Width, Grid Height를 변경하세요.

**Q: 스킬 모양을 더 추가할 수 있나요?**
A: 네, SkillShapeType enum에 새 모양을 추가하고 SkillShapeData.cs에 정의하면 됩니다.

**Q: 저장 데이터를 암호화할 수 있나요?**
A: 현재는 PlayerPrefs에 평문 JSON으로 저장됩니다. 암호화가 필요하면 별도 구현이 필요합니다.

**Q: 여러 캐릭터의 그리드를 따로 저장할 수 있나요?**
A: 네, SkillGridController의 saveKey를 캐릭터별로 다르게 설정하면 됩니다.

---

## 11. 연락처 및 지원

문제가 지속되면 개발자에게 문의하세요.

---

*이 가이드는 스킬 그리드 시스템 v1.0용으로 작성되었습니다.*
