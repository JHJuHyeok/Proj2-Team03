# 전투 시스템 확장 계획 (Phase 2)

> 이 문서는 기존 `CombatSystem_Design.md`의 후속 설계입니다.
> 스테이지/스폰 관리가 구현된 상태에서 부족한 전투 로직을 보완합니다.

---

## 1. 현재 구현 상태

| 완료 | 항목 |
|:---:|:---|
| ✅ | `CombatManager` - 전투 상태 관리 |
| ✅ | `StageManager` - 스테이지 진행도, 보물상자 루프 |
| ✅ | `SpawnManager` - 몬스터 스폰, 풀링 연동 |
| ✅ | `MonsterBase` 및 파생 클래스 |
| ❌ | **플레이어 자동 공격** |
| ❌ | **플레이어 스탯/체력 관리** |
| ❌ | **스킬 호출 인터페이스** |
| ❌ | **발사체(Projectile) 시스템** |
| ❌ | **데미지 계산 시스템** |

---

## 2. 추가 구현 목표

### A. 플레이어 자동 전투 (`PlayerCombat`)

플레이어가 자동으로 가장 가까운 적을 감지하여 **근접 공격**합니다.

**핵심 로직:**
1. **타겟 탐지:** 일정 범위 내 가장 가까운 적 탐색
2. **자동 공격:** 공격 쿨타임 기반으로 지속 공격 (근접만)
3. **타겟 전환:** 현재 타겟이 죽으면 다음 타겟으로 자동 전환

**주요 변수:**
- `AttackRange`: 공격 사거리
- `AttackCooldown`: 공격 간 대기 시간
- `AttackDamage`: 기본 공격력

---

### B. 플레이어 스탯 관리 (`PlayerStats`)

플레이어의 스탯(체력, 공격력 등)을 관리합니다.

**핵심 스탯:**
- `MaxHp` / `CurrentHp`
- `AttackPower`
- `AttackSpeed`
- `HpRegenPerSecond`: 초당 체력 회복량
- `MaxMana` / `CurrentMana`
- `CritRate` / `CritDamage` (선택)

**체력 회복 규칙:**
- **시간 자동 회복:** `HpRegenPerSecond` 수치만큼 매초 자동 회복
- **스테이지 시작 시:** 체력 완전 회복

**이벤트:**
- `OnHpChanged`: UI 체력바 업데이트용
- `OnDeath`: 보스전 실패 판정용

---

### C. 스킬 호출 인터페이스 (`SkillInvoker`)

> **참고:** 스킬 데이터 및 세부 로직은 별도 시스템에서 관리합니다.
> 여기서는 **쿨타임/마나 확인 후 스킬 호출**만 담당합니다.

**역할:**
1. 스킬 쿨타임 체크
2. 마나 소모 가능 여부 확인
3. 조건 충족 시 외부 스킬 시스템에 호출 요청

**사용 모드:**
- **자동 모드:** 쿨타임 완료 & 마나 충분 시 자동 발동
- **수동 모드:** 버튼 터치 시 발동 (UI 연동)

```csharp
public interface ISkillHandler
{
    void ExecuteSkill(int skillIndex);
}

public class SkillInvoker : MonoBehaviour
{
    public bool IsAutoMode { get; set; }
    
    public bool TryUseSkill(int skillIndex, float cooldown, float manaCost)
    {
        // 쿨타임 & 마나 확인 후 스킬 실행 요청
    }
}
```

---

### D. 발사체 시스템 (`Projectile`)

**스킬에서 사용하는 원거리 발사체**를 처리합니다.

**핵심 로직:**
- `PoolManager`를 통해 발사체 생성/반환
- 타겟 방향으로 이동 또는 유도
- 충돌 시 데미지 전달 후 반환

---

### E. 데미지 계산 (`DamageCalculator`)

공격자와 피격자의 스탯을 기반으로 최종 데미지를 계산합니다.

**계산 요소:**
- 기본 공격력
- 스킬 배율
- 치명타 확률/배율

