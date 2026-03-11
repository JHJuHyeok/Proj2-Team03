using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Equipment;
using SlayerLegend.Resource;

/*
[조민희]
EquipAccessoryBinder
- 악세서리 팝업에서 클릭한 장비의 정보를 직접 표시
- EquipPopupJsonBinder와 별도로 작동하며, EquipPopupContentController에서 호출됨
- AccessorieList.json에서 장비 정보를 가져와 UI에 표시
*/
public class EquipAccessoryBinder : MonoBehaviour
{
    [Header("UI 참조 - 상세 정보")]
    [SerializeField] private Image detailIcon;        // 장비 아이콘
    [SerializeField] private TMP_Text detailName;     // 장비 이름
    [SerializeField] private TMP_Text gradeText;      // 등급명 (일반/고급/레어 등)
    [SerializeField] private TMP_Text gradeStepText;  // 등급 수치 (1등급, 2등급 등)

    [Header("UI 참조 - 융합")]
    [SerializeField] private Image currentEquipIcon;  // 현재 장비 아이콘
    [SerializeField] private TMP_Text currentEquipName; // 현재 장비 이름
    [SerializeField] private Image upgradeIcon;       // 상위 등급 아이콘
    [SerializeField] private TMP_Text upgradeName;    // 상위 등급 이름

    private EquipmentManager equipmentManager;

    private void Awake()
    {
        equipmentManager = EquipmentManager.Instance;
    }

    /// <summary>
    /// 장비 데이터로 UI 갱신 - EquipPopupContentController.ApplySelection()에서 호출
    /// </summary>
    public void Bind(EquipData equip, int level)
    {
        if (equip == null)
        {
            ClearUI();
            return;
        }

        // 상세 정보 갱신
        if (detailIcon != null)
        {
            Sprite sprite = ResourceManager.Instance?.LoadSprite(equip.spriteName);
            if (sprite != null)
            {
                detailIcon.sprite = sprite;
                detailIcon.enabled = true;
            }
        }

        if (detailName != null)
        {
            detailName.text = equip.GetName();
        }

        if (gradeText != null)
        {
            gradeText.text = ConvertGradeToKorean(equip.grade.ToString());
        }

        if (gradeStepText != null)
        {
            gradeStepText.text = $"{equip.gradeStep}등급";
        }

        // 현재 장비 아이콘 (융합 영역)
        if (currentEquipIcon != null)
        {
            Sprite sprite = ResourceManager.Instance?.LoadSprite(equip.spriteName);
            if (sprite != null)
            {
                currentEquipIcon.sprite = sprite;
            }
        }

        if (currentEquipName != null)
        {
            currentEquipName.text = equip.GetName();
        }

        // 상위 등급 아이콘
        LoadUpgradeIcon(equip);

        Debug.Log($"[EquipAccessoryBinder] Bind 완료 - ID: {equip.GetId()}, Name: {equip.GetName()}");
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    public void ClearUI()
    {
        if (detailIcon != null) detailIcon.sprite = null;
        if (detailName != null) detailName.text = "";
        if (gradeText != null) gradeText.text = "";
        if (gradeStepText != null) gradeStepText.text = "";
        if (currentEquipIcon != null) currentEquipIcon.sprite = null;
        if (currentEquipName != null) currentEquipName.text = "";
        if (upgradeIcon != null) upgradeIcon.sprite = null;
        if (upgradeName != null) upgradeName.text = "";
    }

    /// <summary>
    /// 상위 등급 장비 아이콘 로드
    /// </summary>
    private void LoadUpgradeIcon(EquipData currentEquip)
    {
        if (upgradeIcon == null && upgradeName == null) return;
        if (currentEquip == null || equipmentManager == null)
        {
            if (upgradeIcon != null) upgradeIcon.sprite = null;
            if (upgradeName != null) upgradeName.text = "";
            return;
        }

        EquipData upgradeEquip = equipmentManager.FindNextGradeData(currentEquip);

        if (upgradeEquip != null)
        {
            if (upgradeIcon != null)
            {
                Sprite sprite = ResourceManager.Instance?.LoadSprite(upgradeEquip.spriteName);
                if (sprite != null)
                {
                    upgradeIcon.sprite = sprite;
                }
            }
            if (upgradeName != null)
            {
                upgradeName.text = upgradeEquip.GetName();
            }
        }
        else
        {
            if (upgradeIcon != null) upgradeIcon.sprite = null;
            if (upgradeName != null) upgradeName.text = "";
        }
    }

    /// <summary>
    /// 등급을 한글로 변환
    /// </summary>
    private string ConvertGradeToKorean(string grade)
    {
        if (string.IsNullOrEmpty(grade)) return "일반";
        switch (grade)
        {
            case "Common": return "일반";
            case "Uncommon": return "고급";
            case "Rare": return "레어";
            case "Epic": return "영웅";
            case "Legendary": return "레전";
            default: return grade;
        }
    }
}
