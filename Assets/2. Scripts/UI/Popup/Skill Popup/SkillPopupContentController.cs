using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SlayerLegend.Skill;

/*
[승문]
SkillPopupContentController
-스킬 상세 팝업 전용 컨트롤러
-메인메뉴에서 클릭한 skillId 하나만 받아 상세 UI 갱신
-리스트/셀/풀링 구조는 메인메뉴에 있으므로 팝업 안에서는 제거
-AUTO 토글/강화 버튼은 현재 선택 스킬 하나에 대해서만 동작
*/
public class SkillPopupContentController : MonoBehaviour
{
    [Header("System Reference(씬/매니저 오브젝트)")]
    [SerializeField] private SkillController systemSkillController; // 팀 스킬 컨트롤러(선택)

    [Header("Popup Root(팝업 프리팹 내부)")]
    [SerializeField] private SkillPopup popupRoot; // 속성 아이콘 반영용

    [Header("Popup UI(Detail)(팝업 프리팹 내부)")]
    [SerializeField] private Image popupDetailIcon;            // 스킬 아이콘
    [SerializeField] private TMP_Text popupGradeAndNameText;   // [등급] 이름
    [SerializeField] private TMP_Text popupExplainText;        // 스킬 설명
    [SerializeField] private TMP_Text popupEffectText;         // 효과 설명
    [SerializeField] private TMP_Text popupDelayLabelText;     // 쿨타임/필요공격수 라벨
    [SerializeField] private TMP_Text popupDelayValueText;     // 쿨타임/필요공격수 값
    [SerializeField] private TMP_Text popupMpValueText;        // MP 숫자
    [SerializeField] private TMP_Text popupUpgradeCostText;    // 강화 비용

    [Header("Popup UI(Actions)(팝업 프리팹 내부)")]
    [SerializeField] private Slider popupAutoSlider;           // AUTO 슬라이더
    [SerializeField] private Button popupUpgradeButton;        // 강화 버튼

    [Header("Resource")]
    [SerializeField] private string spriteResourcesBasePath = "Icons"; // Resources/Icons

    private string currentSkillId; // 현재 선택 스킬 ID

    private void Awake()
    {
        // 스킬 컨트롤러 자동 캐싱
        if (systemSkillController == null)
        {
            systemSkillController = FindFirstObjectByType<SkillController>();
        }

        // 버튼 이벤트 연결
        if (popupUpgradeButton != null)
        {
            popupUpgradeButton.onClick.AddListener(OnClickUpgrade);
        }

        // 슬라이더 이벤트 연결
        if (popupAutoSlider != null)
        {
            popupAutoSlider.onValueChanged.AddListener(OnAutoSliderChanged);
        }

        // 초기 상태 비우기
        ClearTexts();
    }

    /// <summary>
    /// 메인메뉴에서 클릭한 skillId를 상세 패널에 반영
    /// </summary>
    public void SetSkillId(string skillId)
    {
        currentSkillId = skillId;

        if (string.IsNullOrEmpty(currentSkillId))
        {
            ClearTexts();
            return;
        }

        if (DataManager.skills == null)
        {
            Debug.LogError("[SkillPopupContentController] DataManager.skills is null.");
            return;
        }

        SkillData data = FindSkillById(currentSkillId);
        if (data == null)
        {
            Debug.LogWarning("[SkillPopupContentController] SkillData not found: " + currentSkillId);
            return;
        }

        // 속성 아이콘 반영
        if (popupRoot != null)
        {
            popupRoot.SetElementIcon(ConvertElementToAttribute(data.element));
        }

        // 상세 아이콘
        if (popupDetailIcon != null)
        {
            popupDetailIcon.sprite = ResolveSprite(data.spriteName);
        }

        // [등급] 이름
        if (popupGradeAndNameText != null)
        {
            popupGradeAndNameText.text = "[" + ConvertGradeToKorean(data.grade.ToString()) + "] " + data.name;
        }

        // 스킬 설명
        if (popupExplainText != null)
        {
            popupExplainText.text = data.explain;
        }

        // [조민희] 효과 설명 - 레벨에 따른 실제 수치로 동적 생성
        int skillLevel = GetCurrentSkillLevel(currentSkillId);
        if (popupEffectText != null)
        {
            popupEffectText.text = GenerateEffectText(data, skillLevel);
        }

        // request 타입에 따라 라벨/값 변경
        if (popupDelayLabelText != null)
        {
            popupDelayLabelText.text = IsAttackCountRequest(data) ? "필요공격수" : "쿨타임";
        }

        if (popupDelayValueText != null)
        {
            popupDelayValueText.text = IsAttackCountRequest(data)
                ? Mathf.RoundToInt(data.wantedDelay).ToString()
                : data.wantedDelay.ToString("0.##") + "s";
        }

        // MP
        if (popupMpValueText != null)
        {
            popupMpValueText.text = data.needMp.ToString();
        }

        // [조민희] 강화 비용 계산 및 표시
        int currentLevelForCost = GetCurrentSkillLevel(currentSkillId);
        int maxLevel = SkillCalculator.GetMaxLevel(data);
        int cost = SkillCalculator.GetLevelUpCost(data, currentLevelForCost);

        if (popupUpgradeCostText != null)
        {
            if (currentLevelForCost >= maxLevel)
            {
                popupUpgradeCostText.text = "MAX";
            }
            else
            {
                popupUpgradeCostText.text = cost.ToString();
            }
        }

        // [조민희] 버튼 활성화 조건 변경
        if (popupUpgradeButton != null)
        {
            bool canUpgrade = currentLevelForCost < maxLevel;

            // Emerald 보유량 확인
            if (canUpgrade && CurrencyManager.Instance != null)
            {
                double emeraldAmount = CurrencyManager.Instance.GetAmount(CurrencyType.Emerald);
                canUpgrade = emeraldAmount >= cost;
            }

            popupUpgradeButton.interactable = canUpgrade;
        }
    }

