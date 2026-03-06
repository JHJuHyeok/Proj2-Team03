using UnityEngine;
using UnityEngine.UI;
using SlayerLegend.Equipment;

/*
[승문]
EquipPopup
-장비 상세 팝업 루트
-메인메뉴 장비 그리드에서 클릭된 equipId(string)를 받아 상세 패널에 전달
-무기/악세 타입에 따라 상단 카테고리 아이콘만 변경
-강화/융합 탭 전환은 TabManager가 담당하므로 여기서는 건드리지 않음
*/
public class EquipPopup : UIPopup
{
    [Header("Popup UI(팝업 프리팹 내부)")]
    [SerializeField] private Image popupCategoryIconImage; // 상단 카테고리 아이콘
    [SerializeField] private Sprite popupWeaponSprite;     // 무기 카테고리 아이콘
    [SerializeField] private Sprite popupAccessorySprite;  // 악세 카테고리 아이콘

    [Header("Popup Controller(팝업 프리팹 내부)")]
    [SerializeField] private EquipPopupController popupController; // 상세 컨트롤러

    /// <summary>
    /// 팝업 열기
    /// - param은 equipId(string) 하나만 받음
    /// - 리스트/그리드는 메인메뉴에 있고, 팝업은 상세만 표시
    /// </summary>
    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        string equipId = param as string;
        if (string.IsNullOrEmpty(equipId))
        {
            Debug.LogWarning("[EquipPopup] equipId is null or empty.");
            return;
        }

        if (popupController != null)
        {
            popupController.SetEquipId(equipId);
        }
    }

    /// <summary>
    /// 장비 타입에 맞는 카테고리 아이콘 반영
    /// </summary>
    public void SetCategoryIcon(EquipType type)
    {
        if (popupCategoryIconImage == null)
        {
            return;
        }

        if (type == EquipType.Weapon)
        {
            popupCategoryIconImage.sprite = popupWeaponSprite;
            return;
        }

        popupCategoryIconImage.sprite = popupAccessorySprite;
    }
}