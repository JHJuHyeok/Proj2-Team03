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
    [SerializeField] private TMP_Text titleText;   // 상점 카테고리 텍스트
    [SerializeField] private Image iconImage;     // 카테고리 아이콘

    [Header("카테고리 아이콘 스프라이트")]
    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private Sprite accessorySprite;
    [SerializeField] private Sprite skillSprite;

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
            tab = t;

        Apply(tab);
    }

    /// <summary>
    /// 상점 카테고리에 따른 UI 적용
    /// </summary>
    private void Apply(ShopTab tab)
    {
        switch (tab)
        {
            case ShopTab.Weapon:
                titleText.text = "무기 상점";
                iconImage.sprite = weaponSprite;
                break;

            case ShopTab.Accessory:
                titleText.text = "악세서리 상점";
                iconImage.sprite = accessorySprite;
                break;

            case ShopTab.Skill:
                titleText.text = "스킬 상점";
                iconImage.sprite = skillSprite;
                break;
        }
    }
}