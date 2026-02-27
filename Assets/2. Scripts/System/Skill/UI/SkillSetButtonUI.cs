using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Skill;

namespace SlayerLegend.UI
{
    /// <summary>
    /// 스킬 셋 버튼 UI
    /// - 스킬 아이콘 표시
    /// - 쿨타임 또는 공격 횟수 표시
    /// - 작성자: 조민희
    /// </summary>
    public class SkillSetButtonUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text cooldownText;

        [Header("설정")]
        [SerializeField] private Color cooldownColor = Color.red;
        [SerializeField] private Color attackCountColor = Color.cyan;

        private ISkillDisplayable linkedSkill;
        private SkillData skillData;

        /// <summary>
        /// 스킬 연결 및 초기화 (Active/Passive 모두 지원)
        /// </summary>
        public void SetSkill(ISkillDisplayable skill, Sprite icon)
        {
                linkedSkill = skill;
            skillData = skill?.Data;

            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 스킬 해제
        /// </summary>
        public void ClearSkill()
        {
            linkedSkill = null;
            skillData = null;

            if (iconImage != null)
                iconImage.sprite = null;

            if (cooldownText != null)
                cooldownText.text = "";

            // 버튼은 활성화 유지, 내용만 비움
        }

        /// <summary>
        /// UI 갱신 (매 프레임 호출)
        /// </summary>
        public void UpdateDisplay()
        {
            if (linkedSkill == null || skillData == null)
                return;

            UpdateSkillDisplay();
        }

        /// <summary>
        /// 스킬 표시 갱신 (Active/Passive 공통)
        /// </summary>
        private void UpdateSkillDisplay()
        {
            if (cooldownText == null) return;

            // ISkillDisplayable 인터페이스를 통한 표시 (Active/Passive 모두 지원)
            string displayText = linkedSkill.GetDisplayText();
            Color displayColor = linkedSkill.GetDisplayColor();

            if (string.IsNullOrEmpty(displayText))
            {
                // 표시할 텍스트가 없으면 숨김
                cooldownText.gameObject.SetActive(false);
            }
            else
            {
                cooldownText.text = displayText;
                cooldownText.color = displayColor;
                cooldownText.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// 현재 연결된 스킬 반환
        /// </summary>
        public ISkillDisplayable GetLinkedSkill() => linkedSkill;

        /// <summary>
        /// 스킬이 연결되어 있는지 확인
        /// </summary>
        public bool HasSkill() => linkedSkill != null;
    }
}
