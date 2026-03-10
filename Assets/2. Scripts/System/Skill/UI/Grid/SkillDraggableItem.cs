using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SlayerLegend.Skill.UI.Grid
{
    // 드래그 가능한 스킬 아이템
    // 인벤토리/그리드에서 드래그하여 배치, 회전 기능 담당
    public class SkillDraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [Header("스킬 정보")]
        [SerializeField] private string skillId;
        [SerializeField] private string skillName;
        [SerializeField] private SkillShapeType shapeType = SkillShapeType.OneByOne;
        [SerializeField] private SkillType skillType = SkillType.Active;

        [Header("시각 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image background;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("회전 설정")]
        [SerializeField] private KeyCode rotateKey = KeyCode.R;
        [SerializeField] private bool allowRotation = true;

        [Header("셀 이미지 설정")]
        [SerializeField] private Transform cellImagesContainer;
        [SerializeField] private GameObject cellImagePrefab;

        // 셀 이미지 리스트
        private List<Image> cellImages = new List<Image>();

        // 스킬 아이콘 저장 (조민희: 셀 이미지에 스킬 아이콘 표시용)
        private Sprite skillIcon;
        private string spriteName = "";  //원본 스프라이트 이름 (저장/로드용)

        // 상태
        private bool isDragging = false;
        private bool isOnGrid = false;
        private bool destroyOnRemove = false;  // 그리드에서 벗어나면 삭제 여부
        private Vector2Int gridPosition;
        private int currentRotation = 0;
        private Transform originalParent;
        private Vector3 originalPosition;
        private int originalSiblingIndex;
        private int inventorySiblingIndex = -1;  // 인벤토리 내 원래 순서
        private SkillGridManager gridManager;
        private SkillGridController gridController;  // 컨트롤러 참조 (삭제용)
        private Transform inventoryContainer;  // 인벤토리 컨테이너 참조
        private RectTransform rectTransform;

        // 드래그 중 임시 저장
        private static SkillDraggableItem currentDraggingItem = null;

        // 이벤트
        public event System.Action<SkillDraggableItem> OnDragStarted;
        public event System.Action<SkillDraggableItem> OnDragEnded;
        public event System.Action<SkillDraggableItem> OnItemPlaced;
        public event System.Action<SkillDraggableItem> OnItemRemoved;

        // 프로퍼티
        public string SkillId => skillId;
        public string SkillName => skillName;
        public SkillShapeType ShapeType => shapeType;
        public SkillType SkillType => skillType;
        public bool IsOnGrid => isOnGrid;
        public Vector2Int GridPosition => gridPosition;
        public int CurrentRotation => currentRotation;
        public bool IsDragging => isDragging;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // 셀 이미지 컨테이너 자동 생성
            if (cellImagesContainer == null)
            {
                GameObject container = new GameObject("CellImagesContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                RectTransform cr = container.AddComponent<RectTransform>();
                cr.anchorMin = Vector2.zero;
                cr.anchorMax = Vector2.one;
                cr.sizeDelta = Vector2.zero;
                cellImagesContainer = container.transform;
            }
        }

        private void Update()
        {
            // R키로 회전 (드래그 중에만)
            if (isDragging && allowRotation && Input.GetKeyDown(rotateKey))
            {
                Rotate();
            }
        }

        // 초기화
        public void Initialize(string id, string name, SkillShapeType shape, SkillType type, Sprite icon = null, string sprite = "")
        {
            skillId = id;
            skillName = name;
            shapeType = shape;
            skillType = type;

            // 스킬 아이콘 저장 (조민희: 셀 이미지에 표시용)
            skillIcon = icon;
            spriteName = sprite;  //원본 스프라이트 이름 저장

            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
            }

            // 모양에 따른 크기 조정
            UpdateCellImages();
        }

        // 그리드 매니저 설정
        public void SetGridManager(SkillGridManager manager)
        {
            gridManager = manager;
            // gridManager가 설정되면 올바른 cellSize로 다시 그리기
            UpdateCellImages();
        }

        // 그리드 컨트롤러 설정 (삭제용)
        public void SetGridController(SkillGridController controller)
        {
            gridController = controller;
        }

        // 인벤토리 컨테이너 설정
        public void SetInventoryContainer(Transform container)
        {
            inventoryContainer = container;
        }

        // 그리드에서 벗어나면 삭제 여부 설정
        public void SetDestroyOnRemove(bool destroy)
        {
            destroyOnRemove = destroy;
        }

        // 모양에 따른 시각적 크기 및 셀 이미지 업데이트
        //전체 영역에 하나의 아이콘 표시 + 빈 공간에만 반투명 오버레이
        private void UpdateCellImages()
        {
            var shapeData = SkillShapeData.Create(shapeType);
            var occupiedCells = shapeData.GetOccupiedCells(Vector2Int.zero, currentRotation);

            // 그리드에 있으면 그리드 셀 크기 사용, 인벤토리에 있으면 고정값 사용
            float cellSize;
            if (isOnGrid && gridManager != null)
            {
                cellSize = gridManager.CellSize;
            }
            else
            {
                cellSize = 70f;  // 인벤토리용 고정 크기
            }

            // 기존 셀 이미지 제거
            ClearCellImages();

            // 경계 계산
            var bounds = CalculateBounds(occupiedCells);
            float totalWidth = bounds.size.x * cellSize;
            float totalHeight = bounds.size.y * cellSize;
            rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

            // 아이콘 이미지를 전체 영역에 표시 (조민희: 하나의 아이콘으로 채움)
            if (iconImage != null && skillIcon != null)
            {
                iconImage.sprite = skillIcon;
                // 부모 크기에 맞추기 위해 anchor 설정
                iconImage.rectTransform.anchorMin = Vector2.zero;
                iconImage.rectTransform.anchorMax = Vector2.one;
                iconImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                iconImage.rectTransform.anchoredPosition = Vector2.zero;
                iconImage.rectTransform.sizeDelta = new Vector2(-8f, -8f);  // 양쪽 여백 4px씩
                iconImage.gameObject.SetActive(true);
            }

            // 빈 공간에만 반투명 오버레이 생성 (조민희: 점유된 셀은 선명하게, 빈 공간은 흐릿하게)
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var cellPos = new Vector2Int(x, y);
                    // 점유되지 않은 셀(빈 공간)에만 반투명 이미지 생성
                    if (!occupiedCells.Exists(c => c.x == x && c.y == y))
                    {
                        CreateEmptyCellOverlay(cellPos, cellSize, bounds);
                    }
                }
            }
        }

        // 빈 공간에 반투명 오버레이 생성 (조민희: 해당 영역의 아이콘을 흐릿하게)
        private void CreateEmptyCellOverlay(Vector2Int cellPos, float cellSize, BoundsInt bounds)
        {
            if (cellImagesContainer == null) return;

            GameObject cellObj = new GameObject($"Empty_{cellPos.x}_{cellPos.y}");
            cellObj.transform.SetParent(cellImagesContainer, false);

            RectTransform rect = cellObj.AddComponent<RectTransform>();

            // 셀 위치 계산 (중앙 기준)
            float localX = (cellPos.x - bounds.xMin) * cellSize + cellSize / 2f - bounds.size.x * cellSize / 2f;
            float localY = -(cellPos.y - bounds.yMin) * cellSize - cellSize / 2f + bounds.size.y * cellSize / 2f;

            rect.anchoredPosition = new Vector2(localX, localY);
            rect.sizeDelta = new Vector2(cellSize - 4f, cellSize - 4f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image img = cellObj.AddComponent<Image>();
            // 반투명 검은색으로 아이콘 흐릿하게 - 조민희
            img.sprite = GetCellBackgroundSprite();
            img.color = new Color(0f, 0f, 0f, 0.8f);  // 80% 불투명 (더 어둡게)
            img.raycastTarget = false;

            cellImages.Add(img);
        }

        // 셀 이미지 모두 제거
        private void ClearCellImages()
        {
            if (cellImages == null) return;
            foreach (var img in cellImages)
                if (img != null) Destroy(img.gameObject);
            cellImages.Clear();
        }

        // 셀들의 경계 계산
        private BoundsInt CalculateBounds(List<Vector2Int> cells)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var c in cells)
            {
                if (c.x < minX) minX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.x > maxX) maxX = c.x;
                if (c.y > maxY) maxY = c.y;
            }

            return new BoundsInt(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
        }

        // 셀 배경 스프라이트 가져오기 (조민희: 테두리용 흰색 스프라이트)
        private Sprite GetCellBackgroundSprite()
        {
            // 임시 셀 스프라이트 로드 시도
            Sprite sprite = Resources.Load<Sprite>("Skill/Grid/TempCellSprite");
            if (sprite != null) return sprite;

            // 기본 흰색 스프라이트 생성
            Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[64 * 64];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        // 회전
        public void Rotate()
        {
            currentRotation = (currentRotation + 90) % 360;
            UpdateCellImages();
            UpdatePreview();

        }

        // 드래그 시작
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentDraggingItem != null && currentDraggingItem != this)
            {
                return; // 다른 아이템이 드래그 중이면 무시
            }

            isDragging = true;
            currentDraggingItem = this;

            // 원래 위치 저장
            originalParent = transform.parent;
            originalPosition = rectTransform.anchoredPosition;
            originalSiblingIndex = transform.GetSiblingIndex();

            // 인벤토리에 있었다면 인벤토리 내 순서 저장
            if (!isOnGrid && inventoryContainer != null && transform.parent == inventoryContainer)
            {
                inventorySiblingIndex = originalSiblingIndex;
            }

            // 그리드에 있었다면 임시 제거
            if (isOnGrid && gridManager != null)
            {
                gridManager.RemoveSkill(skillId);
                isOnGrid = false;
            }

            // 드래그를 위해 최상위 캔버스로 이동
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();

            // 반투명하게
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.7f;
                canvasGroup.blocksRaycasts = false;
            }

            OnDragStarted?.Invoke(this);

        }

        // 드래그 중
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            // 마우스 위치로 이동
            rectTransform.position = eventData.position;

            // 미리보기 업데이트
            UpdatePreview();
        }

        // 드래그 종료
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            isDragging = false;
            currentDraggingItem = null;

            // 투명도 복원
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            // 그리드 영역 내부인지 확인
            bool isInsideGrid = IsInsideGridArea();

            if (isInsideGrid)
            {
                // 그리드에 배치 시도
                bool placed = TryPlaceOnGrid();

                if (!placed)
                {
                    // 배치 실패 시 인벤토리로 복귀
                    ReturnToInventory();
                }
            }
            else
            {
                // 그리드 밖으로 드래그 시 인벤토리로 복귀
                ReturnToInventory();
            }

            // 하이라이트 제거
            if (gridManager != null)
            {
                gridManager.ClearAllHighlights();
            }

            OnDragEnded?.Invoke(this);
        }

        // 그리드 영역 내부인지 확인 (스킬 모양 고려)
        private bool IsInsideGridArea()
        {
            if (gridManager == null)
            {
                return false;
            }

            Vector2Int gridPos = gridManager.ScreenToGridPosition(Input.mousePosition);

            // 스킬이 차지하는 모든 좌표 계산
            var shapeData = SkillShapeData.Create(shapeType);
            var occupiedCells = shapeData.GetOccupiedCells(gridPos, currentRotation);

            // 모든 좌표가 그리드 범위 내인지 확인
            foreach (var cell in occupiedCells)
            {
                if (!gridManager.IsWithinGrid(cell))
                {
                    return false;  // 하나라도 범위 밖이면 false
                }
            }

            return true;
        }

        // 클릭 (우클릭으로 회전)
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right && allowRotation)
            {
                if (isOnGrid && gridManager != null)
                {
                    // 그리드에 있는 상태에서 회전
                    int newRotation = (currentRotation + 90) % 360;

                    if (SkillGridValidator.CanRotate(gridManager, skillId, newRotation))
                    {
                        if (gridManager.RotateSkill(skillId))
                        {
                            currentRotation = newRotation;
                            UpdateCellImages();
                        }
                        else
                        {
                            Debug.LogWarning($"[SkillDraggableItem] 회전 실패: {skillName}");
                        }
                    }
                }
                else if (!isDragging)
                {
                    // 인벤토리에서 회전
                    Rotate();
                }
            }
        }

        // 미리보기 업데이트
        private void UpdatePreview()
        {
            if (gridManager == null) return;

            Vector2Int hoverPos = gridManager.ScreenToGridPosition(Input.mousePosition);

            var result = SkillGridValidator.ValidateForPreview(
                gridManager,
                hoverPos,
                currentRotation,
                shapeType,
                isOnGrid ? skillId : null
            );

            if (result.isValid)
            {
                gridManager.SetHighlight(result.validCells, CellHighlight.Valid);
            }
            else
            {
                gridManager.SetHighlight(result.validCells, CellHighlight.Valid);
                gridManager.SetHighlight(result.invalidCells, CellHighlight.Invalid);
            }
        }

        // 그리드에 배치 시도
        private bool TryPlaceOnGrid()
        {
            if (gridManager == null)
            {
                Debug.LogWarning("[SkillDraggableItem] 그리드 매니저가 없습니다.");
                return false;
            }

            Vector2Int targetPos = gridManager.ScreenToGridPosition(Input.mousePosition);

            bool success = gridManager.TryPlaceSkill(skillId, targetPos, currentRotation, shapeType);

            if (success)
            {
                isOnGrid = true;
                gridPosition = targetPos;

                // 그리드 컨테이너로 이동
                transform.SetParent(gridManager.GetCellContainer());

                // 스킬 크기 계산
                var shapeData = SkillShapeData.Create(shapeType);
                var size = shapeData.GetRotatedSize(currentRotation);

                // 그리드 좌표에 맞는 위치로 이동 (크기 고려)
                Vector2 localPos = gridManager.GetCellLocalPosition(targetPos, size.x, size.y);
                rectTransform.anchoredPosition = localPos;

                // pivot과 anchor 설정
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                // 크기 업데이트
                UpdateCellImages();

                OnItemPlaced?.Invoke(this);
                return true;
            }

            return false;
        }

        // 원래 위치로 복귀
        private void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            rectTransform.anchoredPosition = originalPosition;
            isOnGrid = false;

            // 인벤토리로 돌아갈 때 크기 업데이트
            UpdateCellImages();

            OnItemRemoved?.Invoke(this);
        }

        // 인벤토리로 복귀 (그리드 밖으로 드래그 시)
        private void ReturnToInventory()
        {
            // 그리드에서 제거 (isOnGrid와 관계없이 saveData에서 제거 시도)
            // 주의: OnBeginDrag에서 이미 RemoveSkill을 호출했을 수 있지만,
            // saveData에 남아있는 경우를 위해 다시 확인
            if (gridManager != null)
            {
                var placedSkill = gridManager.GetSkillAtCell(gridPosition);
                if (placedSkill != null && placedSkill.skillId == skillId)
                {
                    gridManager.RemoveSkill(skillId);
                }
            }

            isOnGrid = false;

            // destroyOnRemove가 true면 컨트롤러를 통해 삭제
            if (destroyOnRemove)
            {
                OnItemRemoved?.Invoke(this);

                if (gridController != null)
                {
                    gridController.DestroyDraggableItem(skillId);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }

            // 기존 방식: 인벤토리로 이동
            if (inventoryContainer == null)
            {
                ReturnToOriginalPosition();
                return;
            }

            // 인벤토리로 이동
            transform.SetParent(inventoryContainer);

            // 원래 인벤토리 순서로 복원
            if (inventorySiblingIndex >= 0 && inventorySiblingIndex < inventoryContainer.childCount)
            {
                transform.SetSiblingIndex(inventorySiblingIndex);
            }
            else
            {
                transform.SetAsLastSibling();
            }

            rectTransform.anchoredPosition = Vector3.zero;

            // 크기 업데이트
            UpdateCellImages();

            OnItemRemoved?.Invoke(this);
        }

        // 그리드에서 제거 (외부 호출용)
        public void RemoveFromGrid()
        {
            if (isOnGrid && gridManager != null)
            {
                gridManager.RemoveSkill(skillId);
                isOnGrid = false;
                OnItemRemoved?.Invoke(this);
            }
        }

        // 위치 동기화 (그리드 매니저가 호출)
        public void SyncPosition(Vector2Int position, int rotation)
        {
            gridPosition = position;
            currentRotation = rotation;
            isOnGrid = true;
            UpdateCellImages();
        }

        // 저장 데이터로부터 복원 시 강제로 그리드에 배치 (시각적 위치 설정 포함)
        public void ForceSetOnGrid(Vector2Int position, int rotation)
        {
            gridPosition = position;
            currentRotation = rotation;
            isOnGrid = true;

            // 그리드 컨테이너로 이동
            if (gridManager != null)
            {
                transform.SetParent(gridManager.GetCellContainer());

                // 스킬 크기 계산
                var shapeData = SkillShapeData.Create(shapeType);
                var size = shapeData.GetRotatedSize(rotation);

                // 그리드 좌표에 맞는 위치로 이동
                Vector2 localPos = gridManager.GetCellLocalPosition(position, size.x, size.y);
                rectTransform.anchoredPosition = localPos;

                // pivot과 anchor 설정
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            }

            // 크기 업데이트
            UpdateCellImages();
        }

        // 현재 상태 정보 반환
        public PlacedSkillData GetPlacedData()
        {
            // spriteName이 있으면 사용, 없으면 skillIcon.name 사용 (fallback) - 조민희
            string sprite = !string.IsNullOrEmpty(spriteName) ? spriteName : (skillIcon != null ? skillIcon.name : "");
            return new PlacedSkillData(skillId, gridPosition, currentRotation, shapeType, skillName, skillType, sprite);
        }

        // 정적 메서드: 현재 드래그 중인 아이템 반환
        public static SkillDraggableItem GetCurrentDraggingItem()
        {
            return currentDraggingItem;
        }

        // 정적 메서드: 드래그 중인지 확인
        public static bool IsAnyDragging()
        {
            return currentDraggingItem != null;
        }

    }
}
