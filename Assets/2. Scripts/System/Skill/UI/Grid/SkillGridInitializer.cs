using System.Collections.Generic;
using UnityEngine;
using SlayerLegend.Skill.Data;
using SlayerLegend.Skill.UI.Grid;

namespace SlayerLegend.Skill.UI
{
    // 스킬 그리드 시스템 초기화기
    // DataManager와 그리드 시스템 간의 연결을 담당
    public class SkillGridInitializer : MonoBehaviour
    {
        [Header("컴포넌트 참조")]
        [SerializeField] private SkillGridController gridController;
        [SerializeField] private SkillInventoryUI inventoryUI;

        [Header("설정")]
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool loadSavedGridData = true;
        [SerializeField] private bool autoInitializeDataManager = true;

        // 초기화 완료 여부
        public bool IsInitialized { get; private set; } = false;

        // 프로퍼티
        public SkillGridController GridController => gridController;
        public SkillInventoryUI InventoryUI => inventoryUI;

        // 이벤트
        public event System.Action OnInitializationComplete;

        private async void Start()
        {
            if (loadOnStart)
            {
                await InitializeAsync();
            }
        }

        // 비동기 초기화
        public async System.Threading.Tasks.Task InitializeAsync()
        {
            // DataManager 자동 초기화
            if (autoInitializeDataManager)
            {
                // skills가 비어있으면 로드
                if (DataManager.skills.GetAll() == null || DataManager.skills.GetAll().Count == 0)
                {
                    await DataManager.LoadAllDatabase();
                }
            }
            else
            {
                // 데이터가 로드될 때까지 대기 (다른 곳에서 로드 중인 경우)
                int maxWait = 100; // 최대 100회 대기 (약 5초)
                int waitCount = 0;
                while ((DataManager.skills.GetAll() == null || DataManager.skills.GetAll().Count == 0) && waitCount < maxWait)
                {
                    await System.Threading.Tasks.Task.Delay(50);
                    waitCount++;
                }

                if (DataManager.skills.GetAll() == null || DataManager.skills.GetAll().Count == 0)
                {
                    Debug.LogError("[SkillGridInitializer] 데이터 로드 대기 시간 초과");
                    return;
                }

            }

            Initialize();
        }

        // 초기화
        public void Initialize()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[SkillGridInitializer] 이미 초기화되었습니다.");
                return;
            }

            // 컴포넌트 참조 확인
            if (gridController == null)
            {
                gridController = GetComponentInChildren<SkillGridController>();
            }

            if (inventoryUI == null)
            {
                inventoryUI = GetComponentInChildren<SkillInventoryUI>();
            }

            // 데이터 로드
            LoadSkillData();

            // 저장된 그리드 데이터 로드
            if (loadSavedGridData && gridController != null)
            {
                gridController.LoadGridData();
            }

            IsInitialized = true;
            OnInitializationComplete?.Invoke();
        }

        // 스킬 데이터 로드
        private void LoadSkillData()
        {
            // DataManager가 초기화되었는지 확인
            if (DataManager.skills == null)
            {
                Debug.LogError("[SkillGridInitializer] DataManager.skills가 null입니다. DataManager.LoadAllDatabase()가 호출되었는지 확인하세요.");
                return;
            }

            // DataManager에서 스킬 목록 가져오기
            var allSkills = DataManager.skills.GetAll();

            if (allSkills == null || allSkills.Count == 0)
            {
                Debug.LogWarning("[SkillGridInitializer] 로드된 스킬 데이터가 없습니다.");
                return;
            }

            // 인벤토리 UI에 로드
            if (inventoryUI != null)
            {
                inventoryUI.LoadSkills(allSkills);
            }
            else
            {
                // 인벤토리 UI가 없으면 컨트롤러에 직접 추가
                LoadSkillsToController(allSkills);
            }
        }

        // 컨트롤러에 직접 스킬 추가
        private void LoadSkillsToController(List<SkillData> skills)
        {
            if (gridController == null) return;

            foreach (var skill in skills)
            {
                gridController.AddSkillToInventory(
                    skill.id,
                    skill.name,
                    skill.GetShapeType(),
                    skill.type,
                    SlayerLegend.Resource.ResourceManager.Instance?.LoadSprite(skill.spriteName),
                    skill.spriteName  // 조민희: spriteName 추가
                );
            }
        }

        // 전체 저장
        public void SaveAll()
        {
            if (gridController != null)
            {
                gridController.SaveGridData();
            }
        }

        // 전체 초기화
        // 주의: 실행 순서가 중요합니다
        // 1. inventoryUI.ClearInventory() - UI 슬롯 제거 (이벤트 구독 해제 포함)
        // 2. gridController.ClearAll() - 내부 데이터 정리 (그리드 셀, itemsById)
        // 3. gridController.ClearSavedData() - 저장 데이터 삭제
        // 4. LoadSkillData() - 스킬 데이터 다시 로드
        public void ResetAll()
        {
            // 1. UI 슬롯 제거
            if (inventoryUI != null)
            {
                inventoryUI.ClearInventory();
            }

            // 2-3. 내부 데이터 정리 및 저장 데이터 삭제
            if (gridController != null)
            {
                gridController.ClearAll();
                gridController.ClearSavedData();
            }

            // 4. 스킬 데이터 다시 로드
            LoadSkillData();
        }

        // 스킬 ID로 그리드에서 제거
        public void RemoveSkillFromGrid(string skillId)
        {
            if (gridController != null)
            {
                var item = gridController.GetItem(skillId);
                if (item != null && item.IsOnGrid)
                {
                    item.RemoveFromGrid();
                }
            }
        }

        // 현재 그리드 상태 가져오기
        public SkillGridSaveData GetGridSaveData()
        {
            if (gridController == null) return null;
            return gridController.GridManager?.GetSaveData();
        }

        // 특정 스킬이 그리드에 배치되어 있는지 확인
        public bool IsSkillPlaced(string skillId)
        {
            if (gridController == null) return false;

            var item = gridController.GetItem(skillId);
            return item != null && item.IsOnGrid;
        }

        // 배치된 모든 스킬 ID 목록 가져오기
        public List<string> GetPlacedSkillIds()
        {
            var result = new List<string>();

            if (gridController != null)
            {
                var placedItems = gridController.GetPlacedItems();
                foreach (var item in placedItems)
                {
                    result.Add(item.SkillId);
                }
            }

            return result;
        }

        #region 에디터 디버그

#if UNITY_EDITOR
        [ContextMenu("초기화")]
        private void DebugInitialize()
        {
            Initialize();
        }

        [ContextMenu("저장")]
        private void DebugSave()
        {
            SaveAll();
        }

        [ContextMenu("초기화 후 재로드")]
        private void DebugReset()
        {
            ResetAll();
        }

        [ContextMenu("상태 출력")]
        private void DebugPrintStatus()
        {
            if (gridController == null)
            {
                Debug.Log("GridController가 없습니다.");
                return;
            }

            var placedIds = GetPlacedSkillIds();
            Debug.Log($"=== 스킬 그리드 상태 ===");
            Debug.Log($"초기화 완료: {IsInitialized}");
            Debug.Log($"배치된 스킬: {placedIds.Count}개");

            foreach (var id in placedIds)
            {
                var item = gridController.GetItem(id);
                if (item != null)
                {
                    Debug.Log($"  - {item.SkillName}: {item.GridPosition}, 회전 {item.CurrentRotation}도");
                }
            }
        }
#endif

        #endregion
    }
}
