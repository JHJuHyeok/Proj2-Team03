using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 스테이지 슬롯 UI 컴포넌트
public class StageSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text stageIdText;              // 스테이지 ID 텍스트
    [SerializeField] private TMP_Text stageNameText;            // 스테이지 이름 텍스트
    [SerializeField] private TMP_Text dropInfoText;             // 드랍 정보 (장비ID + 확률)
    [SerializeField] private GameObject selectedObject;            // 현재 진행 중 표시 오브젝트
    [SerializeField] private Button slotButton;                 // 상세 버튼

    private StageData stageData;

    public void SetStageData(StageData data)
    {
        stageData = data;
        UpdateUI();
    }

    private void Awake()
    {
        if (slotButton == null)
            slotButton = GetComponent<Button>();
    }

    public void SetOnClickAction(Action action)
    {
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => action?.Invoke());
        }
    }

    private void UpdateUI()
    {
        if (stageData == null) return;

        if (stageIdText != null)
            stageIdText.text = stageData.id;

        if (stageNameText != null)
            stageNameText.text = stageData.name;

        // 드랍 장비 ID와 확률을 합쳐서 표시
        if (dropInfoText != null)
        {
            string equipId = stageData.dropEquipID ?? "-";
            string percent = $"{stageData.dropPercent * 100:F2}%";
            dropInfoText.text = $"{equipId} 드랍 확률 {percent}";
        }
    }

    // 현재 진행 중인 스테이지 여부 설정
    public void SetSelected(bool isSelected)
    {
        if (selectedObject != null)
            selectedObject.SetActive(isSelected);
    }

    public StageData GetStageData()
    {
        return stageData;
    }
}
