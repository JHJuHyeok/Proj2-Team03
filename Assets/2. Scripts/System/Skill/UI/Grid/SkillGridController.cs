using System.Collections.Generic;
using UnityEngine;
using SlayerLegend.Skill.Data;

namespace SlayerLegend.Skill.UI.Grid
{
    // 스킬 그리드 컨트롤러
    // 그리드, 인벤토리, 저장/로드를 통합 관리
    public class SkillGridController : MonoBehaviour
    {
        [Header("컴포넌트 참조")]
        [SerializeField] private SkillGridManager gridManager;
        [SerializeField] private Transform inventoryContainer;
        [SerializeField] private GameObject draggableItemPrefab;

        [Header("설정")]
        [SerializeField] private string saveKey = "SkillGridData";

        // 인벤토리 아이템 목록
        private List<SkillDraggableItem> inventoryItems = new List<SkillDraggableItem>();
        private Dictionary<string, SkillDraggableItem> itemsById = new Dictionary<string, SkillDraggableItem>();

        // 스킬 데이터 캐시 (슬롯에서 사용)
        private Dictionary<string, SkillData> skillDataCache = new Dictionary<string, SkillData>();

        // 이벤트
        public event System.Action OnGridUpdated;
        public event System.Action<string> OnSkillAddedToInventory;
        public event System.Action<string> OnSkillRemovedFromInventory;

        // 프로퍼티
        public SkillGridManager GridManager => gridManager;
        public int InventoryCount => inventoryItems.Count;

        private void Awake()
        {
            if (gridManager == null)
            {
                gridManager = GetComponentInChildren<SkillGridManager>();
            }
        }

        private void Start()
        {
            // 그리드 이벤트 구독
            if (gridManager != null)
            {
                gridManager.OnGridChanged += HandleGridChanged;
            }

            // 저장된 데이터 로드 - SkillGridInitializer가 제어하도록 변경 (조민희)
            // LoadGridData(); // 주석 처리: DataManager 초기화 후 SkillGridInitializer에서 호출됨
        }

        private void OnDestroy()
        {
            if (gridManager != null)
            {
                gridManager.OnGridChanged -= HandleGridChanged;
            }
        }

        // 인벤토리에 스킬 추가
        public SkillDraggableItem AddSkillToInventory(
            string skillId,
            string skillName,
            SkillShapeType shapeType,
            SkillType skillType,
            Sprite icon = null,
            string sprite = "")  // 조민희: spriteName 추가
        {
            if (draggableItemPrefab == null || inventoryContainer == null)
            {
                Debug.LogError("[SkillGridController] 프리팹 또는 컨테이너가 없습니다.");
                return null;
            }

            // 이미 존재하는지 확인
            if (itemsById.ContainsKey(skillId))
            {
                Debug.LogWarning($"[SkillGridController] 이미 존재하는 스킬: {skillId}");
                return itemsById[skillId];
            }

            // 아이템 생성
            GameObject itemObj = Instantiate(draggableItemPrefab, inventoryContainer);
            SkillDraggableItem item = itemObj.GetComponent<SkillDraggableItem>();

            if (item == null)
            {
                Debug.LogError("[SkillGridController] 프리팹에 SkillDraggableItem이 없습니다.");
                Destroy(itemObj);
                return null;
            }

            // 초기화
            item.Initialize(skillId, skillName, shapeType, skillType, icon, sprite);  // 조민희: spriteName 추가
            item.SetGridManager(gridManager);
            item.SetInventoryContainer(inventoryContainer);

            // 이벤트 구독
            item.OnItemPlaced += HandleItemPlaced;
            item.OnItemRemoved += HandleItemRemoved;

            // 목록에 추가
            inventoryItems.Add(item);
            itemsById[skillId] = item;

            OnSkillAddedToInventory?.Invoke(skillId);

            Debug.Log($"[SkillGridController] 인벤토리에 스킬 추가: {skillName}");
            return item;
        }

        // 인벤토리에서 스킬 제거
        public void RemoveSkillFromInventory(string skillId)
        {
            if (!itemsById.TryGetValue(skillId, out var item))
            {
                return;
            }

            // 그리드에 있으면 먼저 제거
            if (item.IsOnGrid)
            {
                item.RemoveFromGrid();
            }

            // 이벤트 구독 해제
            item.OnItemPlaced -= HandleItemPlaced;
            item.OnItemRemoved -= HandleItemRemoved;

            // 목록에서 제거
            inventoryItems.Remove(item);
            itemsById.Remove(skillId);

            // 오브젝트 제거
            Destroy(item.gameObject);

            OnSkillRemovedFromInventory?.Invoke(skillId);

            Debug.Log($"[SkillGridController] 인벤토리에서 스킬 제거: {skillId}");
        }

