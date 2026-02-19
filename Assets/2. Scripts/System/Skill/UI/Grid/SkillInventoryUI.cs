using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Skill.Data;
using SlayerLegend.Resource;

namespace SlayerLegend.Skill.UI.Grid
{
    // 스킬 인벤토리 패널 UI
    // 전체 스킬 목록 표시 및 관리
    public class SkillInventoryUI : MonoBehaviour
    {
        [Header("컴포넌트 참조")]
        [SerializeField] private SkillGridController gridController;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("필터링")]
        [SerializeField] private Toggle showActiveToggle;
        [SerializeField] private Toggle showPassiveToggle;
        [SerializeField] private TMP_Dropdown gradeFilterDropdown;

        [Header("정보 표시")]
        [SerializeField] private TextMeshProUGUI totalCountText;
        [SerializeField] private TextMeshProUGUI placedCountText;

        // 슬롯 목록
        private List<InventorySlotUI> slots = new List<InventorySlotUI>();
        private Dictionary<string, InventorySlotUI> slotsBySkillId = new Dictionary<string, InventorySlotUI>();
        private InventorySlotUI selectedSlot;

        // 이벤트
        public event System.Action<SkillData> OnSkillSelected;

        private void Awake()
        {
            if (gridController == null)
            {
                Debug.LogWarning("[SkillInventoryUI] gridController가 할당되지 않았습니다. 일부 기능이 작동하지 않습니다.");
            }
        }

        private void Start()
        {
            InitializeToggles();
            SubscribeToGridController();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGridController();
        }

        // 토글 초기화
        private void InitializeToggles()
        {
            if (showActiveToggle != null)
            {
                showActiveToggle.isOn = true;
                showActiveToggle.onValueChanged.AddListener(_ => RefreshInventory());
            }

            if (showPassiveToggle != null)
            {
                showPassiveToggle.isOn = true;
                showPassiveToggle.onValueChanged.AddListener(_ => RefreshInventory());
            }

            if (gradeFilterDropdown != null)
            {
                gradeFilterDropdown.value = 0; // "전체"
                gradeFilterDropdown.onValueChanged.AddListener(_ => RefreshInventory());
            }
        }

        // 그리드 컨트롤러 이벤트 구독
        private void SubscribeToGridController()
        {
            if (gridController != null)
            {
                gridController.OnGridUpdated += HandleGridUpdated;
                gridController.OnSkillAddedToInventory += HandleSkillAdded;
                gridController.OnSkillRemovedFromInventory += HandleSkillRemoved;
            }
        }

        // 그리드 컨트롤러 이벤트 구독 해제
        private void UnsubscribeFromGridController()
        {
            if (gridController != null)
            {
                gridController.OnGridUpdated -= HandleGridUpdated;
                gridController.OnSkillAddedToInventory -= HandleSkillAdded;
                gridController.OnSkillRemovedFromInventory -= HandleSkillRemoved;
            }
        }

        // 스킬 목록 로드 (외부에서 호출)
        public void LoadSkills(List<SkillData> skills)
        {
            ClearInventory();

            foreach (var skill in skills)
            {
                if (ShouldShowSkill(skill))
                {
                    CreateSlot(skill);
                }
            }

            UpdateCountDisplay();
        }

        // 스킬 표시 여부 확인
        private bool ShouldShowSkill(SkillData skill)
        {
            // 타입 필터
            if (!showActiveToggle?.isOn ?? false && skill.type == SkillType.Active)
                return false;

            if (!showPassiveToggle?.isOn ?? false && skill.type == SkillType.Passive)
                return false;

            // 등급 필터
            if (gradeFilterDropdown != null && gradeFilterDropdown.value > 0)
            {
                int selectedGrade = gradeFilterDropdown.value - 1; // 0은 "전체"
                if ((int)skill.grade != selectedGrade)
                    return false;
            }

            return true;
        }

        // 슬롯 생성
        private void CreateSlot(SkillData skill)
        {
            if (slotPrefab == null || slotContainer == null)
            {
                Debug.LogError("[SkillInventoryUI] 슬롯 프리팹 또는 컨테이너가 없습니다.");
                return;
            }

            // 이미 존재하는지 확인
            if (slotsBySkillId.ContainsKey(skill.id))
            {
                return;
            }

            // 그리드 컨트롤러에 스킬 추가 (아직 배치되지 않은 상태)
            var draggableItem = gridController?.AddSkillToInventory(
                skill.id,
                skill.name,
                skill.GetShapeType(),
                skill.type,
                ResourceManager.Instance?.LoadSprite(skill.spriteName)
            );

            if (draggableItem == null) return;

            // 슬롯 UI 생성
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            InventorySlotUI slot = slotObj.GetComponent<InventorySlotUI>();

            if (slot == null)
            {
                slot = slotObj.AddComponent<InventorySlotUI>();
            }

            slot.Initialize(skill, draggableItem);
            slot.OnSlotClicked += HandleSlotClicked;

            slots.Add(slot);
            slotsBySkillId[skill.id] = slot;
        }

        // 슬롯 클릭 핸들러
        private void HandleSlotClicked(InventorySlotUI slot)
        {
            // 이전 선택 해제
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
            }

            // 새 선택
            selectedSlot = slot;
            selectedSlot.SetSelected(true);

            OnSkillSelected?.Invoke(slot.SkillData);
        }

        // 인벤토리 새로고침
        public void RefreshInventory()
        {
            foreach (var slot in slots)
            {
                bool shouldShow = slot.SkillData != null && ShouldShowSkill(slot.SkillData);
                slot.gameObject.SetActive(shouldShow);
            }

            UpdateCountDisplay();
        }

        // 인벤토리 초기화
        public void ClearInventory()
        {
            // 1. 먼저 ID 매핑 정리 (다른 코드에서 접근 방지)
            slotsBySkillId.Clear();
            selectedSlot = null;

            // 2. 이벤트 구독 해제 및 Destroy
            foreach (var slot in slots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    slot.OnSlotClicked -= HandleSlotClicked;
                    Destroy(slot.gameObject);
                }
            }

            slots.Clear();

            UpdateCountDisplay();
        }

        // 카운트 표시 업데이트
        private void UpdateCountDisplay()
        {
            if (totalCountText != null)
            {
                totalCountText.text = $"전체: {slots.Count}개";
            }

            if (placedCountText != null && gridController != null)
            {
                int placedCount = gridController.GetPlacedItems().Count;
                placedCountText.text = $"배치됨: {placedCount}개";
            }
        }

        // 이벤트 핸들러
        private void HandleGridUpdated()
        {
            UpdateCountDisplay();
        }

        private void HandleSkillAdded(string skillId)
        {
            Debug.Log($"[SkillInventoryUI] 스킬 추가됨: {skillId}");
        }

        private void HandleSkillRemoved(string skillId)
        {
            if (slotsBySkillId.TryGetValue(skillId, out var slot))
            {
                slot.SetSelected(false);
                if (selectedSlot == slot)
                {
                    selectedSlot = null;
                }
            }

            UpdateCountDisplay();
        }

        // 특정 스킬 슬롯 가져오기
        public InventorySlotUI GetSlot(string skillId)
        {
            return slotsBySkillId.TryGetValue(skillId, out var slot) ? slot : null;
        }

        // 선택된 슬롯 가져오기
        public InventorySlotUI GetSelectedSlot()
        {
            return selectedSlot;
        }

        // 선택 해제
        public void ClearSelection()
        {
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
                selectedSlot = null;
            }
        }
    }
}
