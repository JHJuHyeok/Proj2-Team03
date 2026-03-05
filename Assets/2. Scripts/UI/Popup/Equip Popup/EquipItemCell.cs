using UnityEngine;
using UnityEngine.UI;

/*
EquipItemCell
-장비팝업 리스트 셀
-컨트롤러가 넘겨주는 equipId/spriteName으로 표시
-클릭시 equipId를 컨트롤러로 콜백
*/
[RequireComponent(typeof(Button))]
public class EquipItemCell : MonoBehaviour
{
    [SerializeField] private Image iconImage;//셀 아이콘(팝업 프리팹 내부)
    [SerializeField] private GameObject selectedMark;//선택표시(선택)
    [SerializeField] private Button button;//셀 버튼(없으면 자동캐싱)

    private string equipId;
    private System.Action<string> onClick;
    private System.Func<string, Sprite> spriteResolver;

    private void Awake()
    {
        //필수컴포넌트 캐싱
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        //클릭이벤트 연결
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }

        SetSelected(false);
    }

    //셀 바인딩(컨트롤러가 호출)
    public void Bind(string id, string spriteName, System.Func<string, Sprite> resolver, System.Action<string> click)
    {
        equipId = id;
        spriteResolver = resolver;
        onClick = click;

        if (iconImage != null)
        {
            iconImage.sprite = spriteResolver != null ? spriteResolver(spriteName) : null;
        }
    }

    //선택표시
    public void SetSelected(bool selected)
    {
        if (selectedMark == null)
        {
            return;
        }

        selectedMark.SetActive(selected);
    }

    //현재 셀의 equipId 반환(컨트롤러가 선택 비교에 사용)
    public string GetEquipId()
    {
        return equipId;
    }

    //버튼 클릭 처리
    private void HandleClick()
    {
        if (string.IsNullOrEmpty(equipId))
        {
            return;
        }

        if (onClick != null)
        {
            onClick(equipId);
        }
    }
}