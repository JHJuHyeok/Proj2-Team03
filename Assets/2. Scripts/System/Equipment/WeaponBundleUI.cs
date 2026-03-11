using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Equipment;
using SlayerLegend.Resource;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 장비 번들 UI 컴포넌트
    /// 작성자: 조민희
    /// Weapon Bundle 프리팹에 연결하여 장비 정보 표시
    /// [조민희] 클릭 시 EquipPopup 열기 기능 추가
    /// [조민희] PopupOpenButton과 연동하여 equipId 전달 기능 추가
    /// </summary>
    public class WeaponBundleUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image equipmentIcon;      // Equipment Icon
        [SerializeField] private TMP_Text equipmentName;   // Equipment Name
        [SerializeField] private TMP_Text countText;       // Count Text
        [SerializeField] private TMP_Text levelText;       // Level Text
        [SerializeField] private Slider enhanceSlider;     // Equipment Slider
        [SerializeField] private Button clickButton;       // [조민희] 클릭용 버튼

        [Header("등급 색상")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color heroColor = new Color(0.6f, 0.2f, 0.8f); // 보라색
        [SerializeField] private Color legendColor = new Color(1f, 0.5f, 0f);   // 주황색
        [SerializeField] private Color mythColor = new Color(1f, 0.8f, 0f);     // 금색

        private EquipData currentEquipData;
        private PopupOpenButton popupOpenButton; // [조민희] PopupOpenButton 참조
        private string currentEquipId; // [조민희] 현재 장비 ID (이벤트용)
        private int currentCount; // [조민희] 현재 보유 수량

        private void Awake()
        {
            // [조민희] PopupOpenButton 컴포넌트 확인
            popupOpenButton = GetComponent<PopupOpenButton>();

            // [조민희] clickButton이 null이면 자동으로 Button 컴포넌트 찾기
            if (clickButton == null)
            {
                clickButton = GetComponent<Button>();
            }

            // [조민희] 그래도 없으면 자식에서 Equipment Slot Button 찾기
            if (clickButton == null)
            {
                Transform slotButton = FindDeepChild(transform, "Equipment Slot Button");
                if (slotButton != null)
                {
                    clickButton = slotButton.GetComponent<Button>();
                }
            }

            // [조민희] 버튼 이벤트 연결
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnClick);
            }
            else
            {
                Debug.LogWarning($"[WeaponBundleUI] Button을 찾을 수 없습니다: {gameObject.name}");
            }

            // [조민희] 장비 강화 이벤트 구독
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentEnhanced += OnEquipmentEnhanced;
            }
        }

        /// <summary>
        /// [조민희] 깊이 우선 자식 검색
        /// </summary>
        private Transform FindDeepChild(Transform parent, string name)
        {
            // 직접 자식 확인
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
            }

            // 재귀적으로 검색
            foreach (Transform child in parent)
            {
                Transform found = FindDeepChild(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void OnDestroy()
        {
            // [조민희] 버튼 이벤트 해제
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnClick);
            }

            // [조민희] EquipmentManager 이벤트 구독 해제
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.OnEquipmentEnhanced -= OnEquipmentEnhanced;
            }
        }

        /// <summary>
        /// [조민희] 강화 이벤트 핸들러
        /// </summary>
        /// <param name="equipId">장비 ID</param>
        /// <param name="newLevel">새로운 레벨</param>
        private void OnEquipmentEnhanced(string equipId, int newLevel)
        {
            // 현재 장비가 강화된 장비인지 확인
            if (currentEquipData == null || !currentEquipData.GetId().Equals(equipId))
            {
                return;
            }

            // 레벨 텍스트 업데이트
            if (levelText != null)
            {
                levelText.text = $"+{newLevel}";
            }
        }

        /// <summary>
        /// [조민희] 장비 슬롯 클릭 시 EquipPopup 열기
        /// </summary>
        private void OnClick()
        {
            if (currentEquipData == null) return;

            // [조민희] PopupOpenButton이 있으면 equipId 전달 후 사용
            if (popupOpenButton != null)
            {
                popupOpenButton.SetEquipId(currentEquipData.GetId());
                popupOpenButton.Open();
                return;
            }

            // PopupManager를 통해 EquipPopup 열기
            if (PopupManager.Instance != null)
            {
                var param = new EquipPopupParam(EquipTab.Weapon, currentEquipData.GetId());
                PopupManager.Instance.Open(PopupId.Equip, param);
            }
        }

        /// <summary>
        /// 장비 데이터 설정 및 UI 갱신
        /// </summary>
        /// <param name="data">장비 데이터</param>
        /// <param name="count">보유 개수</param>
        /// <param name="level">강화 레벨</param>
        public void SetEquipData(EquipData data, int count, int level = 1)
        {
            currentEquipData = data;

            if (data == null)
            {
                SetEmptyState();
                return;
            }

            // [조민희] PopupOpenButton이 있으면 equipId 미리 설정
            if (popupOpenButton != null)
            {
                popupOpenButton.SetEquipId(data.GetId());
            }

            // 아이콘 로드
            SetEquipIcon(data.spriteName);

            // 이름 설정
            if (equipmentName != null)
            {
                equipmentName.text = data.GetName();
                equipmentName.color = GetGradeColor(data.GetGrade());
            }

            // 개수 설정
            if (countText != null)
            {
                countText.text = $"x{count}";
            }

            // 레벨 설정
            if (levelText != null)
            {
                levelText.text = $"+{level}";
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
                Debug.LogError("[WeaponBundleUI] equipmentIcon이 null입니다! 인스펙터에서 연결해주세요.");
                return;
            }

            if (!string.IsNullOrEmpty(spriteName))
            {
                Sprite sprite = ResourceManager.Instance.LoadSprite(spriteName);
                if (sprite != null)
                {
                    equipmentIcon.sprite = sprite;
                    equipmentIcon.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"[WeaponBundleUI] 스프라이트 로드 실패: {spriteName}");
                    equipmentIcon.enabled = false;
                }
            }
            else
            {
                equipmentIcon.enabled = false;
            }
        }

        /// <summary>
        /// 등급에 따른 색상 반환
        /// </summary>
        private Color GetGradeColor(EquipGrade grade)
        {
            return grade switch
            {
                EquipGrade.Common => commonColor,
                EquipGrade.Uncommon => uncommonColor,
                EquipGrade.Rare => rareColor,
                EquipGrade.Hero => heroColor,
                EquipGrade.Legend => legendColor,
                EquipGrade.Myth => mythColor,
                _ => Color.white
            };
        }

        /// <summary>
        /// 빈 상태로 설정
        /// </summary>
        private void SetEmptyState()
        {
            if (equipmentIcon != null) equipmentIcon.enabled = false;
            if (equipmentName != null) equipmentName.text = "장비 없음";
            if (countText != null) countText.text = "";
            if (levelText != null) levelText.text = "";
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
