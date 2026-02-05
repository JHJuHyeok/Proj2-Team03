using UnityEngine;
using Combat.Drop;

public enum CombatState
{
    Farming,
    BossBattle
}

// 전투 로직을 담당하는 매니저
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Managers")]
    [SerializeField] private StageManager stageManager;
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private DropManager dropManager;

    public SpawnManager SpawnManager => spawnManager;
    public StageManager StageManager => stageManager;
    public DropManager DropManager => dropManager;

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerCombatStats playerStats;

    [Header("Initial Stage")]
    [SerializeField] private string initialStageId = "Stage_1";

    public CombatState CurrentState { get; private set; } = CombatState.Farming;

    // 보스전 타이머
    public const float BOSS_TIME_LIMIT = 30f; // 고정 제한 시간 (초)
    private float _bossTimeRemaining = 0f;
    private bool _isBossTimerActive = false;
    public float BossTimeRemaining => _bossTimeRemaining;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (stageManager) stageManager.Initialize(initialStageId);
        if (spawnManager) spawnManager.Initialize(playerTransform);
        if (dropManager) dropManager.Initialize(playerTransform);

        StartFarming();
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath += OnPlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnDeath -= OnPlayerDeath;
        }
    }
    
    private void Update()
    {
        // 보스전 타이머 체크
        if (CurrentState == CombatState.BossBattle && _isBossTimerActive)
        {
            _bossTimeRemaining -= Time.deltaTime;
            if (_bossTimeRemaining <= 0f)
            {
                _isBossTimerActive = false;
                Debug.Log("[CombatManager] 보스전 시간 초과!");
                HandleBossFail();
            }
        }
    }

    // 적 사망 시 호출
    public void OnEnemyKilled(bool isBoss, bool isRewardBox)
    {
        if (CurrentState == CombatState.BossBattle)
        {
            if (isBoss)
            {
                HandleBossWin();
            }
        }
        else
        {
            if (isRewardBox)
            {
                spawnManager.StartFarmingSpawn(stageManager.CurrentStageData);
            }
            else
            {
                stageManager.AddKill();
            }
        }
    }

    // 보스전 시작
    public void StartBossBattle()
    {
        Debug.Log("[CombatManager] 보스전 시작!");
        CurrentState = CombatState.BossBattle;

        // 제한 시간 타이머 시작
        _bossTimeRemaining = BOSS_TIME_LIMIT;
        _isBossTimerActive = true;

        spawnManager.SpawnBoss();
    }

    private void HandleBossWin()
    {
        _isBossTimerActive = false;
        Debug.Log("[CombatManager] 보스 처치! 스테이지 클리어.");
        // TODO: 다음 스테이지 로드 로직
        StartFarming();
    }

    // 보스전 실패 처리
    public void HandleBossFail()
    {
        _isBossTimerActive = false;
        spawnManager.CleanUpEnemies();
        Debug.Log("[CombatManager] 보스전 실패. 일반 스테이지로 복귀.");
        stageManager.ResetProgress();
        StartFarming();
    }

    private void StartFarming()
    {
        CurrentState = CombatState.Farming;

        if (playerStats != null)
        {
            playerStats.FullRestore();
        }

        spawnManager.StartFarmingSpawn(stageManager.CurrentStageData);
    }

    private void OnPlayerDeath()
    {
        if (CurrentState == CombatState.BossBattle)
        {
            HandleBossFail();
        }
        else
        {
            Debug.Log("[CombatManager] 파밍 중 플레이어 사망. 스테이지 재시작.");
            StartFarming();
        }
    }
}
