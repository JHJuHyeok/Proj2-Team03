using UnityEngine;
using SlayerLegend.Data;

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
                float maxCooldown = skillData.GetCooldown(currentLevel);
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
            if (stats != null && skillData.baseManaCost > 0)
            {
                if (!stats.UseMana(skillData.baseManaCost))
                    return; // 마나 부족하면 발동 안 함
            }

            ExecuteSkill(cachedCaster);
            currentCooldown = skillData.GetCooldown(currentLevel);
        }

        // 스킬 효과 실행 (하위 클래스에서 오버라이드 가능)
        protected virtual void ExecuteSkill(GameObject caster)
        {
            // 이펙트 생성
            if (skillData.effectPrefab != null)
            {
                GameObject effect = Instantiate(skillData.effectPrefab, caster.transform.position, Quaternion.identity);
                var particle = effect.GetComponent<ParticleSystem>();
                Destroy(effect, particle != null ? particle.main.duration : 3f);
            }

            // 데미지 계산
            var stats = caster.GetComponent<IStatsProvider>();
            float skillDamage = skillData.GetDamage(currentLevel);
            float totalDamage = skillDamage;

            if (stats != null)
            {
                totalDamage += stats.AttackDamage;
                bool isCritical = stats.IsCriticalHit();
                totalDamage = stats.CalculateFinalDamage(isCritical);
                string critText = isCritical ? " [치명타!]" : "";
                Debug.Log($"{caster.name}이(가) {skillData.skillName} 자동 발동!{critText} 데미지: {totalDamage:F1}");
            }
            else
            {
                Debug.Log($"{caster.name}이(가) {skillData.skillName} 자동 발동! 데미지: {totalDamage:F1}");
            }
        }

        public void ResetCooldown() => currentCooldown = 0f;

        protected override void OnLevelUp()
        {
            base.OnLevelUp();
            Debug.Log($"{skillData.skillName} 액티브 스킬 레벨업! 쿨타임: {skillData.GetCooldown(currentLevel):F1}초");
        }
    }
}
