using UnityEngine;
using Combat.Drop;
using DamageNumbersPro;
using System;

// [주혁] - DataManager 정적 클래스 전환에 따라 일부 내용 수정(61, 63)

public enum CombatState
{
    Farming,
    BossBattle,
    Adventure
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
    [SerializeField] private DamageNumber damageNumberPrefab;

    public DamageNumber DamageNumberPrefab => damageNumberPrefab;

    [Header("Initial Stage")]
    [SerializeField] private string initialStageId = "STG_01_01";

    public CombatState CurrentState { get; private set; } = CombatState.Farming;

    // 보스전 타이머
    public const float BOSS_TIME_LIMIT = 30f; // 고정 제한 시간 (초)
    private float _bossTimeRemaining = 0f;
    private bool _isBossTimerActive = false;
    public float BossTimeRemaining => _bossTimeRemaining;

    // 모험 관련
    public const float ADVENTURE_TIME_LIMIT = 60f;
    private float _adventureTimeRemaining = 0f;
    private bool _isAdventureTimerActive = false;
    private StageData _savedStageData;
    private StageData _adventureStageData;
    private int _adventureMonsterKillCount;
    private int _adventureTotalMonsters;
    private bool _adventureBossPhase;

    public float AdventureTimeRemaining => _adventureTimeRemaining;
    public int AdventureMonsterKillCount => _adventureMonsterKillCount;
    public int AdventureTotalMonsters => _adventureTotalMonsters;
    public bool IsAdventureBossPhase => _adventureBossPhase;

    // 이벤트
    public event Action<CombatState> OnCombatStateChanged;
    public event Action<int, int> OnAdventureMonsterCountChanged;
    public event Action OnAdventureBossPhaseStarted;
    public event Action<bool> OnAdventureCompleted;

    // 보스 HP 비율 (UI용)
    public float BossHpRatio { get; private set; } = 1f;

    public void UpdateBossHpRatio(float ratio)
    {
        BossHpRatio = Mathf.Clamp01(ratio);
    }

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        // 데이터가 아직 로드되지 않았으면 먼저 로드
        if (DataManager.stages.GetAll().Count == 0)
        {
            Debug.Log("데이터 다시 로드");
            await DataManager.LoadAllDatabase();
        }

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

        // 모험 타이머 체크
        if (CurrentState == CombatState.Adventure && _isAdventureTimerActive)
        {
            _adventureTimeRemaining -= Time.deltaTime;
            if (_adventureTimeRemaining <= 0f)
            {
                _isAdventureTimerActive = false;
                Debug.Log("[CombatManager] 모험 시간 초과!");
                HandleAdventureFail();
            }
        }
    }

    // 적 사망 시 호출
    public void OnEnemyKilled(bool isBoss, bool isRewardBox)
    {
        if (CurrentState == CombatState.Adventure)
        {
            if (_adventureBossPhase)
            {
                if (isBoss)
                {
                    HandleAdventureSuccess();
                }
            }
            else
            {
                _adventureMonsterKillCount++;
                OnAdventureMonsterCountChanged?.Invoke(_adventureMonsterKillCount, _adventureTotalMonsters);
                Debug.Log($"[CombatManager] 모험 몬스터 처치: {_adventureMonsterKillCount}/{_adventureTotalMonsters}");

                if (_adventureMonsterKillCount >= _adventureTotalMonsters)
                {
                    StartAdventureBossPhase();
                }
            }
        }
        else if (CurrentState == CombatState.BossBattle)
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
                stageManager.ResetProgress();
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
        BossHpRatio = 1f;
        OnCombatStateChanged?.Invoke(CurrentState);

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
        stageManager.ResetProgress();
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
        OnCombatStateChanged?.Invoke(CurrentState);

        if (playerStats != null)
        {
            playerStats.FullRestore();
        }

        spawnManager.StartFarmingSpawn(stageManager.CurrentStageData);
    }

    // 스테이지 이동 (UI에서 호출)
    public void MoveToStage(StageData stageData)
    {
        if (stageData == null) return;

        spawnManager.StopSpawning();
        spawnManager.CleanUpEnemies();

        PlayFadeTransition(() =>
        {
            stageManager.SetStage(stageData);
            StartFarming();
        });
    }

    private void PlayFadeTransition(Action midAction)
    {
        var stageUI = StageUIManager.Instance;
        if (stageUI != null)
            stageUI.PlayFadeTransition(midAction);
        else
            midAction?.Invoke();
    }

    private void OnPlayerDeath()
    {
        if (CurrentState == CombatState.Adventure)
        {
            HandleAdventureFail();
        }
        else if (CurrentState == CombatState.BossBattle)
        {
            HandleBossFail();
        }
        else
        {
            Debug.Log("[CombatManager] 파밍 중 플레이어 사망. 스테이지 재시작.");
            StartFarming();
        }
    }

    // 모험 시작
    public void StartAdventure(StageData adventureData)
    {
        if (adventureData == null) return;

        // 현재 스테이지 저장 및 기존 스폰 정리 (페이드 전)
        _savedStageData = stageManager.CurrentStageData;
        spawnManager.StopSpawning();
        spawnManager.CleanUpEnemies();

        PlayFadeTransition(() =>
        {
            Debug.Log($"[CombatManager] 모험 시작: {adventureData.name}");

            _adventureStageData = adventureData;
            _adventureMonsterKillCount = 0;
            _adventureTotalMonsters = adventureData.monsterCount;
            _adventureBossPhase = false;
            BossHpRatio = 1f;

            CurrentState = CombatState.Adventure;
            OnCombatStateChanged?.Invoke(CurrentState);

            _adventureTimeRemaining = ADVENTURE_TIME_LIMIT;
            _isAdventureTimerActive = true;

            if (playerStats != null)
                playerStats.FullRestore();

            stageManager.SetStage(adventureData); // OnStageChanged 발동
            spawnManager.StartAdventureSpawn(adventureData);
            OnAdventureMonsterCountChanged?.Invoke(0, _adventureTotalMonsters);
        });
    }

    private void StartAdventureBossPhase()
    {
        Debug.Log("[CombatManager] 모험 보스 페이즈 시작!");
        _adventureBossPhase = true;
        BossHpRatio = 1f;
        OnAdventureBossPhaseStarted?.Invoke();
        bool spawnSuccess = spawnManager.SpawnAdventureBoss(_adventureStageData);
        if (!spawnSuccess)
        {
            Debug.LogError("[CombatManager] 모험 보스 스폰 실패. 모험 실패 처리.");
            HandleAdventureFail();
        }
    }

    private void HandleAdventureSuccess()
    {
        _isAdventureTimerActive = false;
        Debug.Log("[CombatManager] 모험 성공!");
        if (_adventureStageData != null)
            AdventureSaveManager.MarkCleared(_adventureStageData.id);
        OnAdventureCompleted?.Invoke(true);
        RestoreFromAdventure();
    }

    private void HandleAdventureFail()
    {
        _isAdventureTimerActive = false;
        spawnManager.CleanUpEnemies();
        Debug.Log("[CombatManager] 모험 실패.");
        OnAdventureCompleted?.Invoke(false);
        RestoreFromAdventure();
    }

    private void RestoreFromAdventure()
    {
        PlayFadeTransition(() =>
        {
            if (_savedStageData != null)
            {
                stageManager.SetStage(_savedStageData);
                _savedStageData = null;
            }
            _adventureStageData = null;
            _adventureBossPhase = false;
            StartFarming();
        });
    }
}
