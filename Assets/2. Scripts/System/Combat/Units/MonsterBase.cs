using UnityEngine;
using Combat.Drop;
using SlayerLegend.Skill;

// [주혁] - 몬스터, 스테이지 데이터 리팩토링으로 인해 오류 코드 주석화(23, 24, 132)

[RequireComponent(typeof(SpriteRenderer))]
public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    protected MonsterData _data;
    protected StageData _stageData;
    protected Transform _target;
    protected bool _isDead = false;
    protected double _currentHp;
    protected double _maxHp;

    // [주혁] - 스프라이트 기반 애니메이션 시스템
    // 프레임 범위: 0~4 공격, 5~11 이동, 12 피격, 13~15 대기
    private const int ATTACK_START = 0;
    private const int ATTACK_COUNT = 5;      // 0~4
    private const int MOVE_START = 5;
    private const int MOVE_COUNT = 7;        // 5~11
    private const int HIT_START = 12;
    private const int HIT_COUNT = 1;         // 12
    private const int IDLE_START = 13;
    private const int IDLE_COUNT = 3;        // 13~15
    private const int TOTAL_FRAME_COUNT = 16;

    private Sprite[] _sprites;               // 프리로드된 전체 스프라이트 배열 (0~15)
    protected SpriteRenderer _renderer;      // 캐싱된 SpriteRenderer
    private string _spriteBaseName;          // 몬스터 스프라이트 기본 번호 (예: "001")

    // [주혁] - Animator 상태 제어
    protected Animator _animator;
    private static readonly int StateParam = Animator.StringToHash("state");
    protected const int ANIM_STATE_IDLE = 1;
    protected const int ANIM_STATE_MOVE = 2;
    protected const int ANIM_STATE_DEAD = 3;
    protected const int ANIM_STATE_ATTACK = 10;
    private static readonly int HitParam = Animator.StringToHash("Hit");

    public virtual void Initialize(MonsterData data, StageData stageData, Transform target)
    {
        _data = data;
        _stageData = stageData;
        _target = target;
        _isDead = false;

        // HP 초기화 (StageData에서 가져옴)
        _maxHp = stageData.monsterHp;
        _currentHp = _maxHp;

        // SpriteRenderer 캐싱
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        // Animator 캐싱
        if (_animator == null)
            _animator = GetComponent<Animator>();

        // Sprite Load & 스프라이트 프리로드
        if (!string.IsNullOrEmpty(data.spriteName))
        {
            Debug.Log($"data.spriteName {data.spriteName}");
            LoadSprite(data.spriteName);
            PreloadSprites(data.spriteName);
        }

        // 기본 상태: Idle
        SetAnimState(ANIM_STATE_IDLE);
    }

    protected async void LoadSprite(string spriteName)
    {

        Sprite sprite = await SpriteManager.GetSprite(SpriteManager.AtlasBase + "Atlas_Monster.spriteatlasv2", spriteName);
        if (sprite != null && _renderer != null)
        {
            _renderer.sprite = sprite;
            _renderer.enabled = true;
        }
        else
        {
            Debug.LogError($"[MonsterBase] 스프라이트를 로드할 수 없습니다: {spriteName}");
        }
    }

    /// <summary>
    /// 몬스터 초기화 시 전체 스프라이트 16개(0~15)를 미리 로드
    /// spriteName 형식: "XXX_13" → baseName: "XXX"
    /// </summary>
    protected async void PreloadSprites(string spriteName)
    {
        // RewardBox 판별: SpawnManager의 rewardBoxId와 일치하거나 언더스코어가 없는 스프라이트
        string rewardBoxId = CombatManager.Instance?.SpawnManager?.RewardBoxId;
        bool isRewardBox = (_data != null && _data.id == rewardBoxId)
                        || spriteName.LastIndexOf('_') < 0;

        if (isRewardBox)
        {
            // 단일 스프라이트를 전체 프레임에 적용
            _sprites = new Sprite[TOTAL_FRAME_COUNT];
            string atlasPath = SpriteManager.AtlasBase + "Atlas_Monster.spriteatlasv2";
            Sprite single = await SpriteManager.GetSprite(atlasPath, spriteName);
            if (single != null)
            {
                for (int i = 0; i < TOTAL_FRAME_COUNT; i++)
                    _sprites[i] = single;
            }
            return;
        }

        // "001_13" → "001" 추출
        int underscoreIndex = spriteName.LastIndexOf('_');
        _spriteBaseName = spriteName.Substring(0, underscoreIndex);

        _sprites = new Sprite[TOTAL_FRAME_COUNT];
        string atlas = SpriteManager.AtlasBase + "Atlas_Monster.spriteatlasv2";

        for (int i = 0; i < TOTAL_FRAME_COUNT; i++)
        {
            string frameName = $"{_spriteBaseName}_{i}";
            _sprites[i] = await SpriteManager.GetSprite(atlas, frameName);
        }

    }

    // ──────────────────────────────────────
    // Animator 프레임 이벤트에서 호출되는 메서드들
    // 각 메서드는 해당 애니메이션 내의 상대적 프레임 인덱스를 받음
    // ──────────────────────────────────────

    /// <summary> 공격 애니메이션 (프레임 0~4) </summary>
    public void Animation_Attack_SpriteSetting_Number(int frameIndex)
    {
        SetSpriteByAbsoluteIndex(ATTACK_START + frameIndex);
    }

    /// <summary> 이동 애니메이션 (프레임 5~11) </summary>
    public void Animation_Move_SpriteSetting_Number(int frameIndex)
    {
        SetSpriteByAbsoluteIndex(MOVE_START + frameIndex);
    }

    /// <summary> 피격 애니메이션 (프레임 12) </summary>
    public void Animation_Hit_SpriteSetting_Number(int frameIndex)
    {
        SetSpriteByAbsoluteIndex(HIT_START + frameIndex);
    }

    /// <summary> 대기 애니메이션 (프레임 13~15) </summary>
    public void Animation_Idle_SpriteSetting_Number(int frameIndex)
    {
        SetSpriteByAbsoluteIndex(IDLE_START + frameIndex);
    }

    /// <summary>
    /// 절대 인덱스로 스프라이트 교체 (내부 공통 메서드)
    /// </summary>
    private void SetSpriteByAbsoluteIndex(int absoluteIndex)
    {
        if (_sprites == null || absoluteIndex < 0 || absoluteIndex >= _sprites.Length) return;

        Sprite sprite = _sprites[absoluteIndex];
        if (sprite != null && _renderer != null)
        {
            _renderer.sprite = sprite;
        }
    }

    /// <summary>
    /// Animator의 state 파라미터를 변경하는 헬퍼 메서드
    /// </summary>
    protected void SetAnimState(int state)
    {
        if (_animator != null)
            _animator.SetInteger(StateParam, state);
    }

    protected Vector3 _targetPosition;
    protected bool _hasTargetPosition = false;

    public void SetTargetPosition(Vector3 position)
    {
        _targetPosition = position;
        _hasTargetPosition = true;
    }

    protected virtual void Update()
    {
        if (_isDead) return;
        Move();
    }

    protected virtual void Move()
    {
        if (!_hasTargetPosition) return;

        // Move towards target position
        if (Vector3.Distance(transform.position, _targetPosition) > 0.1f)
        {
            // 이동 중 → Move 상태
            SetAnimState(ANIM_STATE_MOVE);

            float speed = 2f; // Default speed if not in data
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
        }
        else
        {
            // 도착 → Idle 상태
            SetAnimState(ANIM_STATE_IDLE);
        }
    }

    public virtual void TakeDamage(double damage)
    {
        if (_isDead) return;

        _currentHp -= damage;
        _currentHp = System.Math.Max(0, _currentHp);

        // Hit 트리거 발동
        if (_animator != null)
            _animator.SetTrigger(HitParam);

        // 데미지 넘버 표시
        var prefab = CombatManager.Instance?.DamageNumberPrefab;
        if (prefab != null)
        {
            prefab.Spawn(transform.position, damage.ToString("N0"));
        }

        if (IsBoss)
        {
            CombatManager.Instance?.UpdateBossHpRatio((float)HPRatio);
        }

        if (_currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        _isDead = true;

        // 드롭 아이템 생성
        SpawnDropRewards();

        CombatManager.Instance.OnEnemyKilled(IsBoss, IsRewardBox);

        if (CombatManager.Instance.SpawnManager != null)
        {
            CombatManager.Instance.SpawnManager.UnregisterEnemy(this);
        }

        var renderer = _renderer != null ? _renderer : GetComponentInChildren<SpriteRenderer>();
        renderer.enabled = false;

        PoolManager.Instance.ReturnPool(this);
    }

    /// <summary>
    /// 몬스터 사망 시 보상 드롭 아이템 생성
    /// </summary>
    protected virtual void SpawnDropRewards()
    {
        if (DropManager.Instance == null) return;

        // StageData에서 드롭량 가져오기
        var stageData = CombatManager.Instance?.StageManager?.CurrentStageData;
        if (stageData == null) return;

        // 골드 랜덤 범위 계산
        long goldAmount = Random.Range((int)stageData.goldDrop, (int)stageData.goldDrop + 1);
        int expAmount = stageData.expDrop;

        // 드롭 아이템 생성
        DropManager.Instance.SpawnDrops(transform.position, goldAmount, expAmount);
    }

    public abstract bool IsBoss { get; }
    public virtual bool IsRewardBox => _data != null && _data.type == MonsterType.RewardBox;

    // UI/디버깅용 공개 프로퍼티
    public double CurrentHP => _currentHp;
    public double MaxHP => _maxHp;
    public double HPRatio => _maxHp > 0 ? _currentHp / _maxHp : 0;
    public MonsterData Data => _data;
}
