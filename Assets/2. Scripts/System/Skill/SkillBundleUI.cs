using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Resource;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 스킬 번들 UI 컴포넌트
    /// 작성자: 조민희
    /// Skill Bundle 프리팹에 연결하여 스킬 정보 표시
    /// </summary>
    public class SkillBundleUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image skillIcon;           // Skill Icon (Skill Slot Button > Skill Icon)
        [SerializeField] private TMP_Text skillNameText;    // Skill Name Text
        [SerializeField] private Slider skillSlider;        // Skill Slider
        [SerializeField] private TMP_Text sliderText;       // Slider 내부 Text (예: "2/4")

        private SkillData currentSkillData;

        /// <summary>
        /// 스킬 데이터 설정 및 UI 갱신
        /// </summary>
        /// <param name="data">스킬 데이터</param>
        /// <param name="currentLevel">현재 레벨</param>
        /// <param name="maxLevel">최대 레벨</param>
        public void SetSkillData(SkillData data, int currentLevel = 1, int maxLevel = 200)
        {
            currentSkillData = data;

            if (data == null)
            {
                SetEmptyState();
                return;
            }

            // 아이콘 로드
            SetSkillIcon(data.spriteName);

            // 스킬 이름 설정
            if (skillNameText != null)
            {
                skillNameText.text = data.name;
            }

            // 슬라이더 설정
            if (skillSlider != null)
            {
                skillSlider.maxValue = 1f;
                skillSlider.value = (float)currentLevel / maxLevel;
            }

            // 슬라이더 텍스트 설정 (레벨 표시)
            if (sliderText != null)
            {
                sliderText.text = $"{currentLevel}/{maxLevel}";
            }
        }

        /// <summary>
        /// 스킬 아이콘 설정
        /// </summary>
        private void SetSkillIcon(string spriteName)
        {
            if (skillIcon == null)
            {
                Debug.LogError("[SkillBundleUI] skillIcon이 null입니다! 인스펙터에서 연결해주세요.");
                return;
            }

            if (!string.IsNullOrEmpty(spriteName))
            {
                Sprite sprite = ResourceManager.Instance.LoadSprite(spriteName);
                if (sprite != null)
                {
                    skillIcon.sprite = sprite;
                    skillIcon.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"[SkillBundleUI] 스프라이트 로드 실패: {spriteName}");
                    skillIcon.enabled = false;
                }
            }
            else
            {
                skillIcon.enabled = false;
            }
        }

        /// <summary>
        /// 빈 상태로 설정
        /// </summary>
        private void SetEmptyState()
        {
            if (skillIcon != null) skillIcon.enabled = false;
            if (skillNameText != null) skillNameText.text = "";
            if (skillSlider != null) skillSlider.value = 0;
            if (sliderText != null) sliderText.text = "";
        }

        /// <summary>
        /// 현재 설정된 스킬 데이터 반환
        /// </summary>
        public SkillData GetSkillData()
        {
            return currentSkillData;
        }
    }
}