        // ===== 새로운 방식: 슬롯 클릭 → 아이템 생성 =====

        // 스킬 데이터 등록 (초기화 시 호출)
        public void RegisterSkillData(SkillData skillData)
        {
            if (skillData == null || string.IsNullOrEmpty(skillData.id)) return;

            if (!skillDataCache.ContainsKey(skillData.id))
            {
                skillDataCache[skillData.id] = skillData;
            }
        }

        // 스킬이 그리드에 배치되어 있는지 확인
        public bool IsSkillOnGrid(string skillId)
        {
            if (itemsById.TryGetValue(skillId, out var item))
            {
                return item.IsOnGrid;
            }
            return false;
        }

        // 슬롯에서 클릭 시 아이템 생성 및 드래그 시작
        // 반환: 생성된 아이템 (이미 그리드에 있으면 null)
        public SkillDraggableItem CreateDraggableItemFromSlot(string skillId)
        {
            // 이미 그리드에 배치되어 있으면 생성 안 함
            if (IsSkillOnGrid(skillId))
            {
                Debug.Log($"[SkillGridController] 스킬이 이미 그리드에 있음: {skillId}");
                return null;
            }

            // saveData에 고아 데이터가 있는지 확인하고 정리
            // (A-fix 적용 전 저장 데이터 또는 예외 상황 대응)
            if (gridManager != null)
            {
                var placedSkill = gridManager.GetSaveData().GetPlacedSkill(skillId);
                if (placedSkill != null)
                {
                    Debug.Log($"[SkillGridController] saveData에서 고아 스킬 제거: {skillId}");
                    gridManager.RemoveSkill(skillId);
                }
            }

            // 이미 생성된 아이템이 있으면 반환
            if (itemsById.TryGetValue(skillId, out var existingItem))
            {
                return existingItem;
            }

            // 스킬 데이터 확인
            if (!skillDataCache.TryGetValue(skillId, out var skillData))
            {
                Debug.LogWarning($"[SkillGridController] 스킬 데이터를 찾을 수 없음: {skillId}");
                return null;
            }

            // 아이템 생성
            if (draggableItemPrefab == null)
            {
                Debug.LogError("[SkillGridController] draggableItemPrefab이 없습니다.");
                return null;
            }

            GameObject itemObj = Instantiate(draggableItemPrefab, transform);
            SkillDraggableItem item = itemObj.GetComponent<SkillDraggableItem>();

            if (item == null)
            {
                Debug.LogError("[SkillGridController] 프리팹에 SkillDraggableItem이 없습니다.");
                Destroy(itemObj);
                return null;
            }

            // 초기화
            Sprite icon = SlayerLegend.Resource.ResourceManager.Instance?.LoadSprite(skillData.spriteName);
            item.Initialize(skillData.id, skillData.name, skillData.GetShapeType(), skillData.type, icon, skillData.spriteName);  // 조민희: spriteName 추가
            item.SetGridManager(gridManager);
            item.SetGridController(this);  // 컨트롤러 참조 설정 (삭제용)
            item.SetInventoryContainer(inventoryContainer);
            item.SetDestroyOnRemove(true);  // 그리드에서 벗어나면 삭제

            // 이벤트 구독
            item.OnItemPlaced += HandleItemPlaced;
            item.OnItemRemoved += HandleItemRemoved;

            // 목록에 추가
            inventoryItems.Add(item);
            itemsById[skillId] = item;

            OnSkillAddedToInventory?.Invoke(skillId);

            Debug.Log($"[SkillGridController] 슬롯에서 아이템 생성: {skillData.name}");
            return item;
        }

        // 아이템 삭제 (그리드에서 벗어났을 때 호출)
        public void DestroyDraggableItem(string skillId)
        {
            if (!itemsById.TryGetValue(skillId, out var item))
            {
                return;
            }

            // 그리드에 있으면 제거
            if (item.IsOnGrid)
            {
                gridManager?.RemoveSkill(skillId);
            }

            // 이벤트 구독 해제
            item.OnItemPlaced -= HandleItemPlaced;
            item.OnItemRemoved -= HandleItemRemoved;

            // 목록에서 제거
            inventoryItems.Remove(item);
            itemsById.Remove(skillId);

            // 오브젝트 제거
            Destroy(item.gameObject);

            OnSkillRemovedFromInventory?.Invoke(skillId);

            Debug.Log($"[SkillGridController] 아이템 삭제: {skillId}");
        }

