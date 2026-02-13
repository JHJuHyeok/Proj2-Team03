﻿using UnityEngine;
using SlayerLegend.Resource;
using SlayerLegend.Skill.StatusEffects;

namespace SlayerLegend.Skill
{
    // 액티브 스킬: 쿨타임마다 자동 발동
    // - Update()에서 쿨타임 감소 및 자동 발동
    // - 마나 소모, 발사체 생성, 데미지 계산
    public class ActiveSkill : SkillBase
    {
        [Header("액티브 스킬 상태")]
        [SerializeField] private float currentCooldown = 0f;
        [SerializeField] private bool isActive = false;

        [Header("발사체 설정")]
        [SerializeField] private SkillProjectile2D projectilePrefab;
        [SerializeField] private Vector3 fireDirection = Vector3.right;  // 발사 방향
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;   // 발사 위치 오프셋
        [SerializeField] private Vector2 randomXRange = Vector2.zero;  // 조민희 추가: X좌표 랜덤 범위 (min, max)

        [Header("폭발 설정")]
        [SerializeField] private GameObject explosionEffectPrefab;  // 폭발 이펙트 (인스펙터에서 설정 가능)

        [Header("테스트 설정")]
        [SerializeField] private float cooldownMultiplier = 1f;
        [SerializeField] private bool overrideCooldown = false;
        [SerializeField] private float testCooldown = 1f;
        [SerializeField] private bool testNoManaCost = false;  // 조민희 추가: 테스트용 무한 마나

        private GameObject cachedCaster;

        public bool IsOnCooldown => currentCooldown > 0f;

        public float CooldownNormalized
        {
            get
            {
                float maxCooldown = overrideCooldown ? testCooldown : SkillCalculator.GetCooldown(skillData, currentLevel);
                return maxCooldown > 0 ? currentCooldown / maxCooldown : 0f;
            }
        }

        public float CurrentCooldown => currentCooldown;

        // 발사 방향 설정 (초기화 시 사용)
        public void SetFireDirection(Vector3 direction)
        {
            fireDirection = direction;
        }

        public void SetSpawnOffset(Vector3 offset)
        {
            spawnOffset = offset;
        }

        // X좌표 랜덤 범위 설정 (조민희 추가: 메테오 등에서 사용)
        public void SetRandomXRange(float min, float max)
        {
            randomXRange = new Vector2(min, max);
        }

        // 테스트용 쿨타임 설정 (초기화 시 사용)
        // 조민희 추가: 테스트를 위해 쿨타임 오버라이드 설정
        public void SetTestCooldown(float cooldown)
        {
            overrideCooldown = true;
            testCooldown = cooldown;
        }

        // 테스트용 무한 마나 설정 (조민희 추가)
        public void SetTestNoManaCost(bool noManaCost)
        {
            testNoManaCost = noManaCost;
        }

        // 발사체 프리팹 설정 (초기화 시 사용)
        public void SetProjectilePrefab(SkillProjectile2D prefab)
        {
            projectilePrefab = prefab;
        }

        // 폭발 이펙트 프리팹 설정 (폭발 스킬용)
        public void SetExplosionEffectPrefab(GameObject prefab)
        {
            explosionEffectPrefab = prefab;
        }

        // 스킬 활성화/비활성화
        public void SetActive(bool active)
        {
            isActive = active;
            if (active) CacheCaster();
        }

        // 캐스터(플레이어) 찾기
        private void CacheCaster()
        {
            Transform current = transform.parent;
            while (current != null)
            {
                if (current.GetComponent<IStatsProvider>() != null)
                {
                    cachedCaster = current.gameObject;
                    return;
                }
                current = current.parent;
            }
            cachedCaster = gameObject;
        }

