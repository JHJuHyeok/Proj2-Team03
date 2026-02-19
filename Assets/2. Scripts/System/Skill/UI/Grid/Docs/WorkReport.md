# 스킬 그리드 시스템 작업 보고서

## 1. 개요

### 1.1 프로젝트 명
Slayer Legend (슬레이어 키우기) - 스킬 그리드 배치 시스템

### 1.2 작업 기간
Phase 1 ~ Phase 12 (총 12단계)

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

- 배치 가능 여부 검증 로직
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

**상태**: 구현 대기중

---

## 4. 파일 구조

```
2. Scripts/System/Skill/UI/Grid/
├── Data/
│   ├── SkillShapeData.cs        # 테트리스 모양 정의
│   ├── SkillGridSaveData.cs     # 저장 데이터 구조
│   └── SkillDataGridExtensions.cs # SkillData 확장 메서드
├── Editor/
│   └── SkillGridPrefabCreator.cs # 프리팹 생성 에디터
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
├── SkillGridManager.cs          # 그리드 관리자 (수정됨)
├── SkillGridValidator.cs        # 배치 검증
├── SkillDraggableItem.cs        # 드래그 가능 아이템 (수정됨)
├── SkillGridController.cs       # 전체 컨트롤러
├── SkillInventoryUI.cs          # 인벤토리 UI
├── InventorySlotUI.cs           # 인벤토리 슬롯 (분리됨)
└── SkillGridInitializer.cs      # DataManager 연동
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

---

## 6. 미해결 이슈

| 이슈 | 상태 | 해결 방안 |
|------|------|-----------|
| 비정형 스킬(L자, T자) 시각적 표시 | 연구 완료, 구현 대기 | 투명 스프라이트 방식 권장 |
| 디버그 로그 제거 | 보류 | 기능 완료 후 일괄 제거 |

---

## 7. 코드 리뷰 결과

모든 Phase에서 에이전트 코드 리뷰 수행 완료:
- **총 발견 이슈**: 21개
- **해결된 이슈**: 19개
- **해결률**: 90%
- **남은 이슈**: 2개 (비정형 스킬 표시 관련)

---

## 8. 다음 작업 계획

### 우선순위 1: 비정형 스킬 표시 구현
- 투명 영역이 포함된 스킬 스프라이트 제작
- 또는 개별 셀 이미지 방식 재구현

### 우선순위 2: 정리 작업
- 디버그 로그 제거
- 코드 주석 정리

### 우선순위 3: 추가 기능 (선택)
- 스킬 장착 시 효과 적용
- 그리드 프리셋 저장/로드

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
