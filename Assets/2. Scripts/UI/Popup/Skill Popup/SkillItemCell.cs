using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
[승문]
SkillItemCell
-스킬 리스트 셀 UI
-아이콘/이름/등급 표시
-Button 없이 이미지/텍스트만 있는 셀
-클릭 이벤트는 IPointerClickHandler로 처리
*/

public class SkillItemCell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;//스킬 아이콘
    [SerializeField] private TMP_Text nameText;//스킬 이름
    [SerializeField] private TMP_Text gradeText;//등급 텍스트
    [SerializeField] private GameObject selectedMark;//선택 표시

    private SkillData bound;
    private System.Action<SkillData> onClick;
    private System.Func<string, Sprite> spriteResolver;

    //셀 데이터 바인딩
    public void Bind(SkillData data, System.Func<string, Sprite> resolver, System.Action<SkillData> click)
    {
        bound = data;
        spriteResolver = resolver;
        onClick = click;

        if (data == null)
        {
            if (iconImage != null) iconImage.sprite = null;
            if (nameText != null) nameText.text = "";
            if (gradeText != null) gradeText.text = "";
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = spriteResolver != null ? spriteResolver(data.spriteName) : null;
        }

        if (nameText != null)
        {
            nameText.text = data.name;
        }

        if (gradeText != null)
        {
            gradeText.text = data.grade.ToString();
        }
    }

    //선택 표시 설정
    public void SetSelected(bool selected)
    {
        if (selectedMark == null)
        {
            return;
        }

        selectedMark.SetActive(selected);
    }

    //현재 바인딩된 데이터 반환
    public SkillData GetBound()
    {
        return bound;
    }

    //클릭 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        if (bound == null)
        {
            return;
        }

        if (onClick != null)
        {
            onClick(bound);
        }
    }
}