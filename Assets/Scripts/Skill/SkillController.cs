using UnityEngine;
using System.Collections.Generic;

namespace SlayerLegend.Skill
{
    // 스킬 컨트롤러: 플레이어의 모든 스킬을 관리
    // - 액티브/패시브 스킬 목록 관리
    // - 스킬 장착/제거, 생성
    // - 게임 시작 시 자동 활성화
    public class SkillController : MonoBehaviour
    {
        [Header("장착된 스킬")]
        [SerializeField] private List<ActiveSkill> activeSkills = new List<ActiveSkill>();
        [SerializeField] private List<PassiveSkill> passiveSkills = new List<PassiveSkill>();

        public IReadOnlyList<ActiveSkill> ActiveSkills => activeSkills;
        public IReadOnlyList<PassiveSkill> PassiveSkills => passiveSkills;

        private const int MAX_ACTIVE_SKILLS = 5;

        private void Awake()
        {
            // DataManager에서 스킬 데이터 로드 확인
            if (DataManager.Instance == null || DataManager.Instance.skills == null)
                Debug.LogWarning("DataManager가 초기화되지 않았습니다!");
        }

        private void Start()
        {
            ActivateAllActives();
            ActivateAllPassives();
        }

        private void ActivateAllActives()
        {
            foreach (var active in activeSkills)
            {
                if (active != null)
                    active.SetActive(true);
            }
        }

        private void ActivateAllPassives()
        {
            foreach (var passive in passiveSkills)
            {
                if (passive != null)
                    passive.Activate();
            }
        }

        // 액티브 스킬 장착
        public bool AddActiveSkill(ActiveSkill skill)
        {
            if (activeSkills.Count >= MAX_ACTIVE_SKILLS)
            {
                Debug.Log("더 이상 액티브 스킬을 장착할 수 없습니다.");
                return false;
            }

            if (activeSkills.Contains(skill))
            {
                Debug.Log($"{skill.Data.name}은(는) 이미 장착되어 있습니다.");
                return false;
            }

            activeSkills.Add(skill);
            skill.SetActive(true);
            skill.transform.SetParent(transform);
            Debug.Log($"{skill.Data.name} 스킬 장착 완료!");
            return true;
        }

        // 패시브 스킬 장착
        public bool AddPassiveSkill(PassiveSkill skill)
        {
            if (passiveSkills.Contains(skill))
            {
                Debug.Log($"{skill.Data.name}은(는) 이미 장착되어 있습니다.");
                return false;
            }

            passiveSkills.Add(skill);
            skill.Activate();
            skill.transform.SetParent(transform);
            Debug.Log($"{skill.Data.name} 패시브 장착 완료!");
            return true;
        }

        // 액티브 스킬 제거
        public bool RemoveActiveSkill(string skillId)
        {
            var skill = activeSkills.Find(s => s.Data.id == skillId);
            if (skill != null)
            {
                skill.SetActive(false);
                activeSkills.Remove(skill);
                Debug.Log($"{skill.Data.name} 스킬 장착 해제");
                return true;
            }
            return false;
        }

        // 패시브 스킬 제거
        public bool RemovePassiveSkill(string skillId)
        {
            var skill = passiveSkills.Find(s => s.Data.id == skillId);
            if (skill != null)
            {
                skill.Deactivate();
                passiveSkills.Remove(skill);
                Debug.Log($"{skill.Data.name} 패시브 장착 해제");
                return true;
            }
            return false;
        }

        // 액티브 스킬 생성
        public ActiveSkill CreateActiveSkill(SkillData data)
        {
            if (data.type != SkillType.Active)
            {
                Debug.LogWarning($"{data.name}은(는) 액티브 스킬이 아닙니다.");
                return null;
            }

            var skillObj = new GameObject($"ActiveSkill_{data.name}");
            var skill = skillObj.AddComponent<ActiveSkill>();
            skill.Initialize(data);
            return skill;
        }

        // 패시브 스킬 생성
        public PassiveSkill CreatePassiveSkill(SkillData data)
        {
            if (data.type != SkillType.Passive)
            {
                Debug.LogWarning($"{data.name}은(는) 패시브 스킬이 아닙니다.");
                return null;
            }

            var skillObj = new GameObject($"PassiveSkill_{data.name}");
            var skill = skillObj.AddComponent<PassiveSkill>();
            skill.Initialize(data);
            return skill;
        }
    }
}
