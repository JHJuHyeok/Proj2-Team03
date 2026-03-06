using UnityEngine;
using UnityEngine.UI;

/*
[승문]
EquipItemCell
-장비 팝업 리스트에서 1칸을 담당하는 셀
-equipId와 spriteName을 받아서 아이콘 표시
-클릭 시 선택된 equipId를 상위 컨트롤러로 전달
-선택 상태는 selectedMark SetActive로만 표현
*/
[RequireComponent(typeof(Button))]
public class EquipItemCell : MonoBehaviour
{
    [Header("Popup UI(팝업 프리팹 내부)")]
    [SerializeField] private Image iconImage;          // 셀 아이콘 이미지
    [SerializeField] private GameObject selectedMark;  // 선택 표시 오브젝트(선택)
    [SerializeField] private Button button;            // 클릭 버튼(없으면 자동 캐싱)

    // 현재 이 셀에 바인딩된 장비 ID
    private string equipId;

    // 상위 컨트롤러로 클릭을 전달할 콜백
    private System.Action<string> onClick;

    // spriteName -> Sprite 로 바꿔주는 함수
    private System.Func<string, Sprite> spriteResolver;

    private void Awake()
    {
        // 버튼 자동 캐싱
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        // 클릭 이벤트 연결
        if (button != null)
        {
            button.onClick.AddListener(HandleClick);
        }

        // 기본은 선택 해제
        SetSelected(false);
    }

    /// <summary>
    /// 셀 데이터 바인딩
    /// </summary>
    /// <param name="id">장비 ID</param>
    /// <param name="spriteName">아이콘 스프라이트 이름</param>
    /// <param name="resolver">스프라이트 로더</param>
    /// <param name="click">클릭 콜백</param>
    public void Bind(string id, string spriteName, System.Func<string, Sprite> resolver, System.Action<string> click)
    {
        equipId = id;
        spriteResolver = resolver;
        onClick = click;

        // 아이콘 반영
        if (iconImage != null)
        {
            iconImage.sprite = spriteResolver != null ? spriteResolver(spriteName) : null;
        }
    }

    /// <summary>
    /// 선택 표시 ON/OFF
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (selectedMark != null)
        {
            selectedMark.SetActive(selected);
        }
    }

    /// <summary>
    /// 현재 셀에 바인딩된 장비 ID 반환
    /// </summary>
    public string GetEquipId()
    {
        return equipId;
    }

    /// <summary>
    /// 버튼 클릭 처리
    /// </summary>
    private void HandleClick()
    {
        // 장비 ID가 없으면 무시
        if (string.IsNullOrEmpty(equipId))
        {
            return;
        }

        // 상위 컨트롤러로 선택 전달
        onClick?.Invoke(equipId);
    }
}