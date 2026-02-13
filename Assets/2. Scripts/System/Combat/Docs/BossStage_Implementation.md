# 보스 스테이지 시스템 구현 현황

**날짜**: 2026-02-04
**작성자**: AI Assistant (Gemini)
**관련 문서**: 
- `CombatSystem_Design.md`
- `CombatSystem_Design_Phase2.md`
- `CombatSystem_Status_Report.md`

---

## 1. 개요

보스 스테이지 시스템의 핵심 기능을 구현했습니다:
- 수동 버튼으로 보스 스테이지 진입
- 보스가 플레이어를 공격
- 제한 시간 내 보스 미처치 시 실패 → 일반 스테이지 복귀

---

## 2. 수정된 파일

### A. `StageData.cs`

**경로**: `Assets/Scripts/Serializable/StageData.cs`

**변경 내용**:
- `bossId` 필드 추가 (보스 몬스터 ID)

```csharp
public class StageData
{
    // ... 기존 필드 ...
    public string bossId;       // 보스 몬스터 ID [NEW]
}
```

---

### B. `SpawnManager.cs`

**경로**: `Assets/Scripts/Combat/Spawning/SpawnManager.cs`

**변경 내용**:
- `SpawnBoss()` 메서드 완전 재작성
- `StageData.bossId`를 사용하여 실제 보스 스폰

**핵심 로직**:
1. `_currentStageData.bossId` 확인
2. `DataManager`에서 보스 몬스터 데이터 조회
3. **보스 전용 프리팹** (`bossMonsterAddress`)으로 풀링
4. 보스 초기화 및 위치 설정

---

### C. `CombatManager.cs`

**경로**: `Assets/Scripts/Combat/CombatManager.cs`

**변경 내용**:
- 보스전 제한 시간 타이머 시스템 추가
- 고정 30초 제한 시간 (`BOSS_TIME_LIMIT = 30f`)

**추가된 필드**:
```csharp
public const float BOSS_TIME_LIMIT = 30f; // 고정 제한 시간
private float _bossTimeRemaining = 0f;
private bool _isBossTimerActive = false;
public float BossTimeRemaining => _bossTimeRemaining; // UI용
```

**핵심 로직**:
1. `StartBossBattle()`: 타이머 시작, 보스 스폰
2. `Update()`: 매 프레임 타이머 감소, 0 이하 시 `HandleBossFail()` 호출
3. `HandleBossWin()`: 타이머 중지, 스테이지 클리어
4. `HandleBossFail()`: 타이머 중지, 적 정리, 일반 스테이지 복귀

---

## 3. 보스 프리팹 생성 가이드 (Unity 에디터)

### Step 1: 보스 프리팹 생성

1. **Project** 창에서 기존 `CommonMonster` 프리팹 복제 (Ctrl+D / Cmd+D)
2. 복제된 프리팹 이름을 `BossMonster`로 변경
3. 프리팹을 더블클릭하여 열기
4. **Inspector**에서 `NormalMonster` 컴포넌트 **제거** (Remove Component)
5. **Add Component** → `BossMonster` 추가
6. 필요시 스프라이트, 크기, 이펙트 등 조정 후 저장

### Step 2: Addressables 등록

1. **Window → Asset Management → Addressables → Groups** 열기
2. 생성한 `BossMonster` 프리팹을 Addressables Group에 드래그
3. 해당 프리팹의 **Address** 값을 `BossMonster`로 설정
   - (SpawnManager의 `bossMonsterAddress` 필드와 일치해야 함)

### Step 3: SpawnManager 확인

1. **Hierarchy**에서 `SpawnManager` 오브젝트 선택
2. **Inspector**에서 `Boss Monster Address` 필드 확인
3. 값이 `BossMonster`인지 확인

---

## 4. 구현 상태 요약

| 기능 | 상태 | 설명 |
|:---|:---:|:---|
| 보스 스테이지 수동 진입 | ✅ | `CombatManager.StartBossBattle()` |
| 보스 플레이어 공격 | ✅ | `BossMonster.cs` |
| 보스 스폰 (전용 프리팹) | ✅ | `SpawnManager.SpawnBoss()` |
| 제한 시간 시스템 | ✅ | 30초 고정, `CombatManager` |
| 시간 초과 시 실패 | ✅ | `HandleBossFail()` → 일반 스테이지 복귀 |
| 보스 처치 시 승리 | ✅ | `HandleBossWin()` |

---

## 4. 사용 방법

### JSON 데이터 설정

`StageData` JSON에 `bossId` 추가 필요:

```json
{
    "id": "Stage_1",
    "name": "스테이지 1",
    "monsterId": "GOBLIN_01",
    "monsterCount": 10,
    "bossId": "3",  // 보스 몬스터 ID
    "minGoldDrop": 10,
    "maxGoldDrop": 50,
    "expDrop": 5
}
```

### 보스전 시작 (UI 버튼 연결)

#### 방법 1: Unity Inspector에서 직접 연결

1. **Hierarchy**에서 보스 도전 버튼 오브젝트 선택
2. **Inspector**에서 `Button` 컴포넌트 찾기
3. **On Click ()** 섹션에서 `+` 버튼 클릭
4. `CombatManager`가 있는 게임 오브젝트를 드래그하여 **None (Object)** 필드에 드롭
5. 드롭다운 메뉴에서 **CombatManager → StartBossBattle()** 선택

#### 방법 2: 스크립트에서 연결

```csharp
using UnityEngine;
using UnityEngine.UI;

public class BossChallengeButton : MonoBehaviour
{
    [SerializeField] private Button bossButton;

    private void Start()
    {
        bossButton?.onClick.AddListener(() => 
            CombatManager.Instance?.StartBossBattle());
    }
}
```

### 타이머 UI 표시 (게이지 바)

#### Step 1: UI 오브젝트 생성

1. **Hierarchy** 우클릭 → **UI → Image**
2. 오브젝트 이름: `BossTimerGauge`
3. **Inspector** 설정:
   - **Image Type**: `Filled`
   - **Fill Method**: `Horizontal` (또는 원하는 방향)
   - **Fill Origin**: `Left` (또는 `Right`)
   - **Source Image**: 원하는 바 이미지 (없으면 UISprite 등 기본)
4. 배경이 필요하면 뒤에 다른 Image 오브젝트 배치

#### Step 2: 타이머 스크립트 연결

1. `BossTimerUI.cs` 스크립트를 게이지 바 오브젝트(또는 부모 오브젝트)에 추가
2. **Inspector**에서:
   - `Timer Image`: 방금 생성한 `BossTimerGauge` 오브젝트 드래그
   - `Timer Panel`: (선택) 타이머 전체를 감싸는 부모 오브젝트 드래그

```csharp
// CombatManager.BOSS_TIME_LIMIT (30초)를 기준으로 
// 남은 시간 비율을 계산하여 fillAmount를 조정합니다.
// 10초 이하 남았을 때 색상이 붉은색으로 변합니다.
```

---

## 5. 참고 사항

- 제한 시간은 `CombatManager.BOSS_TIME_LIMIT` 상수로 30초 고정
- 제한 시간 변경이 필요하면 해당 상수값 수정
- 보스 몬스터는 `MonsterData`에 등록 필요 (type 상관없이 `MonsterBase`로 스폰)
- 보스 공격 기능은 `BossMonster` 클래스에서 처리되므로, 보스용 프리팹이 `BossMonster` 컴포넌트를 사용하는지 확인 필요