---

## 3. 스테이지 타입 확장

현재 스테이지 타입은 `Farming`, `Boss` 2가지이나, 추후 확장을 고려합니다.

| 타입 | 설명 |
|:---|:---|
| `Farming` | 일반 스테이지 무한 반복 (기본) |
| `Boss` | 보스 도전 모드 |
| `Adventure` | 모험 모드 (추후 구현) |
| `Promotion` | 승급 모드 (추후 구현) |

**설계 방향:**
- `CombatState` enum에 타입 추가
- 각 타입별 진입/클리어 조건을 `StageManager`에서 분기 처리
- 스테이지 타입별 데이터는 `StageDataSO`에 `StageType` 필드 추가

```csharp
public enum StageType
{
    Farming,
    Boss,
    Adventure,
    Promotion
}
```

---

## 4. 확장 폴더 구조

```text
Assets/Scripts/Combat/
├── CombatManager.cs
├── CombatSystem_Design.md       // Phase 1
├── CombatSystem_Design_Phase2.md // Phase 2 (이 문서)
├── Data/
│   ├── EnemyDataSO.cs
│   ├── StageDataSO.cs           // StageType 추가
│   └── PlayerStatsSO.cs         // [NEW]
├── Player/                       // [NEW]
│   ├── PlayerCombat.cs          // 근접 자동 공격
│   ├── PlayerStats.cs           // 체력/마나/회복
│   ├── PlayerTargeting.cs       // 적 탐지
│   └── SkillInvoker.cs          // 스킬 호출 인터페이스
├── Projectile/                   // [NEW]
│   └── ProjectileBase.cs
├── Damage/                       // [NEW]
│   └── DamageCalculator.cs
├── Stage/
│   └── StageManager.cs
├── Spawning/
│   └── SpawnManager.cs
└── Units/
    ├── MonsterBase.cs
    └── ...
```

---

## 5. 클래스 설계 상세

### A. `PlayerCombat.cs`
```csharp
public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    
    private MonsterBase _currentTarget;
    private float _lastAttackTime;
    
    /// <summary>
    /// 근접 공격 실행
    /// </summary>
    public void Attack(MonsterBase target) { ... }
    
    private MonsterBase FindClosestEnemy() { ... }
}
```

### B. `PlayerStats.cs`
```csharp
public class PlayerStats : MonoBehaviour
{
    public event Action<float, float> OnHpChanged;
    public event Action OnDeath;
    
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public float AttackPower { get; private set; }
    public float HpRegenPerSecond { get; private set; }
    
    public float MaxMana { get; private set; }
    public float CurrentMana { get; private set; }
    
    /// <summary>
    /// 스테이지 시작 시 호출 - 체력 완전 회복
    /// </summary>
    public void FullRestore() { ... }
    
    public void TakeDamage(float damage) { ... }
    private void RegenerateHp() { ... } // Update에서 호출
}
```

### C. `SkillInvoker.cs`
```csharp
public class SkillInvoker : MonoBehaviour
{
    public bool IsAutoMode { get; set; } = true;
    
    private ISkillHandler _skillHandler;
    private float[] _skillCooldowns;
    
    /// <summary>
    /// 스킬 사용 시도 (쿨타임/마나 확인)
    /// </summary>
    public bool TryUseSkill(int skillIndex, float manaCost)
    {
        // 쿨타임 확인
        // 마나 확인
        // 조건 충족 시 _skillHandler.ExecuteSkill(skillIndex)
    }
}
```

---

## 6. 구현 순서

1. **`PlayerStats`**: 체력/마나/자동회복 관리
2. **`PlayerCombat`**: 자동 타겟팅 및 근접 자동 공격
3. **`DamageCalculator`**: 데미지 공식 정립
4. **`SkillInvoker`**: 스킬 호출 인터페이스
5. **`Projectile`**: 원거리 스킬용 발사체
6. **`StageType` 확장**: Adventure, Promotion 대비 구조 설계