    /// <summary>
    /// skillId로 SkillData 찾기
    /// </summary>
    private SkillData FindSkillById(string skillId)
    {
        if (DataManager.skills == null)
        {
            return null;
        }

        // GameDB.Get(id)가 있으면 그걸 쓰고, 없으면 전체 탐색
        SkillData found = DataManager.skills.Get(skillId);
        if (found != null)
        {
            return found;
        }

        var all = DataManager.skills.GetAll();
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i] == null)
            {
                continue;
            }

            if (all[i].id == skillId)
            {
                return all[i];
            }
        }

        return null;
    }

    /// <summary>
    /// request가 공격횟수형인지 판단
    /// </summary>
    private bool IsAttackCountRequest(SkillData data)
    {
        if (data == null)
        {
            return false;
        }

        string req = data.request != null ? data.request.ToString() : "";
        return req == "AttackCount";
    }

    /// <summary>
    /// AUTO 슬라이더 변경
    /// </summary>
    private void OnAutoSliderChanged(float value)
    {
        bool on = value >= 0.5f;
        Debug.Log(on ? "[SkillPopupContentController] Auto ON." : "[SkillPopupContentController] Auto OFF.");

        // TODO: 팀 자동 시전 로직 연결
    }

    /// <summary>
    /// 강화 버튼 클릭
    /// [조민희] Emerald를 사용한 스킬 강화 기능 구현
    /// </summary>
    private void OnClickUpgrade()
    {
        if (string.IsNullOrEmpty(currentSkillId))
        {
            return;
        }

        // 스킬 데이터 확인
        SkillData data = FindSkillById(currentSkillId);
        if (data == null)
        {
            Debug.LogWarning("[SkillPopupContentController] 스킬 데이터를 찾을 수 없습니다: " + currentSkillId);
            return;
        }

        // 현재 레벨 확인
        int currentLevel = GetCurrentSkillLevel(currentSkillId);
        int maxLevel = SkillCalculator.GetMaxLevel(data);

        if (currentLevel >= maxLevel)
        {
            Debug.Log($"[SkillPopupContentController] 이미 최대 레벨입니다. ({currentLevel}/{maxLevel})");
            return;
        }

        // 강화 비용 계산 (Emerald)
        int cost = SkillCalculator.GetLevelUpCost(data, currentLevel);

        // Emerald 보유량 확인
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("[SkillPopupContentController] CurrencyManager가 null입니다.");
            return;
        }

        double emeraldAmount = CurrencyManager.Instance.GetAmount(CurrencyType.Emerald);
        if (emeraldAmount < cost)
        {
            Debug.Log($"[SkillPopupContentController] Emerald 부족! 필요: {cost}, 보유: {emeraldAmount}");
            return;
        }

        // Emerald 차감
        CurrencyManager.Instance.ConsumeCurrency(CurrencyType.Emerald, cost);

        // 스킬 레벨 증가
        SetSkillLevel(currentSkillId, currentLevel + 1);

        // [조민희] SkillController에 알려서 ActiveSkill/PassiveSkill의 레벨 갱신
        if (systemSkillController != null)
        {
            systemSkillController.RefreshSkillLevel(currentSkillId);
        }

        Debug.Log($"[SkillPopupContentController] 스킬 강화 성공! {data.name} Lv.{currentLevel} → Lv.{currentLevel + 1}, 사용 Emerald: {cost}");

        // UI 갱신
        RefreshSkillUIs();
    }

    /// <summary>
    /// [조민희] 현재 스킬 레벨 조회
    /// </summary>
    private int GetCurrentSkillLevel(string skillId)
    {
        if (DataManager.CurrentSaveData?.skillInfo == null)
        {
            return 1; // 기본 레벨
        }

        if (DataManager.CurrentSaveData.skillInfo.TryGetValue(skillId, out var info))
        {
            return info.level > 0 ? info.level : 1;
        }

        return 1; // 기본 레벨
    }

    /// <summary>
    /// [조민희] 스킬 레벨 설정
    /// </summary>
    private void SetSkillLevel(string skillId, int newLevel)
    {
        if (DataManager.CurrentSaveData?.skillInfo == null)
        {
            return;
        }

        // 기존 보유량 유지, 레벨만 업데이트
        int currentCount = 0;
        if (DataManager.CurrentSaveData.skillInfo.TryGetValue(skillId, out var existingInfo))
        {
            currentCount = existingInfo.count;
        }

        DataManager.CurrentSaveData.skillInfo[skillId] = new Possesion
        {
            count = currentCount,
            level = newLevel
        };
    }

    /// <summary>
    /// [조민희] 스킬 관련 UI 갱신
    /// </summary>
    private void RefreshSkillUIs()
    {
        // 현재 팝업 UI 갱신 (강화 비용 텍스트 등)
        SetSkillId(currentSkillId);

        // SkillTabPanelUI 갱신
        var tabPanelUI = FindFirstObjectByType<SlayerLegend.Skill.SkillTabPanelUI>();
        if (tabPanelUI != null)
        {
            tabPanelUI.RefreshSkillList();
        }
    }

    /// <summary>
    /// 텍스트/아이콘 초기화
    /// </summary>
    private void ClearTexts()
    {
        if (popupDetailIcon != null) popupDetailIcon.sprite = null;
        if (popupGradeAndNameText != null) popupGradeAndNameText.text = "";
        if (popupExplainText != null) popupExplainText.text = "";
        if (popupEffectText != null) popupEffectText.text = "";
        if (popupDelayLabelText != null) popupDelayLabelText.text = "";
        if (popupDelayValueText != null) popupDelayValueText.text = "";
        if (popupMpValueText != null) popupMpValueText.text = "";
        if (popupUpgradeCostText != null) popupUpgradeCostText.text = "";

        if (popupUpgradeButton != null)
        {
            popupUpgradeButton.interactable = false;
        }
    }

    /// <summary>
    /// 스킬 아이콘 로드
    /// [조민희] Resources.Load 대신 ResourceManager.Instance.LoadSprite 사용
    /// </summary>
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        // [조민희] ResourceManager를 통해 스프라이트 로드 (Addressables 지원)
        if (SlayerLegend.Resource.ResourceManager.Instance != null)
        {
            return SlayerLegend.Resource.ResourceManager.Instance.LoadSprite(spriteName);
        }

        // Fallback: Resources 폴더에서 로드
        return Resources.Load<Sprite>(spriteResourcesBasePath + "/" + spriteName);
    }

    /// <summary>
    /// 스킬 속성 enum 매핑
    /// </summary>
    private SkillAttribute ConvertElementToAttribute(SkillElement element)
    {
        switch (element)
        {
            case SkillElement.Fire:
                return SkillAttribute.Fire;
            case SkillElement.Water:
                return SkillAttribute.Water;
            case SkillElement.Wind:
                return SkillAttribute.Wind;
            case SkillElement.Earth:
                return SkillAttribute.Earth;
        }

        return SkillAttribute.Fire;
    }

    /// <summary>
    /// 등급 한글 변환
    /// </summary>
    private string ConvertGradeToKorean(string grade)
    {
        if (string.IsNullOrEmpty(grade)) return "일반";
        if (grade == "Common") return "일반";
        if (grade == "Uncommon") return "고급";
        if (grade == "Rare") return "레어";
        if (grade == "Epic") return "영웅";
        if (grade == "Legendary") return "레전";
        return grade;
    }

    /// <summary>
    /// [조민희] 레벨에 따른 동적 effect 텍스트 생성
    /// initialRate + (levelUpValue * (level - 1)) 공식 사용
    /// </summary>
    private string GenerateEffectText(SkillData data, int level)
    {
        if (data == null) return "";

        // 기본 effect 텍스트 가져오기
        string baseEffect = data.effect;
        if (string.IsNullOrEmpty(baseEffect)) return "";

        // initialRate와 levelUpValue 가져오기 (필드 직접 접근)
        double initialRate = data.initialRate;
        double levelUpValue = data.levelUpValue;

        // 현재 레벨의 실제 값 계산
        double currentValue = initialRate + (levelUpValue * (level - 1));

        // effect 텍스트에서 숫자+퍼센트 패턴 찾기 (예: "+30%", "310%", "1000%")
        // n% 패턴 또는 숫자% 패턴을 현재 값으로 교체
        string result = System.Text.RegularExpressions.Regex.Replace(
            baseEffect,
            @"n%|\+?\d+\.?\d*%",
            match =>
            {
                // +기호가 있었는지 확인
                bool hasPlus = match.Value.StartsWith("+");
                // 퍼센트 기호가 있는지 확인
                if (match.Value.Contains("%"))
                {
                    return hasPlus ? $"+{currentValue:F0}%" : $"{currentValue:F0}%";
                }
                return match.Value;
            }
        );

        // n% 패턴이 없었고 그냥 n이나 숫자만 있는 경우
        if (result == baseEffect && baseEffect.Contains("n"))
        {
            result = result.Replace("n", $"{currentValue:F0}");
        }

        return result;
    }
}