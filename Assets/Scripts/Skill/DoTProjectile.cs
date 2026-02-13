using UnityEngine;
using SlayerLegend.Skill.StatusEffects;

namespace SlayerLegend.Skill
{
    // 조민희 추가: DoT 발사체 (화상, 독 등 지속 데미지 스킬)
    // - 적과 충돌 시 즉시 데미지 + DoT 상태이상 적용
    // - 같은 종류의 DoT는 스택 중첩 (최대 5스택)
    public class DoTProjectile : SkillProjectile2D
    {
        [Header("DoT 설정")]
        [SerializeField] private float dotDuration = 5f;      // DoT 지속 시간
        [SerializeField] private double damagePerTick = 5.0;    // 틱당 데미지
        [SerializeField] private float tickInterval = 1f;     // 틱 간격
        [SerializeField] private bool isPercentage = false;  // 체력 비례 여부
        [SerializeField] private string dotType = "default"; // DoT 종류 (stackKey)

        protected override void OnHitEnemy(MonsterBase enemy)
        {
            // 1. 즉시 데미지 적용
            enemy.TakeDamage(_damage);

            string critText = _isCritical ? " [치명타]" : "";
            Debug.Log($"[DoTProjectile] {enemy.gameObject.name} 타격: {_damage:F1} 데미지{critText}");

            // 2. 기존에 같은 종류의 DoT가 있는지 확인
            DotEffect existingDot = enemy.GetComponent<DotEffect>();

            if (existingDot != null)
            {
                // 이미 있으면 스택 추가 (시간 초기화)
                Debug.Log($"[DoTProjectile] {enemy.name}에 이미 {dotType} DoT가 있어서 스택 추가");
                existingDot.AddStack(dotDuration, damagePerTick, transform.parent?.gameObject);
            }
            else
            {
                // 없으면 새로 DoT 추가
                var damageable = enemy.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    var dotEffect = enemy.gameObject.AddComponent<DotEffect>();

                    // 체력 비례인 경우 최대 HP 전달
                    float maxHp = 0f;
                    if (isPercentage)
                    {
                        var statusTarget = enemy.GetComponent<IStatusEffectAble>();
                        maxHp = statusTarget?.MaxHealth ?? 0f;
                    }

                    dotEffect.Initialize(
                        dotDuration,
                        damagePerTick,
                        tickInterval,
                        damageable,
                        transform.parent?.gameObject,
                        isPercentage,
                        maxHp,
                        dotType  // stackKey로 사용
                    );

                    string type = isPercentage ? "% 체력비례" : "고정";
                    Debug.Log($"[DoTProjectile] {enemy.name}에 {dotType} DoT 적용! {dotDuration}초간 {damagePerTick}{type}/{tickInterval}s마다");
                }
                else
                {
                    Debug.LogWarning($"[DoTProjectile] {enemy.name}에 IDamageable 컴포넌트가 없어서 DoT 적용 실패");
                }
            }

            // 3. 타격 이펙트 & 파괴
            if (hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            DestroyProjectile();
        }
    }
}
