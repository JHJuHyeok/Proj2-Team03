using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Equipment;

/*
[승문]
EquipMergePanelController
-장비 팝업의 융합 탭 UI 전용 컨트롤러
-재료 장비/결과 장비/수량 조절/융합 버튼을 관리
-EquipmentManager의 CanFuse, FindNextGradeData, Fuse를 사용
*/
public class EquipMergePanelController : MonoBehaviour
{
    [Header("Popup UI(Merge Panel)(팝업 프리팹 내부)")]
    [SerializeField] private TMP_Text popupGuideText;        // 상단 안내 문구

    [SerializeField] private TMP_Text popupSourceNameText;   // 재료 장비 이름
    [SerializeField] private Image popupSourceIconImage;     // 재료 장비 아이콘
    [SerializeField] private TMP_Text popupSourceCountText;  // 재료 장비 개수 표시

    [SerializeField] private TMP_Text popupResultNameText;   // 결과 장비 이름
    [SerializeField] private Image popupResultIconImage;     // 결과 장비 아이콘
    [SerializeField] private TMP_Text popupResultCountText;  // 결과 장비 개수 표시

    [SerializeField] private Button popupMinusButton;        // 수량 감소 버튼
    [SerializeField] private TMP_Text popupMergeCountText;   // 현재 선택 수량
    [SerializeField] private Button popupPlusButton;         // 수량 증가 버튼
    [SerializeField] private Button popupMergeButton;        // 융합 버튼

    [SerializeField] private int mergeNeedCount = 5;         // 융합 필요 재료 개수

    private string selectedEquipId;                  // 현재 선택 장비 ID
    private EquipTab currentTab;                     // 무기/악세 탭
    private EquipmentManager systemEquipmentManager; // 장비 매니저
    private int selectedMergeCount;                  // 현재 선택 융합 수량

    private void Awake()
    {
        // 버튼 이벤트 연결
        if (popupMinusButton != null)
        {
            popupMinusButton.onClick.AddListener(OnClickMinus);
        }

        if (popupPlusButton != null)
        {
            popupPlusButton.onClick.AddListener(OnClickPlus);
        }

        if (popupMergeButton != null)
        {
            popupMergeButton.onClick.AddListener(OnClickMerge);
        }

        // 초기 비선택 상태
        BindSelection(null, EquipTab.Weapon, null, null);
    }

    /// <summary>
    /// 선택 장비를 융합 패널에 반영
    /// </summary>
    public void BindSelection(string equipId, EquipTab tab, EquipmentManager equipmentManager, System.Func<string, Sprite> spriteResolver)
    {
        selectedEquipId = equipId;
        currentTab = tab;
        systemEquipmentManager = equipmentManager;
        selectedMergeCount = 0;

        // 탭에 맞는 안내 문구
        if (popupGuideText != null)
        {
            popupGuideText.text = currentTab == EquipTab.Weapon
                ? "*보유 무기 5개로 다음 단계 무기 제작"
                : "*보유 악세 5개로 다음 단계 악세 제작";
        }

        // 장비 선택 없음
        if (string.IsNullOrEmpty(selectedEquipId))
        {
            ClearTexts();

            if (popupMergeButton != null)
            {
                popupMergeButton.interactable = false;
            }

            return;
        }

        EquipData equip = systemEquipmentManager != null ? systemEquipmentManager.GetEquipData(selectedEquipId) : null;
        int owned = systemEquipmentManager != null ? systemEquipmentManager.GetCount(selectedEquipId) : 0;

        // 재료 장비 표시
        if (popupSourceNameText != null)
        {
            popupSourceNameText.text = equip != null ? equip.GetName() : selectedEquipId;
        }

        if (popupSourceIconImage != null && spriteResolver != null)
        {
            popupSourceIconImage.sprite = equip != null ? spriteResolver(equip.spriteName) : null;
        }

        if (popupSourceCountText != null)
        {
            popupSourceCountText.text = owned.ToString() + "(-0)";
        }

        // 결과 장비 표시
        EquipData nextData = systemEquipmentManager != null && equip != null
            ? systemEquipmentManager.FindNextGradeData(equip)
            : null;

        if (popupResultNameText != null)
        {
            popupResultNameText.text = nextData != null ? nextData.GetName() : "다음 단계 장비";
        }

        if (popupResultIconImage != null && spriteResolver != null)
        {
            popupResultIconImage.sprite = nextData != null ? spriteResolver(nextData.spriteName) : null;
        }

        if (popupResultCountText != null)
        {
            popupResultCountText.text = "0(+0)";
        }

        // 실제 융합 가능 여부 반영
        if (popupMergeButton != null && systemEquipmentManager != null)
        {
            popupMergeButton.interactable = systemEquipmentManager.CanFuse(selectedEquipId);
        }

        RefreshMergeCount(owned);
    }

