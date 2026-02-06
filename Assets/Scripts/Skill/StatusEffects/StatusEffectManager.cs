using System;
using System.Reflection;
using UnityEngine;
using SlayerLegend.Skill.Data;

namespace SlayerLegend.Skill.StatusEffects
{
    // 상태이상 효과 매니저
    // SkillData/SkillEffectData의 데이터를 기반으로 StatusEffect를 생성하고 초기화
    // SkillData.cs를 직접 수정하지 않고 리플렉션으로 데이터를 읽음
    public static class StatusEffectManager
    {
        #region 리플렉션 캐시 (스킬 레벨 정보 전용)

        private static readonly Type _skillDataType;
        private static readonly FieldInfo _maxLevelField;
        private static readonly FieldInfo _needMpField;
        private static readonly FieldInfo _initialRateField;
        private static readonly FieldInfo _levelUpValueField;

        static StatusEffectManager()
        {
            _skillDataType = typeof(SkillData);
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            // 스킬 레벨 관련 필드만 캐싱 (DoT/CC는 SkillEffectData 활용)
            _maxLevelField = _skillDataType.GetField("maxLevel", flags);
            _needMpField = _skillDataType.GetField("needMp", flags);
            _initialRateField = _skillDataType.GetField("initialRate", flags);
            _levelUpValueField = _skillDataType.GetField("levelUpValue", flags);

            // 필드 초기화 실패 시 경고 로그
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_maxLevelField == null)
                Debug.LogError("[StatusEffectManager] 'maxLevel' field not found in SkillData.");
            if (_needMpField == null)
                Debug.LogError("[StatusEffectManager] 'needMp' field not found in SkillData.");
            if (_initialRateField == null)
                Debug.LogError("[StatusEffectManager] 'initialRate' field not found in SkillData.");
            if (_levelUpValueField == null)
                Debug.LogError("[StatusEffectManager] 'levelUpValue' field not found in SkillData.");
            #endif
        }

        #endregion

        #region 스킬 레벨 정보 조회

        // 스킬 레벨 정보
        public class SkillLevelInfo
        {
            public int MaxLevel;
            public int NeedMp;
            public int InitialRate;
            public float LevelUpValue;

            public override string ToString()
            {
                return $"Lv.{MaxLevel} | MP:{NeedMp} | 기본:{InitialRate}% | 성장:{LevelUpValue}";
            }
        }

        // 스킬 레벨 정보 가져오기
        public static SkillLevelInfo GetLevelInfo(SkillData skillData)
        {
            if (skillData == null) return null;

            return new SkillLevelInfo
            {
                MaxLevel = GetFieldValue<int>(_maxLevelField, skillData, 1),
                NeedMp = GetFieldValue<int>(_needMpField, skillData, 0),
                InitialRate = GetFieldValue<int>(_initialRateField, skillData, 100),
                LevelUpValue = GetFieldValue<float>(_levelUpValueField, skillData, 0f)
            };
        }

        // 특정 레벨에서의 스킬 수치 계산
        public static float CalculateSkillValue(SkillData skillData, int level)
        {
            SkillLevelInfo info = GetLevelInfo(skillData);
            if (info == null) return 0f;

            // 수치 = 기본수치 + (레벨-1) * 성장수치
            return info.InitialRate + ((level - 1) * info.LevelUpValue);
        }

        #endregion

        #region DoT 효과 생성

