# 스킬 그리드 시스템 작업 보고서

## 1. 개요

### 1.1 프로젝트 명
Slayer Legend (슬레이어 키우기) - 스킬 그리드 배치 시스템

### 1.2 작업 기간
Phase 1 ~ Phase 17 (총 17단계, 완료)

### 1.3 작업 목표
백팩 히어로(Backpack Hero) 스타일의 테트리스 모양 스킬 슬롯 배치 시스템 구현

---

## 2. 요구사항

| 항목 | 내용 |
|------|------|
| 그리드 크기 | 6 x 6 셀 (설정 가능) |
| 셀 크기 | 80px (기본값, 설정 가능) |
| 스킬 모양 | 1x1, 2x1, 1x2, 2x2, L자, T자 |
| 회전 | 가능 (R키 또는 우클릭) |
| 시너지 | 없음 |
| 저장/로드 | PlayerPrefs 사용 |

---

## 3. Phase별 작업 내용

### Phase 1: 데이터 구조 및 모양 정의
**파일**: `Data/SkillShapeData.cs`, `Data/SkillGridSaveData.cs`

- 테트리스 모양 정의 (6가지)
- 회전 로직 구현
- 저장/로드 데이터 구조 설계

### Phase 2: 그리드 매니저 및 셀
**파일**: `SkillGridCell.cs`, `SkillGridManager.cs`

- 개별 셀 컴포넌트 (점유 상태, 하이라이트)
- 6x6 그리드 관리 (배치, 제거, 이동)

### Phase 3: 배치 검증
**파일**: `SkillGridValidator.cs`

- 배치 가능여부 검증 로직
- 미리보기용 검증 (유효/무효 셀 표시)

### Phase 4: 드래그 앤 드롭 및 회전
**파일**: `SkillDraggableItem.cs`

- 드래그 기능 (IBeginDragHandler, IDragHandler, IEndDragHandler)
- R키 회전, 우클릭 회전
- 그리드 배치 시도

### Phase 5: UI 통합 및 저장/로드
**파일**: `SkillGridController.cs`

- 그리드, 인벤토리 통합 관리
- PlayerPrefs JSON 저장/로드

### Phase 6: SkillData 연동
**파일**: `Data/SkillDataGridExtensions.cs`

- SkillData 확장 메서드 (그리드 모양 매핑)
- 기존 SkillData.cs 수정 없이 연동

### Phase 7: 인벤토리 UI
**파일**: `SkillInventoryUI.cs`, `InventorySlotUI.cs`

- 인벤토리 슬롯 UI
- 필터링 (액티브/패시브, 등급)
- 선택 상태 관리

### Phase 8: DataManager 연동
**파일**: `SkillGridInitializer.cs`

- DataManager와 그리드 시스템 연결
- 초기화, 저장, 리셋 기능

### Phase 9: 프리팹 생성 에디터 도구
**파일**: `Editor/SkillGridPrefabCreator.cs`

- Unity 메뉴 도구 (Tools > Skill Grid)
- 프리팹 자동 생성

### Phase 10: 테스트 씬 구성 및 버그 수정
**날짜**: 2026-02-19

- 새 테스트 씬 설정 가이드 작성 (UserGuide.md 업데이트)
- TestSkillDataLoader 연동 문제 해결
- InventorySlotUI 분리 (별도 파일로)
- Canvas Render Mode 이슈 해결 (Screen Space - Camera 지원)

### Phase 11: 좌표 시스템 및 배치 수정
**날짜**: 2026-02-19

**수정 파일**: `SkillGridManager.cs`, `SkillDraggableItem.cs`

**작업 내용**:
1. **cellSize 동적 참조**
   - 기존: 하드코딩 `80f`
   - 수정: `gridManager.CellSize` 프로퍼티 사용

2. **멀티셀 스킬 위치 보정**
   - 문제: 2x1 스킬이 1칸 + 0.5칸 + 0.5칸으로 표시됨
   - 원인: 스킬이 첫 번째 셀 중앙에 배치되어 좌우로 확장
   - 해결: `GetCellLocalPosition(gridPos, itemWidth, itemHeight)` 오프셋 추가
   ```
   offsetX = (itemWidth - 1) * cellSize / 2
   offsetY = (itemHeight - 1) * cellSize / 2
   ```

