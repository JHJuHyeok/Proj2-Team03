using UnityEngine;
using SlayerLegend.Equipment;

/*
[승문]
EquipPopupController
-장비 팝업 상세 전용 컨트롤러
-메인메뉴에서 전달받은 equipId 하나만 관리
-상단 장비 정보, 강화 탭, 융합 탭에 같은 equipId를 전달
-팝업 안에는 장비 리스트가 없으므로 ScrollView/Cell 관련 코드는 전부 제거
-탭 전환은 TabManager가 처리하므로 여기서는 패널 on/off를 하지 않음
*/
public class EquipPopupController : MonoBehaviour
{
    [Header("System Reference(씬/매니저 오브젝트)")]
    [SerializeField] private EquipmentManager systemEquipmentManager; // 최신 EquipmentManager

    [Header("Popup Detail(팝업 프리팹 내부)")]
    [SerializeField] private EquipPopup popupRoot;                               // 카테고리 아이콘 변경용
    [SerializeField] private EquipPopupJsonBinder popupJsonBinder;               // 상단 장비 정보 바인더
    [SerializeField] private EquipEnhancePanelController popupEnhanceController; // 강화 탭 상세
    [SerializeField] private EquipMergePanelController popupMergeController;     // 융합 탭 상세

    [Header("Resource")]
    [SerializeField] private string spritePath = "Icons"; // Resources/Icons 기준

    private string currentEquipId; // 현재 팝업에서 보고 있는 장비 ID
    private EquipTab currentTab;   // 현재 장비 타입(무기/악세)

    private void Awake()
    {
        // 최신 장비 매니저 자동 연결
        if (systemEquipmentManager == null)
        {
            systemEquipmentManager = EquipmentManager.Instance;
        }

        // 장비 관련 이벤트 연결
        if (systemEquipmentManager != null)
        {
            systemEquipmentManager.OnInventoryChanged += OnInventoryChanged;
            systemEquipmentManager.OnEquipmentEnhanced += OnEquipmentEnhanced;
            systemEquipmentManager.OnEquipmentEquipped += OnEquipmentEquipped;
            systemEquipmentManager.OnFusionComplete += OnFusionComplete;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (systemEquipmentManager != null)
        {
            systemEquipmentManager.OnInventoryChanged -= OnInventoryChanged;
            systemEquipmentManager.OnEquipmentEnhanced -= OnEquipmentEnhanced;
            systemEquipmentManager.OnEquipmentEquipped -= OnEquipmentEquipped;
            systemEquipmentManager.OnFusionComplete -= OnFusionComplete;
        }
    }

    /// <summary>
    /// 메인메뉴에서 클릭한 equipId를 팝업 전체에 반영
    /// </summary>
    public void SetEquipId(string equipId)
    {
        currentEquipId = equipId;

        if (systemEquipmentManager == null)
        {
            Debug.LogError("[EquipPopupController] systemEquipmentManager missing.");
            return;
        }

        if (string.IsNullOrEmpty(currentEquipId))
        {
            Debug.LogWarning("[EquipPopupController] currentEquipId is empty.");
            return;
        }

        EquipData equip = systemEquipmentManager.GetEquipData(currentEquipId);
        if (equip == null)
        {
            Debug.LogWarning("[EquipPopupController] EquipData not found: " + currentEquipId);
            return;
        }

        // 장비 타입 결정
        EquipType type = systemEquipmentManager.GetEquipType(equip);
        currentTab = type == EquipType.Weapon ? EquipTab.Weapon : EquipTab.Accessory;

        // 상단 카테고리 아이콘 반영
        if (popupRoot != null)
        {
            popupRoot.SetCategoryIcon(type);
        }

        // 상단 장비 정보 반영
        if (popupJsonBinder != null)
        {
            popupJsonBinder.SetEquipId(currentEquipId);
            popupJsonBinder.SetOwnedCount(systemEquipmentManager.GetCount(currentEquipId), 5);
        }

        // 강화 탭 반영
        if (popupEnhanceController != null)
        {
            popupEnhanceController.BindSelection(currentEquipId, currentTab, systemEquipmentManager, ResolveSprite);
        }

        // 융합 탭 반영
        if (popupMergeController != null)
        {
            popupMergeController.BindSelection(currentEquipId, currentTab, systemEquipmentManager, ResolveSprite);
        }
    }

    /// <summary>
    /// 인벤토리 변경 시 현재 장비 상세만 다시 반영
    /// </summary>
    private void OnInventoryChanged(EquipType changedType)
    {
        if (string.IsNullOrEmpty(currentEquipId))
        {
            return;
        }

        EquipData current = systemEquipmentManager != null ? systemEquipmentManager.GetEquipData(currentEquipId) : null;
        if (current == null)
        {
            return;
        }

        EquipType currentType = systemEquipmentManager.GetEquipType(current);
        if (currentType != changedType)
        {
            return;
        }

        SetEquipId(currentEquipId);
    }

    /// <summary>
    /// 강화되면 현재 장비면 갱신
    /// </summary>
    private void OnEquipmentEnhanced(string equipId, int newLevel)
    {
        if (string.IsNullOrEmpty(currentEquipId))
        {
            return;
        }

        if (equipId != currentEquipId)
        {
            return;
        }

        SetEquipId(currentEquipId);
    }

    /// <summary>
    /// 장착되면 현재 장비면 갱신
    /// </summary>
    private void OnEquipmentEquipped(string equipId, EquipType type, int level)
    {
        if (string.IsNullOrEmpty(currentEquipId))
        {
            return;
        }

        if (equipId != currentEquipId)
        {
            return;
        }

        SetEquipId(currentEquipId);
    }

    /// <summary>
    /// 융합되면 재료 장비나 결과 장비가 현재와 관련 있으면 갱신
    /// </summary>
    private void OnFusionComplete(string materialId, string resultId)
    {
        if (string.IsNullOrEmpty(currentEquipId))
        {
            return;
        }

        if (currentEquipId != materialId && currentEquipId != resultId)
        {
            return;
        }

        // 재료가 사라질 수도 있으므로 결과 장비로 전환 가능
        string targetId = systemEquipmentManager.GetCount(materialId) > 0 ? materialId : resultId;
        SetEquipId(targetId);
    }

    /// <summary>
    /// spriteName -> Sprite
    /// </summary>
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        return Resources.Load<Sprite>(spritePath + "/" + spriteName);
    }
}