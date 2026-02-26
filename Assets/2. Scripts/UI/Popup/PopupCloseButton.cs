using UnityEngine;
using UnityEngine.UI;
/*
[승문]
PopupCloseButton
-버튼 OnClick에 연결해서 PopupId로 팝업 닫기
*/
[RequireComponent(typeof(Button))]
public class PopupCloseButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.CloseTop();
            }
        });
    }
}