using UnityEngine;
using SlayerLegend.Skill.StatusEffects;
using System.Collections;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    //폭발 스킬용 컴포넌트
    // - 생성된 위치에서 일정 시간 대기 후 폭발
    // - 범위 내 모든 적에게 즉시 데미지 + DoT 적용
    // - 2026-02-24: 타겟 수 제한, 다회 타격 기능 추가
    public class Explosion : MonoBehaviour
    {
        private float _radius;
        private float _delay;
        private GameObject _effectPrefab;
        private double _damage;
        private bool _isCritical;
        private Vector3 _direction;
        private float _dotDuration;
        private double _dotDamage;
        private float _dotTick;
        private string _dotType;
        private Transform _caster;

        //타겟 수 제한 및 다회 타격
        private int _maxTargets = -1;           // -1 = 무제한
        private int _hitCount = 1;              // 타격 횟수
        private float _hitInterval = 0.2f;      // 타격 간격
        private bool _isRandomHit = false;      // 랜덤 타격 모드 (뇌격류)

        //CC 효과 (Phase 4)
        private bool _isStun = false;
        private float _stunDuration = 1f;
        private float _stunChance = 100f;
        private bool _isFreeze = false;
        private float _freezeDuration = 2f;
        private float _freezeChance = 100f;

        //마지막 타격 강화 (Phase 4)
        private float _lastHitMultiplier = 1f;
        private int _currentHitIndex = 0;

        private float _spawnTime;
        private bool _hasExploded;

        /// <summary>
        /// 폭발 초기화 (기본)
        /// </summary>
        public void Initialize(float radius, float delay, GameObject effectPrefab,
            double damage, bool isCritical, Vector3 direction,
            float dotDuration, double dotDamage, float dotTick, string dotType,
            Transform caster)
        {
            Initialize(radius, delay, effectPrefab, damage, isCritical, direction,
                dotDuration, dotDamage, dotTick, dotType, caster, -1, 1, 0.2f, false,
                false, 1f, 100f, false, 2f, 100f, 1f);
        }

        /// <summary>
        /// 폭발 초기화 (타겟 제한 및 다회 타격 지원)
        /// </summary>
        public void Initialize(float radius, float delay, GameObject effectPrefab,
            double damage, bool isCritical, Vector3 direction,
            float dotDuration, double dotDamage, float dotTick, string dotType,
            Transform caster, int maxTargets, int hitCount, float hitInterval, bool isRandomHit = false)
        {
            Initialize(radius, delay, effectPrefab, damage, isCritical, direction,
                dotDuration, dotDamage, dotTick, dotType, caster, maxTargets, hitCount, hitInterval, isRandomHit,
                false, 1f, 100f, false, 2f, 100f, 1f);
        }

        /// <summary>
        /// 폭발 초기화 (CC 효과 포함 - Phase 4)
        /// </summary>
        public void Initialize(float radius, float delay, GameObject effectPrefab,
            double damage, bool isCritical, Vector3 direction,
            float dotDuration, double dotDamage, float dotTick, string dotType,
            Transform caster, int maxTargets, int hitCount, float hitInterval, bool isRandomHit,
            bool isStun, float stunDuration, float stunChance,
            bool isFreeze, float freezeDuration, float freezeChance,
            float lastHitMultiplier = 1f)
        {
            _radius = radius;
            _delay = delay;
            _effectPrefab = effectPrefab;
            _damage = damage;
            _isCritical = isCritical;
            _direction = direction;
            _dotDuration = dotDuration;
            _dotDamage = dotDamage;
            _dotTick = dotTick;
            _dotType = dotType;
            _caster = caster;
            _maxTargets = maxTargets;
            _hitCount = hitCount;
            _hitInterval = hitInterval;
            _isRandomHit = isRandomHit;
            _isStun = isStun;
            _stunDuration = stunDuration;
            _stunChance = stunChance;
            _isFreeze = isFreeze;
            _freezeDuration = freezeDuration;
            _freezeChance = freezeChance;
            _lastHitMultiplier = lastHitMultiplier;
            _currentHitIndex = 0;

            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (_hasExploded) return;

            // 대기 시간 경과 후 폭발
            if (Time.time >= _spawnTime + _delay)
            {
                Explode();
            }
        }

        private void Explode()
        {
            _hasExploded = true;

            //중앙 이펙트 제거 - 대신 HitEnemy()에서 각 적 위치에 이펙트 생성
            // 이펙트는 이제 각 적에게 데미지를 줄 때 해당 위치에 생성됨

            // 범위 내 모든 적 탐색
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _radius);

            // 타겟 수 제한 적용
            int targetCount = 0;
            List<MonsterBase> targets = new List<MonsterBase>();

            foreach (var collider in colliders)
            {
                MonsterBase enemy = collider.GetComponent<MonsterBase>();
                if (enemy != null && enemy.gameObject.activeSelf)
                {
                    targets.Add(enemy);
                    targetCount++;

                    // 최대 타겟 수 도달 시 중단
                    if (_maxTargets > 0 && targetCount >= _maxTargets)
                        break;
                }
            }

            // 랜덤 타격 모드 (뇌격류)
            if (_isRandomHit)
            {
                StartCoroutine(RandomHitCoroutine(targets));
                return;
            }

            // 다회 타격 여부에 따라 처리
            if (_hitCount > 1)
            {
                // 다회 타격: 코루틴으로 처리
                StartCoroutine(MultiHitCoroutine(targets));
            }
            else
            {
                // 1회 타격: 즉시 처리
                foreach (var enemy in targets)
                {
                    HitEnemy(enemy);
                }
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 랜덤 타격 코루틴 (뇌격류)
        /// 매 타격마다 범위 내 랜덤한 적에게 타격
        /// </summary>
        private IEnumerator RandomHitCoroutine(List<MonsterBase> allTargets)
        {
            // null 체크
            if (allTargets == null || allTargets.Count == 0)
            {
                Destroy(gameObject);
                yield break;
            }

            for (int i = 0; i < _hitCount; i++)
            {
                // 범위 내 모든 적 다시 탐색 (새로운 적이 추가되었을 수 있음)
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _radius);
                List<MonsterBase> currentTargets = new List<MonsterBase>();

                foreach (var collider in colliders)
                {
                    MonsterBase enemy = collider.GetComponent<MonsterBase>();
                    if (enemy != null && enemy.gameObject.activeSelf)
                    {
                        currentTargets.Add(enemy);
                    }
                }

                // 타겟이 있으면 랜덤하게 하나 선택해서 타격
                if (currentTargets.Count > 0)
                {
                    int randomIndex = Random.Range(0, currentTargets.Count);
                    MonsterBase randomTarget = currentTargets[randomIndex];
                    HitEnemy(randomTarget);

                    // 랜덤 타격 이펙트 생성 (선택적)
                    if (_effectPrefab != null)
                    {
                        GameObject miniEffect = Instantiate(_effectPrefab, randomTarget.transform.position, Quaternion.identity);
                        Destroy(miniEffect, 1f);  // 짧은 지속 시간
                    }
                }

                // 마지막 타격이 아니면 대기
                if (i < _hitCount - 1)
                {
                    yield return new WaitForSeconds(_hitInterval);
                }
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 다회 타격 코루틴
        /// </summary>
        private IEnumerator MultiHitCoroutine(List<MonsterBase> targets)
        {
            // null 체크
            if (targets == null || targets.Count == 0)
            {
                Destroy(gameObject);
                yield break;
            }

            for (int i = 0; i < _hitCount; i++)
            {
                _currentHitIndex = i;
                bool isLastHit = (i == _hitCount - 1);

                foreach (var enemy in targets)
                {
                    if (enemy != null && enemy.gameObject.activeSelf)
                    {
                        HitEnemy(enemy, isLastHit);
                    }
                }

                // 마지막 타격이 아니면 대기
                if (i < _hitCount - 1)
                {
                    yield return new WaitForSeconds(_hitInterval);
                }
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// 단일 적 타격
        /// </summary>
        /// <param name="enemy">타겟 적</param>
        /// <param name="isLastHit">마지막 타격 여부</param>
        private void HitEnemy(MonsterBase enemy, bool isLastHit = false)
        {
            // 데미지 계산 (마지막 타격 시 배율 적용)
            double finalDamage = _damage;
            if (isLastHit && _lastHitMultiplier > 1f)
            {
                finalDamage *= _lastHitMultiplier;
            }

            //각 적 위치에 이펙트 생성
            if (_effectPrefab != null && enemy != null)
            {
                GameObject hitEffect = Instantiate(_effectPrefab, enemy.transform.position, Quaternion.identity);
                Destroy(hitEffect, 2f);  // 2초 후 제거
            }

            // 즉시 데미지
            enemy.TakeDamage(finalDamage);

            // DoT 적용
            ApplyDot(enemy);

            // CC 적용 (Phase 4)
            ApplyCC(enemy);
        }

        /// <summary>
        /// CC 효과 적용 (Phase 4)
        /// </summary>
        private void ApplyCC(MonsterBase enemy)
        {
            // 스턴 적용
            if (_isStun)
            {
                float roll = Random.Range(0f, 100f);
                if (roll <= _stunChance)
                {
                    var stunnable = enemy.GetComponent<IStunnable>();
                    if (stunnable != null)
                    {
                        // 기존 스턴 효과 확인
                        var existingStun = enemy.GetComponent<StunEffect>();
                        if (existingStun != null)
                        {
                            // 이미 스턴 중이면 지속 시간 갱신
                            existingStun.EndEffect();
                            Destroy(existingStun);
                        }

                        var stunEffect = enemy.gameObject.AddComponent<StunEffect>();
                        stunEffect.Initialize(_stunDuration, stunnable);
                    }
                }
            }

            // 빙결 적용
            if (_isFreeze)
            {
                float roll = Random.Range(0f, 100f);
                if (roll <= _freezeChance)
                {
                    var freezable = enemy.GetComponent<IFreezable>();
                    if (freezable != null)
                    {
                        // 기존 빙결 효과 확인
                        var existingFreeze = enemy.GetComponent<FreezeEffect>();
                        if (existingFreeze != null)
                        {
                            // 이미 빙결 중이면 중첩 (새 효과로 교체)
                            existingFreeze.EndEffect();
                            Destroy(existingFreeze);
                        }

                        var freezeEffect = enemy.gameObject.AddComponent<FreezeEffect>();
                        freezeEffect.Initialize(_freezeDuration, freezable);
                    }
                }
            }
        }

        private void ApplyDot(MonsterBase enemy)
        {
            // 기존 DoT 확인
            DotEffect existingDot = enemy.GetComponent<DotEffect>();

            if (existingDot != null)
            {
                // 있으면 스택 추가
                existingDot.AddStack(_dotDuration, _dotDamage, _caster?.gameObject);
            }
            else
            {
                // 없으면 새로 추가
                var damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    var dotEffect = enemy.gameObject.AddComponent<DotEffect>();

                    dotEffect.Initialize(
                        _dotDuration,
                        _dotDamage,
                        _dotTick,
                        damageable,
                        _caster?.gameObject,
                        false,  // 화상은 체력 비례 아님
                        0f,
                        _dotType
                    );
                }
            }
        }

        // 폭발 범위 가시화 (디버그용)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
