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
    /// </summary>
    public class WeaponBundleUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image equipmentIcon;      // Equipment Icon
        [SerializeField] private TMP_Text equipmentName;   // Equipment Name
        [SerializeField] private TMP_Text countText;       // Count Text
        [SerializeField] private TMP_Text levelText;       // Level Text
        [SerializeField] private Slider enhanceSlider;     // Equipment Slider

        [Header("등급 색상")]
        [SerializeField] private Color commonColor = Color.white;
        [SerializeField] private Color uncommonColor = Color.green;
        [SerializeField] private Color rareColor = Color.blue;
        [SerializeField] private Color heroColor = new Color(0.6f, 0.2f, 0.8f); // 보라색
        [SerializeField] private Color legendColor = new Color(1f, 0.5f, 0f);   // 주황색
        [SerializeField] private Color mythColor = new Color(1f, 0.8f, 0f);     // 금색

        private EquipData currentEquipData;

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
