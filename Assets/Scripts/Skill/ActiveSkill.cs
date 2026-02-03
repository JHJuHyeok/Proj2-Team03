﻿using UnityEngine;
using SlayerLegend.Resource;
// using SlayerLegend.Skill.StatusEffects; // TODO: StatusEffects 시스템 완료 후 해제

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

                // 적에게 데미지 입히기
                enemy.TakeDamage(totalDamage);
                Debug.Log($"{caster.name}이(가) {skillData.name} 발동!{critText} → 적에게 {totalDamage:F1} 데미지");
            }
            else
            {
                enemy.TakeDamage(totalDamage);
                Debug.Log($"{caster.name}이(가) {skillData.name} 발동! → 적에게 {totalDamage:F1} 데미지");
            }

            // 도트 데미지 적용 (TODO: StatusEffects 시스템 완료 후 해제)
            // ApplyDotEffect(enemyObject);
        }

        /// <summary>
        /// 도트 데미지 상태이상 적용
        /// TODO: StatusEffects 시스템 완료 후 해제
        /// </summary>
        /*
        private void ApplyDotEffect(GameObject enemyObject)
        {
            if (!skillData.IsDotSkill()) return;

            var statusEffectAble = enemyObject.GetComponent<IStatusEffectAble>();
            if (statusEffectAble == null)
            {
                Debug.Log("[Skill] 적이 상태이상을 받을 수 없습니다 (IStatusEffectAble 필요)");
                return;
            }

            // IDamageable 캐싱 및 null 체크
            var damageable = enemyObject.GetComponent<IDamageable>();
            if (damageable == null)
            {
                Debug.LogError("[Skill] Enemy has no IDamageable component for DoT effect");
                return;
            }

            // 도트 데미지 효과 생성 (임시 컴포넌트 사용)
            var dotEffect = gameObject.AddComponent<DotEffect>();
            dotEffect.Initialize(
                skillData.GetDotDuration(),
                skillData.GetDotDamagePerTick() + (skillData.levelUpValue * (currentLevel - 1) * 0.5f), // 레벨에 비례 증가
                skillData.GetDotTickInterval(),
                damageable,
                transform.parent?.gameObject
            );

            // 상태이상 적용
            statusEffectAble.ApplyStatusEffect(dotEffect);

            // 효과 컴포넌트는 ApplyStatusEffect 내에서 다시 생성되므로 제거
            Destroy(dotEffect);

            Debug.Log($"[Skill] 도트 데미지 적용! {skillData.GetDotDuration()}초간 {skillData.GetDotTickInterval()}초마다 데미지");
        }
        */

        public void ResetCooldown() => currentCooldown = 0f;

        protected override void OnLevelUp()
        {
            base.OnLevelUp();
            Debug.Log($"{skillData.name} 액티브 스킬 레벨업! 쿨타임: {SkillCalculator.GetCooldown(skillData, currentLevel):F1}초");
        }
    }
}
