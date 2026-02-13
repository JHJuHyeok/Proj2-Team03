# Combat 자동공격 테스트 가이드

## 1. 씬 설정

### 필수 GameObject 구성

| GameObject | 컴포넌트 | 설명 |
|------------|----------|------|
| **Player** | `PlayerStats`, `PlayerCombat`, `PlayerTargeting` | 플레이어 오브젝트 |
| **CombatManager** | `CombatManager`, `StageManager`, `SpawnManager` | 전투 시스템 관리자 |
| **DataManager** | `DataManager` | 데이터 로드 (Singleton) |
| **PoolManager** | `PoolManager` | 오브젝트 풀링 |

---

## 2. 레이어 설정

**Edit → Project Settings → Tags and Layers**

- `Enemy` 레이어 생성 (몬스터용)

---

## 3. JSON 데이터 준비

### `Assets/Resources/Json/Monster/MonsterList.json`
```json
{
  "monsterList": [
    {
      "id": "Monster_01",
      "name": "슬라임",
      "prefabAddress": "Prefabs/Monsters/Slime",
      "maxHp": 100,
      "attack": 10,
      "attackRange": 1.0,
      "attackSpeed": 1.0,
      "type": 0,
      "dropGold": 10,
      "dropExp": 5
    }
  ]
}
```

### `Assets/Resources/Json/Stage/StageList.json`
```json
{
  "stageList": [
    {
      "id": "Stage_1",
      "name": "테스트 스테이지",
      "targetKillCount": 10,
      "enemyIds": ["Monster_01"],
      "bossId": null
    }
  ]
}
```

### `Assets/Resources/Json/Player/PlayerStatsList.json`
이미 생성됨 (`Player_Default`)

---

## 4. Addressable 설정

1. **Window → Asset Management → Addressables → Groups**
2. 몬스터 프리팹을 Addressable로 등록
3. Address 값을 JSON의 `prefabAddress`와 일치시킴
   - 예: `Prefabs/Monsters/Slime`

---

## 5. 몬스터 프리팹 준비

1. 몬스터 프리팹 생성
2. `MonsterBase` 상속 클래스 추가 (예: `NormalMonster.cs`)
3. Collider 추가
4. 레이어를 `Enemy`로 설정

### NormalMonster 예시
```csharp
public class NormalMonster : MonsterBase
{
    public override bool IsBoss => false;
    public override bool IsRewardBox => false;
}
```

---

## 6. Inspector 설정

### CombatManager
| 필드 | 값 |
|------|-----|
| Stage Manager | StageManager 오브젝트 |
| Spawn Manager | SpawnManager 오브젝트 |
| Player Transform | Player 오브젝트 |
| Player Stats | Player의 PlayerStats |
| Initial Stage Id | `Stage_1` |

### SpawnManager
| 필드 | 값 |
|------|-----|
| Spawn Interval | 2 (초) |
| Spawn Radius | 10 (미터) |

### PlayerTargeting
| 필드 | 값 |
|------|-----|
| Enemy Layer | Enemy 레이어 선택 |
| Detection Radius | 15 |

---

## 7. 테스트 실행

1. Play 버튼 클릭
2. 콘솔에서 확인:
   - `데이터 로드 완료`
   - `[SpawnManager] Pools initialized`
   - `[SpawnManager] Spawned 슬라임 at ...`
   - `[PlayerCombat] Attacked ... for X damage`

---

## 트러블슈팅

| 문제 | 해결 |
|------|------|
| 몬스터가 스폰되지 않음 | Addressable 주소 확인, JSON 데이터 확인 |
| 플레이어가 공격하지 않음 | Enemy 레이어 설정, DetectionRange 확인 |
| NullReferenceException | DataManager가 씬에 있는지 확인 |
