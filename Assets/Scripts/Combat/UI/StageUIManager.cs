using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 스테이지 관련 UI를 관리하는 매니저
public class StageUIManager : MonoBehaviour
{
    [Header("일반 전투 패널")]
    [SerializeField] private GameObject normalPanel;
    [SerializeField] private TMP_Text normalStageIdText;
    [SerializeField] private TMP_Text normalStageNameText;
    [SerializeField] private RectTransform progressGauge;
    [SerializeField] private float progressGaugeMaxWidth = 200f;

    [Header("보스 전투 패널")]
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private TMP_Text bossStageIdText;
    [SerializeField] private TMP_Text bossStageNameText;
    [SerializeField] private RectTransform bossTimerGauge;
    [SerializeField] private RectTransform bossHpGauge;
    [SerializeField] private float bossGaugeMaxWidth = 200f;

    private void Start()
    {
        // 이벤트 구독
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged += OnCombatStateChanged;
            CombatManager.Instance.StageManager.OnProgressChanged += OnProgressChanged;
            
            // 초기 UI 설정
            UpdateStageInfo();
            SetNormalPanelActive(true);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged -= OnCombatStateChanged;
            if (CombatManager.Instance.StageManager != null)
            {
                CombatManager.Instance.StageManager.OnProgressChanged -= OnProgressChanged;
            }
        }
    }

    private void Update()
    {
        // 보스전일 때만 타이머와 HP 게이지 업데이트
        if (CombatManager.Instance == null) return;
        if (CombatManager.Instance.CurrentState != CombatState.BossBattle) return;

        UpdateBossTimerGauge();
        UpdateBossHpGauge();
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
        bool isBossBattle = newState == CombatState.BossBattle;
        SetNormalPanelActive(!isBossBattle);
        SetBossPanelActive(isBossBattle);

        if (isBossBattle)
        {
            // 보스전 시작 시 보스 게이지 초기화 (100%)
            SetGaugeWidth(bossTimerGauge, bossGaugeMaxWidth);
            SetGaugeWidth(bossHpGauge, bossGaugeMaxWidth);
        }
        else
        {
            // 일반 전투로 전환 시 진행도 게이지 초기화 (0%)
            SetGaugeWidth(progressGauge, 0f);
        }

        // 스테이지 정보 갱신
        UpdateStageInfo();
    }

    // 진행도 변경 시 호출 (일반 전투)
    private void OnProgressChanged(float progressRatio)
    {
        if (progressGauge == null) return;
        
        float newWidth = progressGaugeMaxWidth * progressRatio;
        SetGaugeWidth(progressGauge, newWidth);
    }

    // 보스 타이머 게이지 업데이트
    private void UpdateBossTimerGauge()
    {
        if (bossTimerGauge == null) return;

        float remaining = CombatManager.Instance.BossTimeRemaining;
        float maxTime = CombatManager.BOSS_TIME_LIMIT;
        float ratio = Mathf.Clamp01(remaining / maxTime);
        
        float newWidth = bossGaugeMaxWidth * ratio;
        SetGaugeWidth(bossTimerGauge, newWidth);
    }

    // 보스 HP 게이지 업데이트
    private void UpdateBossHpGauge()
    {
        if (bossHpGauge == null) return;

        float ratio = CombatManager.Instance.BossHpRatio;
        float newWidth = bossGaugeMaxWidth * ratio;
        SetGaugeWidth(bossHpGauge, newWidth);
    }

    // 게이지 너비 설정
    private void SetGaugeWidth(RectTransform gauge, float width)
    {
        if (gauge == null) return;
        
        Vector2 sizeDelta = gauge.sizeDelta;
        sizeDelta.x = width;
        gauge.sizeDelta = sizeDelta;
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
