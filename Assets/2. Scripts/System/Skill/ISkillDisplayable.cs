using UnityEngine;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// 스킬 표시용 인터페이스
    /// - ActiveSkill과 PassiveSkill 모두 UI에 표시 가능하게 함
    /// - 작성자: 조민희
    /// </summary>
    public interface ISkillDisplayable
    {
        /// <summary>
        /// 스킬 ID
        /// </summary>
        string SkillId { get; }

        /// <summary>
        /// 스킬 데이터
        /// </summary>
        SkillData Data { get; }

        /// <summary>
        /// 스킬 활성화 여부
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// UI 표시용 텍스트 반환
        /// - Active: 쿨타임(초) 또는 남은 공격 횟수
        /// - Passive: 상태 텍스트 (ON/OFF, 스택 수 등)
        /// </summary>
        string GetDisplayText();

        /// <summary>
        /// UI 표시용 색상 반환
        /// - 상태에 따른 색상 구분
        /// </summary>
        Color GetDisplayColor();
    }
}
