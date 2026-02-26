using UnityEngine;

public class SkillPopup : UIPopup
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