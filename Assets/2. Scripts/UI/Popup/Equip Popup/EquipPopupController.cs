using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SlayerLegend.Equipment;

/*
EquipPopupContentController
-EquipPopup 내부 UI 컨트롤러
-EquipmentManager 인벤토리 기반 리스트 생성
-장비 선택 / 장착 / 융합 / 강화 / 보유개수 표시
작성자: [승문] + 조민희 (융합/강화 기능 추가)
*/
public class EquipPopupContentController : MonoBehaviour
{
    [Header("매니저")]
    [SerializeField] private EquipmentManager equipmentManager;//장비 매니저

    [Header("리스트")]
    [SerializeField] private Transform listRoot;//Scroll Content
    [SerializeField] private EquipItemCell cellPrefab;//리스트 셀

    [Header("상세 정보")]
    [SerializeField] private Image detailIcon;//선택 아이콘
    [SerializeField] private TMP_Text detailName;//선택 이름
    [SerializeField] private TMP_Text ownedText;//보유 개수
    [SerializeField] private TMP_Text levelText;//강화 레벨

    [Header("융합 아이콘 (조민희 추가)")]
    [SerializeField] private Image currentEquipIcon; // 현재 장비 아이콘
    [SerializeField] private TMP_Text currentEquipName; // 현재 장비 이름
    [SerializeField] private TMP_Text currentEquipNumber; // 현재 장비 개수 (예: 2(-10))
    [SerializeField] private Image upgradeIcon; // 상위 등급 아이콘
    [SerializeField] private TMP_Text upgradeName; // 상위 등급 이름
    [SerializeField] private TMP_Text upgradeNumber; // 상위 등급 개수 (예: 3(+2)

        private const int FUSION_REQUIRED_COUNT = 5; // 융합에 필요한 장비 개수

    [Header("버튼")]
    [SerializeField] private Button equipButton;//장착 버튼
    [SerializeField] private Button fusionButton;//융합 버튼
    [SerializeField] private Button enhanceButton;//강화 버튼
    [SerializeField] private TMP_Text enhanceCostText;//강화 비용 텍스트

    [SerializeField] private string spritePath = "Icons";//아이콘 경로

    private readonly List<EquipItemCell> cellPool = new List<EquipItemCell>();

    private EquipType currentType;
    private InventoryItem selectedItem;

