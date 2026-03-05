using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SlayerLegend.Equipment;

/*
EquipPopupContentController
-EquipPopup 내부 UI 컨트롤러
-EquipmentManager 인벤토리 기반 리스트 생성
-장비 선택 / 장착 / 보유개수 표시
*/
public class EquipPopupContentController : MonoBehaviour
{
    [SerializeField] private EquipmentManager equipmentManager;//장비 매니저
    [SerializeField] private Transform listRoot;//Scroll Content
    [SerializeField] private EquipItemCell cellPrefab;//리스트 셀

    [SerializeField] private Image detailIcon;//선택 아이콘
    [SerializeField] private TMP_Text detailName;//선택 이름
    [SerializeField] private TMP_Text ownedText;//보유 개수

    [SerializeField] private Button equipButton;//장착 버튼

    [SerializeField] private string spritePath = "Icons";//아이콘 경로

    private readonly List<EquipItemCell> cellPool = new List<EquipItemCell>();

    private EquipType currentType;
    private InventoryItem selectedItem;

    private void Awake()
    {
        if (equipmentManager == null)
        {
            equipmentManager = FindFirstObjectByType<EquipmentManager>();
        }

        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnClickEquip);
        }

        if (equipmentManager != null)
        {
            equipmentManager.OnInventoryChanged += OnInventoryChanged;
        }
    }

    private void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnInventoryChanged -= OnInventoryChanged;
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

    private void OnInventoryChanged(EquipType type)
    {
        if (type != currentType) return;

        RebuildList();
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

            if (equipButton != null) equipButton.interactable = false;

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

        int count = equipmentManager.GetEquipmentCount(equip);

        if (ownedText != null)
        {
            ownedText.text = count.ToString();
        }

        if (equipButton != null)
        {
            equipButton.interactable = true;
        }
    }

    private void OnClickEquip()
    {
        if (selectedItem == null) return;

        equipmentManager.EquipItem(selectedItem.equipment, selectedItem.level);
    }

    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        string path = spritePath + "/" + spriteName;

        return Resources.Load<Sprite>(path);
    }
}