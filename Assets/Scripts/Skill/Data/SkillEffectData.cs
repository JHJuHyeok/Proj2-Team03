using UnityEngine;

namespace SlayerLegend.Skill.Data
{
    // 스킬 효과 데이터
    // SkillData에서 분리된 스킬 레벨, DoT, CC 관련 데이터
    // SkillData.cs를 간소화하기 위해 별도 클래스로 분리
    [System.Serializable]
    public class SkillEffectData
    {
        [Header("스킬 레벨 정보")]
        [Tooltip("최대 레벨")]
        public int maxLevel = 1;

        [Tooltip("소비 MP")]
        public int needMp = 0;

        [Tooltip("기본 수치 (%)")]
        public int initialRate = 100;

        [Tooltip("레벨업 수치 증가")]
        public float levelUpValue = 0f;

        [Header("DoT (Damage over Time)")]
        [Tooltip("도트 데미지 여부")]
        public bool isDot = false;

        [Tooltip("도트 지속 시간 (초)")]
        public float dotDuration = 5f;

        [Tooltip("틱당 데미지")]
        public float dotDamagePerTick = 10f;

        [Tooltip("틱 간격 (초)")]
        public float dotTickInterval = 1f;

        [Tooltip("체력 비례 여부 (true면 % 데미지)")]
        public bool dotIsPercentage = false;

        [Header("CC (Crowd Control)")]
        [Tooltip("기절 여부")]
        public bool isStun = false;

        [Tooltip("기절 지속 시간 (초)")]
        public float stunDuration = 1f;

        [Tooltip("빙결 여부")]
        public bool isFreeze = false;

        [Tooltip("빙결 지속 시간 (초)")]
        public float freezeDuration = 2f;

        [Tooltip("속박 여부")]
        public bool isRoot = false;

        [Tooltip("속박 지속 시간 (초)")]
        public float rootDuration = 1.5f;

        #region 유틸리티 메서드

        // DoT 스킬인지 확인
        public bool IsDotSkill => isDot;

        // CC 스킬인지 확인 (하나라도 true면)
        public bool IsCcSkill => isStun || isFreeze || isRoot;

        // CC 타입 확인 (우선순위: Stun > Freeze > Root)
        public CcType GetCcType()
        {
            if (isStun) return CcType.Stun;
            if (isFreeze) return CcType.Freeze;
            if (isRoot) return CcType.Root;
            return CcType.None;
        }

        // CC 지속 시간 가져오기
        public float GetCcDuration()
        {
            switch (GetCcType())
            {
                case CcType.Stun: return stunDuration;
                case CcType.Freeze: return freezeDuration;
                case CcType.Root: return rootDuration;
                default: return 0f;
            }
        }

        // 데이터 요약 로그
        public void LogSummary()
        {
            UnityEngine.Debug.Log($"=== 스킬 효과 데이터 ===");
            UnityEngine.Debug.Log($"레벨: Lv.{maxLevel} | MP:{needMp} | 기본:{initialRate}% | 성장:{levelUpValue}");
            UnityEngine.Debug.Log($"DoT: {(isDot ? $"활성화 ({dotDuration}s)" : "비활성화")}");
            UnityEngine.Debug.Log($"CC: {GetCcType()} ({GetCcDuration()}s)");
        }

        #endregion
    }

    public enum CcType
    {
        None,
        Stun,
        Freeze,
        Root
    }
}
