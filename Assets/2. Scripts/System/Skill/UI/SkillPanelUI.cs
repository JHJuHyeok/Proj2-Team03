using UnityEngine;
using TMPro;
using System.Collections.Generic;
using SlayerLegend.Skill;

namespace SlayerLegend.UI
{
    // 스킬 패널 UI
    // - 스킬 목록 표시
    // - 스킬 레벨업 처리
    // - 골드 표시
    public class SkillPanelUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private SkillController skillController;
        [SerializeField] private IGoldProvider goldManager;

        [Header("UI")]
        [SerializeField] private GameObject skillSlotPrefab;
        [SerializeField] private Transform activeSkillContent;
        [SerializeField] private Transform passiveSkillContent;
        [SerializeField] private TextMeshProUGUI totalGoldText;

        private List<SkillSlotUI> _activeSkillSlots = new List<SkillSlotUI>();
        private List<SkillSlotUI> _passiveSkillSlots = new List<SkillSlotUI>();

        private void Start()
        {
            if (goldManager == null)
            {
                Debug.LogWarning("IGoldProvider가 설정되지 않았습니다.");
            }
            else
            {
                goldManager.OnGoldChanged += UpdateGoldDisplay;
            }

            InitializeSkillSlots();
            UpdateGoldDisplay(0);
        }

        private void OnDestroy()
        {
            if (goldManager != null)
                goldManager.OnGoldChanged -= UpdateGoldDisplay;
        }

        // 스킬 슬롯 UI 생성
        private void InitializeSkillSlots()
        {
            if (skillController != null)
            {
                foreach (var activeSkill in skillController.ActiveSkills)
                    CreateSkillSlot(activeSkill, activeSkillContent, _activeSkillSlots);

                foreach (var passiveSkill in skillController.PassiveSkills)
                    CreateSkillSlot(passiveSkill, passiveSkillContent, _passiveSkillSlots);
            }
        }

        private void CreateSkillSlot(SkillBase skill, Transform parent, List<SkillSlotUI> slotList)
        {
            if (skillSlotPrefab == null || parent == null)
            {
                Debug.LogWarning("skillSlotPrefab 또는 parent가 설정되지 않았습니다.");
                return;
            }

            GameObject slotObj = Instantiate(skillSlotPrefab, parent);
            var slotUI = slotObj.GetComponent<SkillSlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(skill);
                slotUI.OnSkillLevelUpRequested += HandleSkillLevelUp;
                slotList.Add(slotUI);
            }
            else
            {
                Debug.LogError("skillSlotPrefab에 SkillSlotUI 컴포넌트가 없습니다!");
            }
        }

        // 스킬 레벨업 처리
        private void HandleSkillLevelUp(SkillBase skill)
        {
            if (skill == null) return;

            int cost = SkillCalculator.GetLevelUpCost(skill.Data, skill.CurrentLevel);

            if (goldManager != null && goldManager.CurrentGold >= cost)
            {
                goldManager.SpendGold(cost);
                skill.LevelUp();

                FindSkillSlotUI(skill)?.RefreshUI();
                UpdateGoldDisplay();
                Debug.Log($"{skill.Data.name} 레벨업 성공! 현재 레벨: {skill.CurrentLevel}");
            }
            else
            {
                Debug.Log($"골드가 부족합니다! 필요: {cost}G");
            }
        }

        private SkillSlotUI FindSkillSlotUI(SkillBase skill)
        {
            foreach (var slot in _activeSkillSlots)
                if (slot.GetComponent<SkillBase>() == skill)
                    return slot;

            foreach (var slot in _passiveSkillSlots)
                if (slot.GetComponent<SkillBase>() == skill)
                    return slot;

            return null;
        }

        private void UpdateGoldDisplay(long gold = -1)
        {
            if (totalGoldText != null && goldManager != null)
            {
                long displayGold = gold >= 0 ? gold : goldManager.CurrentGold;
                totalGoldText.text = $"{displayGold:N0} G";
            }
        }

        public void AddActiveSkillUI(SkillBase skill)
            => CreateSkillSlot(skill, activeSkillContent, _activeSkillSlots);

        public void AddPassiveSkillUI(SkillBase skill)
            => CreateSkillSlot(skill, passiveSkillContent, _passiveSkillSlots);
    }
}
