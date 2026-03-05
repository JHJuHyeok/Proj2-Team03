using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopItemSlotView
-상점 슬롯 1칸 UI
-Weapon/Accessory:등급텍스트 표시
-Skill:등급텍스트 숨김 + 스킬이름 표시
*/
public class ShopItemSlotView : MonoBehaviour
{
    [SerializeField] private Image iconImage;//아이콘
    [SerializeField] private TMP_Text gradeText;//등급명 텍스트
    [SerializeField] private TMP_Text gradeNumText;//등급수치 텍스트
    [SerializeField] private TMP_Text skillNameText;//스킬이름 텍스트

    private int slotIndex;
    private System.Action<int> onClickBuy;

    private void Awake()
    {
        ApplyTabLayout(ShopTab.Weapon);
    }

    //슬롯 인덱스 설정
    public void SetIndex(int index)
    {
        slotIndex = index;
    }

    //탭에 맞게 표시 방식 적용
    public void ApplyTabLayout(ShopTab tab)
    {
        bool isSkill = tab == ShopTab.Skill;

        //스킬탭이면 등급 숨김
        if (gradeText != null)
        {
            gradeText.gameObject.SetActive(isSkill == false);
        }

        if (gradeNumText != null)
        {
            gradeNumText.gameObject.SetActive(isSkill == false);
        }

        //스킬이름 표시
        if (skillNameText != null)
        {
            skillNameText.gameObject.SetActive(isSkill);
        }
    }

    //슬롯 내용 바인딩
    public void Bind(Sprite icon, string primaryText, string secondaryText, bool canBuy, System.Action<int> clickBuy)
    {
        onClickBuy = clickBuy;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }

        //장비탭 표시
        if (gradeText != null && gradeText.gameObject.activeSelf)
        {
            gradeText.text = primaryText != null ? primaryText : "";
        }

        if (gradeNumText != null && gradeNumText.gameObject.activeSelf)
        {
            gradeNumText.text = secondaryText != null ? secondaryText : "";
        }

        //스킬탭 표시
        if (skillNameText != null && skillNameText.gameObject.activeSelf)
        {
            skillNameText.text = primaryText != null ? primaryText : "";
        }
    }
}