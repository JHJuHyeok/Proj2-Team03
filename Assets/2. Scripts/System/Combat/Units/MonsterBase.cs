using UnityEngine;
using Combat.Drop;
using SlayerLegend.Skill;

// [주혁] - 몬스터, 스테이지 데이터 리팩토링으로 인해 오류 코드 주석화(23, 24, 132)

[RequireComponent(typeof(SpriteRenderer))]
public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    protected MonsterData _data;
    protected Transform _target;
    protected bool _isDead = false;
    protected double _currentHp;
    protected double _maxHp;

    public virtual void Initialize(MonsterData data, Transform target)
    {
        _data = data;
        _target = target;
        _isDead = false;

        // HP 초기화
        //_maxHp = data.maxHp;
        //_currentHp = _maxHp;

        // Sprite Load
        if (!string.IsNullOrEmpty(data.spriteName))
        {
            LoadSprite(data.spriteName);
        }
    }

    private async void LoadSprite(string spriteName)
    {
        var renderer = GetComponent<SpriteRenderer>();

        Sprite sprite = await SpriteManager.GetSprite(SpriteManager.AtlasBase + "Atlas_Monster.spriteatlasv2", spriteName);
        if (sprite != null && renderer != null)
        {
            renderer.sprite = sprite;
            renderer.enabled = true;
        }
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
            // Simple movement logic
            float speed = 2f; // Default speed if not in data

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);
        }
    }

    public virtual void TakeDamage(double damage)
    {
        if (_isDead) return;

        _currentHp -= damage;
        _currentHp = System.Math.Max(0, _currentHp);

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

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = GetComponentInChildren<SpriteRenderer>();
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