    private void Awake()
    {
        if (equipmentManager == null)
        {
            equipmentManager = EquipmentManager.Instance;
        }

        // 장착 버튼
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnClickEquip);
        }

        // 융합 버튼 (조민희 추가)
        if (fusionButton != null)
        {
            fusionButton.onClick.AddListener(OnClickFusion);
        }

        // 강화 버튼 (조민희 추가)
        if (enhanceButton != null)
        {
            enhanceButton.onClick.AddListener(OnClickEnhance);
        }

        if (equipmentManager != null)
        {
            // OnInventoryChanged 구독 제거 - WeaponBundleUI/AccessoryBundleUI에서 처리
            equipmentManager.OnFusionComplete += OnFusionComplete;
            equipmentManager.OnEquipmentEnhanced += OnEquipmentEnhanced;
        }
    }

    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnFusionComplete -= OnFusionComplete;
            equipmentManager.OnEquipmentEnhanced -= OnEquipmentEnhanced;
        }
    }

    public void SetEquipTab(EquipTab tab)
    {
        if (tab == EquipTab.Weapon)
        {
            currentType = EquipType.Weapon;
        }
        else
        {
            currentType = EquipType.Accessorie;
        }

        selectedItem = null;

        RebuildList();
        ApplySelection(null);
    }

    /// <summary>
    /// [조민희] 초기 선택 장비 설정 - WeaponBundleUI/AccessoryBundleUI에서 클릭 시 호출
    /// </summary>
    public void SetInitialSelection(string equipId)
    {
        if (string.IsNullOrEmpty(equipId) || equipmentManager == null) return;

        // EquipData 가져오기
        EquipData equip = equipmentManager.GetEquipData(equipId);
        if (equip == null)
        {
            Debug.LogWarning($"[EquipPopup] 초기 선택 장비를 찾을 수 없음: {equipId}");
            return;
        }

        // InventoryItem 생성 (레벨 포함)
        int level = equipmentManager.GetLevel(equipId);
        selectedItem = new InventoryItem(equip, level);

        // UI 갱신
        ApplySelection(selectedItem);
    }

    private void RebuildList()
    {
        if (equipmentManager == null) return;

        IReadOnlyList<InventoryItem> inventory = equipmentManager.GetInventory(currentType);

        EnsurePool(inventory.Count);

        for (int i = 0; i < cellPool.Count; i++)
        {
            bool active = i < inventory.Count;

            cellPool[i].gameObject.SetActive(active);

            if (active)
            {
                cellPool[i].Bind(inventory[i], OnClickItem, ResolveSprite);
            }
        }
    }

    private void EnsurePool(int count)
    {
        // [조민희] cellPrefab이 null이면 생성하지 않음
        if (cellPrefab == null) return;

        while (cellPool.Count < count)
        {
            EquipItemCell cell = Instantiate(cellPrefab, listRoot);
            cellPool.Add(cell);
        }
    }

    private void OnClickItem(InventoryItem item)
    {
        selectedItem = item;

        for (int i = 0; i < cellPool.Count; i++)
        {
            bool selected = cellPool[i].Item == item;
            cellPool[i].SetSelected(selected);
        }

        ApplySelection(item);
    }

    private void ApplySelection(InventoryItem item)
    {
        if (item == null)
        {
            if (detailIcon != null) detailIcon.sprite = null;
            if (detailName != null) detailName.text = "";
            if (ownedText != null) ownedText.text = "";
            if (levelText != null) levelText.text = "";
            if (enhanceCostText != null) enhanceCostText.text = "";

            // [조민희] 상위 등급 아이콘 초기화
            if (upgradeIcon != null) upgradeIcon.sprite = null;

            if (equipButton != null) equipButton.interactable = false;
            if (fusionButton != null) fusionButton.interactable = false;
            if (enhanceButton != null) enhanceButton.interactable = false;

            return;
        }

        EquipData equip = item.equipment;

        if (detailIcon != null)
        {
            detailIcon.sprite = ResolveSprite(equip.spriteName);
        }

        if (detailName != null)
        {
            detailName.text = equip.GetName();
        }

        int count = equipmentManager.GetCount(equip.GetId());
        // EquipmentManager에서 최신 레벨 조회 (강화 후 반영)
        int level = equipmentManager.GetLevel(equip.GetId());

        if (ownedText != null)
        {
            ownedText.text = $"보유: {count}개";
        }

        if (levelText != null)
        {
            levelText.text = $"+{level}";
        }

        // 버튼 상태 업데이트 (조민희 추가)
        UpdateButtonStates(equip);

        // [조민희] 현재 장비 아이콘 업데이트
        UpdateCurrentEquipIcon(equip);

        // [조민희] 상위 등급 아이콘 로드
        LoadUpgradeIcon(equip);
    }

    /// <summary>버튼 상태 업데이트 (조민희 추가)</summary>
    private void UpdateButtonStates(EquipData equip)
    {
        if (equip == null)
        {
            if (equipButton != null) equipButton.interactable = false;
            if (fusionButton != null) fusionButton.interactable = false;
            if (enhanceButton != null) enhanceButton.interactable = false;
            return;
        }

        string equipId = equip.GetId();

        // 장착 버튼
        if (equipButton != null)
        {
            equipButton.interactable = equipmentManager.GetCount(equipId) > 0;
        }

        // 융합 버튼 (같은 장비 5개 필요)
        if (fusionButton != null)
        {
            bool canFuse = equipmentManager.CanFuse(equipId);
            fusionButton.interactable = canFuse;
        }

        // 강화 버튼
        if (enhanceButton != null)
        {
            bool canEnhance = EnhanceManager.Instance != null && EnhanceManager.Instance.CanEnhance(equip);
            enhanceButton.interactable = canEnhance;

            // 강화 비용 표시
            if (enhanceCostText != null)
            {
                if (canEnhance)
                {
                    long cost = EnhanceManager.Instance.GetEnhanceCost(equip);
                    enhanceCostText.text = $"${cost:N0}";
                }
                else
                {
                    enhanceCostText.text = "MAX";
                }
            }
        }
    }

    private void OnClickEquip()
    {
        if (selectedItem == null) return;

        equipmentManager.Equip(selectedItem.equipment.GetId());
    }

    /// <summary>융합 버튼 클릭 (조민희 추가)</summary>
    private void OnClickFusion()
    {
        if (selectedItem == null) return;

        EquipData equip = selectedItem.equipment;
        string equipId = equip.GetId();

        // EquipmentManager의 Fuse 메서드 사용
        if (equipmentManager.Fuse(equipId, out string resultId))
        {
            Debug.Log($"[EquipPopup] 융합 성공: {equip.GetName()} → {resultId}");
            // 성공 시 선택 해제
            selectedItem = null;
            ApplySelection(null);
            RebuildList();
        }
        else
        {
            string reason = equipmentManager.GetCannotFuseReason(equipId);
            Debug.LogWarning($"[EquipPopup] 융합 실패: {reason}");
        }
    }

    /// <summary>강화 버튼 클릭 (조민희 추가)</summary>
    private void OnClickEnhance()
    {
        if (selectedItem == null) return;
        if (EnhanceManager.Instance == null)
        {
            Debug.LogError("[EquipPopup] EnhanceManager.Instance가 null입니다.");
            return;
        }

        EquipData equip = selectedItem.equipment;

        if (EnhanceManager.Instance.TryEnhance(equip))
        {
            // 성공 시 UI 갱신
            int newLevel = equipmentManager.GetLevel(equip.GetId());
            selectedItem = new InventoryItem(equip, newLevel);
            ApplySelection(selectedItem);
        }
    }

    /// <summary>융합 완료 이벤트 핸들러 (조민희 추가)</summary>
    private void OnFusionComplete(string materialId, string resultId)
    {
        Debug.Log($"[EquipPopup] 융합 완료 이벤트: {materialId} → {resultId}");
        RebuildList();
    }

    /// <summary>강화 완료 이벤트 핸들러 (조민희 추가)</summary>
    private void OnEquipmentEnhanced(string equipId, int newLevel)
    {
        Debug.Log($"[EquipPopup] 강화 완료 이벤트: {equipId} → Lv.{newLevel}");
        if (selectedItem != null && selectedItem.equipment.GetId() == equipId)
        {
            ApplySelection(selectedItem);
        }
    }

    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        // [조민희] AssetBundleLoader를 통해 스프라이트 로드
        return SlayerLegend.Resource.ResourceManager.Instance.LoadSprite(spriteName);
    }

    #region 테스트용 컨텍스트 메뉴 (조민희 추가)

    [ContextMenu("테스트: 랜덤 장비 추가 (무작위 수량)")]
    public void DebugAddRandomEquipment()
    {
        if (DataManager.CurrentSaveData == null)
        {
            Debug.LogError("[EquipPopup] CurrentSaveData가 null입니다.");
            return;
        }

        if (DataManager.CurrentSaveData.equipInfo == null)
        {
            DataManager.CurrentSaveData.equipInfo = new Dictionary<string, Possesion>();
        }

        var allWeapons = DataManager.weapons.GetAll();
        var allAccessories = DataManager.accessories.GetAll();

        int addedCount = 0;

        // 무기: 50% 확률로 추가, 수량 1~10 랜덤
        foreach (var weapon in allWeapons)
        {
            if (UnityEngine.Random.value < 0.5f)
            {
                int count = UnityEngine.Random.Range(1, 11);
                int level = UnityEngine.Random.Range(1, 6);

                if (!DataManager.CurrentSaveData.equipInfo.ContainsKey(weapon.GetId()))
                {
                    DataManager.CurrentSaveData.equipInfo[weapon.GetId()] = new Possesion
                    {
                        count = count,
                        level = level
                    };
                }
                else
                {
                    DataManager.CurrentSaveData.equipInfo[weapon.GetId()].count += count;
                }
                addedCount++;
            }
        }

        // 악세서리: 30% 확률로 추가, 수량 1~5 랜덤
        foreach (var accessory in allAccessories)
        {
            if (UnityEngine.Random.value < 0.3f)
            {
                int count = UnityEngine.Random.Range(1, 6);
                int level = UnityEngine.Random.Range(1, 4);

                if (!DataManager.CurrentSaveData.equipInfo.ContainsKey(accessory.GetId()))
                {
                    DataManager.CurrentSaveData.equipInfo[accessory.GetId()] = new Possesion
                    {
                        count = count,
                        level = level
                    };
                }
                else
                {
                    DataManager.CurrentSaveData.equipInfo[accessory.GetId()].count += count;
                }
                addedCount++;
            }
        }

        Debug.Log($"[EquipPopup] 랜덤 장비 추가 완료: {addedCount}종류");
        RebuildList();
    }

    [ContextMenu("테스트: 모든 장비 10개씩 추가")]
    public void DebugAddAllEquipment()
    {
        if (DataManager.CurrentSaveData == null)
        {
            Debug.LogError("[EquipPopup] CurrentSaveData가 null입니다.");
            return;
        }

        if (DataManager.CurrentSaveData.equipInfo == null)
        {
            DataManager.CurrentSaveData.equipInfo = new Dictionary<string, Possesion>();
        }

        var allWeapons = DataManager.weapons.GetAll();
        var allAccessories = DataManager.accessories.GetAll();

        int addedCount = 0;

        // 모든 무기 10개씩
        foreach (var weapon in allWeapons)
        {
            string equipId = weapon.GetId();
            if (!DataManager.CurrentSaveData.equipInfo.ContainsKey(equipId))
            {
                DataManager.CurrentSaveData.equipInfo[equipId] = new Possesion
                {
                    count = 10,
                    level = 1
                };
            }
            else
            {
                DataManager.CurrentSaveData.equipInfo[equipId].count += 10;
            }
            addedCount++;
        }

        // 모든 악세서리 10개씩
        foreach (var accessory in allAccessories)
        {
            string equipId = accessory.GetId();
            if (!DataManager.CurrentSaveData.equipInfo.ContainsKey(equipId))
            {
                DataManager.CurrentSaveData.equipInfo[equipId] = new Possesion
                {
                    count = 10,
                    level = 1
                };
            }
            else
            {
                DataManager.CurrentSaveData.equipInfo[equipId].count += 10;
            }
            addedCount++;
        }

        Debug.Log($"[EquipPopup] 모든 장비 10개씩 추가 완료: {addedCount}종류");
        RebuildList();
    }

    [ContextMenu("테스트: 장비 데이터 전체 삭제")]
    public void DebugClearAllEquipment()
    {
        if (DataManager.CurrentSaveData != null && DataManager.CurrentSaveData.equipInfo != null)
        {
            DataManager.CurrentSaveData.equipInfo.Clear();
            Debug.Log("[EquipPopup] 장비 데이터 전체 삭제 완료");
            selectedItem = null;
            ApplySelection(null);
            RebuildList();
        }
    }


    #endregion

    #region 융합 결과 아이콘 로드 (조민희 추가)
    /// <summary>
    /// 상위 등급 장비 아이콘을 로드하여 표시
    /// </summary>
    private void LoadUpgradeIcon(EquipData currentEquip)
    {
        if (upgradeIcon == null && upgradeName == null && upgradeNumber == null) return;
        if (currentEquip == null)
        {
            if (upgradeIcon != null) upgradeIcon.sprite = null;
            if (upgradeName != null) upgradeName.text = "";
            if (upgradeNumber != null) upgradeNumber.text = "";
            return;
        }

        // EquipmentManager를 통해 다음 순서 장비 찾기
        EquipData upgradeEquip = equipmentManager.FindNextGradeData(currentEquip);

        if (upgradeEquip != null)
        {
            // 다음 순서 장비의 아이콘 로드
            if (upgradeIcon != null) upgradeIcon.sprite = ResolveSprite(upgradeEquip.spriteName);
            // 다음 순서 장비의 이름 표시
            if (upgradeName != null) upgradeName.text = upgradeEquip.GetName();
            // 다음 순서 장비의 개수 표시 (다음 장비 현재 보유 + 융합으로 생성될 개수)
            if (upgradeNumber != null)
            {
                // [조민희] 다음 장비의 현재 보유 개수
                int nextEquipOwnedCount = equipmentManager.GetCount(upgradeEquip.GetId());
                // 현재 장비 보유 개수에서 융합으로 얻을 수 있는 개수 (소수점 내림)
                int currentOwnedCount = equipmentManager.GetCount(currentEquip.GetId());
                int fusionResultCount = Mathf.FloorToInt(currentOwnedCount / FUSION_REQUIRED_COUNT);
                // 최종 개수 = 다음 장비 현재 보유 + 융합으로 얻을 개수
                upgradeNumber.text = $"{nextEquipOwnedCount + fusionResultCount}(+{fusionResultCount})";
            }
        }
        else
        {
            if (upgradeIcon != null) upgradeIcon.sprite = null;
            if (upgradeName != null) upgradeName.text = "";
            if (upgradeNumber != null) upgradeNumber.text = "";
        }
    }
    
    #endregion

    /// <summary>
    /// 현재 장비 아이콘, 이름, 개수 업데이트 (조민희 추가)
    /// </summary>
    private void UpdateCurrentEquipIcon(EquipData equip)
    {
        if (currentEquipIcon == null && currentEquipName == null && currentEquipNumber == null) return;
        if (equip == null)
        {
            if (currentEquipIcon != null) currentEquipIcon.sprite = null;
            if (currentEquipName != null) currentEquipName.text = "";
            if (currentEquipNumber != null) currentEquipNumber.text = "";
            return;
        }

        if (currentEquipIcon != null) currentEquipIcon.sprite = ResolveSprite(equip.spriteName);
        if (currentEquipName != null) currentEquipName.text = equip.GetName();

        // 현재 장비 개수 표시 (현재 보유 - 융합에 필요한 개수)
        if (currentEquipNumber != null)
        {
            int ownedCount = equipmentManager.GetCount(equip.GetId());
            currentEquipNumber.text = $"{ownedCount}(-{FUSION_REQUIRED_COUNT})";
        }
    }
}