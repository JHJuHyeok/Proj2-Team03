using UnityEngine;

/*
[승문]
PopupView
- 기본 팝업용 공통 클래스
- 별도 로직이 없는 팝업에 사용
- popupId는 인스펙터에서 설정
- PopupManager에 의해 열리고 닫힘
*/
public class PopupView : UIPopup
{
    // 필요하면 여기서 OnOpen/OnClose 오버라이드
    public override void OnOpen(object param)
    {
        base.OnOpen(param);
        // 초기화 코드
    }

    public override void OnClose()
    {
        // 정리 코드
        base.OnClose();
    }
}