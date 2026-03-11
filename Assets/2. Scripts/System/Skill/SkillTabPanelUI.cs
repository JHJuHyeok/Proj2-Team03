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

        // 이벤트 구독 추적용 (중복 구독 방지) - 조민희 추가
        private HashSet<SkillBundleUI> subscribedBundles = new HashSet<SkillBundleUI>();

        #region Unity 라이프사이클

        private void OnEnable()
        {
            if (autoRefreshOnEnable)
            {
                StartCoroutine(RefreshWhenDataReady());
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제 (조민희 추가)
            foreach (var bundle in subscribedBundles)
            {
                if (bundle != null)
                {
                    bundle.OnBundleClicked -= OnBundleClicked;
                }
            }
            subscribedBundles.Clear();
        }

        #endregion

        #region 데이터 대기 코루틴

        private System.Collections.IEnumerator RefreshWhenDataReady()
        {
            // DataManager 초기화 대기 (최대 100프레임)
            int waitCount = 0;
            while ((DataManager.skills.GetAll() == null || DataManager.CurrentSaveData == null) && waitCount < 100)
            {
                yield return null;
                waitCount++;
            }

            if (DataManager.skills.GetAll() == null)
            {
                Debug.LogWarning("[SkillTabPanelUI] DataManager.skills가 null - 빈 상태로 표시");
            }

            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogWarning("[SkillTabPanelUI] CurrentSaveData가 null - 기본 레벨 1 사용");
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
                        // 스킬 보유량 조회 (없으면 0)
                        int ownedCount = GetSkillCount(skillId);
                        int requiredCount = 4;  // 레벨업에 필요한 개수

                        bundle.SetSkillData(skillData, ownedCount, requiredCount);
                        bundle.gameObject.SetActive(true);

                        // 클릭 이벤트 구독 (중복 방지) - 조민희 추가
                        if (!subscribedBundles.Contains(bundle))
                        {
                            bundle.OnBundleClicked += OnBundleClicked;
                            subscribedBundles.Add(bundle);
                        }
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

        /// <summary>
        /// 스킬 보유량 조회 (저장 데이터에서)
        /// </summary>
        /// <param name="skillId">스킬 ID</param>
        /// <returns>보유 개수 (기본값 0)</returns>
        private int GetSkillCount(string skillId)
        {
            if (DataManager.CurrentSaveData == null) return 0;
            if (DataManager.CurrentSaveData.skillInfo == null) return 0;

            if (DataManager.CurrentSaveData.skillInfo.TryGetValue(skillId, out var possession))
            {
                return possession.count;
            }

            return 0; // 보유하지 않은 스킬은 0개
        }

        /// <summary>
        /// 스킬 번들 클릭 시 팝업 열기 (조민희 추가)
        /// </summary>
        private void OnBundleClicked(SkillBundleUI bundle)
        {
            if (bundle == null) return;

            string skillId = bundle.SkillId;
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogWarning("[SkillTabPanelUI] 클릭한 번들의 스킬 ID가 비어있음");
                return;
            }

            // 팝업 매니저를 통해 스킬 상세 팝업 열기
            if (PopupManager.Instance != null)
            {
                // [조민희] PopupId.SkillDetail → PopupId.Skill로 변경 (기존 SkillPopup 프리팹 사용)
                PopupManager.Instance.Open(PopupId.Skill, skillId);
            }
            else
            {
                Debug.LogWarning("[SkillTabPanelUI] PopupManager.Instance가 null");
            }
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

        [ContextMenu("테스트: 스킬 보유 데이터 추가 (랜덤)")]
        public void DebugAddTestSkillData()
        {
            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogError("[SkillTabPanelUI] CurrentSaveData가 null");
                return;
            }

            if (DataManager.CurrentSaveData.skillInfo == null)
            {
                DataManager.CurrentSaveData.skillInfo = new System.Collections.Generic.Dictionary<string, Possesion>();
            }

            // 랜덤하게 일부 스킬만 보유 (Fire 계열 + 일부 랜덤)
            string[] alwaysOwned = { "Fire_01", "Fire_02", "Fire_03", "Water_01", "Wind_01", "Earth_01" };
            var allSkills = DataManager.skills.GetAll();

            foreach (var skill in allSkills)
            {
                bool shouldOwn = false;
                int count = 0;

                // 항상 보유할 스킬들
                foreach (string id in alwaysOwned)
                {
                    if (skill.id == id)
                    {
                        shouldOwn = true;
                        count = UnityEngine.Random.Range(1, 15);
                        break;
                    }
                }

                // 30% 확률로 추가 보유
                if (!shouldOwn && UnityEngine.Random.value < 0.3f)
                {
                    shouldOwn = true;
                    count = UnityEngine.Random.Range(1, 8);
                }

                if (shouldOwn)
                {
                    DataManager.CurrentSaveData.skillInfo[skill.id] = new Possesion { count = count };
                }
            }

            Debug.Log("[SkillTabPanelUI] 테스트 스킬 데이터 추가 완료");
            RefreshAllSkillUIs();
        }

        [ContextMenu("테스트: 스킬 보유 데이터 전체 삭제")]
        public void DebugClearSkillData()
        {
            if (DataManager.CurrentSaveData != null)
            {
                DataManager.CurrentSaveData.skillInfo.Clear();
                Debug.Log("[SkillTabPanelUI] 스킬 보유 데이터 전체 삭제");
                RefreshAllSkillUIs();
            }
        }

        [ContextMenu("테스트: 모든 스킬 보유 추가")]
        public void DebugOwnAllSkills()
        {
            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogError("[SkillTabPanelUI] CurrentSaveData가 null");
                return;
            }

            if (DataManager.CurrentSaveData.skillInfo == null)
            {
                DataManager.CurrentSaveData.skillInfo = new System.Collections.Generic.Dictionary<string, Possesion>();
            }

            var allSkills = DataManager.skills.GetAll();
            foreach (var skill in allSkills)
            {
                DataManager.CurrentSaveData.skillInfo[skill.id] = new Possesion { count = UnityEngine.Random.Range(1, 20) };
            }

            Debug.Log("[SkillTabPanelUI] 모든 스킬 보유 추가 완료");
            RefreshAllSkillUIs();
        }

        /// <summary>
        /// 모든 스킬 관련 UI 갱신 (조민희 추가)
        /// </summary>
        private void RefreshAllSkillUIs()
        {
            // 자신 갱신
            RefreshSkillList();

            // SkillInventoryUI도 갱신
            var inventoryUI = FindFirstObjectByType<UI.Grid.SkillInventoryUI>();
            if (inventoryUI != null)
            {
                inventoryUI.RefreshSlotOwnedStates();
            }
        }

        #endregion
    }
}
