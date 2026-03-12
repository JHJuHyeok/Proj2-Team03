using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 모험 패널의 지역 버튼 UI 컴포넌트
/// Adventure Panel > Map Panel > Adventure Button에 부착
/// </summary>
public class AdventureAreaButtonUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;          // Image > Name
    [SerializeField] private Image fillGauge;            // Fill Area > Fill
    [SerializeField] private GameObject doneObject;      // Done (클리어 완료 표시)
    [SerializeField] private TMP_Text clearText;         // Done > Clear Text
    [SerializeField] private GameObject todoObject;      // ToDo (미완료 표시)
    [SerializeField] private Button button;              // 클릭 이벤트용

    private AreaData areaData;

    public void SetData(AreaData data)
    {
        areaData = data;

        if (nameText != null)
            nameText.text = data.name;

        // 클리어 상태 데이터 연동
        ClearSaveManager.Load(ClearSaveManager.Adventure);

        int totalCount = data.stageList != null ? data.stageList.Count : 0;
        int clearedCount = 0;

        for (int i = 0; i < totalCount; i++)
        {
            if (ClearSaveManager.IsCleared(ClearSaveManager.Adventure, data.stageList[i].id))
                clearedCount++;
        }

        float progress = totalCount > 0 ? (float)clearedCount / totalCount : 0f;
        bool isAllCleared = totalCount > 0 && clearedCount == totalCount;

        SetClearState(isAllCleared, progress);
    }

    /// <summary>
    /// 클리어 상태 설정
    /// </summary>
    public void SetClearState(bool isCleared, float progress = 0f)
    {
        if (doneObject != null) doneObject.SetActive(isCleared);
        if (todoObject != null) todoObject.SetActive(!isCleared);

        if (fillGauge != null)
            fillGauge.fillAmount = isCleared ? 1f : progress;
    }

    public void SetOnClickAction(Action action)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }
    }

    public AreaData GetAreaData() => areaData;
}
