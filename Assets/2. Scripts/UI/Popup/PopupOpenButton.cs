using UnityEngine;

/*
[승문]
PopupOpenButton
-버튼 OnClick에 연결해서 PopupId로 팝업 열기
*/
public class PopupOpenButton : MonoBehaviour
{
    [SerializeField] private PopupId popupId;

    public void Open()
    {
        if (PopupManager.Instance == null)
        {
            Debug.LogError("PopupManager가 없습니다.");
            return;
        }

        PopupManager.Instance.Open(popupId);
    }
}