3. **배치 후 시각적 위치 업데이트**
   - 배치 성공 시 그리드 컨테이너로 부모 변경
   - 회전/크기에 맞는 위치로 이동
   - pivot/anchor 중앙 정렬

### Phase 12: 비정형 스킬 표시 연구
**날짜**: 2026-02-19

**문제**: L자(3칸), T자(4칸) 스킬이 경계 상자 크기(2x2=4칸, 3x2=6칸)로 표시됨

**연구 결과**: `Docs/IrregularShapeRendering.md` 참조

**결론**: "형태 데이터 분리 + 투명 스프라이트" 조합 방식 권장

**상태**: 완료 (Phase 13에서 구현)

---

### Phase 13: 비정형 스킬 개별 셀 이미지 방식 구현
**날짜**: 2026-02-20

**선택 방식**: 개별 셀 이미지 방식 (Per-Cell Sprite) - IrregularShapeRendering.md의 방법 A

**신규 파일**:
- `Editor/SkillCellSpriteGenerator.cs` - 임시 셀 스프라이트 생성 에디터 도구

**수정 파일**:
- `SkillDraggableItem.cs` - UpdateVisualSize() → UpdateCellImages()로 변경
- `SkillGridPrefabCreator.cs` - CellImagesContainer 생성 코드 추가
- `SkillGridInitializer.cs` - DataManager 자동 초기화 추가
- `SkillGridController.cs` - SetInventoryContainer() 호출 추가
- `GameDB.cs` [조민희] - Unity API 메인 스레드 문제 수정

**핵심 구현 내용**:

1. **UpdateCellImages()** - 점유 셀마다 개별 Image 생성
   ```csharp
   var occupiedCells = shapeData.GetOccupiedCells(Vector2Int.zero, currentRotation);
   foreach (var cell in occupiedCells)
   {
       CreateCellImage(cell, cellSize, bounds);
   }
   ```

2. **CreateCellImage()** - 셀 위치 계산 후 이미지 배치
   - 중앙 기준 좌표 계산
   - 셀 크기 -4px 여백
   - raycastTarget = true

3. **GetCellSprite()** - 흰색 기본 스프라이트 폴백
   - Resources 로드 실패 시 64x64 흰색 스프라이트 생성

4. **인벤토리 복귀 로직** - 원래 위치 유지
   - inventorySiblingIndex 추적
   - 그리드 밖 드래그 시 인벤토리 복귀
   - 배치 실패 시 원래 순서로 복원

**해결된 이슈**:
| 이슈 | 해결 방법 |
|------|-----------|
| DataManager.skills null 체크 | GetAll().Count == 0으로 변경 |
| UnityException get_bytes 메인 스레드 | jsonFile.text를 Task.Run 전에 캡처 |
| 스킬 미리보기 크기 과대 | UpdateCellImages()를 SetGridManager() 후 호출 |
| 스킬이 보이지 않음 | GetCellSprite()에 흰색 폴백 스프라이트 추가 |
| 인벤토리 복귀 시 맨 밑으로 이동 | inventorySiblingIndex 추적 및 복원 |
| 그리드 밖 드래그 시 처리 | ReturnToInventory() 메서드 추가 |

**결과**: L자(3칸), T자(4칸) 스킬이 실제 점유 셀에만 표시됨

---

### Phase 14: 슬롯 클릭 → 아이템 생성 방식 및 로드 시 복원
**날짜**: 2026-02-20

**목표**: 인벤토리 슬롯 클릭 시에만 SkillDraggableItem 생성, 저장된 스킬 로드 시 아이템 복원

**수정 파일**:
- `SkillGridController.cs` - CreateDraggableItemFromSlot(), RestoreDraggableItemFromSave() 추가
- `SkillDraggableItem.cs` - ForceSetOnGrid(), destroyOnRemove 플래그 추가
- `SkillInventoryUI.cs` - 슬롯 클릭 시 아이템 생성 요청
- `InventorySlotUI.cs` - SetDraggableItem() 추가
- `Data/SkillGridSaveData.cs` - PlacedSkillData에 skillName, skillType 필드 추가

**핵심 변경 사항**:

1. **새로운 상호작용 방식**
   ```
   기존: 인벤토리에 미리 아이템 생성 → 드래그해서 그리드에 배치
   변경: 슬롯 클릭 → 아이템 생성 → 드래그 → 그리드에 배치
         (이미 그리드에 있으면 생성 안 함)
   ```

