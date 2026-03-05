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
        [SerializeField] private Button skillButton;        // 클릭용 버튼 (조민희 추가)

        private SkillData currentSkillData;

        /// <summary>
        /// 스킬 번들 클릭 이벤트 (조민희 추가)
        /// </summary>
        public event System.Action<SkillBundleUI> OnBundleClicked;

        private void Awake()
        {
            // 버튼 클릭 이벤트 연결 (조민희 추가)
            if (skillButton != null)
            {
                skillButton.onClick.AddListener(HandleBundleClicked);
            }
        }

        private void OnDestroy()
        {
            // 이벤트 해제 (조민희 추가)
            if (skillButton != null)
            {
                skillButton.onClick.RemoveListener(HandleBundleClicked);
            }
        }

        /// <summary>
        /// 번들 클릭 핸들러 (조민희 추가)
        /// </summary>
        private void HandleBundleClicked()
        {
            if (currentSkillData == null)
            {
                Debug.LogWarning("[SkillBundleUI] 클릭했지만 스킬 데이터가 없음");
                return;
            }

            // 이벤트 발생
            OnBundleClicked?.Invoke(this);
        }

        /// <summary>
        /// 스킬 데이터 설정 및 UI 갱신
        /// </summary>
        /// <param name="data">스킬 데이터</param>
        /// <param name="ownedCount">현재 보유량</param>
        /// <param name="requiredCount">레벨업에 필요한 개수</param>
        public void SetSkillData(SkillData data, int ownedCount = 0, int requiredCount = 4)
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

            // 슬라이더 설정 (보유량/필요량 비율)
            if (skillSlider != null)
            {
                skillSlider.maxValue = 1f;
                skillSlider.value = (float)ownedCount / requiredCount;
            }

            // 슬라이더 텍스트 설정 (보유량/필요량 표시)
            if (sliderText != null)
            {
                sliderText.text = $"{ownedCount}/{requiredCount}";
            }

            // 보유하지 않은 스킬은 아이콘을 반투명하게 처리
            if (skillIcon != null)
            {
                Color color = skillIcon.color;
                color.a = ownedCount > 0 ? 1f : 0.3f;  // 보유: 100%, 미보유: 30%
                skillIcon.color = color;
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

        /// <summary>
        /// 현재 스킬 ID 반환 (조민희 추가)
        /// </summary>
        public string SkillId => currentSkillData?.id ?? "";
    }
}