        // ===== A안: 로드 시 아이템 복원 =====

        // 저장된 데이터로부터 아이템 복원
        private SkillDraggableItem RestoreDraggableItemFromSave(PlacedSkillData placedSkill)
        {
            if (placedSkill == null || string.IsNullOrEmpty(placedSkill.skillId))
            {
                return null;
            }

            // 이미 존재하면 반환
            if (itemsById.ContainsKey(placedSkill.skillId))
            {
                return itemsById[placedSkill.skillId];
            }

            // 프리팹 확인
            if (draggableItemPrefab == null)
            {
                Debug.LogError("[SkillGridController] draggableItemPrefab이 없습니다.");
                return null;
            }

            // 아이템 생성
            GameObject itemObj = Instantiate(draggableItemPrefab, gridManager.GetCellContainer());
            SkillDraggableItem item = itemObj.GetComponent<SkillDraggableItem>();

            if (item == null)
            {
                Debug.LogError("[SkillGridController] 프리팹에 SkillDraggableItem이 없습니다.");
                Destroy(itemObj);
                return null;
            }

            // 아이콘 로드 시도 (저장된 spriteName → skillDataCache → DataManager → 패턴 기반) - 조민희
            Sprite icon = null;
            SkillData skillData = null;

            // 1차: 저장된 spriteName으로 바로 로드 (가장 확실)
            if (!string.IsNullOrEmpty(placedSkill.spriteName))
            {
                icon = SlayerLegend.Resource.ResourceManager.Instance?.LoadSprite(placedSkill.spriteName);
                Debug.Log($"[SkillGridController] 1차 시도 (spriteName): {placedSkill.spriteName} → {(icon != null ? "성공" : "실패")}");
            }

            // 2차: 캐시에서 찾기
            if (icon == null && skillDataCache.TryGetValue(placedSkill.skillId, out skillData))
            {
                icon = SlayerLegend.Resource.ResourceManager.Instance?.LoadSprite(skillData.spriteName);
                Debug.Log($"[SkillGridController] 2차 시도 (cache): {skillData.spriteName} → {(icon != null ? "성공" : "실패")}");
            }

            // 3차: DataManager에서 찾기 (fallback)
            if (icon == null && DataManager.skills != null)
            {
                var allSkills = DataManager.skills.GetAll();
                skillData = allSkills?.Find(s => s.id == placedSkill.skillId);
                if (skillData != null)
                {
                    icon = SlayerLegend.Resource.ResourceManager.Instance?.LoadSprite(skillData.spriteName);
                    Debug.Log($"[SkillGridController] 3차 시도 (DataManager): {skillData.spriteName} → {(icon != null ? "성공" : "실패")}");
                    // 캐시에도 등록
                    skillDataCache[placedSkill.skillId] = skillData;
                }
                else
                {
                    Debug.Log($"[SkillGridController] 3차 시도 (DataManager): 스킬 ID '{placedSkill.skillId}' 찾을 수 없음");
                }
            }

            // 4차: skillId 패턴 기반 직접 로드 (자동 마이그레이션) - 조민희
            if (icon == null)
            {
                icon = TryLoadIconBySkillIdPattern(placedSkill.skillId);
                Debug.Log($"[SkillGridController] 4차 시도 (패턴 매칭): {placedSkill.skillId} → {(icon != null ? "성공" : "실패")}");
            }

            if (icon == null)
            {
                Debug.LogWarning($"[SkillGridController] 아이콘 로드 실패: {placedSkill.skillId}, spriteName: {placedSkill.spriteName}");
            }

            // 저장할 spriteName 결정 (조민희: 우선순위 - 기존 저장값 > skillData > 빈값)
            string finalSpriteName = placedSkill.spriteName;
            if (string.IsNullOrEmpty(finalSpriteName) && skillData != null)
            {
                finalSpriteName = skillData.spriteName;
            }

            // 초기화 (PlacedSkillData의 정보 사용)
            string displayName = string.IsNullOrEmpty(placedSkill.skillName) ? placedSkill.skillId : placedSkill.skillName;
            item.Initialize(placedSkill.skillId, displayName, placedSkill.shapeType, placedSkill.skillType, icon, finalSpriteName);
            item.SetGridManager(gridManager);
            item.SetGridController(this);
            item.SetInventoryContainer(inventoryContainer);
            item.SetDestroyOnRemove(true);

            // 그리드에 배치된 상태로 설정 (드래그 없이 바로 배치)
            item.ForceSetOnGrid(placedSkill.GridPosition, placedSkill.rotation);

            // 이벤트 구독
            item.OnItemPlaced += HandleItemPlaced;
            item.OnItemRemoved += HandleItemRemoved;

            // 목록에 추가
            inventoryItems.Add(item);
            itemsById[placedSkill.skillId] = item;

            Debug.Log($"[SkillGridController] 저장 데이터에서 아이템 복원: {displayName} at {placedSkill.GridPosition}");
            return item;
        }