2. **고아 데이터 문제 해결 (A안)**
   - 문제: PlayerPrefs에서 로드된 saveData에 스킬이 있지만 DraggableItem이 없음
   - 해결: LoadGridData()에서 각 placedSkill에 대해 DraggableItem 인스턴스화
   ```csharp
   // LoadGridData()에서
   foreach (var placedSkill in saveData.placedSkills)
   {
       RestoreDraggableItemFromSave(placedSkill);
   }
   ```

3. **그리드에서 벗어나면 삭제**
   - destroyOnRemove = true 설정
   - 그리드 밖으로 드래그 시 DestroyDraggableItem() 호출

4. **PlacedSkillData 확장**
   - skillName, skillType 필드 추가로 로드 시 완전한 복원 가능

**저장된 데이터 예시**:
```json
{
  "skillId": "Fire_01",
  "skillName": "불꽃 베기",
  "skillType": 0,
  "gridX": 0,
  "gridY": 1,
  "rotation": 0,
  "shapeType": 1
}
```

**해결된 이슈**:
| 이슈 | 해결 방법 |
|------|-----------|
| 고아 데이터 (saveData에만 존재) | LoadGridData에서 아이템 인스턴스화 |
| 2x1 스킬 배치 실패 | IsInsideGridArea()에서 모든 점유 셀 검사 |
| 중복 배치 "이미 배치된 스킬" | RestoreDraggableItemFromSave로 동기화 |

---

### Phase 15: 스킬 아이콘 로드 및 복원 시스템 구현
**날짜**: 2026-02-20 ~ 2026-02-23

**목표**: 스킬 슬롯에 실제 스킬 아이콘 표시 및 씬 재로드 시 복원

**수정 파일**:
- `SkillDraggableItem.cs` - skillIcon 필드, spriteName 필드 추가, Initialize() 확장
- `SkillGridController.cs` - 4단계 fallback 로직, TryLoadIconBySkillIdPattern() 추가
- `SkillGridSaveData.cs` - PlacedSkillData에 spriteName 필드 추가
- `SkillGridInitializer.cs` - AddSkillToInventory 호출 시 spriteName 전달
- `ResourceManager.cs` - 스킬 아이콘 경로 추가

**핵심 구현 내용**:

1. **스킬 아이콘 표시 방식 개선**
   - 기존: 셀마다 개별 이미지
   - 변경: 전체 아이템에 하나의 아이콘 + 빈 셀에 반투명 오버레이

2. **spriteName 저장 및 복원**
   ```csharp
   // SkillDraggableItem.cs
   private Sprite skillIcon;
   private string spriteName = "";  // 원본 스프라이트 이름

   public void Initialize(..., Sprite icon = null, string sprite = "")
   {
       skillIcon = icon;
       spriteName = sprite;  // 저장용
   }

   public PlacedSkillData GetPlacedData()
   {
       string sprite = !string.IsNullOrEmpty(spriteName) ? spriteName : ...;
       return new PlacedSkillData(..., sprite);
   }
   ```

3. **4단계 Fallback 로직 (자동 마이그레이션)**
   ```csharp
   // RestoreDraggableItemFromSave()에서
   // 1차: 저장된 spriteName으로 바로 로드
   // 2차: skillDataCache에서 찾기
   // 3차: DataManager에서 찾기
   // 4차: skillId 패턴 기반 직접 로드 (TryLoadIconBySkillIdPattern)
   ```

4. **패턴 기반 아이콘 로드 (TryLoadIconBySkillIdPattern)**
   - skillId에서 속성 폴더 추정 (Fire → skill_fire)
   - 숫자 부분 추출 (Fire_04 → 04)
   - Resources.LoadAll로 번호로 시작하는 파일 검색
   ```csharp
   // 예: Fire_04 → skill_fire 폴더에서 "04_"로 시작하는 파일 검색
   var allSprites = Resources.LoadAll<Sprite>($"Skill/skillicon/{elementFolder}");
   foreach (var sprite in allSprites)
   {
       if (sprite.name.StartsWith(paddedNumber + "_"))
           return sprite;
   }
   ```

5. **초기화 순서 문제 해결**
   - 문제: SkillGridController.Start()에서 LoadGridData() 호출 시 DataManager 미초기화
   - 해결: Start()에서 자동 로드 제거, SkillGridInitializer가 적절한 시점에 호출

