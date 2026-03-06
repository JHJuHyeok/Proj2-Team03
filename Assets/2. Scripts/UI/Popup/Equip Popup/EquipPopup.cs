using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
EquipPopup
- 장비 팝업 전용 스크립트
- PopupManager에서 PopupId.Equip로 호출
- param으로 EquipTab을 받아 아이콘과 컨텐츠를 변경
- param으로 "열릴 때 기본 탭"을 지정 : param이 int면: 0=강화, 1=융합
- [조민희] EquipPopupParam으로 장비 ID도 전달받아 초기 선택 가능
*/
public class EquipPopup : UIPopup
{
    [Header("Tab")]
    [SerializeField] private TabManager tabManager; // EquipPopup Tab Manager에 붙은 TabManager

    [Header("UI (선택)")]
    [SerializeField] private Image iconImage;//상단 아이콘
    [SerializeField] private Sprite weaponSprite;//무기 아이콘
    [SerializeField] private Sprite accessorySprite;//악세 아이콘

    [SerializeField] private EquipPopupController controller;//팝업 내부 컨텐츠

    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        EquipTab tab = EquipTab.Weapon;
        string initialEquipId = null; // [조민희] 초기 선택할 장비 ID

        // [조민희] EquipPopupParam 처리 (우선순위)
        if (param is EquipPopupParam popupParam)
        {
            tab = popupParam.Tab;
            initialEquipId = popupParam.EquipId;
        }
        else if (param is EquipTab t)
        {
            tab = t;
        }

        ApplyTab(tab);

        if (controller != null)
        {
            controller.SetEquipTab(tab);
            // [조민희] 초기 선택 장비 ID가 있으면 설정
            if (!string.IsNullOrEmpty(initialEquipId))
            {
                controller.SetInitialSelection(initialEquipId);
            }
        }
    }

    private void ApplyTab(EquipTab tab)
    {
        if (tab == EquipTab.Weapon)
        {
            if (iconImage != null) iconImage.sprite = weaponSprite;
        }
        else
        {
            if (iconImage != null) iconImage.sprite = accessorySprite;
        }
    }
}