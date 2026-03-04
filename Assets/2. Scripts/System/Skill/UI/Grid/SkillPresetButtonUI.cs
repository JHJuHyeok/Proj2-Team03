using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlayerLegend.Skill.UI.Grid
{
    /*
    [조민희]
    SkillPresetButtonUI
    - 개별 프리셋 버튼 컴포넌트
    - Skill BTN Bar의 각 버튼에 연결
    - 클릭 시 해당 프리셋으로 전환
    */
    public class SkillPresetButtonUI : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private int presetIndex = 0;  // 0~4 (1~5번 프리셋)

        [Header("UI 참조")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;      // "1", "2", "3" 등
        [SerializeField] private GameObject selectedIndicator;  // 선택됨 표시

        [Header("색상")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;

        private bool isSelected = false;

        public int PresetIndex => presetIndex;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void Start()
        {
            // 텍스트 설정 (1~5)
            if (labelText != null)
            {
                labelText.text = (presetIndex + 1).ToString();
            }

            // 초기 상태 설정
            UpdateSelectedState();
        }

        private void OnEnable()
        {
            // 프리셋 변경 이벤트 구독
            if (SkillPresetManager.Instance != null)
            {
                SkillPresetManager.Instance.OnPresetChanged += OnPresetChanged;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (SkillPresetManager.Instance != null)
            {
                SkillPresetManager.Instance.OnPresetChanged -= OnPresetChanged;
            }
        }

        /// <summary>
        /// 버튼 클릭 핸들러
        /// </summary>
        private void OnButtonClicked()
        {
            if (SkillPresetManager.Instance != null)
            {
                SkillPresetManager.Instance.SwitchPreset(presetIndex);
            }
            else
            {
                Debug.LogWarning("[SkillPresetButtonUI] SkillPresetManager가 없음");
            }
        }

        /// <summary>
        /// 프리셋 변경 이벤트 핸들러
        /// </summary>
        private void OnPresetChanged(int newIndex)
        {
            isSelected = (newIndex == presetIndex);
            UpdateSelectedState();
        }

        /// <summary>
        /// 선택 상태 업데이트
        /// </summary>
        public void UpdateSelectedState()
        {
            // 현재 선택된 프리셋 확인
            if (SkillPresetManager.Instance != null)
            {
                isSelected = (SkillPresetManager.Instance.CurrentPresetIndex == presetIndex);
            }

            // UI 업데이트
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(isSelected);
            }

            if (labelText != null)
            {
                labelText.color = isSelected ? selectedColor : normalColor;
            }
        }

        /// <summary>
        /// 프리셋 인덱스 설정 (에디터용)
        /// </summary>
        public void SetPresetIndex(int index)
        {
            presetIndex = Mathf.Clamp(index, 0, SkillPresetSaveData.MAX_PRESETS - 1);
            if (labelText != null)
            {
                labelText.text = (presetIndex + 1).ToString();
            }
        }

        #region 디버그

        [ContextMenu("선택 상태 토글")]
        public void DebugToggleSelected()
        {
            isSelected = !isSelected;
            UpdateSelectedState();
        }

        #endregion
    }
}