**해결된 이슈**:
| 이슈 | 원인 | 해결 방법 |
|------|------|-----------|
| 아이콘이 표시되지 않음 | spriteName 미저장 | skillIcon.name 대신 원본 spriteName 저장 |
| 씬 재로드 시 아이콘 사라짐 | PlacedSkillData에 spriteName 없음 | spriteName 필드 추가 |
| 초기화 순서로 로드 실패 | Start()에서 너무 이르게 호출 | SkillGridController.Start() 자동 로드 제거 |
| JSON spriteName과 파일명 불일치 | "07_BurningWeapon_2" vs "08_BurningWeapon_2" | 패턴 매칭으로 번호 기반 검색 |

**데이터 구조 변경**:
```json
// PlacedSkillData (업데이트됨)
{
  "skillId": "Fire_04",
  "skillName": "화염의 무기",
  "spriteName": "04_FireWind_Expert",  // ← 추가됨
  "gridX": 0,
  "gridY": 0,
  "rotation": 1,
  "shapeType": 0,
  "skillType": 0
}
```

**리소스 폴더 구조**:
```
Resources/Skill/skillicon/
├── skill_fire/
│   ├── 00_FlameSlash.png
│   ├── 01_BlazeSlash.png
│   ├── ...
│   └── 09_Rage.png
├── skill_water/
├── skill_earth/
├── skill_wind/
└── skill_none/
```

**결과**: 스킬 배치 시 아이콘 표시, 씬 재로드 후에도 아이콘 복원 성공

---

### Phase 16: 디버그 로그 제거
**날짜**: 2026-02-24

**목표**: 게임 에러 파악에 필요한 최소한의 로그를 제외하고 디버그 로그 제거

**수정 파일**:
- `SkillDraggableItem.cs` - 17개 Debug.Log 제거
- `SkillGridController.cs` - 23개 Debug.Log 제거
- `SkillGridInitializer.cs` - 5개 Debug.Log 제거
- `SkillGridManager.cs` - 5개 Debug.Log 제거
- `SkillInventoryUI.cs` - 3개 Debug.Log 제거

**유지된 로그**:
| 종류 | 목적 |
|------|------|
| `Debug.LogError` | 치명적 오류 표시용 |
| `Debug.LogWarning` | 경고 표시용 |
| ContextMenu 디버그 메서드 | 에디터 전용 (개발자 직접 호출 시에만 실행) |
| Editor 폴더 내 로그 | 에디터 도구용 |

**결과**: 런타임 디버그 로그 53개 제거, 에러/경고 로그 및 에디터 도구용 로그 유지

---

### Phase 17: SkillController 연동 시스템 구축
**날짜**: 2026-02-24

**목표**: 스킬 그리드에 배치된 스킬이 SkillController에 자동 등록/해제되도록 연동

**수정 파일**:
- `SkillGridController.cs` - SkillController 참조, 등록/해제 메서드 추가

**핵심 구현 내용**:

1. **SkillController 참조 추가**
   ```csharp
   [Header("스킬 시스템 연동")]
   [SerializeField] private SkillController skillController;
   ```

2. **스킬 배치 시 자동 등록 (RegisterSkillToController)**
   ```csharp
   private void RegisterSkillToController(string skillId)
   {
       // skillDataCache 또는 DataManager에서 스킬 데이터 조회
       // Active/Passive 타입에 따라 CreateActiveSkill/CreatePassiveSkill 호출
       // AddActiveSkill/AddPassiveSkill로 등록
   }
   ```

3. **스킬 제거 시 자동 해제 (DeregisterSkillFromController)**
   ```csharp
   private void DeregisterSkillFromController(string skillId)
   {
       // RemoveActiveSkill/RemovePassiveSkill 호출
   }
   ```

4. **이벤트 핸들러에서 등록/해제 호출**
   ```csharp
   private void HandleItemPlaced(SkillDraggableItem item)
   {
       SaveGridData();
       RegisterSkillToController(item.SkillId);  // 추가
   }

   private void HandleItemRemoved(SkillDraggableItem item)
   {
       SaveGridData();
       DeregisterSkillFromController(item.SkillId);  // 추가
   }
   ```