        // Do 효과 생성
        public static DotEffect CreateDotEffect(SkillData skillData, GameObject target, IDamageable damageTarget, GameObject source, float maxHp = 0f)
        {
            if (skillData == null || target == null)
            {
                Debug.LogError("[StatusEffectManager] skillData 또는 target이 null입니다.");
                return null;
            }

            // SkillDataExtensions의 확장 메서드 활용
            if (!skillData.IsDotSkill())
            {
                return null; // DoT 스킬이 아니면 silent return
            }

            // 유효성 검사 (음수 방지)
            float duration = Mathf.Max(0f, skillData.GetDotDuration());
            float damage = skillData.GetDotDamagePerTick();
            float interval = Mathf.Max(0.01f, skillData.GetDotTickInterval());
            bool isPercentage = skillData.GetDotIsPercentage();

            if (duration <= 0f)
            {
                Debug.LogWarning($"[StatusEffectManager] DoT 지속 시간이 0 이하입니다: {duration}");
                return null;
            }

            try
            {
                DotEffect dotEffect = target.AddComponent<DotEffect>();
                dotEffect.Initialize(duration, damage, interval, damageTarget, source, isPercentage, maxHp);
                return dotEffect;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] DotEffect 컴포넌트 추가 실패: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region CC 효과 생성

        // CC 효과 유형
        public enum CcType
        {
            None,
            Stun,
            Freeze,
            Root
        }

        // CC 효과 정보
        public class CcEffectInfo
        {
            public CcType Type;
            public float Duration;

            public bool IsValid => Type != CcType.None && Duration > 0;

            public override string ToString()
            {
                return Type != CcType.None ? $"{Type}: {Duration}s" : "CC: 없음";
            }
        }

        // CC 효과 정보 가져오기 (우선순위: Stun > Freeze > Root)
        public static CcEffectInfo GetCcInfo(SkillData skillData)
        {
            if (skillData == null) return new CcEffectInfo { Type = CcType.None };

            // 우선순위별로 확인 (SkillDataExtensions 활용)
            if (skillData.IsStunSkill())
            {
                return new CcEffectInfo
                {
                    Type = CcType.Stun,
                    Duration = Mathf.Max(0f, skillData.GetStunDuration())
                };
            }

            if (skillData.IsFreezeSkill())
            {
                return new CcEffectInfo
                {
                    Type = CcType.Freeze,
                    Duration = Mathf.Max(0f, skillData.GetFreezeDuration())
                };
            }

            if (skillData.IsRootSkill())
            {
                return new CcEffectInfo
                {
                    Type = CcType.Root,
                    Duration = Mathf.Max(0f, skillData.GetRootDuration())
                };
            }

            return new CcEffectInfo { Type = CcType.None };
        }

        // 기절 효과 생성
        public static StunEffect CreateStunEffect(SkillData skillData, GameObject target, IStunnable stunTarget)
        {
            if (skillData == null || target == null || stunTarget == null)
            {
                Debug.LogError("[StatusEffectManager] CreateStunEffect: 필수 파라미터가 null입니다.");
                return null;
            }

            CcEffectInfo info = GetCcInfo(skillData);
            if (info.Type != CcType.Stun)
            {
                return null; // 기절 스킬이 아니면 silent return
            }

            if (info.Duration <= 0f)
            {
                Debug.LogWarning($"[StatusEffectManager] 기절 지속 시간이 0 이하입니다: {info.Duration}");
                return null;
            }

            try
            {
                StunEffect stunEffect = target.AddComponent<StunEffect>();
                stunEffect.Initialize(info.Duration, stunTarget);
                return stunEffect;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] StunEffect 컴포넌트 추가 실패: {ex.Message}");
                return null;
            }
        }

