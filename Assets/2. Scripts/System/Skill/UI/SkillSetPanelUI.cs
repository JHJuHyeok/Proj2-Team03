using UnityEngine;
using System.Collections.Generic;
using SlayerLegend.Skill;
using SlayerLegend.Skill.UI.Grid;
using SlayerLegend.Resource;

namespace SlayerLegend.UI
{
    /// <summary>
    /// 스킬 셋 패널 UI
    /// - 그리드에 배치된 스킬들의 아이콘과 쿨타임/공격횟수 표시
    /// - 큐 기반: 배치 순서대로 Button 1, 2, 3...에 표시
    /// - 작성자: 조민희
    /// </summary>
    public class SkillSetPanelUI : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private SkillController skillController;
        [SerializeField] private SkillGridController skillGridController;

        [Header("버튼 목록 (에디터에서 할당)")]
        [SerializeField] private List<SkillSetButtonUI> skillButtons = new List<SkillSetButtonUI>();

        // 배치 순서대로 스킬 ID를 저장하는 큐
        private List<string> placedSkillQueue = new List<string>();

        // 스킬 ID → ActiveSkill 매핑 (빠른 조회용)
        private Dictionary<string, ActiveSkill> skillMap = new Dictionary<string, ActiveSkill>();

        private void Start()
        {
            // SkillController 찾기
            if (skillController == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    skillController = player.GetComponent<SkillController>();
            }

            // SkillGridController 찾기 (자동)
            if (skillGridController == null)
            {
                skillGridController = FindObjectOfType<SkillGridController>();
                if (skillGridController != null)
                {
                    Debug.Log("[SkillSetPanelUI] SkillGridController 자동 연결됨");
                }
            }

            // 이벤트 구독
            if (skillGridController != null)
            {
                skillGridController.OnSkillPlaced += HandleSkillPlaced;
                skillGridController.OnSkillRemoved += HandleSkillRemoved;
                Debug.Log("[SkillSetPanelUI] 이벤트 구독 완료");
            }
            else
            {
                Debug.LogWarning("[SkillSetPanelUI] SkillGridController를 찾을 수 없습니다!");
            }

            // 초기화 지연 (GameSkillInitializer가 스킬을 로드할 시간 확보)
            StartCoroutine(DelayedRefresh());
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (skillGridController != null)
            {
                skillGridController.OnSkillPlaced -= HandleSkillPlaced;
                skillGridController.OnSkillRemoved -= HandleSkillRemoved;
            }
        }

        /// <summary>
        /// 스킬 배치 이벤트 핸들러
        /// </summary>
        private void HandleSkillPlaced(string skillId, SkillData skillData)
        {
            // 이미 큐에 있으면 무시
            if (placedSkillQueue.Contains(skillId))
            {
                return;
            }

            // SkillController에서 ActiveSkill 찾기
            ActiveSkill activeSkill = FindActiveSkill(skillId);
            if (activeSkill == null)
            {
                Debug.LogWarning($"[SkillSetPanelUI] ActiveSkill을 찾을 수 없음: {skillId}");
                return;
            }

            // 큐에 추가
            placedSkillQueue.Add(skillId);
            skillMap[skillId] = activeSkill;

            // 버튼 전체 갱신
            RefreshButtonsFromQueue();
        }

        /// <summary>
        /// 스킬 제거 이벤트 핸들러
        /// </summary>
        private void HandleSkillRemoved(string skillId, SkillData skillData)
        {
            // 큐에서 제거
            if (placedSkillQueue.Remove(skillId))
            {
                skillMap.Remove(skillId);

                // 버튼 전체 갱신 (shift 효과)
                RefreshButtonsFromQueue();
            }
        }

        /// <summary>
        /// 큐 순서대로 버튼 갱신
        /// </summary>
        private void RefreshButtonsFromQueue()
        {
            // 모든 버튼 초기화
            foreach (var button in skillButtons)
            {
                button?.ClearSkill();
            }

            // 큐 순서대로 버튼에 할당
            for (int i = 0; i < placedSkillQueue.Count && i < skillButtons.Count; i++)
            {
                string skillId = placedSkillQueue[i];
                var button = skillButtons[i];

                if (button != null && skillMap.TryGetValue(skillId, out var activeSkill))
                {
                    Sprite icon = ResourceManager.Instance?.LoadSprite(activeSkill.Data.spriteName);
                    button.SetSkill(activeSkill, icon);
                }
            }
        }

        /// <summary>
        /// ActiveSkill 찾기
        /// </summary>
        private ActiveSkill FindActiveSkill(string skillId)
        {
            // 캐시에서 먼저 찾기
            if (skillMap.TryGetValue(skillId, out var cached))
            {
                return cached;
            }

            // SkillController에서 찾기
            if (skillController != null)
            {
                foreach (var skill in skillController.ActiveSkills)
                {
                    if (skill != null && skill.Data != null && skill.Data.id == skillId)
                    {
                        return skill;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 초기화: 저장된 그리드 데이터에서 큐 복원
        /// </summary>
        private System.Collections.IEnumerator DelayedRefresh(float delay = 1f)
        {
            yield return new WaitForSeconds(delay);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // SkillController 대기
            float waitTime = 0f;
            while (skillController == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    skillController = player.GetComponent<SkillController>();

                waitTime += 0.1f;
                if (waitTime > 3f)
                {
                    Debug.LogWarning("[SkillSetPanelUI] SkillController를 찾지 못했습니다");
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }

            // 배치된 스킬들을 큐에 로드
            LoadPlacedSkillsToQueue();
        }

        /// <summary>
        /// 그리드에 배치된 스킬들을 큐에 로드
        /// </summary>
        private void LoadPlacedSkillsToQueue()
        {
            if (skillController == null) return;

            placedSkillQueue.Clear();
            skillMap.Clear();

            // SkillController의 ActiveSkills에서 배치된 스킬들 가져오기
            foreach (var skill in skillController.ActiveSkills)
            {
                if (skill != null && skill.Data != null)
                {
                    placedSkillQueue.Add(skill.Data.id);
                    skillMap[skill.Data.id] = skill;
                }
            }

            // 버튼 갱신
            RefreshButtonsFromQueue();
        }

        private void Update()
        {
            // 모든 버튼 UI 갱신
            foreach (var button in skillButtons)
            {
                if (button != null && button.HasSkill())
                {
                    button.UpdateDisplay();
                }
            }
        }

        /// <summary>
        /// 전체 새로고침 (외부 호출용)
        /// </summary>
        public void RefreshSkillDisplay()
        {
            LoadPlacedSkillsToQueue();
        }

        /// <summary>
        /// SkillController 설정
        /// </summary>
        public void SetSkillController(SkillController controller)
        {
            skillController = controller;
            LoadPlacedSkillsToQueue();
        }
    }
}
