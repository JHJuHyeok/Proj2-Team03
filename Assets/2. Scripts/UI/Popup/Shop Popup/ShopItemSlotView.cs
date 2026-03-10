using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopItemSlotView
- 상점 슬롯 1칸 UI
- Weapon/Accessory: 한글 등급 + 아이콘 + 숫자등급 표시
- Skill: 등급텍스트 숨김 + 스킬이름 표시
*/
public class ShopItemSlotView : MonoBehaviour
{
    [SerializeField] private Image iconImage;         // 아이콘
    [SerializeField] private TMP_Text gradeText;      // 등급명 텍스트
    [SerializeField] private TMP_Text gradeNumText;   // 등급수치 텍스트
    [SerializeField] private TMP_Text skillNameText;  // 스킬이름 텍스트

    private int slotIndex;
    private System.Action<int> onClickBuy;
    private ShopTab currentLayoutTab = ShopTab.Weapon;

    private void Awake()
    {
        ApplyTabLayout(ShopTab.Weapon);
    }

    public void SetIndex(int index)
    {
        slotIndex = index;
    }

    public void ApplyTabLayout(ShopTab tab)
    {
        currentLayoutTab = tab;
        bool isSkill = tab == ShopTab.Skill;

        if (gradeText != null)
        {
            gradeText.gameObject.SetActive(!isSkill);
        }

        if (gradeNumText != null)
        {
            gradeNumText.gameObject.SetActive(!isSkill);
        }

        if (skillNameText != null)
        {
            skillNameText.gameObject.SetActive(isSkill);
        }
    }

    public void Bind(Sprite icon, string primaryText, string secondaryText, bool canBuy, System.Action<int> clickBuy)
    {
        onClickBuy = clickBuy;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = (icon != null);
        }

        bool isSkill = currentLayoutTab == ShopTab.Skill;

        if (!isSkill)
        {
            if (gradeText != null)
            {
                gradeText.text = primaryText ?? "";
            }

            if (gradeNumText != null)
            {
                gradeNumText.text = string.IsNullOrEmpty(secondaryText) ? "" : secondaryText + "등급";
            }

            if (skillNameText != null)
            {
                skillNameText.text = "";
            }
        }
        else
        {
            if (gradeText != null)
            {
                gradeText.text = "";
            }

            if (gradeNumText != null)
            {
                gradeNumText.text = "";
            }

            if (skillNameText != null)
            {
                skillNameText.text = primaryText ?? "";
            }
        }
    }

    public void Clear()
    {
        onClickBuy = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (gradeText != null)
        {
            gradeText.text = "";
        }

        if (gradeNumText != null)
        {
            gradeNumText.text = "";
        }

        if (skillNameText != null)
        {
            skillNameText.text = "";
        }
    }
}