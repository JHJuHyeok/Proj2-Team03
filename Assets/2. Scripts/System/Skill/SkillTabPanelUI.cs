using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill
{
    /// <summary>
    /// Skill Tab Panel 관리자
    /// 작성자: 조민희
    /// Skill Bar별로 4개 속성의 스킬을 표시
    /// Skill Bar 1: Fire_01, Water_01, Wind_01, Earth_01
    /// Skill Bar 2: Fire_02, Water_02, Wind_02, Earth_02
    /// ...
    /// </summary>
    public class SkillTabPanelUI : MonoBehaviour
    {
        [Header("Skill Bar 목록 (각 Bar의 자식 SkillBundleUI 사용)")]
        [SerializeField] private Transform[] skillBars; // Skill Bar 1~11

        [Header("참조")]
        [SerializeField] private bool autoRefreshOnEnable = true;

        // 속성 순서 (Bar 내 Bundle 순서)
        private static readonly string[] ElementOrder = { "Fire", "Water", "Wind", "Earth" };

        #region Unity 라이프사이클

        private void OnEnable()
        {
            if (autoRefreshOnEnable)
            {
                StartCoroutine(RefreshWhenDataReady());
            }
        }

        #endregion

        #region 데이터 대기 코루틴

        private System.Collections.IEnumerator RefreshWhenDataReady()
        {
            // DataManager 초기화 대기 (최대 100프레임)
            int waitCount = 0;
            while (DataManager.skills.GetAll() == null && waitCount < 100)
            {
                yield return null;
                waitCount++;
            }

            if (DataManager.skills.GetAll() == null)
            {
                Debug.LogWarning("[SkillTabPanelUI] DataManager.skills가 null - 빈 상태로 표시");
            }

            RefreshSkillList();
        }

        #endregion

        #region 스킬 목록 새로고침

        /// <summary>
        /// 스킬 목록 새로고침 - 기존 배치된 번들에 데이터만 설정
        /// </summary>
        public void RefreshSkillList()
        {
            if (skillBars == null || skillBars.Length == 0)
            {
                Debug.LogWarning("[SkillTabPanelUI] skillBars가 설정되지 않음");
                return;
            }

            // 각 Skill Bar에 스킬 데이터 설정
            for (int barIndex = 0; barIndex < skillBars.Length; barIndex++)
            {
                FillSkillBar(skillBars[barIndex], barIndex + 1); // 스킬 번호는 1부터 시작
            }

            Debug.Log("[SkillTabPanelUI] 스킬 목록 새로고침 완료");
        }

        /// <summary>
        /// 특정 Skill Bar에 있는 번들들에 데이터 채우기
        /// </summary>
        /// <param name="skillBar">Skill Bar Transform</param>
        /// <param name="skillNumber">스킬 번호 (1~11)</param>
        private void FillSkillBar(Transform skillBar, int skillNumber)
        {
            if (skillBar == null)
            {
                Debug.LogWarning($"[SkillTabPanelUI] Skill Bar {skillNumber}가 null");
                return;
            }

            // 해당 Bar의 모든 SkillBundleUI 가져오기
            SkillBundleUI[] bundles = skillBar.GetComponentsInChildren<SkillBundleUI>(true);

            if (bundles == null || bundles.Length == 0)
            {
                Debug.LogWarning($"[SkillTabPanelUI] Skill Bar {skillNumber}에 SkillBundleUI가 없음");
                return;
            }

            // sibling index 역순으로 정렬 (높은 index가 먼저 오도록 = 왼쪽→오른쪽)
            System.Array.Sort(bundles, (a, b) => b.transform.GetSiblingIndex().CompareTo(a.transform.GetSiblingIndex()));

            // 각 번들에 해당 속성의 스킬 설정
            for (int i = 0; i < bundles.Length; i++)
            {
                SkillBundleUI bundle = bundles[i];

                if (i < ElementOrder.Length)
                {
                    string element = ElementOrder[i];
                    string skillId = $"{element}_{skillNumber:D2}"; // 예: Fire_01, Water_02, ...

                    SkillData skillData = GetSkillById(skillId);

                    if (skillData != null)
                    {
                        // TODO: 실제 레벨 데이터 연동 필요 (임시로 1 레벨, 최대 4단계 사용)
                        int level = 1;
                        int maxStep = 4;  // 스킬 등급 단계 (1~4)

                        bundle.SetSkillData(skillData, level, maxStep);
                        bundle.gameObject.SetActive(true);

                        Debug.Log($"[SkillTabPanelUI] Bar {skillNumber} Bundle {i} 설정: {skillId}, spriteName={skillData.spriteName}");
                    }
                    else
                    {
                        bundle.gameObject.SetActive(false);
                        Debug.LogWarning($"[SkillTabPanelUI] 스킬을 찾을 수 없음: {skillId}");
                    }
                }
                else
                {
                    // 4개 번들 이후는 비활성화
                    bundle.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// ID로 스킬 데이터 가져오기
        /// </summary>
        private SkillData GetSkillById(string skillId)
        {
            var allSkills = DataManager.skills.GetAll();
            if (allSkills == null) return null;

            foreach (var skill in allSkills)
            {
                if (skill.id == skillId)
                {
                    return skill;
                }
            }

            return null;
        }

        #endregion

        #region 디버그

        [ContextMenu("새로고침")]
        public void DebugRefresh()
        {
            RefreshSkillList();
        }

        [ContextMenu("스킬 Bar 개수 출력")]
        public void DebugPrintBarCounts()
        {
            if (skillBars == null)
            {
                Debug.Log("skillBars가 null");
                return;
            }

            Debug.Log($"=== Skill Bar 개수: {skillBars.Length} ===");
            for (int i = 0; i < skillBars.Length; i++)
            {
                if (skillBars[i] != null)
                {
                    int bundleCount = skillBars[i].GetComponentsInChildren<SkillBundleUI>(true).Length;
                    Debug.Log($"  Bar {i + 1}: {skillBars[i].name}, Bundle {bundleCount}개");
                }
                else
                {
                    Debug.Log($"  Bar {i + 1}: null");
                }
            }
        }

        #endregion
    }
}
