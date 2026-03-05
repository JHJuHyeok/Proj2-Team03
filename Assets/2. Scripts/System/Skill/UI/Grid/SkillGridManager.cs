using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill.UI.Grid
{
    // 스킬 그리드 관리자
    // 6x6 그리드의 배치, 제거, 조회 기능 담당
    public class SkillGridManager : MonoBehaviour
    {
        [Header("그리드 설정")]
        [SerializeField] private int gridWidth = 6;
        [SerializeField] private int gridHeight = 6;
        [SerializeField] private float cellSize = 80f;  // 기본값 (자동 계산 시 무시됨)
        [SerializeField] private Vector2 gridOffset = Vector2.zero;

        [Header("셀 프리팹")]
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Transform cellContainer;

        [Header("자동 크기 조정")]
        [Tooltip("체크하면 부모 크기에 맞춰 셀 크기를 자동으로 계산")]
        [SerializeField] private bool autoCellSize = true;

        [Tooltip("셀 간 간격 (픽셀)")]
        [SerializeField] private float cellSpacing = 2f;

        [Tooltip("여백 (픽셀)")]
        [SerializeField] private float padding = 10f;

        // 그리드 데이터
        private SkillGridCell[,] cells;
        private SkillGridSaveData saveData;
        private float calculatedCellSize;  // 실제 계산된 셀 크기

        // 이벤트
        public event Action<PlacedSkillData> OnSkillPlaced;
        public event Action<string> OnSkillRemoved;
        public event Action OnGridChanged;

        // 프로퍼티
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => autoCellSize ? calculatedCellSize : cellSize;
        public Vector2Int GridSize => new Vector2Int(gridWidth, gridHeight);

        private void Awake()
        {
            CalculateCellSize();
            InitializeGrid();
        }

        // 셀 크기 자동 계산
        private void CalculateCellSize()
        {
            if (!autoCellSize)
            {
                calculatedCellSize = cellSize;
                return;
            }

            RectTransform containerRect = cellContainer?.GetComponent<RectTransform>();
            if (containerRect == null)
            {
                calculatedCellSize = cellSize;
                return;
            }

            float availableWidth = containerRect.rect.width - padding * 2 - (gridWidth - 1) * cellSpacing;
            float availableHeight = containerRect.rect.height - padding * 2 - (gridHeight - 1) * cellSpacing;

            float cellWidth = availableWidth / gridWidth;
            float cellHeight = availableHeight / gridHeight;

            // 정사각형 셀을 위해 더 작은 값 사용
            calculatedCellSize = Mathf.Min(cellWidth, cellHeight);

            // 최소 크기 보장
            calculatedCellSize = Mathf.Max(calculatedCellSize, 10f);
        }

        // 그리드 크기 변경 시 재계산
        private void OnRectTransformDimensionsChange()
        {
            if (!Application.isPlaying) return;
            if (cells == null) return;

            CalculateCellSize();
            UpdateCellPositions();
        }

        // 셀 위치 업데이트 (크기 변경 시)
        private void UpdateCellPositions()
        {
            if (cells == null) return;

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (cells[x, y] != null)
                    {
                        UpdateCellPosition(cells[x, y], x, y);
                    }
                }
            }
        }

        // 개별 셀 위치 업데이트
        private void UpdateCellPosition(SkillGridCell cell, int x, int y)
        {
            RectTransform rectTransform = cell.GetComponent<RectTransform>();
            if (rectTransform == null) return;

            float currentCellSize = CellSize;
            float gridTotalWidth = gridWidth * currentCellSize + (gridWidth - 1) * cellSpacing;
            float gridTotalHeight = gridHeight * currentCellSize + (gridHeight - 1) * cellSpacing;

            float startX = -gridTotalWidth / 2f + currentCellSize / 2f;
            float startY = gridTotalHeight / 2f - currentCellSize / 2f;

            float posX = startX + x * (currentCellSize + cellSpacing);
            float posY = startY - y * (currentCellSize + cellSpacing);

            rectTransform.anchoredPosition = new Vector2(posX, posY);
            rectTransform.sizeDelta = new Vector2(currentCellSize, currentCellSize);
        }

        // 그리드 초기화
        public void InitializeGrid()
        {
            CalculateCellSize();
            saveData = new SkillGridSaveData(gridWidth, gridHeight);
            cells = new SkillGridCell[gridWidth, gridHeight];

            CreateGridCells();
        }

        // 그리드 셀 생성
        private void CreateGridCells()
        {
            // 기존 셀 제거
            if (cellContainer != null)
            {
                foreach (Transform child in cellContainer)
                {
                    Destroy(child.gameObject);
                }
            }

            // 새 셀 생성
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    CreateCell(x, y);
                }
            }
        }

        // 개별 셀 생성
        private void CreateCell(int x, int y)
        {
            if (cellPrefab == null || cellContainer == null)
            {
                Debug.LogWarning("[SkillGridManager] 셀 프리팹 또는 컨테이너가 없습니다.");
                return;
            }

            GameObject cellObj = Instantiate(cellPrefab, cellContainer);
            cellObj.name = $"Cell_{x}_{y}";

            // 위치 설정
            RectTransform rectTransform = cellObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // pivot과 anchor를 모두 중앙으로 설정
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                float currentCellSize = CellSize;

                // 그리드 전체 크기 (간격 포함)
                float gridTotalWidth = gridWidth * currentCellSize + (gridWidth - 1) * cellSpacing;
                float gridTotalHeight = gridHeight * currentCellSize + (gridHeight - 1) * cellSpacing;

                // 그리드 시작 위치 (좌상단, pivot 기준)
                float startX = -gridTotalWidth / 2f + currentCellSize / 2f;
                float startY = gridTotalHeight / 2f - currentCellSize / 2f;

                // 셀 위치 계산 (간격 포함)
                float posX = startX + x * (currentCellSize + cellSpacing);
                float posY = startY - y * (currentCellSize + cellSpacing);

                rectTransform.anchoredPosition = new Vector2(posX, posY);
                rectTransform.sizeDelta = new Vector2(currentCellSize, currentCellSize);
            }

            // 셀 컴포넌트 초기화
            SkillGridCell cell = cellObj.GetComponent<SkillGridCell>();
            if (cell == null)
            {
                Debug.LogError($"[SkillGridManager] 셀 프리팹에 SkillGridCell 컴포넌트가 없습니다: {cellPrefab.name}");
                Destroy(cellObj);
                return;
            }
            cell.Initialize(x, y);
            cell.ApplyCheckerPattern();

            cells[x, y] = cell;
        }

        // 스킬 배치 시도
        public bool TryPlaceSkill(string skillId, Vector2Int position, int rotation, SkillShapeType shapeType)
        {
            // 유효성 검사
            if (!CanPlaceSkill(skillId, position, rotation, shapeType, out string reason))
            {
                return false;
            }

            // 배치 데이터 생성
            PlacedSkillData placedSkill = new PlacedSkillData(skillId, position, rotation, shapeType);

            // 그리드에 적용
            ApplySkillToGrid(placedSkill);

            // 저장 데이터에 추가
            saveData.AddPlacedSkill(placedSkill);

            // 이벤트 발생
            OnSkillPlaced?.Invoke(placedSkill);
            OnGridChanged?.Invoke();

            return true;
        }

        // 배치 가능 여부 확인
        public bool CanPlaceSkill(string skillId, Vector2Int position, int rotation, SkillShapeType shapeType, out string reason)
        {
            reason = string.Empty;

            var shapeData = SkillShapeData.Create(shapeType);
            var occupiedCells = shapeData.GetOccupiedCells(position, rotation);

            // 그리드 범위 체크
            foreach (var cell in occupiedCells)
            {
                if (!IsWithinGrid(cell))
                {
                    reason = "그리드 범위를 벗어납니다.";
                    return false;
                }
            }

            // 중복 체크 (같은 스킬 ID가 이미 배치되어 있는지)
            if (saveData.GetPlacedSkill(skillId) != null)
            {
                reason = "이미 배치된 스킬입니다.";
                return false;
            }

            // 다른 스킬과 충돌 체크
            foreach (var cell in occupiedCells)
            {
                var existingSkill = saveData.GetSkillOccupyingCell(cell);
                if (existingSkill != null && existingSkill.skillId != skillId)
                {
                    reason = $"다른 스킬({existingSkill.skillId})과 겹칩니다.";
                    return false;
                }
            }

            return true;
        }

        // 그리드에 스킬 적용 (셀 상태 업데이트)
        private void ApplySkillToGrid(PlacedSkillData skill)
        {
            var occupiedCells = skill.GetOccupiedCells();

            foreach (var cellPos in occupiedCells)
            {
                if (IsWithinGrid(cellPos))
                {
                    cells[cellPos.x, cellPos.y].SetOccupied(skill);
                }
            }
        }

        // 스킬 제거
        public bool RemoveSkill(string skillId)
        {
            PlacedSkillData skill = saveData.GetPlacedSkill(skillId);
            if (skill == null)
            {
                return false;
            }

            // 그리드에서 제거
            RemoveSkillFromGrid(skill);

            // 저장 데이터에서 제거
            saveData.RemovePlacedSkill(skillId);

            // 이벤트 발생
            OnSkillRemoved?.Invoke(skillId);
            OnGridChanged?.Invoke();

            return true;
        }

        // 그리드에서 스킬 제거 (셀 상태 업데이트)
        private void RemoveSkillFromGrid(PlacedSkillData skill)
        {
            var occupiedCells = skill.GetOccupiedCells();

            foreach (var cellPos in occupiedCells)
            {
                if (IsWithinGrid(cellPos))
                {
                    cells[cellPos.x, cellPos.y].ClearOccupied();
                }
            }
        }

        // 스킬 이동 (위치 변경)
        public bool MoveSkill(string skillId, Vector2Int newPosition, int newRotation)
        {
            PlacedSkillData skill = saveData.GetPlacedSkill(skillId);
            if (skill == null) return false;

            // 기존 위치에서 제거
            RemoveSkillFromGrid(skill);

            // 새 위치에서 배치 가능한지 확인
            if (!CanPlaceSkill(skillId, newPosition, newRotation, skill.shapeType, out string reason))
            {
                // 불가능하면 원위치로 복구
                ApplySkillToGrid(skill);
                return false;
            }

            // 새 위치로 업데이트
            var newSkill = new PlacedSkillData(skillId, newPosition, newRotation, skill.shapeType);
            saveData.AddPlacedSkill(newSkill);
            ApplySkillToGrid(newSkill);

            OnGridChanged?.Invoke();
            return true;
        }

        // 스킬 회전
        public bool RotateSkill(string skillId)
        {
            PlacedSkillData skill = saveData.GetPlacedSkill(skillId);
            if (skill == null) return false;

            int newRotation = (skill.rotation + 90) % 360;
            return MoveSkill(skillId, skill.GridPosition, newRotation);
        }

        // 좌표가 그리드 내부인지 확인
        public bool IsWithinGrid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
        }

        // 셀 조회
        public SkillGridCell GetCell(int x, int y)
        {
            if (cells == null) return null;
            if (IsWithinGrid(new Vector2Int(x, y)))
            {
                return cells[x, y];
            }
            return null;
        }

        public SkillGridCell GetCell(Vector2Int pos)
        {
            return GetCell(pos.x, pos.y);
        }

        // 그리드 좌표를 로컬 anchoredPosition으로 변환 (드래그 아이템 배치용)
        // itemWidth, itemHeight: 스킬이 차지하는 셀 수
        public Vector2 GetCellLocalPosition(Vector2Int gridPos, int itemWidth = 1, int itemHeight = 1)
        {
            float currentCellSize = CellSize;
            float gridTotalWidth = gridWidth * currentCellSize + (gridWidth - 1) * cellSpacing;
            float gridTotalHeight = gridHeight * currentCellSize + (gridHeight - 1) * cellSpacing;

            float startX = -gridTotalWidth / 2f + currentCellSize / 2f;
            float startY = gridTotalHeight / 2f - currentCellSize / 2f;

            float posX = startX + gridPos.x * (currentCellSize + cellSpacing);
            float posY = startY - gridPos.y * (currentCellSize + cellSpacing);

            // 멀티셀 스킬의 경우 중심 위치 보정
            // 예: 2x1 스킬은 오른쪽으로 0.5셀 이동해야 함
            if (itemWidth > 1)
            {
                posX += (itemWidth - 1) * (currentCellSize + cellSpacing) / 2f;
            }
            if (itemHeight > 1)
            {
                posY -= (itemHeight - 1) * (currentCellSize + cellSpacing) / 2f;
            }

            return new Vector2(posX, posY);
        }

        // 셀 컨테이너 Transform 반환
        public Transform GetCellContainer()
        {
            return cellContainer;
        }

        // 특정 위치의 스킬 조회
        public PlacedSkillData GetSkillAtCell(Vector2Int pos)
        {
            return saveData.GetSkillOccupyingCell(pos);
        }

        // 배치된 모든 스킬 조회
        public List<PlacedSkillData> GetAllPlacedSkills()
        {
            return saveData.placedSkills;
        }

        // 그리드 전체 초기화
        public void ClearGrid()
        {
            saveData.Clear();

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    cells[x, y].ClearOccupied();
                }
            }

            OnGridChanged?.Invoke();
        }

        // 하이라이트 설정 (드래그 중 미리보기용)
        public void SetHighlight(List<Vector2Int> cellPositions, CellHighlight highlight)
        {
            // 모든 하이라이트 먼저 제거
            ClearAllHighlights();

            // 지정된 셀에 하이라이트 적용
            foreach (var pos in cellPositions)
            {
                SkillGridCell cell = GetCell(pos);
                if (cell != null)
                {
                    cell.SetHighlight(highlight);
                }
            }
        }

        // 모든 하이라이트 제거
        public void ClearAllHighlights()
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    cells[x, y].SetHighlight(CellHighlight.None);
                }
            }
        }

        // 저장 데이터 반환
        public SkillGridSaveData GetSaveData()
        {
            // saveData가 null이면 새로 생성 (조민희 수정)
            if (saveData == null)
            {
                saveData = new SkillGridSaveData(gridWidth, gridHeight);
            }
            return saveData.Clone();
        }

        // 저장 데이터 로드
        public void LoadSaveData(SkillGridSaveData data)
        {
            ClearGrid();

            saveData = data.Clone();

            foreach (var skill in saveData.placedSkills)
            {
                ApplySkillToGrid(skill);
            }

            OnGridChanged?.Invoke();
        }

        // 스크린 좌표를 그리드 좌표로 변환
        public Vector2Int ScreenToGridPosition(Vector2 screenPosition)
        {
            RectTransform gridRect = cellContainer?.GetComponent<RectTransform>();
            if (gridRect == null)
            {
                Debug.LogWarning("[SkillGridManager] cellContainer가 null입니다.");
                return new Vector2Int(-1, -1);
            }

            // Canvas의 카메라 찾기 (Screen Space - Camera 모드 지원)
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = canvas.worldCamera;
                if (uiCamera == null)
                {
                    uiCamera = Camera.main;
                }
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gridRect, screenPosition, uiCamera, out Vector2 localPos))
            {
                Debug.LogWarning("[SkillGridManager] 화면 좌표 변환 실패");
                return new Vector2Int(-1, -1);
            }

            float currentCellSize = CellSize;

            // 그리드 전체 크기 계산 (간격 포함)
            float gridTotalWidth = gridWidth * currentCellSize + (gridWidth - 1) * cellSpacing;
            float gridTotalHeight = gridHeight * currentCellSize + (gridHeight - 1) * cellSpacing;

            // 그리드 시작 위치 (좌상단 기준)
            float startX = -gridTotalWidth / 2f;
            float startY = gridTotalHeight / 2f;

            // 로컬 좌표를 그리드 좌표로 변환 (간격 고려)
            float cellWithSpacing = currentCellSize + cellSpacing;
            int x = Mathf.FloorToInt((localPos.x - startX) / cellWithSpacing);
            int y = Mathf.FloorToInt((startY - localPos.y) / cellWithSpacing);

            return new Vector2Int(x, y);
        }

        // 그리드 좌표를 스크린 좌표로 변환
        public Vector2 GridToScreenPosition(Vector2Int gridPos)
        {
            RectTransform gridRect = cellContainer?.GetComponent<RectTransform>();
            if (gridRect == null) return Vector2.zero;

            float currentCellSize = CellSize;
            float cellWithSpacing = currentCellSize + cellSpacing;

            float localX = gridOffset.x + gridPos.x * cellWithSpacing + currentCellSize / 2;
            float localY = gridOffset.y - gridPos.y * cellWithSpacing - currentCellSize / 2;

            Vector3 worldPos = gridRect.TransformPoint(new Vector3(localX, localY, 0f));

            return new Vector2(worldPos.x, worldPos.y);
        }
    }
}
