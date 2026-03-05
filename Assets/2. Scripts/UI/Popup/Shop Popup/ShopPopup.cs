using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
[승문]
ShopPopup
- 상점 팝업 전용 스크립트
- PopupManager에서 PopupId.Shop으로 호출됨
- param으로 ShopTab(Weapon/Accessory/Skill)을 받아 UI 변경
*/
public class ShopPopup : UIPopup
{
    [Header("UI 참조")]
    [SerializeField] private Image iconImage;//카테고리 아이콘

    [Header("카테고리 아이콘 스프라이트")]
    [SerializeField] private Sprite weaponSprite;//무기 아이콘
    [SerializeField] private Sprite accessorySprite;//악세 아이콘
    [SerializeField] private Sprite skillSprite;//스킬 아이콘

    [Header("Content")]
    [SerializeField] private ShopPopupContentController controller;//상점 내부 컨텐츠

    /// <summary>
    /// 팝업 열릴 때 호출
    /// param으로 ShopTab 전달받음
    /// </summary>
    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        // 기본값 Weapon
        ShopTab tab = ShopTab.Weapon;

        if (param is ShopTab t)
        {
            tab = t;
        }

        Apply(tab);

        if (controller != null)
        {
            controller.SetTab(tab);
        }
    }

    /// <summary>
    /// 상점 카테고리에 따른 UI 적용
    /// </summary>
    private void Apply(ShopTab tab)
    {
        if (iconImage == null)
        {
            return;
        }

        switch (tab)
        {
            case ShopTab.Weapon:
                iconImage.sprite = weaponSprite;
                break;

            case ShopTab.Accessory:
                iconImage.sprite = accessorySprite;
                break;

            case ShopTab.Skill:
                iconImage.sprite = skillSprite;
                break;
        }
    }
}