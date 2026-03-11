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

    [Header("융합 수량 조절 (조민희 추가)")]
    [SerializeField] private Button increaseAmountButton; // + 버튼
    [SerializeField] private Button decreaseAmountButton; // - 버튼
    [SerializeField] private TMP_Text fusionAmountText; // 생산량 숫자

    private const int FUSION_REQUIRED_COUNT = 5; // 융합에 필요한 장비 개수
    private int fusionAmount = 1; // 융합 생산량

    [Header("버튼")]
    [SerializeField] private Button equipButton;//장착 버튼
    [SerializeField] private Button fusionButton;//융합 버튼
    [SerializeField] private Button enhanceButton;//강화 버튼
    [SerializeField] private TMP_Text enhanceCostText;//강화 비용 텍스트

    [Header("강화 효과 텍스트 (조민희 추가)")]
    [SerializeField] private TMP_Text installationEffectText; // 장착 효과 수치 (예: 3%)
    [SerializeField] private TMP_Text holdingEffectText1;     // 보유 효과 1 (AttackBoost)
    [SerializeField] private TMP_Text holdingEffectText2;     // 보유 효과 2 (CriticalDamage - Rare부터)
    [SerializeField] private TMP_Text holdingEffectText3;     // 보유 효과 3 (GoldGain - Myth부터)

    [SerializeField] private string spritePath = "Icons";//아이콘 경로

    [Header("AccessoryBinder (조민희 추가)")]
    [SerializeField] private EquipAccessoryBinder accessoryBinder; // 악세서리 전용 바인더

    private readonly List<EquipItemCell> cellPool = new List<EquipItemCell>();

    private EquipType currentType;
    private InventoryItem selectedItem;

    private void Awake()
    {
        if (equipmentManager == null)
        {
            equipmentManager = EquipmentManager.Instance;
        }

        // [조민희] accessoryBinder가 null이면 같은 게임오브젝트 또는 부모에서 자동으로 찾기
        if (accessoryBinder == null)
        {
            accessoryBinder = GetComponent<EquipAccessoryBinder>();
            if (accessoryBinder == null)
            {
                accessoryBinder = GetComponentInParent<EquipAccessoryBinder>();
            }
            if (accessoryBinder != null)
            {
                Debug.Log($"[EquipPopupContentController] accessoryBinder 자동 할당됨");
            }
        }

        // [조민희] listRoot가 null이면 자동으로 찾기 (Content 또는 ListRoot 이름의 자식 검색)
        if (listRoot == null)
        {
            listRoot = FindListRoot();
            if (listRoot != null)
            {
                Debug.Log($"[EquipPopupContentController] listRoot 자동 할당됨: {listRoot.name}");
            }
            else
            {
                Debug.LogWarning("[EquipPopupContentController] listRoot를 찾을 수 없습니다! 인스펙터에서 수동으로 연결해주세요.");
            }
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

        // 융합 수량 조절 버튼 (조민희 추가)
        if (increaseAmountButton != null)
        {
            increaseAmountButton.onClick.AddListener(OnClickIncreaseAmount);
        }
        if (decreaseAmountButton != null)
        {
            decreaseAmountButton.onClick.AddListener(OnClickDecreaseAmount);
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

    /// <summary>
    /// [조민희] listRoot 자동 찾기 - Content, ListRoot, ScrollView Content 등의 이름을 가진 Transform 검색
    /// </summary>
    private Transform FindListRoot()
    {
        // 1. 직접 자식에서 "Content" 이름 검색
        Transform found = transform.Find("Content");
        if (found != null) return found;

        // 2. 모든 자식에서 검색 (깊이 우선)
        found = FindDeepChild(transform, "Content");
        if (found != null) return found;

        // 3. "ListRoot" 검색
        found = FindDeepChild(transform, "ListRoot");
        if (found != null) return found;

        // 4. "Scroll View/Viewport/Content" 패턴 검색
        found = FindDeepChild(transform, "Viewport");
        if (found != null)
        {
            Transform content = found.Find("Content");
            if (content != null) return content;
        }

        // 5. ScrollRect가 있는 자식의 Content 찾기
        var scrollRects = GetComponentsInChildren<UnityEngine.UI.ScrollRect>(true);
        foreach (var scroll in scrollRects)
        {
            if (scroll.content != null)
            {
                return scroll.content;
            }
        }

        return null;
    }

    /// <summary>
    /// [조민희] 깊이 우선 자식 검색
    /// </summary>
    private Transform FindDeepChild(Transform parent, string name)
    {
        // 직접 자식 확인
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
        }

        // 재귀적으로 검색
        foreach (Transform child in parent)
        {
            Transform found = FindDeepChild(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    public void SetEquipTab(EquipTab tab, string initialEquipId = null)
    {
        Debug.Log($"[EquipPopup] SetEquipTab 시작 - tab: {tab}, initialEquipId: {initialEquipId}");

        if (tab == EquipTab.Weapon)
        {
            currentType = EquipType.Weapon;
        }
        else
        {
            currentType = EquipType.Accessorie;
        }

        selectedItem = null;

        // [조민희] initialEquipId가 있으면 해당 장비 선택, 없으면 첫 번째 아이템 선택
        if (equipmentManager != null)
        {
            if (!string.IsNullOrEmpty(initialEquipId))
            {
                // initialEquipId로 선택
                Debug.Log($"[EquipPopup] GetEquipData 호출 전 - initialEquipId: {initialEquipId}");
                EquipData equip = equipmentManager.GetEquipData(initialEquipId);
                Debug.Log($"[EquipPopup] GetEquipData 결과 - equip: {(equip != null ? equip.GetId() : "NULL")}");

                if (equip != null)
                {
                    int level = equipmentManager.GetLevel(initialEquipId);
                    selectedItem = new InventoryItem(equip, level);
                    Debug.Log($"[EquipPopup] SetEquipTab - initialEquipId로 선택: {initialEquipId}, selectedItem.equipment.GetId(): {selectedItem.equipment.GetId()}");
                }
                else
                {
                    Debug.LogWarning($"[EquipPopup] SetEquipTab - initialEquipId를 찾을 수 없음: {initialEquipId}");
                }
            }

            // initialEquipId가 없거나 찾지 못한 경우 첫 번째 아이템 선택
            if (selectedItem == null)
            {
                IReadOnlyList<InventoryItem> inventory = equipmentManager.GetInventory(currentType);
                if (inventory.Count > 0)
                {
                    selectedItem = inventory[0];
                    Debug.Log($"[EquipPopup] SetEquipTab - 첫 번째 아이템 선택: {selectedItem.equipment.GetId()}");
                }
            }
        }

        Debug.Log($"[EquipPopup] RebuildList 호출 전 - selectedItem: {(selectedItem != null ? selectedItem.equipment.GetId() : "NULL")}");
        RebuildList();
        Debug.Log($"[EquipPopup] RebuildList 호출 후 - selectedItem: {(selectedItem != null ? selectedItem.equipment.GetId() : "NULL")}");

        // [조민희] 선택된 아이템이 있으면 UI 갱신
        if (selectedItem != null)
        {
            ApplySelection(selectedItem);
        }
        else
        {
            ApplySelection(null);
        }
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
        if (equipmentManager == null)
        {
            Debug.LogWarning("[EquipPopup] EquipmentManager가 null입니다!");
            return;
        }

        IReadOnlyList<InventoryItem> inventory = equipmentManager.GetInventory(currentType);

        // [조민희] 디버그: 인벤토리 내용 확인
        Debug.Log($"[EquipPopup] RebuildList - currentType: {currentType}, inventory.Count: {inventory.Count}");

        // [조민희] 디버그: cellPrefab, listRoot 확인
        Debug.Log($"[EquipPopup] cellPrefab: {(cellPrefab != null ? cellPrefab.name : "NULL")}, listRoot: {(listRoot != null ? listRoot.name : "NULL")}, cellPool.Count: {cellPool.Count}");

        foreach (var item in inventory)
        {
            Debug.Log($"  [EquipPopup] 아이템: {item.equipment.GetId()} - {item.equipment.GetName()}");
        }

        // [조민희] cellPrefab이 있을 때만 리스트 생성 시도
        if (cellPrefab != null && listRoot != null)
        {
            EnsurePool(inventory.Count);

            // [조민희] 디버그: 풀 생성 후 확인
            Debug.Log($"[EquipPopup] EnsurePool 후 cellPool.Count: {cellPool.Count}");

            for (int i = 0; i < cellPool.Count; i++)
            {
                bool active = i < inventory.Count;

                cellPool[i].gameObject.SetActive(active);

                if (active)
                {
                    Debug.Log($"[EquipPopup] Bind 호출 - 인덱스: {i}, 아이템: {inventory[i].equipment.GetId()}");
                    cellPool[i].Bind(inventory[i], OnClickItem, ResolveSprite);
                }
            }
        }
        else
        {
            // [조민희] cellPrefab이 없으면 리스트 없이 상세 정보만 표시
            if (cellPrefab == null)
            {
                Debug.LogWarning("[EquipPopupContentController] cellPrefab이 null - 리스트 없이 상세 정보만 표시");
            }
        }
    }

    private void EnsurePool(int count)
    {
        // [조민희] cellPrefab 또는 listRoot가 null이면 생성하지 않음
        if (cellPrefab == null)
        {
            Debug.LogWarning("[EquipPopupContentController] cellPrefab이 null - 리스트 생성 건너뜀");
            return;
        }

        if (listRoot == null)
        {
            // 다시 한번 listRoot 찾기 시도
            listRoot = FindListRoot();
            if (listRoot == null)
            {
                Debug.LogWarning("[EquipPopupContentController] listRoot가 null - 리스트 생성 건너뜀");
                return;
            }
        }

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
        Debug.Log($"[EquipPopup] ApplySelection 호출 - item: {(item != null ? item.equipment?.GetId() : "NULL")}");

        if (item == null)
        {
            if (detailIcon != null) detailIcon.sprite = null;
            if (detailName != null) detailName.text = "";
            if (ownedText != null) ownedText.text = "";
            if (levelText != null) levelText.text = "";
            if (enhanceCostText != null) enhanceCostText.text = "";

            // [조민희] 상위 등급 아이콘 초기화
            if (upgradeIcon != null) upgradeIcon.sprite = null;
            if (upgradeName != null) upgradeName.text = "";
            if (upgradeNumber != null) upgradeNumber.text = "";
            if (currentEquipIcon != null) currentEquipIcon.sprite = null;
            if (currentEquipName != null) currentEquipName.text = "";
            if (currentEquipNumber != null) currentEquipNumber.text = "";

            // [조민희] 융합 수량 초기화
            fusionAmount = 1;
            UpdateFusionAmountUI();

            // [조민희] 강화 효과 텍스트 초기화
            if (installationEffectText != null) installationEffectText.text = "";
            if (holdingEffectText1 != null) holdingEffectText1.text = "";
            if (holdingEffectText2 != null) holdingEffectText2.text = "";
            if (holdingEffectText3 != null) holdingEffectText3.text = "";

            if (equipButton != null) equipButton.interactable = false;
            if (fusionButton != null) fusionButton.interactable = false;
            if (enhanceButton != null) enhanceButton.interactable = false;

            return;
        }

        EquipData equip = item.equipment;

        if (detailIcon != null)
        {
            Sprite loadedSprite = ResolveSprite(equip.spriteName);
            Debug.Log($"[EquipPopup] ResolveSprite 호출 - spriteName: {equip.spriteName}, sprite: {(loadedSprite != null ? loadedSprite.name : "NULL")}");
            detailIcon.sprite = loadedSprite;
        }

        if (detailName != null)
        {
            detailName.text = equip.GetName();
            Debug.Log($"[EquipPopup] detailName 설정 - {equip.GetName()}");
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

        // [조민희] 융합 수량 초기화 및 UI 업데이트
        fusionAmount = 1;
        UpdateFusionAmountUI();

        // [조민희] 현재 장비 아이콘 업데이트
        UpdateCurrentEquipIcon(equip);

        // [조민희] 상위 등급 아이콘 로드
        LoadUpgradeIcon(equip);

        // [조민희] 장비 효과 텍스트 업데이트
        UpdateEffectTexts(equip, level);

        // [조민희] 악세서리인 경우 EquipAccessoryBinder 사용
        if (currentType == EquipType.Accessorie && accessoryBinder != null)
        {
            accessoryBinder.Bind(equip, level);
        }
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
        if (fusionAmount <= 0)
        {
            Debug.LogWarning("[EquipPopup] 융합 수량이 0입니다.");
            return;
        }

        EquipData equip = selectedItem.equipment;
        string equipId = equip.GetId();

        int successCount = 0;
        string lastResultId = null;

        // fusionAmount만큼 융합 수행
        for (int i = 0; i < fusionAmount; i++)
        {
            if (equipmentManager.Fuse(equipId, out string resultId))
            {
                successCount++;
                lastResultId = resultId;
            }
            else
            {
                break; // 재료 부족 시 중단
            }
        }

        if (successCount > 0)
        {
            Debug.Log($"[EquipPopup] 융합 성공: {equip.GetName()} x{successCount * FUSION_REQUIRED_COUNT} → {lastResultId} x{successCount}");
            // 성공 시 선택 해제
            selectedItem = null;
            fusionAmount = 1; // 생산량 초기화
            ApplySelection(null);
            RebuildList();
        }
        else
        {
            string reason = equipmentManager.GetCannotFuseReason(equipId);
            Debug.LogWarning($"[EquipPopup] 융합 실패: {reason}");
        }
    }

    /// <summary>융합 수량 증가 (+버튼) (조민희 추가)</summary>
    private void OnClickIncreaseAmount()
    {
        if (selectedItem == null) return;

        int maxAmount = GetMaxFusionAmount();
        if (fusionAmount < maxAmount)
        {
            fusionAmount++;
            UpdateFusionAmountUI();
            UpdateCurrentEquipIcon(selectedItem.equipment);
            LoadUpgradeIcon(selectedItem.equipment);
        }
    }

    /// <summary>융합 수량 감소 (-버튼) (조민희 추가)</summary>
    private void OnClickDecreaseAmount()
    {
        if (fusionAmount > 0)
        {
            fusionAmount--;
            UpdateFusionAmountUI();
            if (selectedItem != null)
            {
                UpdateCurrentEquipIcon(selectedItem.equipment);
                LoadUpgradeIcon(selectedItem.equipment);
            }
        }
    }

    /// <summary>최대 융합 가능 수량 계산 (조민희 추가)</summary>
    private int GetMaxFusionAmount()
    {
        if (selectedItem == null || equipmentManager == null) return 0;

        int ownedCount = equipmentManager.GetCount(selectedItem.equipment.GetId());
        return ownedCount / FUSION_REQUIRED_COUNT;
    }

    /// <summary>융합 수량 UI 업데이트 (조민희 추가)</summary>
    private void UpdateFusionAmountUI()
    {
        if (fusionAmountText != null)
        {
            fusionAmountText.text = fusionAmount.ToString();
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
                // 최종 개수 = 다음 장비 현재 보유 + 융합으로 얻을 개수 (fusionAmount 사용)
                upgradeNumber.text = $"{nextEquipOwnedCount + fusionAmount}(+{fusionAmount})";
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
            int consumeAmount = fusionAmount * FUSION_REQUIRED_COUNT;
            currentEquipNumber.text = $"{ownedCount}(-{consumeAmount})";
        }
    }

    /// <summary>
    /// 장비 효과 텍스트 업데이트 (조민희 추가)
    /// </summary>
    private void UpdateEffectTexts(EquipData equip, int level)
    {
        // 장착 효과 (Installation Effect)
        if (installationEffectText != null)
        {
            ItemEffect equipEffect = equip.GetEquipEffect();
            if (equipEffect != null)
            {
                float value = equipEffect.initValue + (level - 1) * equipEffect.levelUpValue;
                installationEffectText.text = FormatEffectValue(value, equipEffect.type);
            }
            else
            {
                installationEffectText.text = "";
            }
        }

        // 보유 효과 (Holding Effects) - 여러 개 표시
        var holdEffects = equip.GetHoldEffects();

        // 보유 효과 1 - AttackBoost (모든 등급)
        if (holdingEffectText1 != null)
        {
            if (holdEffects != null && holdEffects.Count >= 1)
            {
                ItemEffect effect = holdEffects[0];
                float value = effect.initValue + (level - 1) * effect.levelUpValue;
                holdingEffectText1.text = FormatEffectValue(value, effect.type);
            }
            else
            {
                holdingEffectText1.text = "";
            }
        }

        // 보유 효과 2 - CriticalDamage (Rare부터)
        if (holdingEffectText2 != null)
        {
            if (holdEffects != null && holdEffects.Count >= 2)
            {
                ItemEffect effect = holdEffects[1];
                float value = effect.initValue + (level - 1) * effect.levelUpValue;
                holdingEffectText2.text = FormatEffectValue(value, effect.type);
            }
            else
            {
                holdingEffectText2.text = "";
            }
        }

        // 보유 효과 3 - GoldGain (Myth부터)
        if (holdingEffectText3 != null)
        {
            if (holdEffects != null && holdEffects.Count >= 3)
            {
                ItemEffect effect = holdEffects[2];
                float value = effect.initValue + (level - 1) * effect.levelUpValue;
                holdingEffectText3.text = FormatEffectValue(value, effect.type);
            }
            else
            {
                holdingEffectText3.text = "";
            }
        }
    }

    /// <summary>
    /// 효과 수치를 포맷팅 (조민희 추가)
    /// </summary>
    private string FormatEffectValue(float value, EffectType effectType)
    {
        // 효과 타입에 따라 단위 결정
        switch (effectType)
        {
            case EffectType.AttackBoost:
            case EffectType.CriticalDamage:
            case EffectType.GoldGain:
            case EffectType.ExpGain:
                return $"{value:0.##}%";
            case EffectType.HealthBoost:
            case EffectType.ManaBoost:
                return $"{value:0.##}";
            default:
                return $"{value:0.##}%";
        }
    }
}