using System;
using UnityEngine;
using SlayerLegend.Skill.Data;

namespace SlayerLegend.Skill.StatusEffects
{
    // 도트 데미지 데이터를 JSON에서 읽어오기 위한 확장 클래스
    // 팀원의 SkillData를 수정하지 않고 리플렉션으로 도트 필드를 읽음
    public static class SkillDataExtensions
    {
        // effectData 필드 (SkillData에 있음)
        private static readonly System.Reflection.FieldInfo _effectDataField;

        // DoT 필드 (effectData 안에 있음)
        private static readonly System.Reflection.FieldInfo _isDotField;
        private static readonly System.Reflection.FieldInfo _dotDurationField;
        private static readonly System.Reflection.FieldInfo _dotDamagePerTickField;
        private static readonly System.Reflection.FieldInfo _dotTickIntervalField;
        private static readonly System.Reflection.FieldInfo _dotIsPercentageField;

        // CC 필드 (effectData 안에 있음)
        private static readonly System.Reflection.FieldInfo _isStunField;
        private static readonly System.Reflection.FieldInfo _stunDurationField;
        private static readonly System.Reflection.FieldInfo _isFreezeField;
        private static readonly System.Reflection.FieldInfo _freezeDurationField;
        private static readonly System.Reflection.FieldInfo _isRootField;
        private static readonly System.Reflection.FieldInfo _rootDurationField;

        // 레벨 관련 필드 (SkillData에 직접 있음)
        private static readonly System.Reflection.FieldInfo _maxLevelField;
        private static readonly System.Reflection.FieldInfo _needMpField;
        private static readonly System.Reflection.FieldInfo _initialRateField;
        private static readonly System.Reflection.FieldInfo _levelUpValueField;

        static SkillDataExtensions()
        {
            var skillDataType = typeof(SkillData);
            var effectDataType = typeof(SkillEffectData);
            var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase;

            // effectData 필드 초기화 (SkillData에서)
            _effectDataField = skillDataType.GetField("effectData", flags);

            // DoT 필드 초기화 (SkillEffectData에서)
            _isDotField = effectDataType.GetField("isDot", flags);
            _dotDurationField = effectDataType.GetField("dotDuration", flags);
            _dotDamagePerTickField = effectDataType.GetField("dotDamagePerTick", flags);
            _dotTickIntervalField = effectDataType.GetField("dotTickInterval", flags);
            _dotIsPercentageField = effectDataType.GetField("dotIsPercentage", flags);

            // CC 필드 초기화 (SkillEffectData에서)
            _isStunField = effectDataType.GetField("isStun", flags);
            _stunDurationField = effectDataType.GetField("stunDuration", flags);
            _isFreezeField = effectDataType.GetField("isFreeze", flags);
            _freezeDurationField = effectDataType.GetField("freezeDuration", flags);
            _isRootField = effectDataType.GetField("isRoot", flags);
            _rootDurationField = effectDataType.GetField("rootDuration", flags);

            // 레벨 필드 초기화 (SkillData에서 직접)
            _maxLevelField = skillDataType.GetField("maxLevel", flags);
            _needMpField = skillDataType.GetField("needMp", flags);
            _initialRateField = skillDataType.GetField("initialRate", flags);
            _levelUpValueField = skillDataType.GetField("levelUpValue", flags);

            // 필드 초기화 실패 시 경고 로그
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_effectDataField == null)
                Debug.LogWarning("[SkillDataExtensions] 'effectData' field not found in SkillData.");
            if (_isDotField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isDot' field not found in SkillEffectData.");
            if (_dotIsPercentageField == null)
                Debug.LogWarning("[SkillDataExtensions] 'dotIsPercentage' field not found in SkillEffectData.");
            if (_isStunField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isStun' field not found in SkillEffectData.");
            if (_isFreezeField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isFreeze' field not found in SkillEffectData.");
            if (_isRootField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isRoot' field not found in SkillEffectData.");
            if (_maxLevelField == null)
                Debug.LogWarning("[SkillDataExtensions] 'maxLevel' field not found in SkillData.");
            if (_needMpField == null)
                Debug.LogWarning("[SkillDataExtensions] 'needMp' field not found in SkillData.");
            if (_initialRateField == null)
                Debug.LogWarning("[SkillDataExtensions] 'initialRate' field not found in SkillData.");
            if (_levelUpValueField == null)
                Debug.LogWarning("[SkillDataExtensions] 'levelUpValue' field not found in SkillData.");
            #endif
        }

        // effectData 객체 가져오기 (DoT/CC 필드 접근용)
        private static SkillEffectData GetEffectData(SkillData data)
        {
            if (data == null || _effectDataField == null) return null;
            var value = _effectDataField.GetValue(data);
            return value as SkillEffectData;
        }

        #region DoT 관련 확장 메서드

        // 도트 데미지 스킬인지 확인
        public static bool IsDotSkill(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _isDotField == null) return false;
            var value = _isDotField.GetValue(effectData);
            return value is bool && (bool)value;
        }

        // 도트 지속 시간
        public static float GetDotDuration(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _dotDurationField == null) return 5f; // 기본값
            var value = _dotDurationField.GetValue(effectData);
            return value is float f ? f : 5f;
        }

        // 틱당 데미지
        public static float GetDotDamagePerTick(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _dotDamagePerTickField == null) return 10f; // 기본값
            var value = _dotDamagePerTickField.GetValue(effectData);
            return value is float f ? f : 10f;
        }

