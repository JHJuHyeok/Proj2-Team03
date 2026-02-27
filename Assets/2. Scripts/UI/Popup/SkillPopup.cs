using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SkillPopup
- 스킬 팝업 전용
- PopupManager에서 PopupId.Skill로 호출
- param으로 SkillAttribute(Fire/Water/Wind/Earth) 전달받음
- 속성은 텍스트 없이 아이콘으로만 표시
*/
public class SkillPopup : UIPopup
{
    [Header("아이콘 이미지")]
    [SerializeField] private Image elementIcon;  // 스킬 아이콘 이미지

    [Header("속성 아이콘 스프라이트")]
    [SerializeField] private Sprite fireSprite;
    [SerializeField] private Sprite waterSprite;
    [SerializeField] private Sprite windSprite;
    [SerializeField] private Sprite earthSprite;

    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        SkillAttribute attribute = SkillAttribute.Fire;

        if (param is SkillAttribute a)
            attribute = a;

        Apply(attribute);
    }

    private void Apply(SkillAttribute attribute)
    {
        if (elementIcon == null) return;

        switch (attribute)
        {
            case SkillAttribute.Fire:
                elementIcon.sprite = fireSprite;
                break;

            case SkillAttribute.Water:
                elementIcon.sprite = waterSprite;
                break;

            case SkillAttribute.Wind:
                elementIcon.sprite = windSprite;
                break;

            case SkillAttribute.Earth:
                elementIcon.sprite = earthSprite;
                break;
        }
    }
}