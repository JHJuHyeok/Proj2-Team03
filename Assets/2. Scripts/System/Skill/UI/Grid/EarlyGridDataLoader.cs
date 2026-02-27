using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SlayerLegend.Skill;
using SlayerLegend.Skill.Data;

namespace SlayerLegend.Skill.UI.Grid
{
    /// <summary>
    /// 게임 시작 시점에 저장된 그리드 데이터를 로드하는 컴포넌트
    /// - SkillGridController가 활성화되기 전에 스킬을 SkillController에 등록
    /// - SkillSetPanelUI가 스킬을 표시할 수 있도록 이벤트 발생
    /// 작성자: 조민희
    /// </summary>
    public class EarlyGridDataLoader : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private SkillController skillController;

        [Header("설정")]
        [SerializeField] private string saveKey = "SkillGridData";
        [SerializeField] private bool loadOnStart = true;

        // 저장된 스킬 ID 목록
        private List<string> loadedSkillIds = new List<string>();

        // static 플래그: 게임 시작 시 조기 로드 완료 여부 (중복 방지용)
        public static bool IsDataLoadedEarly { get; private set; } = false;

        // 이벤트: 그리드 데이터 로드 완료 시 발생
        public event System.Action<List<string>> OnEarlyGridDataLoaded;

        // 프로퍼티
        public List<string> LoadedSkillIds => loadedSkillIds;

        private IEnumerator Start()
        {
            if (!loadOnStart)
                yield break;

            // SkillController 찾기
            if (skillController == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    skillController = player.GetComponent<SkillController>();
            }

            // DataManager 대기
            int waitCount = 0;
            while (DataManager.skills.Get("Fire_01") == null && waitCount < 100)
            {
                yield return new WaitForSeconds(0.1f);
                waitCount++;
            }

            if (DataManager.skills.Get("Fire_01") == null)
            {
                Debug.LogWarning("[EarlyGridDataLoader] 스킬 데이터 로드 실패");
                yield break;
            }

            // 그리드 데이터 로드
            LoadGridDataEarly();
        }

        /// <summary>
        /// 게임 시작 시 저장된 그리드 데이터 로드
        /// </summary>
        public void LoadGridDataEarly()
        {
            loadedSkillIds.Clear();

            if (!PlayerPrefs.HasKey(saveKey))
            {
                return;
            }

            string json = PlayerPrefs.GetString(saveKey);
            var saveData = SkillGridSaveData.FromJson(json);

            if (saveData == null || saveData.placedSkills.Count == 0)
            {
                return;
            }

            // 각 배치된 스킬을 SkillController에 등록
            int registeredCount = 0;
            foreach (var placedSkill in saveData.placedSkills)
            {
                if (RegisterSkillToController(placedSkill.skillId))
                {
                    loadedSkillIds.Add(placedSkill.skillId);
                    registeredCount++;
                }
            }

            if (registeredCount > 0)
            {
                // static 플래그 설정 (중복 로드 방지)
                IsDataLoadedEarly = true;

                // 이벤트 발생 (SkillSetPanelUI에서 구독)
                OnEarlyGridDataLoaded?.Invoke(loadedSkillIds);
            }
        }

        /// <summary>
        /// 스킬을 SkillController에 등록
        /// </summary>
        private bool RegisterSkillToController(string skillId)
        {
            if (skillController == null)
            {
                Debug.LogWarning("[EarlyGridDataLoader] SkillController가 없습니다.");
                return false;
            }

            var skillData = DataManager.skills.Get(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[EarlyGridDataLoader] 스킬 데이터를 찾을 수 없음: {skillId}");
                return false;
            }

            // 이미 등록되어 있는지 확인
            if (IsSkillRegistered(skillId))
            {
                return true;
            }

            // 스킬 타입에 따라 등록
            if (skillData.type == SkillType.Active)
            {
                var skill = skillController.CreateActiveSkill(skillData);
                if (skill != null)
                {
                    skillController.AddActiveSkill(skill);
                    return true;
                }
            }
            else if (skillData.type == SkillType.Passive)
            {
                var skill = skillController.CreatePassiveSkill(skillData);
                if (skill != null)
                {
                    skillController.AddPassiveSkill(skill);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 스킬이 이미 등록되어 있는지 확인
        /// </summary>
        private bool IsSkillRegistered(string skillId)
        {
            if (skillController == null) return false;

            // ActiveSkills 확인
            foreach (var skill in skillController.ActiveSkills)
            {
                if (skill != null && skill.Data != null && skill.Data.id == skillId)
                    return true;
            }

            // PassiveSkills 확인
            foreach (var skill in skillController.PassiveSkills)
            {
                if (skill != null && skill.Data != null && skill.Data.id == skillId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// SkillController 설정
        /// </summary>
        public void SetSkillController(SkillController controller)
        {
            skillController = controller;
        }
    }
}
