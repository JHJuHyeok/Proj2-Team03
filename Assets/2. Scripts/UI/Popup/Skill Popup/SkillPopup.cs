using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SkillPopup
-스킬 상세 팝업 루트
-메인메뉴 스킬 그리드에서 클릭된 skillId(string)를 받아 상세 패널에 전달
-속성 아이콘은 전달받은 skillId의 실제 SkillData.element를 보고 결정
-팝업 내부 리스트는 없고 상세만 표시
*/
public class SkillPopup : UIPopup
{
    [Header("Popup UI(팝업 프리팹 내부)")]
    [SerializeField] private Image popupElementIconImage; // 속성 아이콘
    [SerializeField] private Sprite popupFireSprite;      // 불 아이콘
    [SerializeField] private Sprite popupWaterSprite;     // 물 아이콘
    [SerializeField] private Sprite popupWindSprite;      // 바람 아이콘
    [SerializeField] private Sprite popupEarthSprite;     // 땅 아이콘

    [Header("Popup Controller(팝업 프리팹 내부)")]
    [SerializeField] private SkillPopupContentController popupController; // 상세 컨트롤러

    /// <summary>
    /// 팝업 열기
    /// - param은 skillId(string) 하나만 받음
    /// </summary>
    public override void OnOpen(object param)
    {
        base.OnOpen(param);

        string skillId = param as string;
        if (string.IsNullOrEmpty(skillId))
        {
            Debug.LogWarning("[SkillPopup] skillId is null or empty.");
            return;
        }

        if (popupController != null)
        {
            popupController.SetSkillId(skillId);
        }
    }

    /// <summary>
    /// 속성 아이콘 반영
    /// </summary>
    public void SetElementIcon(SkillAttribute attribute)
    {
        if (popupElementIconImage == null)
        {
            return;
        }

        switch (attribute)
        {
            case SkillAttribute.Fire:
                popupElementIconImage.sprite = popupFireSprite;
                break;

            case SkillAttribute.Water:
                popupElementIconImage.sprite = popupWaterSprite;
                break;

            case SkillAttribute.Wind:
                popupElementIconImage.sprite = popupWindSprite;
                break;

            case SkillAttribute.Earth:
                popupElementIconImage.sprite = popupEarthSprite;
                break;
        }
    }
}