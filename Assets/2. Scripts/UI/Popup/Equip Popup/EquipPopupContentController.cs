using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SlayerLegend.Equipment;

/*
EquipPopupContentController
-EquipPopup 내부 UI 컨트롤러
-EquipmentManager의 EquipInfo는 private라 직접 못읽음
-대신 DataManager DB를 훑고 EquipmentManager.GetCount(equipId)>0 인 것만 리스트로 구성
-선택시 상세(아이콘/이름/보유수) 갱신
-장착은 EquipmentManager.Equip(equipId) 호출
*/
public class EquipPopupContentController : MonoBehaviour
{
    [Header("Popup에서 연결")]
    [SerializeField] private EquipmentManager equipmentManager;//씬에 있는 장비매니저
    [SerializeField] private Transform listRoot;//팝업 ScrollContent
    [SerializeField] private EquipItemCell cellPrefab;//팝업 셀 프리팹

    [Header("Popup 상세UI에서 연결")]
    [SerializeField] private Image detailIcon;//상세 아이콘
    [SerializeField] private TMP_Text detailName;//상세 이름
    [SerializeField] private TMP_Text ownedText;//보유개수 텍스트
    [SerializeField] private Button equipButton;//장착버튼

    [Header("Resource")]
    [SerializeField] private string spritePath = "Icons";//Resources/Icons

    private readonly List<EquipItemCell> cellPool = new List<EquipItemCell>(64);
    private readonly List<string> ownedIds = new List<string>(256);

    private EquipType currentType;
    private string selectedEquipId;

    private void Awake()
    {
        //참조 자동캐싱
        if (equipmentManager == null)
        {
            equipmentManager = FindFirstObjectByType<EquipmentManager>();
        }

        //버튼 이벤트
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnClickEquip);
        }

        //인벤토리 변경 이벤트
        if (equipmentManager != null)
        {
            equipmentManager.OnInventoryChanged += OnInventoryChanged;
        }

        //초기상태
        selectedEquipId = null;
        ApplySelection(null);
    }

    private void OnDestroy()
    {
        //이벤트 해제
        if (equipmentManager != null)
        {
            equipmentManager.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    //팝업에서 탭 전달
    public void SetEquipTab(EquipTab tab)
    {
        //탭->EquipType 매핑
        if (tab == EquipTab.Weapon)
        {
            currentType = EquipType.Weapon;
        }
        else
        {
            currentType = EquipType.Accessorie;
        }

        selectedEquipId = null;

        RebuildList();
        ApplySelection(null);
    }

    //인벤토리 변경 시 현재 탭만 갱신
    private void OnInventoryChanged(EquipType type)
    {
        if (type != currentType)
        {
            return;
        }

        RebuildList();
        ApplySelection(selectedEquipId);
    }

    //리스트 재구성
    private void RebuildList()
    {
        if (equipmentManager == null)
        {
            Debug.LogError("[EquipPopupContentController] equipmentManager missing.");
            return;
        }

        ownedIds.Clear();

        //DB에서 전부 훑어서 보유한 것만 수집
        if (currentType == EquipType.Weapon)
        {
            if (DataManager.weapons == null)
            {
                Debug.LogError("[EquipPopupContentController] DataManager.weapons is null.");
                return;
            }

            List<EquipData> all = DataManager.weapons.GetAll();
            for (int i = 0; i < all.Count; i++)
            {
                EquipData data = all[i];
                if (data == null)
                {
                    continue;
                }

                string id = data.GetId();
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                int count = equipmentManager.GetCount(id);
                if (count > 0)
                {
                    ownedIds.Add(id);
                }
            }
        }
        else
        {
            if (DataManager.accessories == null)
            {
                Debug.LogError("[EquipPopupContentController] DataManager.accessories is null.");
                return;
            }

            List<EquipData> all = DataManager.accessories.GetAll();
            for (int i = 0; i < all.Count; i++)
            {
                EquipData data = all[i];
                if (data == null)
                {
                    continue;
                }

                string id = data.GetId();
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                int count = equipmentManager.GetCount(id);
                if (count > 0)
                {
                    ownedIds.Add(id);
                }
            }
        }

        EnsurePool(ownedIds.Count);

        for (int i = 0; i < cellPool.Count; i++)
        {
            bool active = i < ownedIds.Count;
            EquipItemCell cell = cellPool[i];

            cell.gameObject.SetActive(active);

            if (active == false)
            {
                continue;
            }

            string equipId = ownedIds[i];
            EquipData equipData = equipmentManager.GetEquipData(equipId);

            string spriteName = "";
            if (equipData != null)
            {
                spriteName = equipData.spriteName;
            }

            //cell.Bind(equipId, spriteName, ResolveSprite, OnClickItem);
            cell.SetSelected(string.IsNullOrEmpty(selectedEquipId) == false && selectedEquipId == equipId);
        }
    }

    //풀 확보
    private void EnsurePool(int need)
    {
        if (cellPrefab == null || listRoot == null)
        {
            Debug.LogError("[EquipPopupContentController] cellPrefab/listRoot missing.");
            return;
        }

        while (cellPool.Count < need)
        {
            EquipItemCell cell = Instantiate(cellPrefab, listRoot);
            cell.gameObject.SetActive(false);
            cellPool.Add(cell);
        }
    }

    //셀 클릭
    private void OnClickItem(string equipId)
    {
        selectedEquipId = equipId;

        for (int i = 0; i < cellPool.Count; i++)
        {
            if (cellPool[i].gameObject.activeSelf == false)
            {
                continue;
            }

            //bool selected = cellPool[i].GetEquipId() == selectedEquipId;
            //cellPool[i].SetSelected(selected);
        }

        ApplySelection(selectedEquipId);
    }

    //상세 갱신
    private void ApplySelection(string equipId)
    {
        if (string.IsNullOrEmpty(equipId))
        {
            if (detailIcon != null) detailIcon.sprite = null;
            if (detailName != null) detailName.text = "";
            if (ownedText != null) ownedText.text = "";

            if (equipButton != null) equipButton.interactable = false;
            return;
        }

        EquipData equip = equipmentManager != null ? equipmentManager.GetEquipData(equipId) : null;

        if (detailIcon != null)
        {
            detailIcon.sprite = equip != null ? ResolveSprite(equip.spriteName) : null;
        }

        if (detailName != null)
        {
            detailName.text = equip != null ? equip.GetName() : equipId;
        }

        if (ownedText != null && equipmentManager != null)
        {
            ownedText.text = equipmentManager.GetCount(equipId).ToString();
        }

        if (equipButton != null)
        {
            equipButton.interactable = true;
        }
    }

    //장착 버튼
    private void OnClickEquip()
    {
        if (equipmentManager == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(selectedEquipId))
        {
            return;
        }

        bool ok = equipmentManager.Equip(selectedEquipId);
        Debug.Log(ok ? "[EquipPopupContentController] Equip ok." : "[EquipPopupContentController] Equip fail.");
    }

    //스프라이트 로드
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        string path = spritePath + "/" + spriteName;
        return Resources.Load<Sprite>(path);
    }
}