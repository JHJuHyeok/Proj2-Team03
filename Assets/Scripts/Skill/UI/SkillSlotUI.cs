using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Resource;
using SlayerLegend.Skill;

namespace SlayerLegend.UI
{
    // 스킬 슬롯 UI
    // - 아이콘, 이름, 레벨, 쿨타임 표시
    // - 레벨업 버튼 처리
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI cooldownText;

        [Header("액티브 스킬 전용")]
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private GameObject cooldownOverlay;

        [Header("레벨업 버튼")]
        [SerializeField] private Button levelUpButton;
        [SerializeField] private TextMeshProUGUI levelUpCostText;

        private SkillBase _skill;
        private SkillData _skillData;

        public void Initialize(SkillBase skillBase)
        {
            _skill = skillBase;
            _skillData = _skill.Data;

            // UI 초기화
            iconImage.sprite = ResourceManager.Instance.LoadSprite(_skillData.spriteName);
            nameText.text = _skillData.name;
            descriptionText.text = _skillData.explain;
            UpdateLevelInfo();

            // 버튼 이벤트
            levelUpButton.onClick.AddListener(OnLevelUpClicked);

            // 액티브 스킬 쿨타임 UI 활성화
            bool isActiveSkill = _skill is ActiveSkill;
            cooldownFillImage.gameObject.SetActive(isActiveSkill);
            cooldownOverlay.SetActive(isActiveSkill);
        }

        private void UpdateLevelInfo()
        {
            int maxLevel = SkillCalculator.GetMaxLevel(_skillData);
            levelText.text = $"Lv.{_skill.CurrentLevel}/{maxLevel}";

            if (_skill.IsMaxLevel)
            {
                levelUpCostText.text = "MAX";
                levelUpButton.interactable = false;
            }
            else
            {
                int cost = SkillCalculator.GetLevelUpCost(_skillData, _skill.CurrentLevel);
                levelUpCostText.text = $"{cost:N0} G";
                levelUpButton.interactable = true;
            }
        }

        private void OnLevelUpClicked()
            => OnSkillLevelUpRequested?.Invoke(_skill);

        // 쿨타임 UI 업데이트 (액티브 스킬만)
        private void Update()
        {
            if (_skill is ActiveSkill activeSkill)
            {
                cooldownFillImage.fillAmount = activeSkill.CooldownNormalized;

                if (activeSkill.IsOnCooldown)
                {
                    cooldownText.text = $"{activeSkill.CurrentCooldown:F1}s";
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }

                var canvasGroup = cooldownOverlay.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = cooldownOverlay.AddComponent<CanvasGroup>();
                canvasGroup.alpha = activeSkill.IsOnCooldown ? 0.5f : 0f;
            }
        }

        public System.Action<SkillBase> OnSkillLevelUpRequested;

        public void RefreshUI() => UpdateLevelInfo();
    }
}