        // 조민희 추가: Update()에서 쿨타임 처리 및 스킬 자동 발동
        private void Update()
        {
            if (!isActive) return;

            // 쿨타임 감소
            if (currentCooldown > 0f)
            {
                currentCooldown -= Time.deltaTime;
                return;
            }

            // 쿨타임 완료 시 스킬 발동
            if (skillData != null)
            {
                // 폭발 스킬 여부 확인 (effectData 사용)
                if (skillData.effectData != null && skillData.effectData.isBlastSkill)
                {
                    ExecuteBlastSkill();
                }
                else
                {
                    TryAutoCast();
                }
            }
        }

        // 자동 발동 시도
        private void TryAutoCast()
        {
            if (cachedCaster == null) CacheCaster();
            if (cachedCaster == null) return;

            var stats = cachedCaster.GetComponent<IStatsProvider>();
            int manaCost = SkillCalculator.GetManaCost(skillData);

            // 조민희 추가: 테스트용 무한 마나 모드
            if (!testNoManaCost)
            {
                if (stats != null && manaCost > 0)
                {
                    int currentMana = (int)stats.CurrentMana;  // 수정: CurrentMana 속성 사용
                    if (!stats.UseMana(manaCost))
                    {
                        Debug.Log($"[Skill] {skillData.name} 발동 실패: 마나 부족 (필요: {manaCost}, 현재: {currentMana})");
                        return; // 마나 부족하면 발동 안 함
                    }
                }
            }

            Debug.Log($"[Skill] {skillData.name} 발동 시도...");
            ExecuteSkill(cachedCaster);

            // 쿨타임 설정 (테스트용 오버라이드 지원)
            if (overrideCooldown)
            {
                currentCooldown = testCooldown * cooldownMultiplier;
            }
            else
            {
                currentCooldown = SkillCalculator.GetCooldown(skillData, currentLevel) * cooldownMultiplier;
            }
        }

        // 조민희 추가: 폭발 스킬 전용 실행 메서드
        protected virtual void ExecuteBlastSkill()
        {
            // 마나 소모 체크
            if (cachedCaster == null) CacheCaster();
            if (cachedCaster == null) return;

            var stats = cachedCaster.GetComponent<IStatsProvider>();
            int manaCost = SkillCalculator.GetManaCost(skillData);

            // 테스트용 무한 마나 모드
            if (!testNoManaCost)
            {
                if (stats != null && manaCost > 0)
                {
                    int currentMana = (int)stats.CurrentMana;
                    if (!stats.UseMana(manaCost))
                    {
                        Debug.Log($"[Skill] {skillData.name} 발동 실패: 마나 부족 (필요: {manaCost}, 현재: {currentMana})");
                        return;
                    }
                }
            }

            Debug.Log($"[Skill] {skillData.name} 폭발 스킬 발동!");

            // 폭발 파라미터 가져오기 (effectData 사용)
            float explosionRadius = skillData.effectData?.explosionRadius ?? 3f;
            float explodeDelay = skillData.effectData?.explodeDelay ?? 0.5f;
            // 폭발 이펙트: 인스펙터 설정 우선, 없으면 effectData 사용
            GameObject explosionEffect = explosionEffectPrefab ?? skillData.effectData?.explosionEffect;

            // 화상 DoT 파라미터 가져오기 (effectData 사용)
            float dotDuration = skillData.effectData?.dotDuration ?? 3f;
            double damagePerTick = skillData.effectData?.dotDamagePerTick ?? 25.0;
            float tickInterval = skillData.effectData?.dotTickInterval ?? 1f;
            string dotType = skillData.effectData?.dotType ?? "burn";

            // 폭발 실행
            ExecuteExplosion(explosionRadius, explodeDelay, explosionEffect,
                dotDuration, damagePerTick, tickInterval, dotType);

            // 쿨타임 설정
            if (overrideCooldown)
            {
                currentCooldown = testCooldown * cooldownMultiplier;
            }
            else
            {
                currentCooldown = SkillCalculator.GetCooldown(skillData, currentLevel) * cooldownMultiplier;
            }
        }

