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

        private ActiveSkill linkedSkill;
        private SkillData skillData;

        /// <summary>
        /// 스킬 연결 및 초기화
        /// </summary>
        public void SetSkill(ActiveSkill skill, Sprite icon)
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

            UpdateCooldownDisplay();
        }

        /// <summary>
        /// 쿨타임/공격횟수 표시 갱신
        /// </summary>
        private void UpdateCooldownDisplay()
        {
            if (cooldownText == null) return;

            // 디버그: skillData 확인
            if (skillData == null)
            {
                Debug.LogWarning($"[SkillSetButtonUI] skillData가 null입니다.");
                return;
            }

            // AttackCount 모드인 경우
            if (skillData.request == SkillRequest.AttackCount)
            {
                // 디버그: 값 확인
                if (linkedSkill != null)
                {
                    int remaining = linkedSkill.RequiredAttackCount - linkedSkill.CurrentAttackCount;
                    cooldownText.text = $"{remaining}";
                    cooldownText.color = attackCountColor;
                    cooldownText.gameObject.SetActive(true);

                    // 디버그 로그 (한 번만)
                    if (linkedSkill.CurrentAttackCount == 0)
                    {
                        Debug.Log($"[SkillSetButtonUI] {skillData.name}: AttackCount 모드 - 필요: {linkedSkill.RequiredAttackCount}, 현재: {linkedSkill.CurrentAttackCount}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SkillSetButtonUI] linkedSkill이 null입니다.");
                }
            }
            // Cooldown 모드인 경우
            else
            {
                if (linkedSkill.IsOnCooldown)
                {
                    // 1초 단위로 표시 (올림) - 숫자만
                    int seconds = Mathf.CeilToInt(linkedSkill.CurrentCooldown);
                    cooldownText.text = $"{seconds}";
                    cooldownText.color = cooldownColor;
                    cooldownText.gameObject.SetActive(true);
                }
                else
                {
                    // 쿨타임이 아니면 텍스트 숨김
                    cooldownText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 현재 연결된 스킬 반환
        /// </summary>
        public ActiveSkill GetLinkedSkill() => linkedSkill;

        /// <summary>
        /// 스킬이 연결되어 있는지 확인
        /// </summary>
        public bool HasSkill() => linkedSkill != null;
    }
}
