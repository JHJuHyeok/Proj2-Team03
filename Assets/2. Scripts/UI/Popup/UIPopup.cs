using UnityEngine;

/*[승문]
UIPopup
-팝업 베이스
-PopupManager가 Open/Close를 호출
-필요하면 PopupId를 통해 종류를 구분
*/
public abstract class UIPopup : MonoBehaviour
{
    [SerializeField] private PopupId popupId = PopupId.None;
    public PopupId PopupId => popupId;

    public virtual void OnOpen(object param)
    {
        gameObject.SetActive(true);
    }

    public virtual void OnClose()
    {
        gameObject.SetActive(false);
    }

    // 닫기 버튼에서 호출
    public void CloseSelf()
    {
        if (PopupManager.Instance == null) return;
        PopupManager.Instance.CloseIfTop(this);
    }
}