        // 빙결 효과 생성
        public static FreezeEffect CreateFreezeEffect(SkillData skillData, GameObject target, IFreezable freezeTarget)
        {
            if (skillData == null || target == null || freezeTarget == null)
            {
                Debug.LogError("[StatusEffectManager] CreateFreezeEffect: 필수 파라미터가 null입니다.");
                return null;
            }

            CcEffectInfo info = GetCcInfo(skillData);
            if (info.Type != CcType.Freeze)
            {
                return null; // 빙결 스킬이 아니면 silent return
            }

            if (info.Duration <= 0f)
            {
                Debug.LogWarning($"[StatusEffectManager] 빙결 지속 시간이 0 이하입니다: {info.Duration}");
                return null;
            }

            try
            {
                FreezeEffect freezeEffect = target.AddComponent<FreezeEffect>();
                freezeEffect.Initialize(info.Duration, freezeTarget);
                return freezeEffect;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] FreezeEffect 컴포넌트 추가 실패: {ex.Message}");
                return null;
            }
        }

        // 속박 효과 생성
        public static RootEffect CreateRootEffect(SkillData skillData, GameObject target, IRootable rootTarget)
        {
            if (skillData == null || target == null || rootTarget == null)
            {
                Debug.LogError("[StatusEffectManager] CreateRootEffect: 필수 파라미터가 null입니다.");
                return null;
            }

            CcEffectInfo info = GetCcInfo(skillData);
            if (info.Type != CcType.Root)
            {
                return null; // 속박 스킬이 아니면 silent return
            }

            if (info.Duration <= 0f)
            {
                Debug.LogWarning($"[StatusEffectManager] 속박 지속 시간이 0 이하입니다: {info.Duration}");
                return null;
            }

            try
            {
                RootEffect rootEffect = target.AddComponent<RootEffect>();
                rootEffect.Initialize(info.Duration, rootTarget);
                return rootEffect;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] RootEffect 컴포넌트 추가 실패: {ex.Message}");
                return null;
            }
        }

        // CC 효과 자동 생성 (타입에 따라)
        public static StatusEffect CreateCcEffect(SkillData skillData, GameObject target, object ccTarget)
        {
            if (skillData == null || target == null || ccTarget == null)
            {
                Debug.LogError("[StatusEffectManager] CreateCcEffect: 필수 파라미터가 null입니다.");
                return null;
            }

            CcEffectInfo info = GetCcInfo(skillData);
            if (!info.IsValid)
            {
                return null; // CC 효과 없음
            }

            switch (info.Type)
            {
                case CcType.Stun:
                    if (ccTarget is IStunnable stunnable)
                        return CreateStunEffect(skillData, target, stunnable);
                    Debug.LogError($"[StatusEffectManager] CC 대상이 IStunnable을 구현하지 않았습니다.");
                    break;

                case CcType.Freeze:
                    if (ccTarget is IFreezable freezable)
                        return CreateFreezeEffect(skillData, target, freezable);
                    Debug.LogError($"[StatusEffectManager] CC 대상이 IFreezable을 구현하지 않았습니다.");
                    break;

                case CcType.Root:
                    if (ccTarget is IRootable rootable)
                        return CreateRootEffect(skillData, target, rootable);
                    Debug.LogError($"[StatusEffectManager] CC 대상이 IRootable을 구현하지 않았습니다.");
                    break;
            }

            return null;
        }

        #endregion

        #region 유틸리티

        // 필드 값 가져오기 (안전한 기본값 포함)
        private static T GetFieldValue<T>(FieldInfo field, object instance, T defaultValue)
        {
            if (field == null) return defaultValue;
            object value = field.GetValue(instance);
            return value is T typedValue ? typedValue : defaultValue;
        }

        // 스킬 데이터 요약 출력
        public static void LogSkillDataSummary(SkillData skillData)
        {
            if (skillData == null) return;

            try
            {
                Debug.Log("=== 스킬 데이터 요약 ===");
                Debug.Log($"이름: {skillData.name}");
                Debug.Log($"ID: {skillData.id}");
                Debug.Log($"레벨 정보: {GetLevelInfo(skillData)}");
                Debug.Log($"DoT 정보: {(skillData.IsDotSkill() ? $"활성화 ({skillData.GetDotDuration()}s)" : "비활성화")}");
                Debug.Log($"CC 정보: {GetCcInfo(skillData)}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[StatusEffectManager] 스킬 데이터 로깅 실패: {ex.Message}");
            }
        }

        #endregion
    }
}
