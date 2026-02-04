﻿using System;
using UnityEngine;

namespace SlayerLegend.Skill.StatusEffects
{
    /// <summary>
    /// 도트 데미지 데이터를 JSON에서 읽어오기 위한 확장 클래스
    /// 팀원의 SkillData를 수정하지 않고 리플렉션으로 도트 필드를 읽음
    /// </summary>
    public static class SkillDataExtensions
    {
        // 리플렉션 캐시 (성능 최적화)
        private static readonly System.Reflection.FieldInfo _isDotField;
        private static readonly System.Reflection.FieldInfo _dotDurationField;
        private static readonly System.Reflection.FieldInfo _dotDamagePerTickField;
        private static readonly System.Reflection.FieldInfo _dotTickIntervalField;
        private static readonly System.Reflection.FieldInfo _dotIsPercentageField;

        // CC 관련 필드
        private static readonly System.Reflection.FieldInfo _isStunField;
        private static readonly System.Reflection.FieldInfo _stunDurationField;
        private static readonly System.Reflection.FieldInfo _isFreezeField;
        private static readonly System.Reflection.FieldInfo _freezeDurationField;
        private static readonly System.Reflection.FieldInfo _isRootField;
        private static readonly System.Reflection.FieldInfo _rootDurationField;

        static SkillDataExtensions()
        {
            var skillDataType = typeof(SkillData);

            // DoT 필드 초기화
            _isDotField = skillDataType.GetField("isDot",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _dotDurationField = skillDataType.GetField("dotDuration",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _dotDamagePerTickField = skillDataType.GetField("dotDamagePerTick",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _dotTickIntervalField = skillDataType.GetField("dotTickInterval",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _dotIsPercentageField = skillDataType.GetField("dotIsPercentage",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

            // CC 필드 초기화
            _isStunField = skillDataType.GetField("isStun",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _stunDurationField = skillDataType.GetField("stunDuration",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _isFreezeField = skillDataType.GetField("isFreeze",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _freezeDurationField = skillDataType.GetField("freezeDuration",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _isRootField = skillDataType.GetField("isRoot",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            _rootDurationField = skillDataType.GetField("rootDuration",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);

            // 필드 초기화 실패 시 경고 로그
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_isDotField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isDot' field not found in SkillData. JSON data may be missing.");
            if (_dotIsPercentageField == null)
                Debug.LogWarning("[SkillDataExtensions] 'dotIsPercentage' field not found in SkillData. JSON data may be missing.");
            if (_isStunField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isStun' field not found in SkillData. JSON data may be missing.");
            if (_isFreezeField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isFreeze' field not found in SkillData. JSON data may be missing.");
            if (_isRootField == null)
                Debug.LogWarning("[SkillDataExtensions] 'isRoot' field not found in SkillData. JSON data may be missing.");
            #endif
        }

        /// <summary>
        /// 도트 데미지 스킬인지 확인
        /// </summary>
        public static bool IsDotSkill(this SkillData data)
        {
            if (data == null || _isDotField == null) return false;
            var value = _isDotField.GetValue(data);
            return value is bool && (bool)value;
        }

        /// <summary>
        /// 도트 지속 시간
        /// </summary>
        public static float GetDotDuration(this SkillData data)
        {
            if (data == null || _dotDurationField == null) return 5f; // 기본값
            var value = _dotDurationField.GetValue(data);
            return value is float f ? f : 5f;
        }

        /// <summary>
        /// 틱당 데미지
        /// </summary>
        public static float GetDotDamagePerTick(this SkillData data)
        {
            if (data == null || _dotDamagePerTickField == null) return 10f; // 기본값
            var value = _dotDamagePerTickField.GetValue(data);
            return value is float f ? f : 10f;
        }

        /// <summary>
        /// 틱 간격
        /// </summary>
        public static float GetDotTickInterval(this SkillData data)
        {
            if (data == null || _dotTickIntervalField == null) return 1f; // 기본값
            var value = _dotTickIntervalField.GetValue(data);
            return value is float f ? f : 1f;
        }

        /// <summary>
        /// 체력 비례 데미지 여부
        /// </summary>
        public static bool GetDotIsPercentage(this SkillData data)
        {
            if (data == null || _dotIsPercentageField == null) return false;
            var value = _dotIsPercentageField.GetValue(data);
            return value is bool && (bool)value;
        }

        #region CC 관련 확장 메서드

        /// <summary>
        /// 기절 스킬인지 확인
        /// </summary>
        public static bool IsStunSkill(this SkillData data)
        {
            if (data == null || _isStunField == null) return false;
            var value = _isStunField.GetValue(data);
            return value is bool && (bool)value;
        }

        /// <summary>
        /// 기절 지속 시간
        /// </summary>
        public static float GetStunDuration(this SkillData data)
        {
            if (data == null || _stunDurationField == null) return 1f; // 기본값
            var value = _stunDurationField.GetValue(data);
            return value is float f ? f : 1f;
        }

        /// <summary>
        /// 빙결 스킬인지 확인
        /// </summary>
        public static bool IsFreezeSkill(this SkillData data)
        {
            if (data == null || _isFreezeField == null) return false;
            var value = _isFreezeField.GetValue(data);
            return value is bool && (bool)value;
        }

        /// <summary>
        /// 빙결 지속 시간
        /// </summary>
        public static float GetFreezeDuration(this SkillData data)
        {
            if (data == null || _freezeDurationField == null) return 2f; // 기본값
            var value = _freezeDurationField.GetValue(data);
            return value is float f ? f : 2f;
        }

        /// <summary>
        /// 속박 스킬인지 확인
        /// </summary>
        public static bool IsRootSkill(this SkillData data)
        {
            if (data == null || _isRootField == null) return false;
            var value = _isRootField.GetValue(data);
            return value is bool && (bool)value;
        }

        /// <summary>
        /// 속박 지속 시간
        /// </summary>
        public static float GetRootDuration(this SkillData data)
        {
            if (data == null || _rootDurationField == null) return 1.5f; // 기본값
            var value = _rootDurationField.GetValue(data);
            return value is float f ? f : 1.5f;
        }

        #endregion
    }
}