5. **로드 시 등록**
   - LoadGridData()에서 복원된 각 스킬에 대해 RegisterSkillToController() 호출

6. **ClearAll 시 해제**
   - ClearAll()에서 모든 배치 스킬에 대해 DeregisterSkillFromController() 호출

**동작 흐름**:
```
[스킬 배치]
슬롯 클릭 → 드래그 → 그리드 배치 → HandleItemPlaced()
    → RegisterSkillToController() → SkillController.AddActiveSkill()
    → 스킬 발동 시작

[스킬 제거]
그리드에서 드래그 아웃 → HandleItemRemoved()
    → DeregisterSkillFromController() → SkillController.RemoveActiveSkill()
    → 스킬 발동 중지

[로드 시]
LoadGridData() → RestoreDraggableItemFromSave() → RegisterSkillToController()
    → 이전에 배치했던 스킬들 자동 활성화
```

**결과**: 스킬 그리드 배치 ↔ SkillController 등록 양방향 연동 완료

---

## 4. 파일 구조

```
2. Scripts/System/Skill/UI/Grid/
├── Data/
│   ├── SkillShapeData.cs        # 테트리스 모양 정의
│   ├── SkillGridSaveData.cs     # 저장 데이터 구조 (Phase 15 수정: spriteName 추가)
│   └── SkillDataGridExtensions.cs # SkillData 확장 메서드
├── Editor/
│   ├── SkillGridPrefabCreator.cs # 프리팹 생성 에디터
│   └── SkillCellSpriteGenerator.cs # 셀 스프라이트 생성 (Phase 13 추가)
├── Docs/
│   ├── WorkReport.md            # 이 보고서
│   ├── UserGuide.md             # 사용자 가이드
│   └── IrregularShapeRendering.md # 비정형 스킬 렌더링 연구
├── Prefabs/                      # 프리팹 폴더
│   ├── SkillGridCell.prefab
│   ├── SkillDraggableItem.prefab
│   ├── InventorySlot.prefab
│   └── SkillGridSystem.prefab
├── SkillGridCell.cs             # 개별 셀
├── SkillGridManager.cs          # 그리드 관리자
├── SkillGridValidator.cs        # 배치 검증
├── SkillDraggableItem.cs        # 드래그 가능 아이템 (Phase 15 수정: spriteName 추가)
├── SkillGridController.cs       # 전체 컨트롤러 (Phase 15 수정: 4단계 fallback)
├── SkillInventoryUI.cs          # 인벤토리 UI
├── InventorySlotUI.cs           # 인벤토리 슬롯
└── SkillGridInitializer.cs      # DataManager 연동 (Phase 15 수정)

2. Scripts/Managers/Resource/
└── ResourceManager.cs           # 리소스 로드/캐싱 (Phase 15 수정: 스킬 아이콘 경로 추가)
```

---

## 5. 해결된 이슈 목록

