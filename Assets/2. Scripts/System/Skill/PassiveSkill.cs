using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    // 조민희 추가: 누적형 버프 타입 (Phase 7)
    public enum AccumulateType
    {
        None,           // 일반 버프 (누적 없음)
        TimeBased,      // 시간 기반 누적 (N초마다)
        AttackBased     // 공격 기반 누적 (N회 공격마다)
    }

    // 조민희 추가: 특수 발동 타입 (Phase 8)
    public enum SpecialActivationType
    {
        None,               // 일반 즉시 발동
        HealthSacrifice,    // 체력 소모 후 발동
        DelayedBuff,        // 지연 발동 (전투 돌입 N초 후)
        PeriodicRestore     // 주기적 회복
    }

    // 패시브 스킬: 항상 활성화된 버프
    // - Activate()로 버프 적용, Deactivate()로 제거
    // - 레벨에 따른 버프 효과 증가
    // - Phase 7: 누적형 버프 지원 (시간/공격 기반)
    public class PassiveSkill : SkillBase, ISkillDisplayable
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
            // 조민희 추가: 확장 버프
            { PassiveBuffType.AttackSpeedPercent, (s, src, v) => s.AddAttackSpeedModifier(src, v) },
            { PassiveBuffType.MoveSpeedPercent, (s, src, v) => s.AddMoveSpeedModifier(src, v) },
            { PassiveBuffType.ManaRegenPercent, (s, src, v) => s.AddManaRegenModifier(src, v) },
            { PassiveBuffType.HealthRegenPercent, (s, src, v) => s.AddHealthRegenModifier(src, v) },
            // 조민희 추가: Phase 8 특수 버프
            { PassiveBuffType.CooldownReduction, (s, src, v) => s.AddCooldownReductionModifier(src, v) },
            { PassiveBuffType.Evasion, (s, src, v) => s.AddEvasionModifier(src, v) },
            { PassiveBuffType.DamageBasedOnMissingHp, (s, src, v) => s.AddMissingHpDamageModifier(src, v) },
            { PassiveBuffType.HealthSacrificeAttack, (s, src, v) => s.AddAttackDamagePercentModifier(src, v) },
            { PassiveBuffType.HealthSacrificeSpeed, (s, src, v) => s.AddAttackSpeedModifier(src, v) },
            { PassiveBuffType.DelayedAttackBuff, (s, src, v) => s.AddAttackDamagePercentModifier(src, v) },
            { PassiveBuffType.DelayedSpeedBuff, (s, src, v) => s.AddAttackSpeedModifier(src, v) },
            { PassiveBuffType.HealthManaRestore, (s, src, v) => { /* 회복은 별도 처리 */ } },
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
            // 조민희 추가: 확장 버프
            { PassiveBuffType.AttackSpeedPercent, (s, src) => s.RemoveAttackSpeedModifier(src) },
            { PassiveBuffType.MoveSpeedPercent, (s, src) => s.RemoveMoveSpeedModifier(src) },
            { PassiveBuffType.ManaRegenPercent, (s, src) => s.RemoveManaRegenModifier(src) },
            { PassiveBuffType.HealthRegenPercent, (s, src) => s.RemoveHealthRegenModifier(src) },
            // 조민희 추가: Phase 8 특수 버프
            { PassiveBuffType.CooldownReduction, (s, src) => s.RemoveCooldownReductionModifier(src) },
            { PassiveBuffType.Evasion, (s, src) => s.RemoveEvasionModifier(src) },
            { PassiveBuffType.DamageBasedOnMissingHp, (s, src) => s.RemoveMissingHpDamageModifier(src) },
            { PassiveBuffType.HealthSacrificeAttack, (s, src) => s.RemoveAttackDamagePercentModifier(src) },
            { PassiveBuffType.HealthSacrificeSpeed, (s, src) => s.RemoveAttackSpeedModifier(src) },
            { PassiveBuffType.DelayedAttackBuff, (s, src) => s.RemoveAttackDamagePercentModifier(src) },
            { PassiveBuffType.DelayedSpeedBuff, (s, src) => s.RemoveAttackSpeedModifier(src) },
            { PassiveBuffType.HealthManaRestore, (s, src) => { /* 회복은 별도 처리 */ } },
        };

        [Header("패시브 스킬 상태")]
        [SerializeField] private bool isActive = false;

        // 조민희 추가: 누적형 버프 (Phase 7)
        [Header("누적형 버프 설정")]
        [SerializeField] private int currentStacks = 0;         // 현재 스택 수
        [SerializeField] private int attackCounter = 0;         // 공격 카운터 (공격 기반 누적용)
        private Coroutine accumulateCoroutine;                   // 시간 기반 누적 코루틴
        private Coroutine decayCoroutine;                        // 스택 감소 코루틴 (공격 기반)
        private float lastAttackTime;                            // 마지막 공격 시간 (감소용)
        private float lastStackTime;                             // 마지막 스택 획득 시간 (만료용)

        // 조민희 추가: IStatsProvider 캐싱 (Phase 7 개선)
        private IStatsProvider cachedStats;
        private IStatsProvider CachedStats
        {
            get
            {
                if (cachedStats == null)
                    cachedStats = GetComponentInParent<IStatsProvider>();
                return cachedStats;
            }
        }

        // 조민희 추가: 특수 스킬 설정 (Phase 8)
        [Header("특수 스킬 설정 (Phase 8)")]
        private Coroutine specialCoroutine;              // 특수 발동 코루틴
        private Coroutine buffDurationCoroutine;         // 버프 지속 코루틴
        private float combatStartTime;                   // 전투 시작 시간
        private bool isSpecialBuffActive;                // 특수 버프 활성화 여부

        public bool IsActive => isActive;
        public int CurrentStacks => currentStacks;

        // 현재 레벨의 버프 효과량
        public float GetBuffValue()
        {
            return SkillCalculator.GetBuffValue(skillData, currentLevel);
        }

        // 버프 타입 (스킬 ID에 따라 결정)
        private PassiveBuffType BuffType
        {
            get
            {
                if (skillData == null) return PassiveBuffType.None;

                // 스킬 ID에 따라 버프 타입 결정 (실제 JSON ID 매핑)
                return skillData.id switch
                {
                    // Fire 패시브 - 공격력 관련
                    "Fire_02" => PassiveBuffType.AttackDamagePercent,      // 불의 검 (10초간 공격력 +n%)
                    "Fire_05" => PassiveBuffType.AttackDamagePercent,      // 타오르는 검 (5초마다 공격력 +)
                    "Fire_09" => PassiveBuffType.DamageBasedOnMissingHp,   // 분노 (체력 비례 공격력)
                    "Fire_11" => PassiveBuffType.HealthSacrificeAttack,    // 워리어 번 (체력 50% 소모, 공격력 증가) - Phase 8

                    // Water 패시브 - 마나/공속 관련
                    "Water_02" => PassiveBuffType.ManaRegenPercent,        // 마나의 축복 (마나 회복 +n%)
                    "Water_05" => PassiveBuffType.AttackSpeedPercent,      // 흐르는 칼날 (10초간 공격속도 +n%)
                    "Water_06" => PassiveBuffType.AttackSpeedPercent,      // 굽이치는 칼날 (4초마다 공속 +)
                    "Water_07" => PassiveBuffType.Evasion,                 // 춤추는 파도 (회피) - Phase 8
                    "Water_09" => PassiveBuffType.CooldownReduction,       // 메디테이션 (쿨타임 감소) - Phase 8
                    "Water_11" => PassiveBuffType.DelayedSpeedBuff,        // 격류 (2초 후 공속 증가) - Phase 8

                    // Wind 패시브 - 이속/공속 관련
                    "Wind_03" => PassiveBuffType.MoveSpeedPercent,         // 고속이동 (이동속도 +n%)
                    "Wind_05" => PassiveBuffType.AttackSpeedPercent,       // 가속의 검 (5공격마다 공속 +)
                    "Wind_07" => PassiveBuffType.AttackSpeedPercent,       // 바람의 검 (공격속도 +n%)
                    "Wind_11" => PassiveBuffType.HealthSacrificeSpeed,     // 뇌신 (체력 50% 소모, 공속 증가) - Phase 8

                    // Earth 패시브 - 체력/공격력 관련
                    "Earth_02" => PassiveBuffType.HealthRegenPercent,      // 대지의 축복 (체력회복 n%)
                    "Earth_05" => PassiveBuffType.AttackDamagePercent,     // 땅의 의지 (7공격마다 공격력 +)
                    "Earth_06" => PassiveBuffType.AttackDamagePercent,     // 강철의 의지 (전체 공격력 +n%)
                    "Earth_07" => PassiveBuffType.HealthManaRestore,       // 라이프 마나 (HP+MP 회복) - Phase 8
                    "Earth_11" => PassiveBuffType.DelayedAttackBuff,       // 괴력난신 (20초 후 공격력 증가) - Phase 8

                    // 기존 테스트용 ID (하위 호환)
                    "attack_boost" => PassiveBuffType.AttackDamagePercent,
                    "crit_boost" => PassiveBuffType.CriticalRate,

                    _ => PassiveBuffType.AttackDamagePercent  // 기본값
                };
            }
        }

        // 조민희 추가: 누적형 버프 타입 (Phase 7)
        private AccumulateType AccumulateType
        {
            get
            {
                if (skillData == null) return AccumulateType.None;

                return skillData.id switch
                {
                    // 시간 기반 누적
                    "Fire_05" => AccumulateType.TimeBased,     // 타오르는 검 (5초마다)
                    "Water_06" => AccumulateType.TimeBased,    // 굽이치는 칼날 (4초마다)

                    // 공격 기반 누적
                    "Wind_05" => AccumulateType.AttackBased,   // 가속의 검 (5공격마다)
                    "Earth_05" => AccumulateType.AttackBased,  // 땅의 의지 (7공격마다)

                    _ => AccumulateType.None
                };
            }
        }

        // 조민희 추가: 누적 주기 (Phase 7)
        private float AccumulateInterval
        {
            get
            {
                if (skillData == null) return 0f;

                return skillData.id switch
                {
                    "Fire_05" => 5f,      // 5초마다
                    "Water_06" => 4f,     // 4초마다
                    _ => 0f
                };
            }
        }

        // 조민희 추가: 공격 기반 누적 필요 횟수 (Phase 7)
        private int AttacksNeeded
        {
            get
            {
                if (skillData == null) return 0;

                return skillData.id switch
                {
                    "Wind_05" => 5,       // 5회 공격마다
                    "Earth_05" => 7,      // 7회 공격마다
                    _ => 0
                };
            }
        }

        // 조민희 추가: 스킬별 최대 스택 수 (Phase 7 개선)
        private int MaxStacks
        {
            get
            {
                if (skillData == null) return 999;

                return skillData.id switch
                {
                    // 시간 기반 누적 - 적은 최대 스택
                    "Fire_05" => 10,      // 타오르는 검: 최대 10스택
                    "Water_06" => 15,     // 굽이치는 칼날: 최대 15스택

                    // 공격 기반 누적 - 더 많은 최대 스택
                    "Wind_05" => 20,      // 가속의 검: 최대 20스택
                    "Earth_05" => 15,     // 땅의 의지: 최대 15스택

                    _ => 999              // 기본값 (사실상 무제한)
                };
            }
        }

        // 조민희 추가: 스택 감소 설정 (Phase 7 개선)
        // 공격 기반 누적 스킬에서 일정 시간 공격 안 하면 스택 감소
        private float StackDecayDelay => 8f;           // N초 후 감소 시작
        private float StackDecayInterval => 2f;        // N초마다 1스택 감소
        private bool EnableStackDecay => AccumulateType == AccumulateType.AttackBased;

        // 조민희 추가: 스택 효과 공식 (Phase 7 개선)
        // 스택당 효과 계산 (첫 스택 100%, 이후 80%씩 적용 - 감소하는 효과)
        private float GetStackEffectMultiplier(int stackNumber)
        {
            if (stackNumber <= 0) return 0f;
            if (stackNumber == 1) return 1f;

            // 감소 공식: 1 + 0.8 + 0.64 + 0.512 + ... (80%씩 감소)
            // 대안: 1 + 0.9 + 0.8 + 0.7 + ... (고정 감소)
            float multiplier = 1f;
            for (int i = 2; i <= stackNumber; i++)
            {
                multiplier += Mathf.Max(0.5f, 1f - (i - 1) * 0.1f); // 최소 50% 효과
            }
            return multiplier;
        }

        // 조민희 추가: 누적형 스킬 여부
        public bool IsAccumulating => AccumulateType != AccumulateType.None;

        // 조민희 추가: 특수 발동 타입 (Phase 8)
        private SpecialActivationType SpecialType
        {
            get
            {
                if (skillData == null) return SpecialActivationType.None;

                return skillData.id switch
                {
                    // 체력 소모형
                    "Fire_11" => SpecialActivationType.HealthSacrifice,    // 워리어 번
                    "Wind_11" => SpecialActivationType.HealthSacrifice,    // 뇌신

                    // 지연 발동형
                    "Water_11" => SpecialActivationType.DelayedBuff,       // 격류 (2초 후)
                    "Earth_11" => SpecialActivationType.DelayedBuff,       // 괴력난신 (20초 후)

                    // 주기적 회복
                    "Earth_07" => SpecialActivationType.PeriodicRestore,   // 라이프 마나

                    _ => SpecialActivationType.None
                };
            }
        }

        // 조민희 추가: 특수 스킬 여부
        public bool IsSpecialSkill => SpecialType != SpecialActivationType.None;

        // 조민희 추가: 지연 발동 시간 (Phase 8)
        private float DelayedActivationTime
        {
            get
            {
                if (skillData == null) return 0f;

                return skillData.id switch
                {
                    "Water_11" => 2f,      // 격류: 2초 후
                    "Earth_11" => 20f,     // 괴력난신: 20초 후
                    _ => 0f
                };
            }
        }

        // 조민희 추가: 버프 지속 시간 (Phase 8)
        private float BuffDuration
        {
            get
            {
                if (skillData == null) return 0f;

                return skillData.id switch
                {
                    "Fire_11" => 5f,       // 워리어 번: 5초
                    "Water_07" => 5f,      // 춤추는 파도: n초 (기본 5초)
                    "Water_11" => 6f,      // 격류: 6초
                    "Wind_11" => 5f,       // 뇌신: 5초
                    "Earth_11" => 5f,      // 괴력난신: 5초
                    _ => 0f
                };
            }
        }

        // 조민희 추가: 체력 소모 비율 (Phase 8)
        private float HealthSacrificePercent
        {
            get
            {
                if (skillData == null) return 0f;

                return skillData.id switch
                {
                    "Fire_11" => 0.5f,     // 워리어 번: 50%
                    "Wind_11" => 0.5f,     // 뇌신: 50%
                    _ => 0f
                };
            }
        }

        // 버프 활성화
        public void Activate()
        {
            if (isActive)
            {
                return;
            }

            isActive = true;
            currentStacks = 0;      // 스택 초기화
            attackCounter = 0;      // 공격 카운터 초기화
            lastAttackTime = Time.time;
            lastStackTime = Time.time;
            combatStartTime = Time.time;  // Phase 8: 전투 시작 시간

            // 조민희 추가: Phase 8 특수 스킬 처리
            if (IsSpecialSkill)
            {
                ActivateSpecialSkill();
                return;
            }

            // 조민희 추가: 누적형 버프 처리 (Phase 7)
            if (IsAccumulating)
            {
                if (AccumulateType == AccumulateType.TimeBased)
                {
                    // 시간 기반 누적 코루틴 시작
                    accumulateCoroutine = StartCoroutine(TimeBasedAccumulateCoroutine());
                }
                else if (AccumulateType == AccumulateType.AttackBased && EnableStackDecay)
                {
                    // 공격 기반: 스택 감소 코루틴 시작
                    decayCoroutine = StartCoroutine(StackDecayCoroutine());
                }
            }
            else
            {
                // 일반 버프: 즉시 적용
                ApplyPassiveEffect();
            }
        }

        // 조민희 추가: 특수 스킬 활성화 (Phase 8)
        private void ActivateSpecialSkill()
        {
            switch (SpecialType)
            {
                case SpecialActivationType.HealthSacrifice:
                    // 체력 소모 후 즉시 버프 적용 (Fire_11, Wind_11)
                    ActivateHealthSacrificeBuff();
                    break;

                case SpecialActivationType.DelayedBuff:
                    // 지연 발동 코루틴 시작 (Water_11, Earth_11)
                    specialCoroutine = StartCoroutine(DelayedBuffCoroutine());
                    break;

                case SpecialActivationType.PeriodicRestore:
                    // 주기적 회복 코루틴 시작 (Earth_07)
                    specialCoroutine = StartCoroutine(PeriodicRestoreCoroutine());
                    break;
            }
        }

        // 조민희 추가: 체력 소모형 버프 발동 (Phase 8)
        private void ActivateHealthSacrificeBuff()
        {
            if (CachedStats == null) return;

            // 체력 소모
            CachedStats.SacrificeHealth(HealthSacrificePercent);

            // 버프 적용
            ApplyPassiveEffect();
            isSpecialBuffActive = true;

            // 지속 시간 후 버프 제거
            if (BuffDuration > 0)
            {
                buffDurationCoroutine = StartCoroutine(BuffDurationCoroutine());
            }
        }

        // 조민희 추가: 지연 발동 코루틴 (Phase 8)
        private IEnumerator DelayedBuffCoroutine()
        {
            // 지연 시간 대기
            yield return new WaitForSeconds(DelayedActivationTime);

            if (!isActive) yield break;

            // 버프 적용
            ApplyPassiveEffect();
            isSpecialBuffActive = true;

            // 지속 시간 후 버프 제거
            if (BuffDuration > 0)
            {
                buffDurationCoroutine = StartCoroutine(BuffDurationCoroutine());
            }
        }

        // 조민희 추가: 주기적 회복 코루틴 (Phase 8)
        private IEnumerator PeriodicRestoreCoroutine()
        {
            // 전투 중 주기적으로 회복
            while (isActive)
            {
                yield return new WaitForSeconds(5f); // 5초마다 회복

                if (CachedStats != null && isActive)
                {
                    float healPercent = GetBuffValue() / 100f; // 버프값을 퍼센트로 변환
                    CachedStats.RestoreHealth(healPercent);
                    CachedStats.RestoreMana(0.3f); // 마나 30% 회복
                }
            }
        }

        // 조민희 추가: 버프 지속 시간 코루틴 (Phase 8)
        private IEnumerator BuffDurationCoroutine()
        {
            yield return new WaitForSeconds(BuffDuration);

            if (isSpecialBuffActive)
            {
                RemovePassiveEffect();
                isSpecialBuffActive = false;

                // 쿨타임 후 재발동 (주기적)
                if (isActive && SpecialType == SpecialActivationType.HealthSacrifice)
                {
                    yield return new WaitForSeconds(15f); // 15초 쿨타임
                    if (isActive)
                    {
                        ActivateHealthSacrificeBuff();
                    }
                }
            }
        }

        // 버프 비활성화
        public void Deactivate()
        {
            if (!isActive) return;

            isActive = false;

            // 조민희 추가: Phase 8 특수 스킬 정리
            if (IsSpecialSkill)
            {
                if (specialCoroutine != null)
                {
                    StopCoroutine(specialCoroutine);
                    specialCoroutine = null;
                }

                if (buffDurationCoroutine != null)
                {
                    StopCoroutine(buffDurationCoroutine);
                    buffDurationCoroutine = null;
                }

                if (isSpecialBuffActive)
                {
                    RemovePassiveEffect();
                    isSpecialBuffActive = false;
                }
                return;
            }

            // 조민희 추가: 누적형 버프 정리 (Phase 7)
            if (IsAccumulating)
            {
                if (accumulateCoroutine != null)
                {
                    StopCoroutine(accumulateCoroutine);
                    accumulateCoroutine = null;
                }

                if (decayCoroutine != null)
                {
                    StopCoroutine(decayCoroutine);
                    decayCoroutine = null;
                }

                // 누적된 버프 모두 제거
                RemoveAllStacks();
            }
            else
            {
                RemovePassiveEffect();
            }
        }

        // 조민희 추가: 시간 기반 누적 코루틴 (Phase 7)
        private IEnumerator TimeBasedAccumulateCoroutine()
        {
            while (isActive)
            {
                yield return new WaitForSeconds(AccumulateInterval);

                if (currentStacks < MaxStacks)
                {
                    AddStack();
                }
            }
        }

        // 조민희 추가: 스택 감소 코루틴 (Phase 7 개선)
        // 공격 기반 누적 스킬에서 일정 시간 공격 안 하면 스택 감소
        private IEnumerator StackDecayCoroutine()
        {
            while (isActive)
            {
                yield return new WaitForSeconds(StackDecayInterval);

                // 마지막 공격으로부터 DecayDelay 이상 지났으면 스택 감소
                if (Time.time - lastAttackTime >= StackDecayDelay && currentStacks > 0)
                {
                    RemoveStack();
                }
            }
        }

        // 조민희 추가: 공격 시 호출 (공격 기반 누적용) (Phase 7)
        public void OnAttack()
        {
            if (!isActive || AccumulateType != AccumulateType.AttackBased) return;

            lastAttackTime = Time.time;  // 마지막 공격 시간 갱신
            attackCounter++;

            // 설정된 공격 횟수에 도달하면 스택 증가
            if (attackCounter >= AttacksNeeded)
            {
                attackCounter = 0;  // 카운터 리셋

                if (currentStacks < MaxStacks)
                {
                    AddStack();
                }
            }
        }

        // 조민희 추가: 스택 추가 (Phase 7 개선)
        private void AddStack()
        {
            currentStacks++;
            lastStackTime = Time.time;

            // 스택 효과 공식 적용 (감소하는 효과)
            float baseBuffValue = GetBuffValue();
            float stackEffect = baseBuffValue * GetStackEffectMultiplier(currentStacks);

            // 기존 버프 제거 후 새로운 값으로 재적용
            if (CachedStats != null && ApplyActions.TryGetValue(BuffType, out var applyAction))
            {
                // 먼저 기존 효과 제거
                if (RemoveActions.TryGetValue(BuffType, out var removeAction))
                {
                    removeAction(CachedStats, this);
                }
                // 새로운 누적 값으로 적용
                applyAction(CachedStats, this, stackEffect);
            }
        }

        // 조민희 추가: 스택 제거 (Phase 7 개선)
        private void RemoveStack()
        {
            if (currentStacks <= 0) return;

            currentStacks--;

            // 기존 버프 제거 후 새로운 값으로 재적용
            if (CachedStats != null)
            {
                // 기존 효과 제거
                if (RemoveActions.TryGetValue(BuffType, out var removeAction))
                {
                    removeAction(CachedStats, this);
                }

                // 남은 스택이 있으면 재적용
                if (currentStacks > 0 && ApplyActions.TryGetValue(BuffType, out var applyAction))
                {
                    float baseBuffValue = GetBuffValue();
                    float stackEffect = baseBuffValue * GetStackEffectMultiplier(currentStacks);
                    applyAction(CachedStats, this, stackEffect);
                }
            }
        }

        // 조민희 추가: 모든 스택 제거 (Phase 7)
        private void RemoveAllStacks()
        {
            if (CachedStats != null && RemoveActions.TryGetValue(BuffType, out var removeAction))
            {
                removeAction(CachedStats, this);
            }
            currentStacks = 0;
        }

        // IStatsProvider에 버프 적용
        private void ApplyPassiveEffect()
        {
            if (CachedStats == null)
            {
                Debug.LogWarning("IStatsProvider를 찾을 수 없습니다.");
                return;
            }

            if (ApplyActions.TryGetValue(BuffType, out var applyAction))
            {
                applyAction(CachedStats, this, GetBuffValue());
            }
            else
            {
                Debug.LogWarning($"지원하지 않는 버프 타입: {BuffType}");
            }
        }

        // 버프 제거
        private void RemovePassiveEffect()
        {
            if (CachedStats == null) return;

            if (RemoveActions.TryGetValue(BuffType, out var removeAction))
            {
                removeAction(CachedStats, this);
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

        #region ISkillDisplayable 구현

        public string SkillId => skillData?.id ?? "";
        public SkillData Data => skillData;
        bool ISkillDisplayable.IsActive => isActive;

        public string GetDisplayText()
        {
            if (skillData == null) return "";

            // 특수 스킬인 경우
            if (IsSpecialSkill)
            {
                return GetSpecialSkillDisplayText();
            }

            // 누적형 스킬인 경우
            if (IsAccumulating)
            {
                return $"{currentStacks}";
            }

            // 일반 패시브 스킬
            return isActive ? "ON" : "OFF";
        }

        public Color GetDisplayColor()
        {
            if (skillData == null) return Color.gray;

            // 특수 스킬인 경우
            if (IsSpecialSkill)
            {
                return GetSpecialSkillDisplayColor();
            }

            // 누적형 스킬인 경우
            if (IsAccumulating)
            {
                // 스택이 있으면 주황색, 없으면 회색
                return currentStacks > 0 ? new Color(1f, 0.5f, 0f) : Color.gray;
            }

            // 일반 패시브 스킬
            return isActive ? new Color(0f, 0.8f, 0f) : Color.gray; // 초록/회색
        }

        private string GetSpecialSkillDisplayText()
        {
            switch (SpecialType)
            {
                case SpecialActivationType.HealthSacrifice:
                    // 체력 소모형: 버프 활성화 여부 표시
                    return isSpecialBuffActive ? "ON" : "RDY";

                case SpecialActivationType.DelayedBuff:
                    // 지연 발동형: 남은 시간 또는 활성화 여부
                    if (isSpecialBuffActive)
                        return "ON";
                    else
                    {
                        float elapsed = Time.time - combatStartTime;
                        float remaining = DelayedActivationTime - elapsed;
                        return remaining > 0 ? $"{Mathf.CeilToInt(remaining)}" : "RDY";
                    }

                case SpecialActivationType.PeriodicRestore:
                    // 주기적 회복: 활성화 상태만 표시
                    return isActive ? "ON" : "OFF";

                default:
                    return isActive ? "ON" : "OFF";
            }
        }

        private Color GetSpecialSkillDisplayColor()
        {
            switch (SpecialType)
            {
                case SpecialActivationType.HealthSacrifice:
                    // 체력 소모형: 빨간색 (활성) / 회색 (대기)
                    return isSpecialBuffActive ? Color.red : Color.gray;

                case SpecialActivationType.DelayedBuff:
                    // 지연 발동형: 노란색 (대기) / 초록색 (활성)
                    return isSpecialBuffActive ? Color.green : Color.yellow;

                case SpecialActivationType.PeriodicRestore:
                    // 주기적 회복: 초록색 (활성) / 회색 (비활성)
                    return isActive ? new Color(0f, 0.8f, 0f) : Color.gray;

                default:
                    return isActive ? Color.green : Color.gray;
            }
        }

        #endregion
    }
}
