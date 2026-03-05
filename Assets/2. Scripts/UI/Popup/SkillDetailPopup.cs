using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Skill.Data;
using SlayerLegend.Resource;

/*
[조민희]
SkillDetailPopup
- Skill Tab Panel에서 Skill Bundle 클릭 시 표시되는 스킬 상세 팝업
- param으로 skillId 전달받음
- 스킬 정보 표시, 레벨업 기능
*/
public class SkillDetailPopup : UIPopup
{
    [Header("UI 참조")]
    [SerializeField] private Image skillIcon;           // 스킬 아이콘
    [SerializeField] private TMP_Text skillNameText;    // 스킬 이름
    [SerializeField] private TMP_Text skillDescText;    // 스킬 설명
    [SerializeField] private TMP_Text levelText;         // 현재 레벨
    [SerializeField] private TMP_Text ownedCountText;    // 보유량/필요량 (예: 10/4)
    [SerializeField] private Slider levelSlider;        // 레벨업 진행도
    [SerializeField] private Button levelUpButton;      // 레벨업 버튼
    [SerializeField] private Button closeButton;        // 닫기 버튼

    [Header("설정")]
    [SerializeField] private int requiredCountForLevelUp = 4;  // 레벨업에 필요한 개수

    // 현재 스킬 정보
    private string currentSkillId;
    private SkillData currentSkillData;
    private int currentLevel = 1;
    private int currentOwnedCount = 0;

    private void Awake()
    {
        if (levelUpButton != null)
        {
            levelUpButton.onClick.AddListener(OnLevelUpClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }
    }

    /// <summary>
    /// 팝업 열릴 때 호출 - skillId 전달받음
    /// </summary>
    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        string skillId = param as string;

        if (string.IsNullOrEmpty(skillId))
        {
            Debug.LogWarning("[SkillDetailPopup] skillId가 null 또는 비어있음");
            return;
        }

        currentSkillId = skillId;
        LoadSkillData();
        UpdateUI();
    }

    /// <summary>
    /// 스킬 데이터 로드
    /// </summary>
    private void LoadSkillData()
    {
        // SkillData 조회
        currentSkillData = DataManager.skills.Get(currentSkillId);
        if (currentSkillData == null)
        {
            Debug.LogWarning($"[SkillDetailPopup] 스킬 데이터를 찾을 수 없음: {currentSkillId}");
            return;
        }

        // 보유량/레벨 조회
        if (DataManager.CurrentSaveData?.skillInfo != null &&
            DataManager.CurrentSaveData.skillInfo.TryGetValue(currentSkillId, out var possession))
        {
            currentOwnedCount = possession.count;
            currentLevel = possession.level;
        }
        else
        {
            currentOwnedCount = 0;
            currentLevel = 1;
        }
    }

    /// <summary>
    /// UI 갱신
    /// </summary>
    private void UpdateUI()
    {
        if (currentSkillData == null) return;

        // 스킬 아이콘
        if (skillIcon != null)
        {
            Sprite sprite = ResourceManager.Instance?.LoadSprite(currentSkillData.spriteName);
            if (sprite != null)
            {
                skillIcon.sprite = sprite;
            }
        }

        // 스킬 이름
        if (skillNameText != null)
        {
            skillNameText.text = currentSkillData.name;
        }

        // 스킬 설명
        if (skillDescText != null)
        {
            skillDescText.text = currentSkillData.explain ?? "";
        }

        // 레벨
        if (levelText != null)
        {
            levelText.text = $"Lv.{currentLevel}";
        }

        // 보유량/필요량
        if (ownedCountText != null)
        {
            ownedCountText.text = $"{currentOwnedCount}/{requiredCountForLevelUp}";
        }

        // 슬라이더
        if (levelSlider != null)
        {
            levelSlider.maxValue = 1f;
            levelSlider.value = (float)currentOwnedCount / requiredCountForLevelUp;
        }

        // 레벨업 버튼 활성화/비활성화
        if (levelUpButton != null)
        {
            levelUpButton.interactable = currentOwnedCount >= requiredCountForLevelUp;
        }
    }

    /// <summary>
    /// 레벨업 버튼 클릭
    /// </summary>
    private void OnLevelUpClicked()
    {
        if (currentOwnedCount < requiredCountForLevelUp)
        {
            Debug.Log($"[SkillDetailPopup] 보유량 부족: {currentOwnedCount}/{requiredCountForLevelUp}");
            return;
        }

        // 데이터 수정
        if (DataManager.CurrentSaveData?.skillInfo == null)
        {
            Debug.LogError("[SkillDetailPopup] skillInfo가 null");
            return;
        }

        // 보유량 차감,        currentOwnedCount -= requiredCountForLevelUp;
        // 레벨 증가
        currentLevel++;

        // 저장 데이터 업데이트
        DataManager.CurrentSaveData.skillInfo[currentSkillId] = new Possesion
        {
            count = currentOwnedCount,
            level = currentLevel
        };

        Debug.Log($"[SkillDetailPopup] 레벨업! {currentSkillData.name} Lv.{currentLevel}, 남은 보유량: {currentOwnedCount}");

        // UI 갱신
        UpdateUI();

        // 다른 UI들도 갱신 (조민희 추가)
        RefreshAllSkillUIs();
    }

    /// <summary>
    /// 모든 스킬 관련 UI 갱신 (조민희 추가)
    /// </summary>
    private void RefreshAllSkillUIs()
    {
        // SkillTabPanelUI 갱신
        var tabPanelUI = FindFirstObjectByType<SlayerLegend.Skill.SkillTabPanelUI>();
        if (tabPanelUI != null)
        {
            tabPanelUI.RefreshSkillList();
        }

        // SkillInventoryUI 갱신
        var inventoryUI = FindFirstObjectByType<SlayerLegend.Skill.UI.Grid.SkillInventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.RefreshSlotOwnedStates();
        }
    }

    /// <summary>
    /// 닫기 버튼 클릭    /// </summary>
    private void OnCloseClicked()
    {
        CloseSelf();
    }
}
