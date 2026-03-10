using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/*
[승문]
SleepModePanel
- 절전모드 전면 패널 제어
- 스크립트가 붙은 오브젝트는 항상 활성화 유지
- 실제 표시/숨김은 panelRoot만 ON/OFF
- 슬라이더를 끝까지 밀면 절전모드 해제
- 중간에 놓으면 슬라이더를 원위치로 복귀
- 일정 시간 입력이 없으면 자동으로 절전모드 진입
- 자동 진입 시간은 인스펙터에서 조절 가능
- 패널 표시 시간은 현실시간
- 절전모드 진입 후 경과시간 표시 가능
*/
public class SleepModePanel : MonoBehaviour, IPointerUpHandler
{
    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Slider releaseSlider;

    [Header("Texts")]
    [SerializeField] private TMP_Text currentTimeText;     // 현실 시간
    [SerializeField] private TMP_Text sleepElapsedText;    // 절전모드 경과 시간
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text stageNameText;
    [SerializeField] private TMP_Text battleText;

    [Header("Release")]
    [SerializeField] private float releaseThreshold = 0.95f;

    [Header("Auto Sleep")]
    [SerializeField] private bool useAutoSleep = true;
    [SerializeField] private float autoSleepAfterSeconds = 30f;
    [SerializeField] private bool ignoreAutoSleepWhilePointerDown = true;

    private bool isSleeping;
    private float lastInputTime;
    private bool pointerDownOnSlider;

    private DateTime sleepStartTime;

    private void Awake()
    {
        if (releaseSlider != null)
        {
            releaseSlider.onValueChanged.AddListener(OnSliderValueChanged);
            releaseSlider.value = 0f;
        }

        lastInputTime = Time.unscaledTime;
        isSleeping = false;
        pointerDownOnSlider = false;

        SetPanelVisible(false);
    }

    private void Update()
    {
        UpdateAutoSleepTimer();

        if (!isSleeping)
            return;

        UpdateTexts();

        if (releaseSlider != null && releaseSlider.value >= releaseThreshold)
        {
            ExitSleepMode();
        }
    }

    /// <summary>
    /// 절전모드 진입
    /// </summary>
    public void EnterSleepMode()
    {
        if (isSleeping)
            return;

        isSleeping = true;
        pointerDownOnSlider = false;
        sleepStartTime = DateTime.Now;

        if (releaseSlider != null)
        {
            releaseSlider.value = 0f;
        }

        SetPanelVisible(true);
        UpdateTexts();
    }

    /// <summary>
    /// 절전모드 해제
    /// </summary>
    public void ExitSleepMode()
    {
        if (!isSleeping)
            return;

        isSleeping = false;
        pointerDownOnSlider = false;

        if (releaseSlider != null)
        {
            releaseSlider.value = 0f;
        }

        SetPanelVisible(false);
        lastInputTime = Time.unscaledTime;
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        if (!isSleeping)
            return;

        if (value >= releaseThreshold)
        {
            ExitSleepMode();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSleeping)
            return;

        if (releaseSlider == null)
            return;

        pointerDownOnSlider = false;

        if (releaseSlider.value < releaseThreshold)
        {
            releaseSlider.value = 0f;
        }
    }

    /// <summary>
    /// Slider EventTrigger PointerDown에 연결 가능
    /// </summary>
    public void NotifySliderPointerDown()
    {
        pointerDownOnSlider = true;
    }

    /// <summary>
    /// Slider EventTrigger PointerUp에 연결 가능
    /// </summary>
    public void NotifySliderPointerUp()
    {
        pointerDownOnSlider = false;

        if (isSleeping && releaseSlider != null && releaseSlider.value < releaseThreshold)
        {
            releaseSlider.value = 0f;
        }
    }

    private void UpdateAutoSleepTimer()
    {
        if (!useAutoSleep)
            return;

        if (isSleeping)
            return;

        bool hasInput =
            Input.anyKeyDown ||
            Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1) ||
            Input.touchCount > 0;

        if (hasInput)
        {
            lastInputTime = Time.unscaledTime;
        }

        if (ignoreAutoSleepWhilePointerDown && pointerDownOnSlider)
        {
            lastInputTime = Time.unscaledTime;
        }

        float idleTime = Time.unscaledTime - lastInputTime;

        if (idleTime >= autoSleepAfterSeconds)
        {
            EnterSleepMode();
        }
    }

    private void UpdateTexts()
    {
        // 1. 현재 현실 시간
        if (currentTimeText != null)
        {
            currentTimeText.text = DateTime.Now.ToString("HH : mm");
        }

        // 2. 절전모드 경과 시간
        if (sleepElapsedText != null)
        {
            TimeSpan elapsed = DateTime.Now - sleepStartTime;
            sleepElapsedText.text = FormatElapsed(elapsed);
        }

        // 아래는 프로젝트 상황에 맞게 나중에 실제값 연결 가능
        if (stageText != null)
        {
            stageText.text = "스테이지";
        }

        if (stageNameText != null)
        {
            stageNameText.text = "전투 중...";
        }

        if (battleText != null)
        {
            battleText.text = "절전모드";
        }
    }

    private string FormatElapsed(TimeSpan elapsed)
    {
        int totalHours = (int)elapsed.TotalHours;
        int minutes = elapsed.Minutes;
        int seconds = elapsed.Seconds;

        return totalHours.ToString("00") + " : " +
               minutes.ToString("00") + " : " +
               seconds.ToString("00");
    }

    /// <summary>
    /// 외부에서 자동 절전 타이머 초기화
    /// 버튼 클릭, 팝업 열기, 전투 시작 등에서 호출 가능
    /// </summary>
    public void ResetAutoSleepTimer()
    {
        lastInputTime = Time.unscaledTime;
    }

    /// <summary>
    /// 절전모드 강제 토글
    /// </summary>
    public void ToggleSleepMode()
    {
        if (isSleeping)
            ExitSleepMode();
        else
            EnterSleepMode();
    }

    public bool IsSleeping => isSleeping;
}