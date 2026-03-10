using UnityEngine;

/*
[승문]
ShopPopup
- 상점 팝업 전용 스크립트
- PopupManager에서 PopupId.Shop으로 호출됨
- param으로 ShopTab(Weapon/Accessory/Skill)을 받아 내부 컨텐츠만 변경
- 카테고리 표시용 상단 아이콘은 사용하지 않음
*/
public class ShopPopup : UIPopup
{
    [Header("Content")]
    [SerializeField] private ShopPopupContentController controller; // 상점 내부 컨텐츠

    /// <summary>
    /// 팝업 열릴 때 호출
    /// param으로 ShopTab 전달받음
    /// </summary>
    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        ShopTab tab = ShopTab.Weapon;

        if (param is ShopTab t)
        {
            tab = t;
        }

        if (controller != null)
        {
            controller.SetTab(tab);
        }
        else
        {
            Debug.LogWarning("[ShopPopup] controller is null.");
        }
    }
}