        // 스킬 조회
        public SkillDraggableItem GetItem(string skillId)
        {
            return itemsById.TryGetValue(skillId, out var item) ? item : null;
        }

        // 그리드에 배치된 스킬 목록
        public List<SkillDraggableItem> GetPlacedItems()
        {
            var placedItems = new List<SkillDraggableItem>();
            foreach (var item in inventoryItems)
            {
                if (item.IsOnGrid)
                {
                    placedItems.Add(item);
                }
            }
            return placedItems;
        }

        // 인벤토리에 있는 스킬 목록
        public List<SkillDraggableItem> GetInventoryItems()
        {
            var invItems = new List<SkillDraggableItem>();
            foreach (var item in inventoryItems)
            {
                if (!item.IsOnGrid)
                {
                    invItems.Add(item);
                }
            }
            return invItems;
        }

        // 이벤트 핸들러
        private void HandleGridChanged()
        {
            SaveGridData();
            OnGridUpdated?.Invoke();
        }

        private void HandleItemPlaced(SkillDraggableItem item)
        {
            Debug.Log($"[SkillGridController] 스킬 배치됨: {item.SkillName}");
            SaveGridData();
        }

        private void HandleItemRemoved(SkillDraggableItem item)
        {
            Debug.Log($"[SkillGridController] 스킬 제거됨: {item.SkillName}");
            SaveGridData();
        }

        #region 저장/로드

        // 그리드 데이터 저장
        public void SaveGridData()
        {
            if (gridManager == null) return;

            var saveData = gridManager.GetSaveData();

            // PlayerPrefs에 JSON으로 저장
            string json = saveData.ToJson();
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();

            Debug.Log($"[SkillGridController] 그리드 데이터 저장 완료 ({saveData.placedSkills.Count}개 스킬)");
        }

        // 그리드 데이터 로드
        public void LoadGridData()
        {
            if (gridManager == null) return;

            if (!PlayerPrefs.HasKey(saveKey))
            {
                Debug.Log("[SkillGridController] 저장된 데이터가 없습니다.");
                return;
            }

            string json = PlayerPrefs.GetString(saveKey);
            var saveData = SkillGridSaveData.FromJson(json);

            if (saveData == null || saveData.placedSkills.Count == 0)
            {
                Debug.Log("[SkillGridController] 로드할 데이터가 없습니다.");
                return;
            }

            // 그리드 셀 상태 적용
            gridManager.LoadSaveData(saveData);

            // A안: 각 배치된 스킬에 대해 DraggableItem 인스턴스화
            int restoredCount = 0;
            foreach (var placedSkill in saveData.placedSkills)
            {
                var item = RestoreDraggableItemFromSave(placedSkill);
                if (item != null)
                {
                    restoredCount++;
                }
            }

            Debug.Log($"[SkillGridController] 그리드 데이터 로드 완료 ({restoredCount}/{saveData.placedSkills.Count}개 스킬 복원)");
        }

        // 저장 데이터 삭제
        public void ClearSavedData()
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();

            Debug.Log("[SkillGridController] 저장된 데이터 삭제");
        }

        // 저장 데이터 존재 여부
        public bool HasSavedData()
        {
            return PlayerPrefs.HasKey(saveKey);
        }

        #endregion

