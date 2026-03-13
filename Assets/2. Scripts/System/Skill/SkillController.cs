using UnityEngine;
using System.Collections.Generic;

// [주혁] -DataManager 정적 클래스 전환에 의해 코드 수정(26)

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
            if (DataManager.skills == null)
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
                return false;
            }

            if (activeSkills.Contains(skill))
            {
                return false;
            }

            activeSkills.Add(skill);
            skill.SetActive(true);
            skill.transform.SetParent(transform);
            return true;
        }

        // 패시브 스킬 장착
        public bool AddPassiveSkill(PassiveSkill skill)
        {
            if (passiveSkills.Contains(skill))
            {
                return false;
            }

            passiveSkills.Add(skill);
            skill.Activate();
            skill.transform.SetParent(transform);
            return true;
        }

        // 조민희 추가: 공격 시 모든 스킬에 알림 (AttackCount 발동용)
        public void NotifyAttackToAllSkills()
        {
            // 패시브 스킬에 알림 (누적형 버프)
            foreach (var passive in passiveSkills)
            {
                passive?.OnAttack();
            }

            // 액티브 스킬에 알림 (AttackCount 발동 조건)
            foreach (var active in activeSkills)
            {
                active?.OnAttack();
            }
        }

        // 액티브 스킬 제거
        public bool RemoveActiveSkill(string skillId)
        {
            var skill = activeSkills.Find(s => s.Data.id == skillId);
            if (skill != null)
            {
                skill.SetActive(false);
                activeSkills.Remove(skill);
                Destroy(skill.gameObject);  // [조민희] 프리셋 전환 시 중복 생성 방지용 Destroy 추가
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
                Destroy(skill.gameObject);  // [조민희] 프리셋 전환 시 중복 생성 방지용 Destroy 추가
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

        // [조민희] 스킬 레벨 갱신 (외부에서 강화했을 때 호출)
        public void RefreshSkillLevel(string skillId)
        {
            // 액티브 스킬에서 해당 ID 찾기
            var activeSkill = activeSkills.Find(s => s.Data != null && s.Data.id == skillId);
            if (activeSkill != null)
            {
                activeSkill.RefreshLevel();
                return;
            }

            // 패시브 스킬에서 해당 ID 찾기
            var passiveSkill = passiveSkills.Find(s => s.Data != null && s.Data.id == skillId);
            if (passiveSkill != null)
            {
                passiveSkill.RefreshLevel();
            }
        }

        #region 스킬 보유 관리 (상점 소환 연동용)

        /// <summary>스킬 획득 이벤트</summary>
        public event System.Action<string> OnSkillAcquired;

        /// <summary>스킬 제거 이벤트</summary>
        public event System.Action<string> OnSkillRemoved;

        /// <summary>
        /// 스킬 획득 (상점 소환 등에서 사용)
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <param name="count">획득 개수 (기본값 1)</param>
        /// <param name="level">스킬 레벨 (기본값 1)</param>
        public void AddSkill(string skillId, int count = 1, int level = 1)
        {
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogWarning("[SkillController] 스킬 ID가 null 또는 비어있습니다.");
                return;
            }

            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogWarning("[SkillController] CurrentSaveData가 null입니다.");
                return;
            }

            var skillInfo = DataManager.CurrentSaveData.skillInfo;

            if (!skillInfo.ContainsKey(skillId))
            {
                skillInfo[skillId] = new Possesion { count = 0, level = level };
            }

            skillInfo[skillId].count += count;
            skillInfo[skillId].level = System.Math.Max(skillInfo[skillId].level, level);

            OnSkillAcquired?.Invoke(skillId);
        }

        /// <summary>
        /// 스킬 제거
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <param name="count">제거 개수 (기본값 1)</param>
        /// <returns>제거 성공 여부</returns>
        public bool RemoveSkill(string skillId, int count = 1)
        {
            if (string.IsNullOrEmpty(skillId)) return false;

            if (DataManager.CurrentSaveData == null) return false;

            var skillInfo = DataManager.CurrentSaveData.skillInfo;

            if (!skillInfo.ContainsKey(skillId)) return false;

            skillInfo[skillId].count -= count;

            if (skillInfo[skillId].count <= 0)
            {
                skillInfo.Remove(skillId);
            }

            OnSkillRemoved?.Invoke(skillId);
            return true;
        }

        /// <summary>
        /// 스킬 보유 개수 확인
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>보유 개수 (없으면 0)</returns>
        public int GetSkillCount(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return 0;
            if (DataManager.CurrentSaveData == null) return 0;

            var skillInfo = DataManager.CurrentSaveData.skillInfo;

            if (skillInfo.TryGetValue(skillId, out var possession))
            {
                return possession.count;
            }

            return 0;
        }

        /// <summary>
        /// 스킬 보유 여부 확인
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>보유 여부</returns>
        public bool HasSkill(string skillId)
        {
            return GetSkillCount(skillId) > 0;
        }

        #endregion
    }
}