| Phase | 이슈 | 해결 방법 |
|-------|------|-----------|
| Phase 1 | 회전 좌표 오류 | 회전 후 전역 오프셋 보정 |
| Phase 1 | GetSkillAtPosition 버그 | 모든 점유 셀 검사 |
| Phase 2 | ScreenToGridPosition 불일치 | pivot 0.5, 0.5로 설정 |
| Phase 2 | GetCell null 체크 누락 | null 체크 추가 |
| Phase 3 | CanRotate 복잡도 | 단순화 |
| Phase 3 | ValidatePlacement 중복 체크 | 전체 스킬 검사 |
| Phase 4 | 회전 상태 불일치 | RotateSkill 반환값 확인 |
| Phase 6 | 미사용 변수 | 제거 |
| Phase 7 | Destroy 순서 | Dictionary 먼저 정리 |
| Phase 7 | 이벤트 구독 해제 | OnDestroy 추가 |
| Phase 8 | DataManager null 체크 | null 체크 추가 |
| Phase 9 | SerializedProperty 필드명 | 실제 필드명으로 수정 |
| Phase 9 | cellPrefab 타입 | GameObject 직접 할당 |
| Phase 9 | ScrollRect viewport | 별도 Viewport 오브젝트 생성 |
| Phase 10 | TextAlignmentOptions.MixedReality | MidlineLeft로 수정 |
| Phase 10 | InventorySlotUI 스크립트 누락 | 별도 파일로 분리 |
| Phase 10 | cellContainer 미할당 | SerializedObject로 할당 |
| Phase 10 | Backend 초기화 필요 | TestSkillDataLoader로 우회 |
| Phase 10 | Canvas Camera null | Canvas 검색 후 적절한 카메라 사용 |
| Phase 11 | cellSize 하드코딩 | CellSize 프로퍼티 추가 |
| Phase 11 | 멀티셀 위치 불일치 | GetCellLocalPosition에 크기 매개변수 추가 |
| Phase 11 | 배치 후 위치 미업데이트 | TryPlaceOnGrid에서 위치 설정 추가 |
| Phase 13 | 비정형 스킬 경계 상자 표시 | 개별 셀 이미지 방식으로 구현 |
| Phase 13 | DataManager null 체크 무효 | GetAll().Count로 변경 |
| Phase 13 | UnityException 메인 스레드 | jsonFile.text 캡처 후 Task.Run |
| Phase 13 | 스킬 미리보기 크기 과대 | SetGridManager 후 UpdateCellImages |
| Phase 13 | 스킬 보이지 않음 | 흰색 폴백 스프라이트 생성 |
| Phase 13 | 인벤토리 복귀 위치 | inventorySiblingIndex 추적 |
| Phase 15 | 아이콘 표시 안 됨 | skillIcon + spriteName 필드 추가 |
| Phase 15 | 씬 재로드 시 아이콘 사라짐 | PlacedSkillData에 spriteName 저장 |
| Phase 15 | 초기화 순서로 로드 실패 | Start() 자동 로드 제거 |
| Phase 15 | JSON spriteName과 파일명 불일치 | 패턴 매칭 (번호 기반 검색) |
| Phase 15 | skillIcon.name이 원본과 다름 | 원본 spriteName 별도 저장 |
| Phase 16 | 런타임 디버그 로그 과다 | Debug.Log 제거 (에러/경고 로그 유지) |

---

## 6. 미해결 이슈

| 이슈 | 상태 | 해결 방안 |
|------|------|-----------|
| ~~비정형 스킬(L자, T자) 시각적 표시~~ | ~~연구 완료, 구현 대기~~ | **Phase 13에서 해결됨** |
| ~~스킬 아이콘 표시 안 됨~~ | ~~진행 중~~ | **Phase 15에서 해결됨** |
| ~~디버그 로그 제거~~ | ~~보류~~ | **Phase 16에서 해결됨** |
| 셀 스프라이트 임시 사용 중 | 보류 | 아티스트 스프라이트로 교체 필요 |

---

## 7. 코드 리뷰 결과

모든 Phase에서 에이전트 코드 리뷰 수행 완료:
- **총 발견 이슈**: 32개
- **해결된 이슈**: 32개
- **해결률**: 100%
- **남은 이슈**: 0개

---

## 8. 다음 작업 계획

### 우선순위 1: ~~스킬 아이콘 표시 (Phase 15)~~ ✓ 완료
- **Phase 15에서 4단계 fallback 로직 및 패턴 매칭으로 구현 완료**
- spriteName 저장/복원 시스템 구축
- 초기화 순서 문제 해결

### 우선순위 2: ~~비정형 스킬 표시 구현~~ ✓ 완료
- **Phase 13에서 개별 셀 이미지 방식으로 구현 완료**

### 우선순위 3: ~~정리 작업~~ ✓ 완료
- ~~디버그 로그 제거~~ **Phase 16에서 완료**
- 코드 주석 정리

### 우선순위 4: 추가 기능 (선택)
- 스킬 장착 시 효과 적용
- 그리드 프리셋 저장/로드
- 아티스트 셀 스프라이트로 교체

---

## 9. 사용 방법 요약

1. Unity 메뉴: `Tools > Skill Grid > Create All Prefabs` 실행
2. 새 씬 생성 후 Canvas 배치 (Render Mode: Overlay 권장)
3. TestSkillDataLoader 오브젝트 배치
4. SkillGridSystem 프리팹을 Canvas 안에 배치
5. Play 버튼 클릭

---

## 10. 참고 사항

- 팀원 파일(SkillData.cs) 수정 없이 확장 메서드로 연동
- PlayerPrefs 사용으로 별도 데이터베이스 불필요
- UIManager 등 다른 시스템과 독립적으로 동작
- cellSize 변경 시 자동 대응 (CellSize 프로퍼티 사용)