        // 틱 간격
        public static float GetDotTickInterval(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _dotTickIntervalField == null) return 1f; // 기본값
            var value = _dotTickIntervalField.GetValue(effectData);
            return value is float f ? f : 1f;
        }

        // 체력 비례 데미지 여부
        public static bool GetDotIsPercentage(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _dotIsPercentageField == null) return false;
            var value = _dotIsPercentageField.GetValue(effectData);
            return value is bool && (bool)value;
        }

        #endregion

        #region CC 관련 확장 메서드

        // 기절 스킬인지 확인
        public static bool IsStunSkill(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _isStunField == null) return false;
            var value = _isStunField.GetValue(effectData);
            return value is bool && (bool)value;
        }

        // 기절 지속 시간
        public static float GetStunDuration(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _stunDurationField == null) return 1f; // 기본값
            var value = _stunDurationField.GetValue(effectData);
            return value is float f ? f : 1f;
        }

        // 빙결 스킬인지 확인
        public static bool IsFreezeSkill(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _isFreezeField == null) return false;
            var value = _isFreezeField.GetValue(effectData);
            return value is bool && (bool)value;
        }

        // 빙결 지속 시간
        public static float GetFreezeDuration(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _freezeDurationField == null) return 2f; // 기본값
            var value = _freezeDurationField.GetValue(effectData);
            return value is float f ? f : 2f;
        }

        // 속박 스킬인지 확인
        public static bool IsRootSkill(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _isRootField == null) return false;
            var value = _isRootField.GetValue(effectData);
            return value is bool && (bool)value;
        }

        // 속박 지속 시간
        public static float GetRootDuration(this SkillData data)
        {
            var effectData = GetEffectData(data);
            if (effectData == null || _rootDurationField == null) return 1.5f; // 기본값
            var value = _rootDurationField.GetValue(effectData);
            return value is float f ? f : 1.5f;
        }

        #endregion

        #region Level-related 확장 메서드

        // 최대 레벨
        public static int GetMaxLevel(this SkillData data)
        {
            if (data == null || _maxLevelField == null) return 1; // 기본값
            var value = _maxLevelField.GetValue(data);
            return value is int i ? i : 1;
        }

        // 소비 MP
        public static int GetNeedMp(this SkillData data)
        {
            if (data == null || _needMpField == null) return 0; // 기본값
            var value = _needMpField.GetValue(data);
            return value is int i ? i : 0;
        }

        // 기본 수치 (%)
        public static int GetInitialRate(this SkillData data)
        {
            if (data == null || _initialRateField == null) return 100; // 기본값
            var value = _initialRateField.GetValue(data);
            return value is int i ? i : 100;
        }

        // 레벨업 수치 증가
        public static float GetLevelUpValue(this SkillData data)
        {
            if (data == null || _levelUpValueField == null) return 0f; // 기본값
            var value = _levelUpValueField.GetValue(data);
            return value is float f ? f : 0f;
        }

        #endregion
    }
}
