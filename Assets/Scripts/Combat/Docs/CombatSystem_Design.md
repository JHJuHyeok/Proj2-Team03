# 방치형 RPG 전투 시스템 (Combat System) 구현 계획

## 1. 시스템 구조: 중앙 집중형 전투 제어

`CombatManager`를 최상위 컨트롤러로 두고, 하위의 `StageManager`, `SpawnManager`를 조율하는 중앙 집중형 구조를 채택합니다.

### 구조적 특징
*   **CombatManager (Main Controller):** 외부 시스템(UI, 데이터 등)과의 통신 창구이자 전체 전투 흐름을 제어합니다.
*   **Encapsulation (캡슐화):** 하위 매니저의 복잡한 로직을 숨기고 단순화된 인터페이스만 제공합니다.
*   **Mediator (중재자):** `StageManager`와 `SpawnManager` 사이의 데이터 교환을 중재합니다.

---

## 2. 핵심 로직 상세

### A. 스테이지 진행 및 보스전 규칙
사용자의 피드백을 반영하여 다음과 같이 확정합니다.

1.  **일반 스테이지 반복 (Farming Loop):**
    *   일반 몬스터 처치 시 진행도가 증가합니다.
    *   **진행도 100% 달성 시:**
        *   **보물상자 적 (Treasure Box Enemy)**이 마지막으로 스폰됩니다.
        *   보물상자 적 처치 시: 해당 스테이지의 **진행도가 0%로 초기화**되며 처음부터 다시 사냥을 시작합니다. (무한 반복)
    *   이 과정에서 얻은 재화로 플레이어는 성장합니다.

2.  **보스 도전 (Stage Clear):**
    *   **도전 조건:** 진행도와 상관없이 **언제든 도전 가능**합니다.
    *   **보스 승리 시:** 다음 스테이지로 이동 (Stage Level + 1), 진행도 0% 시작.
    *   **보스 패배 시:** 일반 스테이지로 복귀, 진행도 0% 초기화.

3.  **몬스터 행동 패턴:**
    *   **일반 몬스터:** 공격 불가 (이동만 함).
    *   **보물상자 적:** 공격 불가. 처치 시 대량의 보상 + 스테이지 루프 리셋 트리거.
    *   **보스 몬스터:** 공격 가능 (플레이어 위협).

---

## 3. 상세 클래스 설계

### A. `CombatManager` (Main)
*   **위치:** `Assets/Scripts/Combat/CombatManager.cs`
*   **역할:**
    *   전투 시스템 초기화 (`Initialize`)
    *   UI 버튼 이벤트 수신 (`OnBossChallengeButtonClicked`)
    *   전투 상태 관리 (`CombatState`: Farming, BossBattle)
    *   결과 처리 (`HandleBossWin`, `HandleBossFail`)

### B. `StageManager` (Sub)
*   **위치:** `Assets/Scripts/Combat/Stage/StageManager.cs`
*   **역할:**
    *   스테이지 데이터 관리 (`CurrentStageIdx`, `StageProgress`)
    *   `TryEnterBossStep()`: 보스전 진입 요청 처리 (진행도 조건 없음).
    *   `ResetProgress()`: 보스전 실패 시 진행도 초기화.
    *   보물상자 처치 시 진행도 리셋 로직 포함.

### C. `SpawnManager` (Sub)
*   **위치:** `Assets/Scripts/Combat/Spawning/SpawnManager.cs`
*   **역할:**
    *   `PoolManager`를 통한 몬스터 스폰/반환.
    *   `StartFarmingSpawn()`: 일반 몬스터 주기적 스폰.
    *   `SpawnRewardBox()`: 진행도 100% 시 호출. 일반 스폰 중단 후 상자 소환.
    *   `StartBossSpawn(GameObject bossPrefab)`: 보스 소환 및 일반 스폰 중단.
    *   `CleanUp()`: 필드 위 모든 적 제거.

### D. 데이터 구조 (ScriptableObject)
*   **`EnemyDataSO`**: 적 유닛 스탯 정보.
*   **`StageDataSO`**: 스테이지별 몬스터 구성, 보스 정보, 보물상자 정보, 목표 킬 카운트.

---

## 4. 폴더 및 파일 구조

```text
Assets/Scripts/Combat/
├── CombatManager.cs         // [Main Controller]
├── CombatSystem_Design.md   // [Design Doc] 이 설계 문서 파일
├── Data/                    // [Data Structures]
│   ├── StageDataSO.cs
│   └── EnemyDataSO.cs
├── Stage/                   // [Stage Logic]
│   └── StageManager.cs
├── Spawning/                // [Spawn Logic]
│   └── SpawnManager.cs
└── Units/                   // [Unit Logic]
    ├── MonsterBase.cs
    ├── NormalMonster.cs
    ├── BossMonster.cs
    ├── RewardBoxMonster.cs
    └── PlayerCombat.cs
```
