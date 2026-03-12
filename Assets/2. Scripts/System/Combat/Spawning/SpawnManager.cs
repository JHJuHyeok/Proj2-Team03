using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [주혁] - DataManager 정적 클래스 전환에 의해 일부 내용 수정(122, 141, 215)

// 몬스터 및 아이템 스폰 관리
public class SpawnManager : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxQueueSize = 20;
    [SerializeField] private float queueSpacing = 2f;
    [SerializeField] private float queueBaseOffset = 1.5f;
    [SerializeField] private MonsterBase commonMonsterPrefab;
    [SerializeField] private MonsterBase bossMonsterPrefab;
    [SerializeField] private string rewardBoxId = "Box_001"; // 보상 상자 몬스터 ID
    public string RewardBoxId => rewardBoxId;

    private Coroutine _spawnCoroutine;
    private StageData _currentStageData;
    private Transform _playerTransform;
    private bool _isAdventureMode = false;

    // 활성화된 몬스터 추적 (Queue)
    // 인덱스 0이 가장 앞에 있는 몬스터 (플레이어와 가장 가까움)
    private List<MonsterBase> _enemyQueue = new List<MonsterBase>();
    private int _totalSpawnedCount = 0;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    public void InitializePools(StageData stageData)
    {
        if (stageData == null) return;

        if (commonMonsterPrefab != null)
            PoolManager.Instance.CreatePool(commonMonsterPrefab, maxQueueSize + 5, transform);
        if (bossMonsterPrefab != null)
            PoolManager.Instance.CreatePool(bossMonsterPrefab, 2, transform);
    }

    public void StartFarmingSpawn(StageData stageData)
    {
        _isAdventureMode = false;
        _currentStageData = stageData;

        InitializePools(stageData);

        _totalSpawnedCount = 0;

        StopSpawning();
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StartAdventureSpawn(StageData stageData)
    {
        _isAdventureMode = true;
        _currentStageData = stageData;

        InitializePools(stageData);

        _totalSpawnedCount = 0;

        StopSpawning();
        _spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public bool SpawnAdventureBoss(StageData stageData)
    {
        StopSpawning();
        CleanUpEnemies();

        _currentStageData = stageData;

        MonsterData bossData = DataManager.monsters.Get(stageData.bossId);
        if (bossData == null)
        {
            Debug.LogError($"[SpawnManager] 모험 보스 데이터 없음: {stageData.bossId}");
            return false;
        }

        MonsterBase boss = SpawnMonster(bossData, bossMonsterPrefab, GetSpawnPosition() + Vector3.right * 3);
        return boss != null;
    }

    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    public void SpawnRewardBox()
    {
        StopSpawning();

        if (string.IsNullOrEmpty(rewardBoxId))
        {
            Debug.LogWarning("[SpawnManager] 보상 상자 ID가 설정되지 않음.");
            return;
        }

        MonsterData boxData = DataManager.monsters.Get(rewardBoxId);
        if (boxData == null)
        {
            Debug.LogError($"[SpawnManager] 보상 상자 데이터 없음 ID: {rewardBoxId}");
            return;
        }

        SpawnMonster(boxData, commonMonsterPrefab, GetSpawnPosition() + Vector3.right * 5);

    }

    public void SpawnBoss()
    {
        StopSpawning();
        CleanUpEnemies();

        MonsterData bossData = DataManager.monsters.Get(_currentStageData.bossId);
        if (bossData == null)
        {
            Debug.LogError($"[SpawnManager] 보스 데이터 없음: {_currentStageData.bossId}");
            return;
        }

        SpawnMonster(bossData, bossMonsterPrefab, GetSpawnPosition() + Vector3.right * 3);
    }

    public void CleanUpEnemies()
    {
        for (int index = _enemyQueue.Count - 1; index >= 0; index--)
        {
            if (_enemyQueue[index] != null)
            {
                PoolManager.Instance.ReturnPool(_enemyQueue[index]);
            }
        }
        _enemyQueue.Clear();
    }

    private void Update()
    {
        UpdateQueuePositions();
    }

    private void UpdateQueuePositions()
    {
        if (_playerTransform == null) return;

        // 파괴되었지만 UnregisterEnemy가 호출되지 않은 null 엔트리 정리
        _enemyQueue.RemoveAll(e => e == null);

        for (int i = 0; i < _enemyQueue.Count; i++)
        {
            MonsterBase enemy = _enemyQueue[i];

            // 타겟 위치: 플레이어 + 오른쪽 * (기본 + 인덱스 * 간격)
            // XZ 평면에서의 2D 로직 가정 (X가 수평)
            Vector3 targetPos = _playerTransform.position + Vector3.right * (queueBaseOffset + i * queueSpacing);

            // Y 위치는 동일하게 유지하거나 지면 높이로 설정
            targetPos.y = enemy.transform.position.y;
            targetPos.z = _playerTransform.position.z;

            enemy.SetTargetPosition(targetPos);
        }
    }

    private IEnumerator SpawnRoutine()
    {
        // 초기 스폰 배치: 큐를 즉시 채움
        int initialNeeded = maxQueueSize - _enemyQueue.Count;

        // 전체 제한에 따라 초기 필요량 스폰 가능 여부 확인
        if (_currentStageData != null)
        {
            int remainingToSpawn = _currentStageData.monsterCount - _totalSpawnedCount;
            if (initialNeeded > remainingToSpawn)
            {
                initialNeeded = remainingToSpawn;
            }
        }

        for (int i = 0; i < initialNeeded; i++)
        {
            if (_currentStageData != null && !string.IsNullOrEmpty(_currentStageData.monsterId))
            {
                MonsterData monsterData = DataManager.monsters.Get(_currentStageData.monsterId);
                if (monsterData != null)
                {
                    SpawnEnemy(monsterData);
                }
                else
                {
                    Debug.LogError($"[SpawnManager] 몬스터 데이터를 찾을 수 없음! ID: '{_currentStageData.monsterId}', monsters DB 개수: {DataManager.monsters.GetAll().Count}");
                }
            }
        }

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 전체 제한 확인
            if (_currentStageData != null && _totalSpawnedCount >= _currentStageData.monsterCount)
            {
                yield break; // 제한 도달 시 스폰 중지
            }

            if (_enemyQueue.Count < maxQueueSize)
            {
                if (_currentStageData != null && !string.IsNullOrEmpty(_currentStageData.monsterId))
                {
                    MonsterData monsterData = DataManager.monsters.Get(_currentStageData.monsterId);
                    if (monsterData != null)
                    {
                        SpawnEnemy(monsterData);
                    }
                }
            }
        }
    }

    private void SpawnEnemy(MonsterData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[SpawnManager] 스폰 불가: 데이터 누락");
            return;
        }

        MonsterBase enemy = SpawnMonster(data, commonMonsterPrefab, GetSpawnPosition());
        if (enemy != null)
        {
            _totalSpawnedCount++;
            // Debug.Log($"[SpawnManager] {data.name} 소환됨. 총 소환: {_totalSpawnedCount}/{_currentStageData.monsterCount}");
        }
    }

    // 공통 몬스터 스폰 로직
    // 프리팹 조회 → 풀 가져오기 → 위치 설정 → 초기화 → flipX → 레이어 설정 → 큐 추가
    private MonsterBase SpawnMonster(MonsterData data, MonsterBase prefab, Vector3 position)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[SpawnManager] 프리팹이 할당되지 않음");
            return null;
        }

        MonsterBase monster = PoolManager.Instance.GetFromPool(prefab);
        if (monster == null) return null;

        monster.transform.position = position;
        monster.Initialize(data, _currentStageData, _playerTransform);

        // 몬스터 스프라이트를 왼쪽(플레이어 방향)으로 전환
        SpriteRenderer spriteRenderer = monster.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.flipX = true;

        _enemyQueue.Add(monster);
        return monster;
    }

    private Vector3 GetSpawnPosition()
    {
        if (_playerTransform == null) return Vector3.zero;

        float spawnXOffset = queueBaseOffset + (_enemyQueue.Count * queueSpacing) + queueSpacing;

        Vector3 spawnPos = _playerTransform.position + Vector3.right * spawnXOffset;
        spawnPos.z = _playerTransform.position.z;
        return spawnPos;
    }

    public void UnregisterEnemy(MonsterBase enemy)
    {
        if (_enemyQueue.Contains(enemy))
        {
            _enemyQueue.Remove(enemy);

            // 모험 모드에서는 CombatManager가 처리하므로 리워드박스/스테이지클리어 로직 스킵
            if (_isAdventureMode) return;

            // 죽은 적이 보상 상자라면, 다른 것을 소환하지 않음.
            if (enemy.IsRewardBox)
            {
                // Debug.Log("[SpawnManager] 보상 상자 처치됨. 스테이지 재시작.");
                // 파밍 스폰을 재시작하면 카운트와 루프가 초기화됨
                StartFarmingSpawn(_currentStageData);
                return;
            }

            // 스테이지의 모든 몬스터가 클리어되었는지 확인
            if (_currentStageData != null && _totalSpawnedCount >= _currentStageData.monsterCount && _enemyQueue.Count == 0)
            {
                SpawnRewardBox();
            }
            // 아직 스폰할 몬스터가 남아있으면 즉시 보충 스폰
            else if (_currentStageData != null && _totalSpawnedCount < _currentStageData.monsterCount
                     && _enemyQueue.Count < maxQueueSize)
            {
                MonsterData monsterData = DataManager.monsters.Get(_currentStageData.monsterId);
                if (monsterData != null)
                {
                    SpawnEnemy(monsterData);
                }
            }
        }
    }

    public MonsterBase GetFirstEnemy()
    {
        for (int i = 0; i < _enemyQueue.Count; i++)
        {
            if (_enemyQueue[i] != null)
                return _enemyQueue[i];
        }
        return null;
    }
}