    /// <summary>
    /// 비선택 상태로 초기화
    /// </summary>
    private void ClearTexts()
    {
        if (popupSourceNameText != null) popupSourceNameText.text = "";
        if (popupSourceIconImage != null) popupSourceIconImage.sprite = null;
        if (popupSourceCountText != null) popupSourceCountText.text = "";

        if (popupResultNameText != null) popupResultNameText.text = "";
        if (popupResultIconImage != null) popupResultIconImage.sprite = null;
        if (popupResultCountText != null) popupResultCountText.text = "";

        if (popupMergeCountText != null) popupMergeCountText.text = "0";
    }

    /// <summary>
    /// 수량 감소
    /// </summary>
    private void OnClickMinus()
    {
        if (selectedMergeCount <= 0)
        {
            return;
        }

        selectedMergeCount--;
        ApplyMergeTexts();
    }

    /// <summary>
    /// 수량 증가
    /// </summary>
    private void OnClickPlus()
    {
        if (string.IsNullOrEmpty(selectedEquipId) || systemEquipmentManager == null)
        {
            return;
        }

        int owned = systemEquipmentManager.GetCount(selectedEquipId);
        int maxCount = owned / mergeNeedCount;

        if (selectedMergeCount >= maxCount)
        {
            return;
        }

        selectedMergeCount++;
        ApplyMergeTexts();
    }

    /// <summary>
    /// 융합 버튼 클릭
    /// </summary>
    private void OnClickMerge()
    {
        if (string.IsNullOrEmpty(selectedEquipId) || systemEquipmentManager == null)
        {
            return;
        }

        string resultId;
        bool ok = systemEquipmentManager.Fuse(selectedEquipId, out resultId);

        Debug.Log(ok
            ? "[EquipMergePanelController] Merge ok: " + resultId
            : "[EquipMergePanelController] Merge fail.");
    }

    /// <summary>
    /// 보유량 기준으로 선택 수량 범위 보정
    /// </summary>
    private void RefreshMergeCount(int owned)
    {
        int maxCount = owned / mergeNeedCount;

        if (selectedMergeCount > maxCount)
        {
            selectedMergeCount = maxCount;
        }

        ApplyMergeTexts();
    }

    /// <summary>
    /// 현재 선택 수량을 UI에 반영
    /// </summary>
    private void ApplyMergeTexts()
    {
        if (popupMergeCountText != null)
        {
            popupMergeCountText.text = selectedMergeCount.ToString();
        }

        if (string.IsNullOrEmpty(selectedEquipId) || systemEquipmentManager == null)
        {
            return;
        }

        int owned = systemEquipmentManager.GetCount(selectedEquipId);
        int consume = selectedMergeCount * mergeNeedCount;

        if (popupSourceCountText != null)
        {
            popupSourceCountText.text = owned.ToString() + "(-" + consume.ToString() + ")";
        }

        if (popupResultCountText != null)
        {
            popupResultCountText.text = "0(+" + selectedMergeCount.ToString() + ")";
        }
    }
}