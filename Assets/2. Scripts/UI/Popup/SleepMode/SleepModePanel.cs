using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/*
[승문]
SleepModePanel
- 절전모드 전면 패널
- 슬라이더를 끝까지 밀면 절전모드 해제
- 중간에 놓으면 슬라이더를 원위치로 복귀
*/
public class SleepModePanel : MonoBehaviour, IPointerUpHandler
{
    [Header("UI")]
    [SerializeField] private Slider releaseSlider;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text battleText;

    [Header("Options")]
    [SerializeField] private float releaseThreshold = 0.95f;

    private bool isSleeping;

    private void Awake()
    {
        if (releaseSlider != null)
        {
            releaseSlider.onValueChanged.AddListener(OnSliderValueChanged);
            releaseSlider.value = 0f;
        }

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isSleeping) return;

        UpdateTexts();
    }

    /// <summary>
    /// 절전모드 진입
    /// </summary>
    public void EnterSleepMode()
    {
        isSleeping = true;
        gameObject.SetActive(true);

        if (releaseSlider != null)
            releaseSlider.value = 0f;

        UpdateTexts();

        // 필요하면 여기서 게임 속도/입력 제한 등 처리
        // Time.timeScale = 0f; 는 전투까지 멈출 거면 사용
    }

    /// <summary>
    /// 절전모드 해제
    /// </summary>
    public void ExitSleepMode()
    {
        isSleeping = false;

        if (releaseSlider != null)
            releaseSlider.value = 0f;

        gameObject.SetActive(false);

        // 필요하면 여기서 원상복구
        // Time.timeScale = 1f;
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isSleeping) return;

        if (value >= releaseThreshold)
        {
            ExitSleepMode();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSleeping) return;
        if (releaseSlider == null) return;

        // 끝까지 안 밀고 손을 떼면 원위치
        if (releaseSlider.value < releaseThreshold)
            releaseSlider.value = 0f;
    }

    private void UpdateTexts()
    {
        if (timeText != null)
            timeText.text = System.DateTime.Now.ToString("HH : mm");

        // 아래는 프로젝트 상황에 맞게 연결
        if (stageText != null)
            stageText.text = "스테이지";

        if (stageNameText != null)
            stageNameText.text = "전투 중...";

        if (battleText != null)
            battleText.text = "절전모드";
    }
}