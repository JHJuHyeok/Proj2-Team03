using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Data
{
    // 전체 스킬 데이터베이스 (ScriptableObject)
    // - 모든 스킬을 중앙 관리
    // - ID로 스킬 검색, 타입별 필터링
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "SlayerLegend/Skill Database")]
    public class SkillDatabase : ScriptableObject
    {
        public List<SkillData> skills;

        // ID로 스킬 찾기
        public SkillData GetSkillById(string skillId)
        {
            foreach (var skill in skills)
            {
                if (skill.skillId == skillId)
                    return skill;
            }
            Debug.LogWarning($"스킬 ID {skillId}를 찾을 수 없습니다.");
            return null;
        }

        // 타입별 스킬 목록 가져오기
        public List<SkillData> GetSkillsByType(SkillType type)
        {
            List<SkillData> result = new List<SkillData>();
            foreach (var skill in skills)
            {
                if (skill.skillType == type)
                    result.Add(skill);
            }
            return result;
        }

        // 에디터용: 중복 ID 체크
        public void OnValidate()
        {
            HashSet<string> ids = new HashSet<string>();
            foreach (var skill in skills)
            {
                if (skill != null && ids.Contains(skill.skillId))
                {
                    Debug.LogWarning($"중복된 스킬 ID 발견: {skill.skillId}");
                }
                ids.Add(skill.skillId);
            }
        }
    }
}
