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

        // 상태
        private bool isDragging = false;
        private bool isOnGrid = false;
        private Vector2Int gridPosition;
        private int currentRotation = 0;
        private Transform originalParent;
        private Vector3 originalPosition;
        private SkillGridManager gridManager;
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
        public void Initialize(string id, string name, SkillShapeType shape, SkillType type, Sprite icon = null)
        {
            skillId = id;
            skillName = name;
            shapeType = shape;
            skillType = type;

            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
            }

            // 모양에 따른 크기 조정
            UpdateVisualSize();
        }

        // 그리드 매니저 설정
        public void SetGridManager(SkillGridManager manager)
        {
            gridManager = manager;
        }

        // 모양에 따른 시각적 크기 업데이트
        private void UpdateVisualSize()
        {
            var shapeData = SkillShapeData.Create(shapeType);
            var size = shapeData.GetRotatedSize(currentRotation);

            if (rectTransform != null)
            {
                float cellSize = gridManager != null ? gridManager.CellSize : 70f; // 기본값
                rectTransform.sizeDelta = new Vector2(size.x * cellSize, size.y * cellSize);
            }
        }

        // 회전
        public void Rotate()
        {
            currentRotation = (currentRotation + 90) % 360;
            UpdateVisualSize();
            UpdatePreview();

            Debug.Log($"[SkillDraggableItem] {skillName} 회전: {currentRotation}도");
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

            Debug.Log($"[SkillDraggableItem] 드래그 시작: {skillName}");
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

            // 그리드에 배치 시도
            bool placed = TryPlaceOnGrid();

            if (!placed)
            {
                // 배치 실패 시 원래 위치로 복귀
                ReturnToOriginalPosition();
            }

            // 하이라이트 제거
            if (gridManager != null)
            {
                gridManager.ClearAllHighlights();
            }

            OnDragEnded?.Invoke(this);

            Debug.Log($"[SkillDraggableItem] 드래그 종료: {skillName}, 배치: {placed}");
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
                            UpdateVisualSize();
                            Debug.Log($"[SkillDraggableItem] 그리드에서 회전: {skillName} -> {currentRotation}도");
                        }
                        else
                        {
                            Debug.LogWarning($"[SkillDraggableItem] 회전 실패: {skillName}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[SkillDraggableItem] 회전 불가: {skillName}");
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
                UpdateVisualSize();

                OnItemPlaced?.Invoke(this);
                Debug.Log($"[SkillDraggableItem] 배치 성공: {skillName} at {targetPos}");
                return true;
            }

            return false;
        }

        // 원래 위치로 복귀
        private void ReturnToOriginalPosition()
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
            isOnGrid = false;
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
            UpdateVisualSize();
        }

        // 현재 상태 정보 반환
        public PlacedSkillData GetPlacedData()
        {
            return new PlacedSkillData(skillId, gridPosition, currentRotation, shapeType);
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