        // skillId 패턴 기반 아이콘 로드 (자동 마이그레이션) - 조민희
        // 예: "Fire_01" → Skill/skillicon/skill_fire/Fire_01 시도
        // 확장: 다양한 파일명 패턴 시도 (숫자만 추출 등)
        private Sprite TryLoadIconBySkillIdPattern(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return null;

            string lowerId = skillId.ToLower();
            string elementFolder = "skill_none"; // 기본값

            // 속성 폴더 결정
            if (lowerId.StartsWith("fire") || lowerId.Contains("_fire"))
                elementFolder = "skill_fire";
            else if (lowerId.StartsWith("water") || lowerId.Contains("_water"))
                elementFolder = "skill_water";
            else if (lowerId.StartsWith("earth") || lowerId.Contains("_earth"))
                elementFolder = "skill_earth";
            else if (lowerId.StartsWith("wind") || lowerId.Contains("_wind"))
                elementFolder = "skill_wind";

            // 숫자 부분 추출 (예: Fire_04 → 04)
            string numberPart = "";
            var parts = skillId.Split('_');
            if (parts.Length > 1)
            {
                numberPart = parts[parts.Length - 1];
            }

            // 가능한 경로들 시도 (다양한 패턴)
            List<string> paths = new List<string>
            {
                $"Skill/skillicon/{elementFolder}/{skillId}",
                $"Skill/skillicon/{elementFolder}/{skillId.ToLower()}",
                $"Skill/skillicon/{skillId}",
                $"Skill/skillicon/{skillId.ToLower()}",
                $"Skill/skillicon/skill_none/{skillId}",
                $"Skill/skillicon/skill_none/{skillId.ToLower()}"
            };

            // 숫자 패턴 추가 (예: Fire_04 → skill_fire/04, skill_fire/fire_04)
            if (!string.IsNullOrEmpty(numberPart))
            {
                paths.Add($"Skill/skillicon/{elementFolder}/{numberPart}");
                paths.Add($"Skill/skillicon/{elementFolder}/{elementFolder}_{numberPart}");
                paths.Add($"Skill/skillicon/{numberPart}");

                // 0 패딩 추가 (예: 4 → 04)
                if (numberPart.Length == 1 && int.TryParse(numberPart, out _))
                {
                    string paddedNumber = "0" + numberPart;
                    paths.Add($"Skill/skillicon/{elementFolder}/{paddedNumber}");
                    paths.Add($"Skill/skillicon/{elementFolder}/{paddedNumber}_*");  // 와일드카드는 Resources.Load에서 작동 안함 but 로그용
                }
            }

            // Unity Resources.Load는 와일드카드를 지원하지 않으므로
            // 번호로 시작하는 파일 찾기 위해 Resources.LoadAll 사용 시도
            if (!string.IsNullOrEmpty(numberPart))
            {
                string paddedNumber = numberPart.Length == 1 ? "0" + numberPart : numberPart;
                var allSprites = Resources.LoadAll<Sprite>($"Skill/skillicon/{elementFolder}");
                foreach (var sprite in allSprites)
                {
                    if (sprite.name.StartsWith(paddedNumber + "_"))
                    {
                        Debug.Log($"[SkillGridController] 번호 매칭 성공: {skillId} → {sprite.name}");
                        return sprite;
                    }
                }
            }

            foreach (string path in paths)
            {
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                {
                    Debug.Log($"[SkillGridController] 패턴 매칭 성공: {skillId} → {path}");
                    return sprite;
                }
            }

            return null;
        }

        #region 디버그/테스트

        // 테스트용 스킬 추가
        [ContextMenu("Add Test Skill (1x1)")]
        public void AddTestSkillOneByOne()
        {
            AddSkillToInventory("test_1x1", "테스트 1x1", SkillShapeType.OneByOne, SkillType.Active, null);
        }

        [ContextMenu("Add Test Skill (2x1)")]
        public void AddTestSkillTwoByOne()
        {
            AddSkillToInventory("test_2x1", "테스트 2x1", SkillShapeType.TwoByOne, SkillType.Active, null);
        }

        [ContextMenu("Add Test Skill (L-Shape)")]
        public void AddTestSkillLShape()
        {
            AddSkillToInventory("test_L", "테스트 L자", SkillShapeType.L_Shape, SkillType.Active, null);
        }

        [ContextMenu("Clear All")]
        public void ClearAll()
        {
            // 모든 아이템 제거
            var itemsToRemove = new List<string>(itemsById.Keys);
            foreach (var id in itemsToRemove)
            {
                RemoveSkillFromInventory(id);
            }

            // 그리드 초기화
            if (gridManager != null)
            {
                gridManager.ClearGrid();
            }

            Debug.Log("[SkillGridController] 전체 초기화 완료");
        }

        [ContextMenu("Print Status")]
        public void PrintStatus()
        {
            Debug.Log($"=== 스킬 그리드 상태 ===");
            Debug.Log($"인벤토리 스킬: {GetInventoryItems().Count}개");
            Debug.Log($"배치된 스킬: {GetPlacedItems().Count}개");
            Debug.Log($"그리드 사용률: {SkillGridValidator.CalculateGridUsage(gridManager) * 100:F1}%");
        }

        #endregion
    }
}
