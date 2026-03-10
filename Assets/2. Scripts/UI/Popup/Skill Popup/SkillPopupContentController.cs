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

        // 효과 설명
        if (popupEffectText != null)
        {
            popupEffectText.text = data.effect;
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

        // 강화 비용(임시)
        if (popupUpgradeCostText != null)
        {
            popupUpgradeCostText.text = "0";
        }

        // 버튼 활성
        if (popupUpgradeButton != null)
        {
            popupUpgradeButton.interactable = true;
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
    /// </summary>
    private void OnClickUpgrade()
    {
        if (string.IsNullOrEmpty(currentSkillId))
        {
            return;
        }

        Debug.Log("[SkillPopupContentController] Upgrade clicked. Hook skill upgrade logic here: " + currentSkillId);
        // TODO: 팀 스킬 강화 시스템 연결
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
}