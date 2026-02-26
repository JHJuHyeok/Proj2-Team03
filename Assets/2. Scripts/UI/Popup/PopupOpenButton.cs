using UnityEngine;
using UnityEngine.UI;

/*
[승문]
PopupOpenButton
-버튼 OnClick에 연결해서 PopupId로 팝업 열기
*/
[RequireComponent(typeof(Button))]
public class PopupOpenButton : MonoBehaviour
{
    [SerializeField] private PopupId popupId = PopupId.None;
    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(Open);
    }

    public void Open()
    {
        if (PopupManager.Instance == null) return;
        PopupManager.Instance.Open(popupId);
    }
}