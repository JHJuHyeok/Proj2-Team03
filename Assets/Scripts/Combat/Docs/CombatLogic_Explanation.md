# 전투 시스템 로직 설명

## 1. SpawnManager 작동 방식 (큐 시스템)
`SpawnManager`는 이제 **큐(Queue) 기반 시스템**으로 작동하여 적들이 오른쪽에서 줄을 지어 나타납니다.

### 핵심 로직 (`SpawnManager.cs`)
1.  **큐 관리 (Queue Management)**: `_enemyQueue` 리스트를 유지 관리합니다. 인덱스 0번이 가장 앞의 적입니다.
2.  **스폰 (Spawning)**: 
    - `maxQueueSize`에 도달할 때까지 주기적으로 적을 스폰합니다.
    - 새로운 적은 큐의 맨 뒤에 추가됩니다.
3.  **위치 지정 (Positioning)**:
    - `UpdateQueuePositions()`에서 모든 적의 **목표 위치(Target Position)**가 동적으로 계산됩니다.
    - 공식: `PlayerPos + Right * (BaseOffset + Index * Spacing)`
    - 이를 통해 적들이 정렬됩니다: 인덱스 0이 가장 가깝고, 인덱스 1이 그 뒤에 서는 식입니다.

## 2. 적 이동 (Enemy Movement)
실제 이동 코드는 **`MonsterBase.cs`**에 있습니다.

### 이동 로직 (`MonsterBase.Move`)
- `SpawnManager`가 적에게 `SetTargetPosition(pos)`를 호출하여 목표 위치를 설정합니다.
- `MonsterBase.Update()` -> `Move()`에서:
    - `_hasTargetPosition`이 true인지 확인합니다.
    - `Vector3.MoveTowards`를 사용하여 현재 위치에서 `_targetPosition`으로 부드럽게 이동합니다.
    - 속도는 `attackSpeed` (임시 사용) 또는 기본값을 따릅니다.

## 3. 플레이어 자동 공격 디버깅 (Player Auto-Attack Debugging)
진단을 돕기 위해 `PlayerCombat.cs`에 디버그 로그를 추가했습니다.

- **타겟 획득**: `[PlayerCombat] Acquired target: [EnemyName]`
    - `PlayerCombat`이 새로운 타겟을 선택했을 때 나타납니다 (큐의 첫 번째 적 우선).
- **공격 실행**: `[PlayerCombat] Attacked ...`
    - 실제 공격이 발생했을 때 나타납니다.

### 플레이어가 공격하지 않는 이유
콘솔에서 다음 문제들을 확인해보세요:
1.  **사거리 (Range)**: `TryAttack`의 주석 처리된 로그를 활성화하면 "Target out of range"가 보이는지 확인하세요.
    - `PlayerStats.AttackRange` (기본값: 2f) 확인.
    - `SpawnManager`의 `queueBaseOffset` 확인. 오프셋이 사거리보다 크면 플레이어가 닿을 수 없습니다.
2.  **쿨타임 (Cooldown)**: 공격 속도가 빈도를 제한합니다.
3.  **태그/레이어 (Tag/Layer)**: 적 프리팹에 `MonsterBase` 컴포넌트가 있고 "Enemy" 레이어에 있는지 확인하세요 (`SpawnManager`가 이를 보장하긴 합니다).

## 4. 설정 (Configuration)
인스펙터에서 다음 값들을 조절하세요:
- **SpawnManager**:
    - `Queue Base Offset`: 첫 번째 적과 플레이어 사이의 거리.
    - `Queue Spacing`: 적들 사이의 간격.
- **PlayerStats**:
    - `Attack Range`: 플레이어의 공격 가능 범위.