        // 스킬 효과 실행 (하위 클래스에서 오버라이드 가능)
        protected virtual void ExecuteSkill(GameObject caster)
        {
            // 적은 항상 오른쪽에서 왼쪽으로 일직선 이동하므로,
            // 굳이 적을 찾지 않고 오른쪽으로 발사

            // 데미지 계산
            var stats = caster.GetComponent<IStatsProvider>();
            double skillDamage = SkillCalculator.GetDamage(skillData, currentLevel);
            double totalDamage = skillDamage;
            bool isCritical = false;

            if (stats != null)
            {
                totalDamage += stats.AttackDamage;
                isCritical = stats.IsCriticalHit();
                totalDamage = stats.CalculateFinalDamage(isCritical);
                string critText = isCritical ? " [치명타!]" : "";
                Debug.Log($"{caster.name}이(가) {skillData.name} 발사!{critText} → 예상 데미지: {totalDamage:F1}");
            }

            // 발사체 생성 (지정된 방향으로 발사)
            if (projectilePrefab != null)
            {
                Vector3 offset = spawnOffset;

                // 조민희 추가: X좌표 랜덤 범위가 설정되어 있으면 적용
                if (randomXRange.x != 0 || randomXRange.y != 0)
                {
                    float randomX = Random.Range(randomXRange.x, randomXRange.y);
                    offset.x = randomX;
                }

                Vector3 spawnPosition = caster.transform.position + offset;
                SkillProjectile2D.Create(projectilePrefab, spawnPosition, totalDamage, isCritical, fireDirection);
            }
            else
            {
                Debug.LogWarning($"[Skill] {skillData.name}에 발사체 프리팹이 없습니다.");
            }
        }

        // 도트 데미지 상태이상 적용
        private void ApplyDotEffect(GameObject enemyObject)
        {
            if (!skillData.IsDotSkill()) return;

            var damageable = enemyObject.GetComponent<IDamageable>();
            if (damageable == null)
            {
                Debug.LogError("[Skill] Enemy has no IDamageable component for DoT effect");
                return;
            }

            // 체력 비례 데미지를 위해 IStatusEffectAble 확인
            var statusTarget = enemyObject.GetComponent<IStatusEffectAble>();
            float targetMaxHp = statusTarget?.MaxHealth ?? 0f;
            bool isPercentage = skillData.GetDotIsPercentage();

            // 체력 비례 데미지인 경우 레벨업 보정을 %에 적용
            double damageValue = skillData.GetDotDamagePerTick();
            if (isPercentage)
            {
                damageValue += skillData.GetLevelUpValue() * (currentLevel - 1) * 0.1; // 0.1%씩 증가
            }
            else
            {
                damageValue += skillData.GetLevelUpValue() * (currentLevel - 1) * 0.5; // 고정값 증가
            }

            // 적에게 직접 컴포넌트 추가 (불필요한 임시 생성 제거)
            var dotEffect = enemyObject.AddComponent<DotEffect>();
            dotEffect.Initialize(
                skillData.GetDotDuration(),
                damageValue,
                skillData.GetDotTickInterval(),
                damageable,
                transform.parent?.gameObject,
                isPercentage,
                targetMaxHp
            );

            Debug.Log($"[Skill] 도트 데미지 적용! {skillData.GetDotDuration()}초간 {skillData.GetDotTickInterval()}초마다 {(isPercentage ? $"최대HP의 {damageValue:F1}%" : $"{damageValue:F1}데미지")}");
        }

