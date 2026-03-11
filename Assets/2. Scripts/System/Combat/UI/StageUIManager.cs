using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

// 스테이지 관련 UI를 관리하는 매니저
public class StageUIManager : MonoBehaviour
{
    public static StageUIManager Instance { get; private set; }

    [Header("일반 전투 패널")]
    [SerializeField] private GameObject normalPanel;
    [SerializeField] private TMP_Text normalStageIdText;
    [SerializeField] private TMP_Text normalStageNameText;
    [SerializeField] private Image progressGauge;

    [Header("보스 전투 패널")]
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private TMP_Text bossStageIdText;
    [SerializeField] private TMP_Text bossStageNameText;
    [SerializeField] private Image bossTimerGauge;
    [SerializeField] private Image bossHpGauge;

    [Header("트랜지션")]
    [SerializeField] private CanvasGroup transitionPanel;
    [SerializeField] private float transitionDuration = 0.3f;

    [Header("버튼")]
    [SerializeField] private Button areaPopupButton;
    [SerializeField] private Button bossButton;

    private AreaUIManager areaUIManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 이벤트 구독
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged += OnCombatStateChanged;
            CombatManager.Instance.StageManager.OnProgressChanged += OnProgressChanged;
            CombatManager.Instance.OnAdventureMonsterCountChanged += OnAdventureMonsterCountChanged;
            CombatManager.Instance.OnAdventureBossPhaseStarted += OnAdventureBossPhaseStarted;

            // 초기 UI 설정
            UpdateStageInfo();
            SetNormalPanelActive(true);
            SetBossPanelActive(false);

            // 진행도 게이지 초기화 (0%에서 시작)
            SetGaugeFill(progressGauge, 0f);
        }

        areaUIManager = FindFirstObjectByType<AreaUIManager>();
        // 버튼 클릭 이벤트를 코드로 연결
        areaPopupButton.onClick.AddListener(() => areaUIManager.OpenPopup());
        bossButton.onClick.AddListener(() => CombatManager.Instance.StartBossBattle());

    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged -= OnCombatStateChanged;
            CombatManager.Instance.OnAdventureMonsterCountChanged -= OnAdventureMonsterCountChanged;
            CombatManager.Instance.OnAdventureBossPhaseStarted -= OnAdventureBossPhaseStarted;
            if (CombatManager.Instance.StageManager != null)
            {
                CombatManager.Instance.StageManager.OnProgressChanged -= OnProgressChanged;
            }
        }
    }

    private void Update()
    {
        if (CombatManager.Instance == null) return;

        var state = CombatManager.Instance.CurrentState;
        if (state == CombatState.BossBattle)
        {
            UpdateBossTimerGauge();
            UpdateBossHpGauge();
        }
        else if (state == CombatState.Adventure)
        {
            UpdateAdventureTimerGauge();
            if (CombatManager.Instance.IsAdventureBossPhase)
                UpdateBossHpGauge();
        }
    }

    // 스테이지 정보 업데이트 (양쪽 패널 모두)
    private void UpdateStageInfo()
    {
        var stageData = CombatManager.Instance?.StageManager?.CurrentStageData;
        if (stageData == null) return;

        // 일반 패널 텍스트 업데이트
        if (normalStageIdText != null)
            normalStageIdText.text = stageData.id;
        if (normalStageNameText != null)
            normalStageNameText.text = stageData.name;

        // 보스 패널 텍스트 업데이트
        if (bossStageIdText != null)
            bossStageIdText.text = stageData.id;
        if (bossStageNameText != null)
            bossStageNameText.text = stageData.name;
    }

    // 전투 상태 변경 시 호출
    private void OnCombatStateChanged(CombatState newState)
    {
        SetNormalPanelActive(newState == CombatState.Farming);
        SetBossPanelActive(newState == CombatState.BossBattle || newState == CombatState.Adventure);

        switch (newState)
        {
            case CombatState.BossBattle:
                SetGaugeFill(bossTimerGauge, 1f);
                SetGaugeFill(bossHpGauge, 1f);
                break;
            case CombatState.Adventure:
                SetGaugeFill(bossTimerGauge, 1f); // 타이머 100%
                SetGaugeFill(bossHpGauge, 1f);    // 남은 몬스터 100%
                break;
            case CombatState.Farming:
                SetGaugeFill(progressGauge, 0f);
                break;
        }

        UpdateStageInfo();
    }

    // 진행도 변경 시 호출 (일반 전투)
    private void OnProgressChanged(float progressRatio)
    {
        SetGaugeFill(progressGauge, progressRatio);
    }

    // 보스 타이머 게이지 업데이트
    private void UpdateBossTimerGauge()
    {
        float ratio = Mathf.Clamp01(CombatManager.Instance.BossTimeRemaining / CombatManager.BOSS_TIME_LIMIT);
        SetGaugeFill(bossTimerGauge, ratio);
    }

    // 보스 HP 게이지 업데이트
    private void UpdateBossHpGauge()
    {
        SetGaugeFill(bossHpGauge, CombatManager.Instance.BossHpRatio);
    }

    // 모험 타이머 게이지 (bossTimerGauge 재사용)
    private void UpdateAdventureTimerGauge()
    {
        float ratio = Mathf.Clamp01(
            CombatManager.Instance.AdventureTimeRemaining / CombatManager.ADVENTURE_TIME_LIMIT);
        SetGaugeFill(bossTimerGauge, ratio);
    }

    // 남은 몬스터 수 게이지 (bossHpGauge 재사용) — 100%→0%로 감소
    private void OnAdventureMonsterCountChanged(int killed, int total)
    {
        if (total <= 0) return;
        SetGaugeFill(bossHpGauge, (float)(total - killed) / total);
    }

    // 모험 보스 페이즈 시작 → bossHpGauge 100% 리셋
    private void OnAdventureBossPhaseStarted()
    {
        SetGaugeFill(bossHpGauge, 1f);
    }

    public void PlayFadeTransition(Action midAction)
    {
        StartCoroutine(FadeTransition(midAction));
    }

    private IEnumerator FadeTransition(Action midAction)
    {
        // Fade in (transparent → black)
        if (transitionPanel != null)
        {
            transitionPanel.gameObject.SetActive(true);
            float t = 0f;
            while (t < transitionDuration)
            {
                transitionPanel.alpha = t / transitionDuration;
                t += Time.deltaTime;
                yield return null;
            }
            transitionPanel.alpha = 1f;
        }

        midAction?.Invoke();
        yield return null; // 1프레임 대기 (스폰/씬 정리 반영)

        // Fade out (black → transparent)
        if (transitionPanel != null)
        {
            float t = 0f;
            while (t < transitionDuration)
            {
                transitionPanel.alpha = 1f - (t / transitionDuration);
                t += Time.deltaTime;
                yield return null;
            }
            transitionPanel.alpha = 0f;
            transitionPanel.gameObject.SetActive(false);
        }
    }

    // 게이지 채우기 설정
    private void SetGaugeFill(Image gauge, float ratio)
    {
        if (gauge == null) return;
        gauge.fillAmount = Mathf.Clamp01(ratio);
    }

    // 일반 패널 활성화/비활성화
    private void SetNormalPanelActive(bool active)
    {
        if (normalPanel != null)
            normalPanel.SetActive(active);
    }

    // 보스 패널 활성화/비활성화
    private void SetBossPanelActive(bool active)
    {
        if (bossPanel != null)
            bossPanel.SetActive(active);
    }
}
