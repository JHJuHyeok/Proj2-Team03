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

        // TODO: 클리어 상태 데이터 연동 시 구현
        // 현재는 기본적으로 미완료 상태 표시
        if (doneObject != null) doneObject.SetActive(false);
        if (todoObject != null) todoObject.SetActive(true);

        // 진행도 게이지 초기화
        if (fillGauge != null)
        {
            fillGauge.fillAmount = 0f;
        }
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
