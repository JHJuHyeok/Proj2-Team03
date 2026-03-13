using SlayerLegend.Equipment;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("상단 장비 정보")]
    [SerializeField] private TMP_Text rarityText;//신화
    [SerializeField] private TMP_Text itemNameText;//귀멸의 검 - 데몬 슬레이어
    [SerializeField] private TMP_Text enhanceText;//+200
    [SerializeField] private TMP_Text gradeText;//1등급
    [SerializeField] private TMP_Text countText;//3/5

    [Header("융합 아이콘 (조민희 추가)")]
    [SerializeField] private Image currentEquipIcon;
    [SerializeField] private TMP_Text currentEquipName;
    [SerializeField] private TMP_Text currentEquipNumber;
    [SerializeField] private Image upgradeIcon;
    [SerializeField] private TMP_Text upgradeName;
    [SerializeField] private TMP_Text upgradeNumber;

    [Header("융합 수량 조절 (조민희 추가)")]
    [SerializeField] private Button increaseAmountButton;
    [SerializeField] private Button decreaseAmountButton;
    [SerializeField] private TMP_Text fusionAmountText;

    private const int FUSION_REQUIRED_COUNT = 5;
    private int fusionAmount = 1;

    [Header("버튼")]
    [SerializeField] private Button equipButton;
    [SerializeField] private Button fusionButton;
    [SerializeField] private Button enhanceButton;
    [SerializeField] private TMP_Text enhanceCostText;

    [Header("강화 효과 텍스트 (조민희 추가)")]
    [SerializeField] private TMP_Text installationEffectText;
    [SerializeField] private TMP_Text holdingEffectText1;
    [SerializeField] private TMP_Text holdingEffectText2;
    [SerializeField] private TMP_Text holdingEffectText3;

    [Header("효과 이름 텍스트 추가")]
    [SerializeField] private TMP_Text installationEffectLabel;
    [SerializeField] private TMP_Text holdingEffectLabel1;
    [SerializeField] private TMP_Text holdingEffectLabel2;
    [SerializeField] private TMP_Text holdingEffectLabel3;

    [SerializeField] private string spritePath = "Icons";

    [Header("AccessoryBinder (조민희 추가)")]
    [SerializeField] private EquipAccessoryBinder accessoryBinder;

    private readonly List<EquipItemCell> cellPool = new List<EquipItemCell>();

    private EquipType currentType;
    private InventoryItem selectedItem;

    private void Awake()
    {
        if (equipmentManager == null)
        {
            equipmentManager = EquipmentManager.Instance;
        }

        if (accessoryBinder == null)
        {
            accessoryBinder = GetComponent<EquipAccessoryBinder>();
            if (accessoryBinder == null)
            {
                accessoryBinder = GetComponentInParent<EquipAccessoryBinder>();
            }

            if (accessoryBinder != null)
            {
                Debug.Log("[EquipPopupContentController] accessoryBinder 자동 할당됨");
            }
        }

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

        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnClickEquip);
        }

        if (fusionButton != null)
        {
            fusionButton.onClick.AddListener(OnClickFusion);
        }

        if (enhanceButton != null)
        {
            enhanceButton.onClick.AddListener(OnClickEnhance);
        }

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

    private Transform FindListRoot()
    {
        Transform found = transform.Find("Content");
        if (found != null) return found;

        found = FindDeepChild(transform, "Content");
        if (found != null) return found;

        found = FindDeepChild(transform, "ListRoot");
        if (found != null) return found;

        found = FindDeepChild(transform, "Viewport");
        if (found != null)
        {
            Transform content = found.Find("Content");
            if (content != null) return content;
        }

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

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
        }

        foreach (Transform child in parent)
        {
            Transform found = FindDeepChild(child, name);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    public void SetEquipTab(EquipTab tab, string initialEquipId = null)
    {
        Debug.Log($"[EquipPopupContentController] SetEquipTab 시작 - tab: {tab}, initialEquipId: {initialEquipId}");

        if (tab == EquipTab.Weapon)
        {
            currentType = EquipType.Weapon;
        }
        else
        {
            currentType = EquipType.Accessorie;
        }

        Debug.Log($"[EquipPopupContentController] currentType 설정 완료: {currentType}");

        selectedItem = null;

        if (equipmentManager != null)
        {
            if (!string.IsNullOrEmpty(initialEquipId))
            {
                Debug.Log($"[EquipPopupContentController] GetEquipData 호출 전 - initialEquipId: {initialEquipId}");
                EquipData equip = equipmentManager.GetEquipData(initialEquipId);
                Debug.Log($"[EquipPopupContentController] GetEquipData 결과 - equip: {(equip != null ? equip.GetId() : "NULL")}");

                if (equip != null)
                {
                    int level = equipmentManager.GetLevel(initialEquipId);
                    selectedItem = new InventoryItem(equip, level);
                    Debug.Log($"[EquipPopupContentController] initialEquipId로 선택 성공: {initialEquipId}, level: {level}");
                }
                else
                {
                    Debug.LogWarning($"[EquipPopupContentController] initialEquipId를 찾을 수 없음: {initialEquipId}");
                }
            }

            if (selectedItem == null)
            {
                IReadOnlyList<InventoryItem> inventory = equipmentManager.GetInventory(currentType);
                Debug.Log($"[EquipPopupContentController] fallback inventory.Count: {inventory.Count}");

                if (inventory.Count > 0)
                {
                    selectedItem = inventory[0];
                    Debug.Log($"[EquipPopupContentController] 첫 번째 아이템 fallback 선택: {selectedItem.equipment.GetId()}");
                }
                else
                {
                    Debug.LogWarning("[EquipPopupContentController] 인벤토리가 비어있음");
                }
            }
        }
        else
        {
            Debug.LogWarning("[EquipPopupContentController] equipmentManager가 null입니다.");
        }

        Debug.Log($"[EquipPopupContentController] RebuildList 호출 전 - selectedItem: {(selectedItem != null ? selectedItem.equipment.GetId() : "NULL")}");
        RebuildList();
        Debug.Log($"[EquipPopupContentController] RebuildList 호출 후 - selectedItem: {(selectedItem != null ? selectedItem.equipment.GetId() : "NULL")}");

        if (selectedItem != null)
        {
            ApplySelection(selectedItem);
        }
        else
        {
            ApplySelection(null);
        }
    }

    public void SetInitialSelection(string equipId)
    {
        if (string.IsNullOrEmpty(equipId) || equipmentManager == null) return;

        EquipData equip = equipmentManager.GetEquipData(equipId);
        if (equip == null)
        {
            Debug.LogWarning($"[EquipPopupContentController] 초기 선택 장비를 찾을 수 없음: {equipId}");
            return;
        }

        int level = equipmentManager.GetLevel(equipId);
        selectedItem = new InventoryItem(equip, level);

        Debug.Log($"[EquipPopupContentController] SetInitialSelection 성공 - equipId: {equipId}, level: {level}");

        ApplySelection(selectedItem);
    }

    private void RebuildList()
    {
        if (equipmentManager == null)
        {
            Debug.LogWarning("[EquipPopupContentController] EquipmentManager가 null입니다!");
            return;
        }

        IReadOnlyList<InventoryItem> inventory = equipmentManager.GetInventory(currentType);

        Debug.Log($"[EquipPopupContentController] RebuildList - currentType: {currentType}, inventory.Count: {inventory.Count}");
        Debug.Log($"[EquipPopupContentController] cellPrefab: {(cellPrefab != null ? cellPrefab.name : "NULL")}, listRoot: {(listRoot != null ? listRoot.name : "NULL")}, cellPool.Count: {cellPool.Count}");

        foreach (var item in inventory)
        {
            Debug.Log($"  [EquipPopupContentController] 아이템: {item.equipment.GetId()} - {item.equipment.GetName()}");
        }

        if (cellPrefab != null && listRoot != null)
        {
            EnsurePool(inventory.Count);

            Debug.Log($"[EquipPopupContentController] EnsurePool 후 cellPool.Count: {cellPool.Count}");

            for (int i = 0; i < cellPool.Count; i++)
            {
                bool active = i < inventory.Count;
                cellPool[i].gameObject.SetActive(active);

                if (active)
                {
                    Debug.Log($"[EquipPopupContentController] Bind 호출 - 인덱스: {i}, 아이템: {inventory[i].equipment.GetId()}");
                    cellPool[i].Bind(inventory[i], OnClickItem, ResolveSprite);
                }
            }
        }
        else
        {
            if (cellPrefab == null)
            {
                Debug.LogWarning("[EquipPopupContentController] cellPrefab이 null - 리스트 없이 상세 정보만 표시");
            }

            if (listRoot == null)
            {
                Debug.LogWarning("[EquipPopupContentController] listRoot가 null - 리스트 없이 상세 정보만 표시");
            }
        }
    }

    private void EnsurePool(int count)
    {
        if (cellPrefab == null)
        {
            Debug.LogWarning("[EquipPopupContentController] cellPrefab이 null - 리스트 생성 건너뜀");
            return;
        }

        if (listRoot == null)
        {
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
            Debug.Log($"[EquipPopupContentController] cell 생성 - 현재 pool.Count: {cellPool.Count}");
        }
    }

    private void OnClickItem(InventoryItem item)
    {
        selectedItem = item;

        Debug.Log($"[EquipPopupContentController] OnClickItem - 선택된 아이템: {(item != null ? item.equipment.GetId() : "NULL")}");

        for (int i = 0; i < cellPool.Count; i++)
        {
            bool selected = cellPool[i].Item == item;
            cellPool[i].SetSelected(selected);
        }

        ApplySelection(item);
    }

    private void ApplySelection(InventoryItem item)
    {
        Debug.Log($"[EquipPopupContentController] ApplySelection 호출 - item: {(item != null ? item.equipment?.GetId() : "NULL")}");

        if (item == null)
        {
            if (detailIcon != null) detailIcon.sprite = null;
            if (detailName != null) detailName.text = "";
            if (ownedText != null) ownedText.text = "";
            if (levelText != null) levelText.text = "";
            if (enhanceCostText != null) enhanceCostText.text = "";

            if (rarityText != null) rarityText.text = "";
            if (itemNameText != null) itemNameText.text = "";
            if (enhanceText != null) enhanceText.text = "";
            if (gradeText != null) gradeText.text = "";
            if (countText != null) countText.text = "";

            if (upgradeIcon != null) upgradeIcon.sprite = null;
            if (upgradeName != null) upgradeName.text = "";
            if (upgradeNumber != null) upgradeNumber.text = "";
            if (currentEquipIcon != null) currentEquipIcon.sprite = null;
            if (currentEquipName != null) currentEquipName.text = "";
            if (currentEquipNumber != null) currentEquipNumber.text = "";

            fusionAmount = 1;
            UpdateFusionAmountUI();

            if (installationEffectText != null) installationEffectText.text = "";
            if (holdingEffectText1 != null) holdingEffectText1.text = "";
            if (holdingEffectText2 != null) holdingEffectText2.text = "";
            if (holdingEffectText3 != null) holdingEffectText3.text = "";

            if (installationEffectLabel != null) installationEffectLabel.text = "";
            if (holdingEffectLabel1 != null) holdingEffectLabel1.text = "";
            if (holdingEffectLabel2 != null) holdingEffectLabel2.text = "";
            if (holdingEffectLabel3 != null) holdingEffectLabel3.text = "";

            if (equipButton != null) equipButton.interactable = false;
            if (fusionButton != null) fusionButton.interactable = false;
            if (enhanceButton != null) enhanceButton.interactable = false;

            return;
        }

        EquipData equip = item.equipment;
        int count = equipmentManager.GetCount(equip.GetId());
        int level = equipmentManager.GetLevel(equip.GetId());

        if (detailIcon != null)
        {
            Sprite loadedSprite = ResolveSprite(equip.spriteName);
            Debug.Log($"[EquipPopupContentController] ResolveSprite 호출 - spriteName: {equip.spriteName}, sprite: {(loadedSprite != null ? loadedSprite.name : "NULL")}");
            detailIcon.sprite = loadedSprite;
        }

        if (detailName != null)
        {
            detailName.text = equip.GetName();
            Debug.Log($"[EquipPopupContentController] detailName 설정 - {equip.GetName()}");
        }

        if (ownedText != null)
        {
            ownedText.text = $"보유: {count}개";
        }

        if (levelText != null)
        {
            levelText.text = $"+{level}";
        }

        if (rarityText != null)
        {
            rarityText.text = GetRarityText(equip);
        }

        if (itemNameText != null)
        {
            itemNameText.text = equip.GetName();
        }

        if (enhanceText != null)
        {
            enhanceText.text = $"+{level}";
        }

        if (gradeText != null)
        {
            gradeText.text = GetGradeText(equip);
        }

        if (countText != null)
        {
            countText.text = $"{count}/{FUSION_REQUIRED_COUNT}";
        }

        UpdateButtonStates(equip);

        fusionAmount = 1;
        UpdateFusionAmountUI();

        UpdateCurrentEquipIcon(equip);
        LoadUpgradeIcon(equip);
        UpdateEffectTexts(equip, level);

        if (currentType == EquipType.Accessorie && accessoryBinder != null)
        {
            Debug.Log($"[EquipPopupContentController] accessoryBinder.Bind 호출 - equipId: {equip.GetId()}, level: {level}");
            accessoryBinder.Bind(equip, level);
        }
    }

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

        if (equipButton != null)
        {
            equipButton.interactable = equipmentManager.GetCount(equipId) > 0;
        }

        if (fusionButton != null)
        {
            bool canFuse = equipmentManager.CanFuse(equipId);
            fusionButton.interactable = canFuse;
        }

        if (enhanceButton != null)
        {
            bool canEnhance = EnhanceManager.Instance != null && EnhanceManager.Instance.CanEnhance(equip);
            enhanceButton.interactable = canEnhance;

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

    private void OnClickFusion()
    {
        if (selectedItem == null) return;
        if (fusionAmount <= 0)
        {
            Debug.LogWarning("[EquipPopupContentController] 융합 수량이 0입니다.");
            return;
        }

        EquipData equip = selectedItem.equipment;
        string equipId = equip.GetId();

        int successCount = 0;
        string lastResultId = null;

        for (int i = 0; i < fusionAmount; i++)
        {
            if (equipmentManager.Fuse(equipId, out string resultId))
            {
                successCount++;
                lastResultId = resultId;
            }
            else
            {
                break;
            }
        }

        if (successCount > 0)
        {
            Debug.Log($"[EquipPopupContentController] 융합 성공: {equip.GetName()} x{successCount * FUSION_REQUIRED_COUNT} → {lastResultId} x{successCount}");
            selectedItem = null;
            fusionAmount = 1;
            ApplySelection(null);
            RebuildList();
        }
        else
        {
            string reason = equipmentManager.GetCannotFuseReason(equipId);
            Debug.LogWarning($"[EquipPopupContentController] 융합 실패: {reason}");
        }
    }

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

    private int GetMaxFusionAmount()
    {
        if (selectedItem == null || equipmentManager == null) return 0;

        int ownedCount = equipmentManager.GetCount(selectedItem.equipment.GetId());
        return ownedCount / FUSION_REQUIRED_COUNT;
    }

    private void UpdateFusionAmountUI()
    {
        if (fusionAmountText != null)
        {
            fusionAmountText.text = fusionAmount.ToString();
        }
    }

    private void OnClickEnhance()
    {
        if (selectedItem == null) return;
        if (EnhanceManager.Instance == null)
        {
            Debug.LogError("[EquipPopupContentController] EnhanceManager.Instance가 null입니다.");
            return;
        }

        EquipData equip = selectedItem.equipment;

        if (EnhanceManager.Instance.TryEnhance(equip))
        {
            int newLevel = equipmentManager.GetLevel(equip.GetId());
            selectedItem = new InventoryItem(equip, newLevel);
            ApplySelection(selectedItem);
        }
    }

    private void OnFusionComplete(string materialId, string resultId)
    {
        Debug.Log($"[EquipPopupContentController] 융합 완료 이벤트: {materialId} → {resultId}");
        RebuildList();
    }

    private void OnEquipmentEnhanced(string equipId, int newLevel)
    {
        Debug.Log($"[EquipPopupContentController] 강화 완료 이벤트: {equipId} → Lv.{newLevel}");
        if (selectedItem != null && selectedItem.equipment.GetId() == equipId)
        {
            ApplySelection(selectedItem);
        }
    }

    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;
        return SlayerLegend.Resource.ResourceManager.Instance.LoadSprite(spriteName);
    }

    [ContextMenu("테스트: 랜덤 장비 추가 (무작위 수량)")]
    public void DebugAddRandomEquipment()
    {
        if (DataManager.CurrentSaveData == null)
        {
            Debug.LogError("[EquipPopupContentController] CurrentSaveData가 null입니다.");
            return;
        }

        if (DataManager.CurrentSaveData.equipInfo == null)
        {
            DataManager.CurrentSaveData.equipInfo = new Dictionary<string, Possesion>();
        }

        var allWeapons = DataManager.weapons.GetAll();
        var allAccessories = DataManager.accessories.GetAll();

        int addedCount = 0;

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

        Debug.Log($"[EquipPopupContentController] 랜덤 장비 추가 완료: {addedCount}종류");
        RebuildList();
    }

    [ContextMenu("테스트: 모든 장비 10개씩 추가")]
    public void DebugAddAllEquipment()
    {
        if (DataManager.CurrentSaveData == null)
        {
            Debug.LogError("[EquipPopupContentController] CurrentSaveData가 null입니다.");
            return;
        }

        if (DataManager.CurrentSaveData.equipInfo == null)
        {
            DataManager.CurrentSaveData.equipInfo = new Dictionary<string, Possesion>();
        }

        var allWeapons = DataManager.weapons.GetAll();
        var allAccessories = DataManager.accessories.GetAll();

        int addedCount = 0;

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

        Debug.Log($"[EquipPopupContentController] 모든 장비 10개씩 추가 완료: {addedCount}종류");
        RebuildList();
    }

    [ContextMenu("테스트: 장비 데이터 전체 삭제")]
    public void DebugClearAllEquipment()
    {
        if (DataManager.CurrentSaveData != null && DataManager.CurrentSaveData.equipInfo != null)
        {
            DataManager.CurrentSaveData.equipInfo.Clear();
            Debug.Log("[EquipPopupContentController] 장비 데이터 전체 삭제 완료");
            selectedItem = null;
            ApplySelection(null);
            RebuildList();
        }
    }

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

        EquipData upgradeEquip = equipmentManager.FindNextGradeData(currentEquip);

        if (upgradeEquip != null)
        {
            if (upgradeIcon != null) upgradeIcon.sprite = ResolveSprite(upgradeEquip.spriteName);
            if (upgradeName != null) upgradeName.text = upgradeEquip.GetName();
            if (upgradeNumber != null)
            {
                int nextEquipOwnedCount = equipmentManager.GetCount(upgradeEquip.GetId());
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

        if (currentEquipNumber != null)
        {
            int ownedCount = equipmentManager.GetCount(equip.GetId());
            int consumeAmount = fusionAmount * FUSION_REQUIRED_COUNT;
            currentEquipNumber.text = $"{ownedCount}(-{consumeAmount})";
        }
    }

    private void UpdateEffectTexts(EquipData equip, int level)
    {
        if (installationEffectText != null || installationEffectLabel != null)
        {
            ItemEffect equipEffect = equip.GetEquipEffect();
            if (equipEffect != null)
            {
                float value = equipEffect.initValue + (level - 1) * equipEffect.levelUpValue;

                if (installationEffectLabel != null)
                {
                    installationEffectLabel.text = GetEffectLabel(equipEffect.type);
                }

                if (installationEffectText != null)
                {
                    installationEffectText.text = FormatEffectValue(value, equipEffect.type);
                }
            }
            else
            {
                if (installationEffectLabel != null) installationEffectLabel.text = "";
                if (installationEffectText != null) installationEffectText.text = "";
            }
        }

        var holdEffects = equip.GetHoldEffects();

        if (holdingEffectText1 != null || holdingEffectLabel1 != null)
        {
            if (holdEffects != null && holdEffects.Count >= 1)
            {
                ItemEffect effect = holdEffects[0];
                float value = effect.initValue + (level - 1) * effect.levelUpValue;

                if (holdingEffectLabel1 != null)
                {
                    holdingEffectLabel1.text = GetEffectLabel(effect.type);
                }

                if (holdingEffectText1 != null)
                {
                    holdingEffectText1.text = FormatEffectValue(value, effect.type);
                }
            }
            else
            {
                if (holdingEffectLabel1 != null) holdingEffectLabel1.text = "";
                if (holdingEffectText1 != null) holdingEffectText1.text = "";
            }
        }

        if (holdingEffectText2 != null || holdingEffectLabel2 != null)
        {
            if (holdEffects != null && holdEffects.Count >= 2)
            {
                ItemEffect effect = holdEffects[1];
                float value = effect.initValue + (level - 1) * effect.levelUpValue;

                if (holdingEffectLabel2 != null)
                {
                    holdingEffectLabel2.text = GetEffectLabel(effect.type);
                }

                if (holdingEffectText2 != null)
                {
                    holdingEffectText2.text = FormatEffectValue(value, effect.type);
                }
            }
            else
            {
                if (holdingEffectLabel2 != null) holdingEffectLabel2.text = "";
                if (holdingEffectText2 != null) holdingEffectText2.text = "";
            }
        }

        if (holdingEffectText3 != null || holdingEffectLabel3 != null)
        {
            if (holdEffects != null && holdEffects.Count >= 3)
            {
                ItemEffect effect = holdEffects[2];
                float value = effect.initValue + (level - 1) * effect.levelUpValue;

                if (holdingEffectLabel3 != null)
                {
                    holdingEffectLabel3.text = GetEffectLabel(effect.type);
                }

                if (holdingEffectText3 != null)
                {
                    holdingEffectText3.text = FormatEffectValue(value, effect.type);
                }
            }
            else
            {
                if (holdingEffectLabel3 != null) holdingEffectLabel3.text = "";
                if (holdingEffectText3 != null) holdingEffectText3.text = "";
            }
        }
    }

    private string GetRarityText(EquipData equip)
    {
        if (equip == null) return "";

        string id = equip.GetId();

        if (id.Contains("myth") || id.Contains("Myth"))
            return "신화";

        if (id.Contains("legend") || id.Contains("Legend"))
            return "전설";

        if (id.Contains("hero") || id.Contains("Hero"))
            return "영웅";

        if (id.Contains("rare") || id.Contains("Rare"))
            return "희귀";

        return "일반";
    }

    private string GetGradeText(EquipData equip)
    {
        if (equip == null) return "";

        string id = equip.GetId();

        if (id.Contains("grade1") || id.Contains("_1"))
            return "1등급";

        if (id.Contains("grade2") || id.Contains("_2"))
            return "2등급";

        if (id.Contains("grade3") || id.Contains("_3"))
            return "3등급";

        if (id.Contains("grade4") || id.Contains("_4"))
            return "4등급";

        return "1등급";
    }

    private string GetEffectLabel(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.AttackBoost:
                return "공격력 증가";

            case EffectType.CriticalDamage:
                return "치명타 데미지 증가";

            case EffectType.GoldGain:
                return "골드 획득량 증가";

            case EffectType.HealthBoost:
                return "체력 및 체력 회복량 증가";

            case EffectType.ManaBoost:
                return "마나 및 마나 회복량 증가";

            case EffectType.ExpGain:
                return "경험치 획득량 증가";

            default:
                return "";
        }
    }

    private string FormatEffectValue(float value, EffectType effectType)
    {
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