using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
[승문]
EquipPopup
- 장비 팝업 전용 스크립트
- PopupManager에서 PopupId.Equip로 호출됨
- param으로 EquipTab(Weapon / Accessory)을 받아 UI 변경
*/
public class EquipPopup : UIPopup
{
    [Header("UI 참조")]
    [SerializeField] private TMP_Text titleText;   // 상단 제목 텍스트
    [SerializeField] private Image iconImage;     // 상단 아이콘 이미지

    [Header("아이콘 스프라이트")]
    [SerializeField] private Sprite weaponSprite;     // 무기 아이콘
    [SerializeField] private Sprite accessorySprite;  // 악세서리 아이콘

    /// <summary>
    /// 팝업 열릴 때 호출
    /// param으로 EquipTab 전달받음
    /// </summary>
    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        // 기본값은 Weapon
        EquipTab tab = EquipTab.Weapon;

        // param이 EquipTab이면 해당 값으로 적용
        if (param is EquipTab t)
            tab = t;

        ApplyTab(tab);
    }

    /// <summary>
    /// 무기 / 악세서리 UI 적용
    /// </summary>
    private void ApplyTab(EquipTab tab)
    {
        if (tab == EquipTab.Weapon)
        {
            if (titleText != null) titleText.text = "무기";
            if (iconImage != null) iconImage.sprite = weaponSprite;
        }
        else
        {
            if (titleText != null) titleText.text = "악세서리";
            if (iconImage != null) iconImage.sprite = accessorySprite;
        }
    }
}