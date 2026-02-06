﻿using UnityEngine;
using SlayerLegend.Resource;
using SlayerLegend.Skill.StatusEffects;

namespace SlayerLegend.Skill
{
    // 액티브 스킬: 쿨타임마다 자동 발동
    // - Update()에서 쿨타임 감소 및 자동 발동
    // - 마나 소모, 이펙트 생성, 데미지 계산
    public class ActiveSkill : SkillBase
    {
        [Header("액티브 스킬 상태")]
        [SerializeField] private float currentCooldown = 0f;
        [SerializeField] private bool isActive = false;

        private GameObject cachedCaster;

        public bool IsOnCooldown => currentCooldown > 0f;

        public float CooldownNormalized
        {
            get
            {
                float maxCooldown = SkillCalculator.GetCooldown(skillData, currentLevel);
                return maxCooldown > 0 ? currentCooldown / maxCooldown : 0f;
            }
        }

        public float CurrentCooldown => currentCooldown;

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

        // 쿨타임 감소 및 자동 발동
        private void Update()
        {
            if (!isActive) return;

            if (currentCooldown > 0f)
            {
                currentCooldown -= Time.deltaTime;
                if (currentCooldown < 0f) currentCooldown = 0f;
            }
            else
            {
                TryAutoCast();
            }
        }

        // 자동 발동 시도
        private void TryAutoCast()
        {
            if (cachedCaster == null) CacheCaster();
            if (cachedCaster == null) return;

            var stats = cachedCaster.GetComponent<IStatsProvider>();
            int manaCost = SkillCalculator.GetManaCost(skillData);
            if (stats != null && manaCost > 0)
            {
                if (!stats.UseMana(manaCost))
                    return; // 마나 부족하면 발동 안 함
            }

            ExecuteSkill(cachedCaster);
            currentCooldown = SkillCalculator.GetCooldown(skillData, currentLevel);
        }

        // 스킬 효과 실행 (하위 클래스에서 오버라이드 가능)
        protected virtual void ExecuteSkill(GameObject caster)
        {
            // 이펙트 생성 (JSON 데이터에는 effectPrefab이 없으므로 일단 생략)
            // TODO: 팀원과 협의하여 effectPrefabPath 추가 시 ResourceManager 사용

            // 적 찾기
            var enemyObject = GameObject.FindWithTag("Enemy");
            if (enemyObject == null)
            {
                Debug.Log($"[Skill] 적을 찾을 수 없습니다 (태그 'Enemy' 필요)");
                return;
            }

            var enemy = enemyObject.GetComponent<IDamageable>();
            if (enemy == null)
            {
                Debug.Log($"[Skill] 적에 IDamageable 컴포넌트가 없습니다");
                return;
            }

            // 데미지 계산
            var stats = caster.GetComponent<IStatsProvider>();
            float skillDamage = SkillCalculator.GetDamage(skillData, currentLevel);
            float totalDamage = skillDamage;

            if (stats != null)
            {
                totalDamage += stats.AttackDamage;
                bool isCritical = stats.IsCriticalHit();
                totalDamage = stats.CalculateFinalDamage(isCritical);
                string critText = isCritical ? " [치명타!]" : "";

                // 적에게 데미지 입히기 (스킬 이름 포함)
                DealDamageWithSkillName(enemyObject, totalDamage, skillData.name);
                Debug.Log($"{caster.name}이(가) {skillData.name} 발동!{critText} → 적에게 {totalDamage:F1} 데미지");
            }
            else
            {
                DealDamageWithSkillName(enemyObject, totalDamage, skillData.name);
                Debug.Log($"{caster.name}이(가) {skillData.name} 발동! → 적에게 {totalDamage:F1} 데미지");
            }

            // 도트 데미지 적용
            ApplyDotEffect(enemyObject);

            // CC 상태이상 적용
            ApplyCCEffect(enemyObject);
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
            float damageValue = skillData.GetDotDamagePerTick();
            if (isPercentage)
            {
                damageValue += skillData.GetLevelUpValue() * (currentLevel - 1) * 0.1f; // 0.1%씩 증가
            }
            else
            {
                damageValue += skillData.GetLevelUpValue() * (currentLevel - 1) * 0.5f; // 고정값 증가
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
        private void DealDamageWithSkillName(GameObject enemyObject, float damage, string skillName)
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

        public void ResetCooldown() => currentCooldown = 0f;

        protected override void OnLevelUp()
        {
            base.OnLevelUp();
            Debug.Log($"{skillData.name} 액티브 스킬 레벨업! 쿨타임: {SkillCalculator.GetCooldown(skillData, currentLevel):F1}초");
        }
    }
}