        // CC 상태이상 적용
        private void ApplyCCEffect(GameObject enemyObject)
        {
            // 기절 (Stun)
            if (skillData.IsStunSkill())
            {
                var stunTarget = enemyObject.GetComponent<IStunnable>();
                if (stunTarget != null)
                {
                    // 적에게 직접 컴포넌트 추가 (불필요한 임시 생성 제거)
                    var stunEffect = enemyObject.AddComponent<StunEffect>();
                    stunEffect.Initialize(skillData.GetStunDuration(), stunTarget);
                    Debug.Log($"[Skill] 기절 적용! {skillData.GetStunDuration()}초간 행동 불가");
                }
            }

            // 빙결 (Freeze)
            if (skillData.IsFreezeSkill())
            {
                var freezeTarget = enemyObject.GetComponent<IFreezable>();
                if (freezeTarget != null)
                {
                    // 적에게 직접 컴포넌트 추가
                    var freezeEffect = enemyObject.AddComponent<FreezeEffect>();
                    // FreezeStacks는 Initialize 내부에서 계산하도록 변경 (중복 관리 제거)
                    freezeEffect.Initialize(skillData.GetFreezeDuration(), freezeTarget);
                    Debug.Log($"[Skill] 빙결 적용! {skillData.GetFreezeDuration()}초간 이속 감소");
                }
            }

            // 속박 (Root)
            if (skillData.IsRootSkill())
            {
                var rootTarget = enemyObject.GetComponent<IRootable>();
                if (rootTarget != null)
                {
                    // 적에게 직접 컴포넌트 추가
                    var rootEffect = enemyObject.AddComponent<RootEffect>();
                    rootEffect.Initialize(skillData.GetRootDuration(), rootTarget);
                    Debug.Log($"[Skill] 속박 적용! {skillData.GetRootDuration()}초간 이동 불가");
                }
            }
        }

        // 스킬 이름과 함께 데미지를 입힘 (테스트용 로그 개선)
        private void DealDamageWithSkillName(GameObject enemyObject, double damage, string skillName)
        {
            // DummyEnemy 테스트 클래스인 경우 스킬 이름 포함 메서드 사용
            var dummyEnemy = enemyObject.GetComponent<Testing.DummyEnemy>();
            if (dummyEnemy != null)
            {
                dummyEnemy.TakeDamage(damage, skillName);
                return;
            }

            // DummyEnemy가 아닌 경우 기본 TakeDamage 사용
            var damageable = enemyObject.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
        }

        // 조민희 추가: 폭발 스킬은 발사체 없이 직접 폭발 로직
        private void ExecuteExplosion(float radius, float delay, GameObject effect,
            float dotDuration, double dotDamage, float dotTick, string dotType)
        {
            // 플레이어 위치에 폭발 오브젝트 생성
            Vector3 spawnPosition = cachedCaster.transform.position + spawnOffset;

            // X좌표 랜덤 범위 적용 (플레이어 기준 앞쪽)
            if (randomXRange.x != 0 || randomXRange.y != 0)
            {
                float randomX = Random.Range(randomXRange.x, randomXRange.y);
                spawnPosition.x = cachedCaster.transform.position.x + randomX;
            }

            // 데미지 계산 (ExecuteSkill과 동일한 로직)
            var stats = cachedCaster.GetComponent<IStatsProvider>();
            double skillDamage = SkillCalculator.GetDamage(skillData, currentLevel);
            double totalDamage = skillDamage;
            bool isCritical = false;

            if (stats != null)
            {
                totalDamage += stats.AttackDamage;
                isCritical = stats.IsCriticalHit();
                totalDamage = stats.CalculateFinalDamage(isCritical);
            }

            // 폭발 오브젝트
            GameObject explosionObj = new GameObject($"Explosion_{skillData.name}");
            explosionObj.transform.position = spawnPosition;

            // 폭발 로직 처리
            var explosionLogic = explosionObj.AddComponent<Explosion>();
            explosionLogic.Initialize(radius, delay, effect,
                totalDamage, isCritical, fireDirection,
                dotDuration, dotDamage, dotTick, dotType,
                cachedCaster.transform);

            string critText = isCritical ? " [치명타!]" : "";
            Debug.Log($"[Skill] {skillData.name} 폭발 생성! → 예상 데미지: {totalDamage:F1}{critText}");

            // 일정 시간 후 제거
            Destroy(explosionObj, 5f);
        }

        public void ResetCooldown() => currentCooldown = 0f;

        protected override void OnLevelUp()
        {
            base.OnLevelUp();
            Debug.Log($"{skillData.name} 액티브 스킬 레벨업! 쿨타임: {SkillCalculator.GetCooldown(skillData, currentLevel):F1}초");
        }
    }
}
