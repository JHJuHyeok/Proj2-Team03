using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Skill.UI.Grid
{
    /*
    [조민희]
    SkillPresetManager
    - 5개 스킬 프리셋 관리
    - 프리셋 전환 시 현재 그리드 저장, 새 그리드 로드
    - PlayerPrefs에 저장/로드
    */
    public class SkillPresetManager : MonoBehaviour
    {
        public static SkillPresetManager Instance { get; private set; }

        [Header("참조")]
        [SerializeField] private SkillGridController gridController;

        [Header("설정")]
        [SerializeField] private string saveKey = "SkillPresetData";

        // 프리셋 데이터
        private SkillPresetSaveData presetData;

        // 이벤트
        public event System.Action<int> OnPresetChanged;  // 프리셋 변경 시 (새 인덱스)
        public event System.Action<int, List<PlacedSkillData>> OnPresetSaved;  // 프리셋 저장 시

        // 프로퍼티
        public int CurrentPresetIndex => presetData?.currentPresetIndex ?? 0;
        public int MaxPresets => SkillPresetSaveData.MAX_PRESETS;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 프리셋 데이터 초기화
            presetData = new SkillPresetSaveData();

            // GridController 자동 찾기
            if (gridController == null)
            {
                gridController = FindFirstObjectByType<SkillGridController>();
            }
        }

        private void Start()
        {
            // 저장된 프리셋 데이터 로드
            LoadPresetData();

            // GridController가 없으면 다시 찾기 (조민희 추가)
            if (gridController == null)
            {
                gridController = FindFirstObjectByType<SkillGridController>();
            }

            // 저장된 프리셋이 있으면 그리드에 로드 (조민희 추가)
            if (gridController != null)
            {
                StartCoroutine(LoadPresetAfterDelay());
            }
        }

        // 약간의 지연 후 프리셋 로드 (GridController 초기화 대기) - 조민희 추가
        private System.Collections.IEnumerator LoadPresetAfterDelay()
        {
            // GridController와 DataManager 초기화 대기
            int waitCount = 0;
            while ((gridController == null || DataManager.skills == null) && waitCount < 50)
            {
                yield return null;
                waitCount++;
            }

            // 다시 GridController 찾기 (씬 로드 후)
            if (gridController == null)
            {
                gridController = FindFirstObjectByType<SkillGridController>();
            }

            if (gridController != null)
            {
                var currentPresetSkills = presetData.GetCurrentPreset();
                if (currentPresetSkills != null && currentPresetSkills.Count > 0)
                {
                    LoadPresetToGrid();
                }
            }
        }

        private void OnDestroy()
        {
            // 게임 종료/씬 변경 시 현재 그리드 저장 (조민희 추가)
            AutoSaveCurrentGrid();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        // 게임 종료 시 자동 저장 (조민희 추가)
        private void OnApplicationQuit()
        {
            AutoSaveCurrentGrid();
        }

        // 비활성화 시 자동 저장 (조민희 추가)
        private void OnDisable()
        {
            AutoSaveCurrentGrid();
        }

        /// <summary>
        /// 현재 그리드를 프리셋에 자동 저장 (조민희 추가)
        /// </summary>
        private void AutoSaveCurrentGrid()
        {
            // null 체크 및 파괴된 객체 확인 (조민희 수정)
            if (gridController == null || presetData == null) return;

            // GridController가 파괴되었는지 확인
            if (gridController == null || gridController.GridManager == null) return;

            try
            {
                var placedSkills = gridController.GetPlacedSkillDataList();
                if (placedSkills != null && placedSkills.Count > 0)
                {
                    presetData.SetCurrentPreset(placedSkills);
                    SavePresetData();
                }
            }
            catch (System.Exception e)
            {
                // 초기화 중 에러는 무시 (아직 그리드가 준비되지 않음)
                Debug.LogWarning($"[SkillPresetManager] 자동 저장 스킵: {e.Message}");
            }
        }

        /// <summary>
        /// 프리셋 전환
        /// </summary>
        public void SwitchPreset(int newIndex)
        {
            if (presetData == null)
            {
                presetData = new SkillPresetSaveData();
            }

            if (newIndex < 0 || newIndex >= SkillPresetSaveData.MAX_PRESETS)
            {
                Debug.LogWarning($"[SkillPresetManager] 잘못된 프리셋 인덱스: {newIndex}");
                return;
            }

            if (newIndex == presetData.currentPresetIndex)
            {
                return;
            }

            // 1. 현재 그리드 데이터 저장
            SaveCurrentGridToPreset();

            // 2. 프리셋 전환
            int oldIndex = presetData.currentPresetIndex;
            presetData.SwitchPreset(newIndex);

            // 3. 새 프리셋 데이터로 그리드 로드
            LoadPresetToGrid();

            // 4. 저장
            SavePresetData();

            // 이벤트 호출
            OnPresetChanged?.Invoke(newIndex);
        }

        /// <summary>
        /// 현재 그리드 상태를 현재 프리셋에 저장
        /// </summary>
        public void SaveCurrentGridToPreset()
        {
            if (gridController == null)
            {
                Debug.LogWarning("[SkillPresetManager] GridController가 없음");
                return;
            }

            var placedSkills = gridController.GetPlacedSkillDataList();

            if (placedSkills != null)
            {
                presetData.SetCurrentPreset(placedSkills);
                OnPresetSaved?.Invoke(presetData.currentPresetIndex, placedSkills);
            }
        }

        /// <summary>
        /// 현재 프리셋 데이터를 그리드에 로드
        /// </summary>
        public void LoadPresetToGrid()
        {
            if (gridController == null)
            {
                Debug.LogWarning("[SkillPresetManager] GridController가 없음");
                return;
            }

            // 현재 프리셋 데이터 가져오기
            var currentPresetSkills = presetData.GetCurrentPreset();

            if (currentPresetSkills == null || currentPresetSkills.Count == 0)
            {
                // 빈 프리셋이면 그리드만 클리어
                gridController.ClearAll();
                return;
            }

            // 새로운 메서드 사용하여 로드
            gridController.LoadFromPresetData(currentPresetSkills);
        }

        /// <summary>
        /// 특정 프리셋의 스킬 개수 반환
        /// </summary>
        public int GetPresetSkillCount(int index)
        {
            if (presetData == null) return 0;
            var preset = presetData.GetPreset(index);
            return preset?.Count ?? 0;
        }

        /// <summary>
        /// 프리셋 데이터 저장 (PlayerPrefs)
        /// </summary>
        public void SavePresetData()
        {
            if (presetData == null) return;

            string json = presetData.ToJson();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 프리셋 데이터 로드 (PlayerPrefs)
        /// </summary>
        public void LoadPresetData()
        {
            if (PlayerPrefs.HasKey(saveKey))
            {
                string json = PlayerPrefs.GetString(saveKey);
                presetData = SkillPresetSaveData.FromJson(json);
            }
            else
            {
                presetData = new SkillPresetSaveData();
            }
        }

        /// <summary>
        /// 특정 프리셋 초기화
        /// </summary>
        public void ClearPreset(int index)
        {
            if (presetData == null) return;

            presetData.ClearPreset(index);
            SavePresetData();

            // 현재 프리셋이면 그리드도 클리어
            if (index == presetData.currentPresetIndex)
            {
                gridController?.ClearAll();
            }
        }

        /// <summary>
        /// GridController 설정
        /// </summary>
        public void SetGridController(SkillGridController controller)
        {
            gridController = controller;
        }

        #region 디버그

        [ContextMenu("현재 프리셋 저장")]
        public void DebugSaveCurrentPreset()
        {
            SaveCurrentGridToPreset();
            SavePresetData();
        }

        [ContextMenu("프리셋 정보 출력")]
        public void DebugPrintPresetInfo()
        {
            if (presetData == null)
            {
                Debug.Log("프리셋 데이터가 null");
                return;
            }

            Debug.Log($"=== 프리셋 정보 ===");
            Debug.Log($"현재 프리셋: {presetData.currentPresetIndex + 1}");
            for (int i = 0; i < SkillPresetSaveData.MAX_PRESETS; i++)
            {
                var count = presetData.GetPreset(i)?.Count ?? 0;
                Debug.Log($"  프리셋 {i + 1}: {count}개 스킬");
            }
        }

        [ContextMenu("모든 프리셋 초기화")]
        public void DebugClearAllPresets()
        {
            presetData?.ClearAllPresets();
            SavePresetData();
            gridController?.ClearAll();
            Debug.Log("모든 프리셋 초기화 완료");
        }

        #endregion
    }
}
