using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// [주혁] - DataManager 정적 클래스 전환에 의해 코드 수정(114, 144, 146, 155, 157)
// [주혁] - StageData 리팩토링으로 인한 코드 수정(100, 104)

// 스테이지 상세 정보 팝업 UI를 관리하는 매니저
public class StageDetailPopupUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private RectTransform popupContent;        // 팝업 내부 컨텐츠 영역 (이 밖을 클릭하면 닫힘)

    [Header("기본 정보")]
    [SerializeField] private TMP_Text stageNameText;

    [Header("보상 정보")]
    [SerializeField] private TMP_Text goldRewardText;           // 골드획득량
    [SerializeField] private TMP_Text expRewardText;            // 경험치획득량
    [SerializeField] private TMP_Text autoGoldRewardText;           // 분당 골드획득량 (minGoldDrop * 80)
    [SerializeField] private TMP_Text autoExpRewardText;            // 분당 경험치획득량 (expDrop * 80)

    [Header("큐브 정보")]
    [SerializeField] private TMP_Text cubeCountText;            // 큐브 획득량
    [SerializeField] private TMP_Text cubePercentText;          // 큐브 획득 확률

    [Header("장비 정보")]
    [SerializeField] private TMP_Text dropPercentText;          // 장비 드랍 확률
    [SerializeField] private Image equipIconImage;              // 장비 아이콘
    [SerializeField] private Image equipGradeBackgroundImage;   // 장비 등급 배경

    private StageData currentStageData;
    private bool skipFrame;

    // 팝업 열기 (StageData 포함)
    public void Open(StageData stageData)
    {
        if (stageData == null) return;

        currentStageData = stageData;
        OpenPopup();
    }

    // 팝업 열기 (버튼 연결용)
    public void OpenPopup()
    {
        // 데이터가 설정되지 않았다면 현재 진행 중인 스테이지 데이터 사용
        if (currentStageData == null)
        {
            if (CombatManager.Instance != null && CombatManager.Instance.StageManager != null)
            {
                currentStageData = CombatManager.Instance.StageManager.CurrentStageData;
            }
        }

        gameObject.SetActive(true);
        skipFrame = true;
        UpdateUI();
    }

    // 팝업 닫기
    public void Close()
    {
        currentStageData = null; // 데이터 초기화 (다음에 열 때 현재 스테이지 정보를 불러올 수 있도록)
        gameObject.SetActive(false);
    }

    // 영역 밖 클릭 시 닫기 (LateUpdate에서 Input으로 감지)
    private void LateUpdate()
    {
        if (skipFrame) { skipFrame = false; return; }

        if (Input.GetMouseButtonDown(0) && popupContent != null)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                popupContent, Input.mousePosition, GetUICamera()))
            {
                Close();
            }
        }
    }

    private Camera GetUICamera()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            return canvas.rootCanvas.worldCamera;
        return null;
    }

    private void UpdateUI()
    {
        if (currentStageData == null) return;

        // 기본 정보
        if (stageNameText != null) stageNameText.text = currentStageData.name;

        if (goldRewardText != null) goldRewardText.text = currentStageData.goldDrop.ToString("N0");
        if (expRewardText != null) expRewardText.text = currentStageData.expDrop.ToString("N0");

        // 골드/경험치 보상 (분당 획득량 = 기본값 * 80)
        string goldPerMin = currentStageData.goldPerMin.ToString("N0");
        string expPerMin = currentStageData.expPerMin.ToString("N0");

        if (autoGoldRewardText != null) autoGoldRewardText.text = $"{goldPerMin}/m";
        if (autoExpRewardText != null) autoExpRewardText.text = $"{expPerMin}/m";

        // 드랍 확률
        if (dropPercentText != null)
            dropPercentText.text = $"{currentStageData.dropPercent * 100:F2}%";

        // 큐브 정보
        if (cubeCountText != null) cubeCountText.text = $"{currentStageData.cubeCount}";
        if (cubePercentText != null) cubePercentText.text = $"{currentStageData.cubeRate * 100:F2}%";

        // 장비 정보 업데이트
        UpdateEquipInfo();
    }

    private async void UpdateEquipInfo()
    {
        if (string.IsNullOrEmpty(currentStageData.dropEquipID))
        {
            return;
        }

        // 드랍 장비 ID로 EquipData 찾기 (무기 또는 장신구)
        EquipData equipData = FindEquipData(currentStageData.dropEquipID);

        if (equipData != null)
        {
            // 장비 아이콘 설정
            if (equipIconImage != null && !string.IsNullOrEmpty(equipData.spriteName))
            {
                Sprite icon = await SpriteManager.GetSprite(SpriteManager.AtlasBase + "Atlas_Sword.spriteatlasv2", equipData.spriteName);
                if (icon != null) equipIconImage.sprite = icon;
            }

            // 장비 등급 배경 설정
            if (equipGradeBackgroundImage != null)
            {
                // Debug.Log("장비 등급 배경 설정: " + equipData.grade.ToString());
                Sprite bg = await SpriteManager.GetSprite(SpriteManager.AtlasBase + "Atlas_UI.spriteatlasv2", equipData.grade.ToString());
                if (bg != null) equipGradeBackgroundImage.sprite = bg;
            }
        }
    }

    // ID로 장비 데이터 찾기 (무기 -> 장신구 순서 검색)
    private EquipData FindEquipData(string equipId)
    {
        // 무기에서 검색
        if (DataManager.weapons != null)
        {
            var weapons = DataManager.weapons.GetAll();
            if (weapons != null)
            {
                var weapon = weapons.Find(w => w.id == equipId);
                if (weapon != null) return weapon;
            }
        }

        // 장신구에서 검색
        if (DataManager.accessories != null)
        {
            var accessories = DataManager.accessories.GetAll();
            if (accessories != null)
            {
                var accessory = accessories.Find(a => a.id == equipId);
                if (accessory != null) return accessory;
            }
        }

        return null;
    }
}
