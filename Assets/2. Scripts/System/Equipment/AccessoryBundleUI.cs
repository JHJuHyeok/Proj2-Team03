using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Equipment;
using SlayerLegend.Resource;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 악세서리 번들 UI 컴포넌트
    /// 작성자: 조민희
    /// Accessory Bundle 프리팹에 연결하여 악세서리 정보 표시
    /// WeaponBundleUI보다 단순한 구조 (이름/개수 텍스트 없음)
    /// [조민희] 클릭 시 EquipPopup 열기 기능 추가
    /// </summary>
    public class AccessoryBundleUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image equipmentIcon;      // Equipment Icon
        [SerializeField] private TMP_Text enhanceText;     // Enhance Text (강화 레벨)
        [SerializeField] private TMP_Text gradeText;       // Grade Text (등급 단계)
        [SerializeField] private Slider enhanceSlider;     // Equipment Slider
        [SerializeField] private Button clickButton;       // [조민희] 클릭용 버튼

        private EquipData currentEquipData;

        private void Awake()
        {
            // [조민희] 버튼 이벤트 연결
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnClick);
            }
        }

        private void OnDestroy()
        {
            // [조민희] 버튼 이벤트 해제
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnClick);
            }
        }

        /// <summary>
        /// [조민희] 장비 슬롯 클릭 시 EquipPopup 열기
        /// </summary>
        private void OnClick()
        {
            if (currentEquipData == null) return;

            // PopupManager를 통해 EquipPopup 열기
            if (PopupManager.Instance != null)
            {
                var param = new EquipPopupParam(EquipTab.Accessory, currentEquipData.GetId());
                PopupManager.Instance.Open(PopupId.Equip, param);
            }
        }

        /// <summary>
        /// 장비 데이터 설정 및 UI 갱신
        /// </summary>
        /// <param name="data">장비 데이터</param>
        /// <param name="count">보유 개수 (사용하지 않음 - Accessory Bundle엔 표시 영역 없음)</param>
        /// <param name="level">강화 레벨</param>
        public void SetEquipData(EquipData data, int count, int level = 1)
        {
            currentEquipData = data;

            if (data == null)
            {
                SetEmptyState();
                return;
            }

            // 아이콘 로드
            SetEquipIcon(data.spriteName);

            // 강화 레벨 설정
            if (enhanceText != null)
            {
                enhanceText.text = $"+{level}";
                enhanceText.gameObject.SetActive(level > 0);
            }

            // 등급 단계 설정 (gradeStep 표시)
            if (gradeText != null)
            {
                gradeText.text = $"{data.gradeStep}등급";
            }

            // 슬라이더 설정 (gradeStep 표시용)
            if (enhanceSlider != null)
            {
                enhanceSlider.value = data.gradeStep / 4f;
            }
        }

        /// <summary>
        /// 장비 아이콘 설정
        /// </summary>
        private void SetEquipIcon(string spriteName)
        {
            if (equipmentIcon == null)
            {
                Debug.LogError("[AccessoryBundleUI] equipmentIcon이 null입니다! 인스펙터에서 연결해주세요.");
                return;
            }

            if (!string.IsNullOrEmpty(spriteName))
            {
                Sprite sprite = ResourceManager.Instance.LoadSprite(spriteName);
                if (sprite != null)
                {
                    equipmentIcon.sprite = sprite;
                    equipmentIcon.enabled = true;
                    // SetNativeSize() 제거 - 스프라이트의 pivot이 (0.25, 0.25)이라 위치가 어긋남
                }
                else
                {
                    Debug.LogWarning($"[AccessoryBundleUI] 스프라이트 로드 실패: {spriteName}");
                    equipmentIcon.enabled = false;
                }
            }
            else
            {
                equipmentIcon.enabled = false;
            }
        }

        /// <summary>
        /// 빈 상태로 설정
        /// </summary>
        private void SetEmptyState()
        {
            if (equipmentIcon != null) equipmentIcon.enabled = false;
            if (enhanceText != null) enhanceText.gameObject.SetActive(false);
            if (gradeText != null) gradeText.text = "";
            if (enhanceSlider != null) enhanceSlider.value = 0;
        }

        /// <summary>
        /// 현재 설정된 장비 데이터 반환
        /// </summary>
        public EquipData GetEquipData()
        {
            return currentEquipData;
        }
    }
}
