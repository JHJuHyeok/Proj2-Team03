using UnityEngine;
using System;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    // 패시브 스킬: 항상 활성화된 버프
    // - Activate()로 버프 적용, Deactivate()로 제거
    // - 레벨에 따른 버프 효과 증가
    public class PassiveSkill : SkillBase
    {
        // 버프 타입별 처리 로직 매핑
        private static readonly Dictionary<PassiveBuffType, Action<IStatsProvider, object, float>> ApplyActions = new()
        {
            { PassiveBuffType.AttackDamagePercent, (s, src, v) => s.AddAttackDamagePercentModifier(src, v) },
            { PassiveBuffType.AttackDamageFixed, (s, src, v) => s.AddAttackDamageModifier(src, v) },
            { PassiveBuffType.MaxHealthPercent, (s, src, v) => s.AddMaxHealthPercentModifier(src, v) },
            { PassiveBuffType.MaxHealthFixed, (s, src, v) => s.AddMaxHealthModifier(src, v) },
            { PassiveBuffType.CriticalRate, (s, src, v) => s.AddCriticalRateModifier(src, v) },
            { PassiveBuffType.CriticalDamage, (s, src, v) => s.AddCriticalDamageModifier(src, v) },
            { PassiveBuffType.GoldGainPercent, (s, src, v) => s.AddGoldGainPercentModifier(src, v) },
        };

        private static readonly Dictionary<PassiveBuffType, Action<IStatsProvider, object>> RemoveActions = new()
        {
            { PassiveBuffType.AttackDamagePercent, (s, src) => s.RemoveAttackDamagePercentModifier(src) },
            { PassiveBuffType.AttackDamageFixed, (s, src) => s.RemoveAttackDamageModifier(src) },
            { PassiveBuffType.MaxHealthPercent, (s, src) => s.RemoveMaxHealthPercentModifier(src) },
            { PassiveBuffType.MaxHealthFixed, (s, src) => s.RemoveMaxHealthModifier(src) },
            { PassiveBuffType.CriticalRate, (s, src) => s.RemoveCriticalRateModifier(src) },
            { PassiveBuffType.CriticalDamage, (s, src) => s.RemoveCriticalDamageModifier(src) },
            { PassiveBuffType.GoldGainPercent, (s, src) => s.RemoveGoldGainPercentModifier(src) },
        };

        [Header("패시브 스킬 상태")]
        [SerializeField] private bool isActive = false;

        public bool IsActive => isActive;

        // 현재 레벨의 버프 효과량
        public float GetBuffValue()
        {
            return SkillCalculator.GetBuffValue(skillData, currentLevel);
        }

        // 버프 타입 (JSON 데이터에 없으므로 기본값 반환, 필요 시 확장)
        private PassiveBuffType BuffType
        {
            get
            {
                // TODO: 팀원과 협의하여 JSON에 buffType 추가
                return PassiveBuffType.AttackDamagePercent; // 임시 기본값
            }
        }

        // 버프 활성화
        public void Activate()
        {
            if (isActive)
            {
                Debug.Log($"{skillData.name}은(는) 이미 활성화되어 있습니다.");
                return;
            }

            isActive = true;
            ApplyPassiveEffect();
            Debug.Log($"{skillData.name} 패시브 활성화! 효과: {GetBuffValue():F1}");
        }

        // 버프 비활성화
        public void Deactivate()
        {
            if (!isActive) return;

            isActive = false;
            RemovePassiveEffect();
            Debug.Log($"{skillData.name} 패시브 비활성화");
        }

        // IStatsProvider에 버프 적용
        private void ApplyPassiveEffect()
        {
            var stats = GetComponentInParent<IStatsProvider>();
            if (stats == null)
            {
                Debug.LogWarning("IStatsProvider를 찾을 수 없습니다.");
                return;
            }

            if (ApplyActions.TryGetValue(BuffType, out var applyAction))
            {
                applyAction(stats, this, GetBuffValue());
            }
            else
            {
                Debug.LogWarning($"지원하지 않는 버프 타입: {BuffType}");
            }
        }

        // 버프 제거
        private void RemovePassiveEffect()
        {
            var stats = GetComponentInParent<IStatsProvider>();
            if (stats == null) return;

            if (RemoveActions.TryGetValue(BuffType, out var removeAction))
            {
                removeAction(stats, this);
            }
        }

        // 레벨업 시 버프 갱신
        protected override void OnLevelUp()
        {
            base.OnLevelUp();

            if (isActive)
            {
                RemovePassiveEffect();
                ApplyPassiveEffect();
                Debug.Log($"{skillData.name} 패시브 레벨업! 새로운 효과: {GetBuffValue():F1}");
            }
        }

        private void OnDestroy()
        {
            if (isActive) Deactivate();
        }
    }
}
