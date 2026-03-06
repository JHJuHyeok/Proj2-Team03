using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Equipment;

/*
[승문]
EquipEnhancePanelController
-장비 팝업의 강화 탭 UI 전용 컨트롤러
-장착효과/보유효과 수치를 각각 한 줄 텍스트로 표시
-예: 7% > 9%, +21% > +27%
-실제 효과 계산은 팀 장비 로직 연결 지점으로 남겨둠
*/
public class EquipEnhancePanelController : MonoBehaviour
{
    [Header("Popup UI(Enhance Panel)(팝업 프리팹 내부)")]
    [SerializeField] private TMP_Text popupEquipEffectNameText;   // 장착 효과 이름
    [SerializeField] private TMP_Text popupEquipEffectValueText;  // 장착 효과 수치(예: 7% > 9%)

    [SerializeField] private TMP_Text popupHoldEffectNameText;    // 보유 효과 이름
    [SerializeField] private TMP_Text popupHoldEffectValueText;   // 보유 효과 수치(예: +21% > +27%)

    [SerializeField] private Button popupEnhanceButton;           // 강화 버튼
    [SerializeField] private Button popupEquipButton;             // 장착 버튼
    [SerializeField] private TMP_Text popupEnhanceCostText;       // 강화 비용 텍스트

    private string selectedEquipId;                 // 현재 선택 장비 ID
    private EquipTab currentTab;                    // 무기/악세 탭
    private EquipmentManager systemEquipmentManager;// 장비 매니저

    private void Awake()
    {
        // 버튼 이벤트 연결
        if (popupEnhanceButton != null)
        {
            popupEnhanceButton.onClick.AddListener(OnClickEnhance);
        }

        if (popupEquipButton != null)
        {
            popupEquipButton.onClick.AddListener(OnClickEquip);
        }

        // 초기 비선택 상태
        BindSelection(null, EquipTab.Weapon, null, null);
    }

    /// <summary>
    /// 선택 장비를 강화 탭 UI에 반영
    /// </summary>
    public void BindSelection(string equipId, EquipTab tab, EquipmentManager equipmentManager, System.Func<string, Sprite> spriteResolver)
    {
        selectedEquipId = equipId;
        currentTab = tab;
        systemEquipmentManager = equipmentManager;

        // 장비 선택 없음
        if (string.IsNullOrEmpty(selectedEquipId))
        {
            ClearTexts();

            if (popupEnhanceButton != null) popupEnhanceButton.interactable = false;
            if (popupEquipButton != null) popupEquipButton.interactable = false;
            return;
        }

        if (popupEnhanceButton != null) popupEnhanceButton.interactable = true;
        if (popupEquipButton != null) popupEquipButton.interactable = true;

        int level = systemEquipmentManager != null ? systemEquipmentManager.GetLevel(selectedEquipId) : 1;

        // TODO: 실제 장착효과/보유효과 계산 로직 연결
        if (currentTab == EquipTab.Weapon)
        {
            if (popupEquipEffectNameText != null) popupEquipEffectNameText.text = "공격력 증가";
            if (popupHoldEffectNameText != null) popupHoldEffectNameText.text = "공격력 증가";

            if (popupEquipEffectValueText != null) popupEquipEffectValueText.text = "0% > 0%";
            if (popupHoldEffectValueText != null) popupHoldEffectValueText.text = "+0% > +0%";
        }
        else
        {
            if (popupEquipEffectNameText != null) popupEquipEffectNameText.text = "체력 증가";
            if (popupHoldEffectNameText != null) popupHoldEffectNameText.text = "체력 증가";

            if (popupEquipEffectValueText != null) popupEquipEffectValueText.text = "0% > 0%";
            if (popupHoldEffectValueText != null) popupHoldEffectValueText.text = "+0% > +0%";
        }

        // 강화 비용 표시
        if (popupEnhanceCostText != null && systemEquipmentManager != null)
        {
            popupEnhanceCostText.text = systemEquipmentManager.GetEnhanceCost(level).ToString();
        }
    }

    /// <summary>
    /// 비선택 상태로 초기화
    /// </summary>
    private void ClearTexts()
    {
        if (popupEquipEffectNameText != null) popupEquipEffectNameText.text = "";
        if (popupEquipEffectValueText != null) popupEquipEffectValueText.text = "";

        if (popupHoldEffectNameText != null) popupHoldEffectNameText.text = "";
        if (popupHoldEffectValueText != null) popupHoldEffectValueText.text = "";

        if (popupEnhanceCostText != null) popupEnhanceCostText.text = "";
    }

    /// <summary>
    /// 강화 버튼 클릭
    /// </summary>
    private void OnClickEnhance()
    {
        if (string.IsNullOrEmpty(selectedEquipId) || systemEquipmentManager == null)
        {
            return;
        }

        bool ok = systemEquipmentManager.Enhance(selectedEquipId);
        Debug.Log(ok ? "[EquipEnhancePanelController] Enhance ok." : "[EquipEnhancePanelController] Enhance fail.");
    }

    /// <summary>
    /// 장착 버튼 클릭
    /// </summary>
    private void OnClickEquip()
    {
        if (string.IsNullOrEmpty(selectedEquipId) || systemEquipmentManager == null)
        {
            return;
        }

        bool ok = systemEquipmentManager.Equip(selectedEquipId);
        Debug.Log(ok ? "[EquipEnhancePanelController] Equip ok." : "[EquipEnhancePanelController] Equip fail.");
    }
}